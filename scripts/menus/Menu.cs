using System.Collections.Generic;
using Godot;

public partial class Menu : Node2D
{
	[Signal] public delegate void LoadSceneEventHandler(string scenePath);
	[Signal] public delegate void CloseMenuEventHandler();

	protected string _currentMenuId = "";
	protected int _currentIndex = 0;
	protected EntityState _currentState = EntityState.HIDDEN;
	protected bool _isDestroyable = false;
	protected Dictionary<string, List<MenuItem>> _childrenByParentId = new();
	protected Dictionary<string, MenuItem> _itemsById = new();
	protected Stack<string> _menuStack = new();

	private Menu() {}
	public Menu(MenuDataset dataset)
	{
    	BuildIndex(dataset);
		ResetMenuNavigation();
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
	}
	public void ResetMenuNavigation()
	{
		_menuStack.Clear();
		_currentMenuId = "";
		_currentIndex = 0;
	}

	protected virtual void ExecuteFunction(string functionCode) {}
	protected virtual void UpdateMenuDisplay() {}

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

		UpdateMenuDisplay();
	}
	private void NavigateBack()
	{
		if (_menuStack.Count == 0)
		{
			if(_isDestroyable)
				EmitSignal(SignalName.CloseMenu);	

			return;
		}
		
		_currentMenuId = _menuStack.Pop();
		_currentIndex = 0;

		UpdateMenuDisplay(); 
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

		if (down || up)  UpdateMenuDisplay();
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
}
