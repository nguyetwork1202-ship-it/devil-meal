using System.Collections.Generic;
using Godot;

public partial class Menu : Node2D
{
	[Export] protected MenuDataset _dataset;
	[Export] protected CanvasLayer _layer;
	[Export] protected Texture2D _backgroundTexture;

	[Signal] public delegate void LoadSceneEventHandler(string scenePath);
	[Signal] public delegate void CloseMenuEventHandler();

	protected string _currentMenuId = "";
	protected int _currentIndex = 0;
	protected EntityState _currentState = EntityState.ACTIVE;
	protected bool _isDestroyable = false;
	protected Dictionary<string, List<MenuItem>> _childrenByParentId = new();
	protected Dictionary<string, MenuItem> _itemsById = new();
	protected Stack<string> _menuStack = new();

	public override void _Ready()
	{
		BuildIndex(_dataset);
		ResetMenuNavigation();
		QueueRedraw();

		UILayerManager.Instance.Push(_layer);
	}
	public override void _Process(double delta)
	{
		if(_currentState != EntityState.ACTIVE) return;

		HandleBackInput();
		HandleNavigationInput();
		HandleConfirmInput();	
	}

	public List<MenuItem> CurrentMenu => _childrenByParentId.GetValueOrDefault(_currentMenuId, new()); 
	public int CurrentIndex => _currentIndex;
	public EntityState State => _currentState;

	public void SetState(EntityState state)
	{
		_currentState = state;
		Visible = state != EntityState.HIDDEN;
		QueueRedraw();

		if(state == EntityState.HIDDEN)
			UILayerManager.Instance.Remove(_layer);	
	}
	public void ResetMenuNavigation()
	{
		_menuStack.Clear();
		_currentMenuId = "";
		_currentIndex = 0;
	}

	protected virtual void DrawCurrentMenu() {}
	protected virtual void ExecuteFunction(string functionCode) {}

	private void BuildIndex(MenuDataset dataset)
    {
        _childrenByParentId.Clear();
        _itemsById.Clear();

        foreach (var item in dataset.Items)
        {
            _itemsById[item.Id] = item;

            string parent = item.ParentId ?? "";
            if (!_childrenByParentId.ContainsKey(parent))
                _childrenByParentId[parent] = new List<MenuItem>();
            _childrenByParentId[parent].Add(item);
        }
    }
	
	private void NavigateToMenu(string menuId)
	{
    	_menuStack.Push(_currentMenuId);

    	_currentMenuId = menuId;
    	_currentIndex = 0;

		QueueRedraw();
	}
	private void NavigateBack()
	{
		if (_menuStack.Count == 0)
		{
			if (_isDestroyable)
				EmitSignal(SignalName.CloseMenu);

			return;
		}
		
		_currentMenuId = _menuStack.Pop();
		_currentIndex = 0;

		QueueRedraw();
	}

	private void HandleBackInput()
	{
		if (Input.IsActionJustPressed(GlobalVariables.BackAction))
		{
			NavigateBack();
		}
	}
	private void HandleNavigationInput()
	{
		var items = CurrentMenu;
		if (items.Count == 0) return;

		bool down = Input.IsActionJustPressed(GlobalVariables.MoveDownAction);
		bool up = Input.IsActionJustPressed(GlobalVariables.MoveUpAction);

		if (down)
			_currentIndex = (_currentIndex + 1) % items.Count;
		else if (up)
			_currentIndex = (_currentIndex - 1 + items.Count) % items.Count;

		if (down || up)  QueueRedraw();
	}
	private void HandleConfirmInput()
	{
		if (!Input.IsActionJustPressed(GlobalVariables.AcceptAction)) return;

		var items = CurrentMenu;
		if (items.Count == 0) return;

		var item = items[_currentIndex];

		switch (item.Action)
		{
			case MenuAction.OPEN_SUBMENU:
				NavigateToMenu(item.Id);
				break;
			case MenuAction.LOAD_SCENE:
				EmitSignal(SignalName.LoadScene, item.ActionTarget);
				break;
			case MenuAction.EXECUTE:
				ExecuteFunction(item.ActionTarget);
				break;	
		}
	}

	public override void _Draw()
	{
		DrawBackground();
		DrawCurrentMenu();
	}
	protected virtual void DrawBackground()
	{
		if (_backgroundTexture == null) return;

		var viewportSize = GetViewportRect().Size;
		var rect = new Rect2(Vector2.Zero, viewportSize);

		DrawTextureRect(_backgroundTexture, rect, tile: false);
	}
}
