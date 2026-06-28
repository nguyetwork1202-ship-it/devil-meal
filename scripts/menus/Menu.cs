using System.Collections.Generic;
using Godot;

public partial class Menu : Node2D
{
	[Signal] public delegate void LoadSceneRequestedEventHandler(string scenePath);

	private string _currentMenuId = "";
	private int _currentIndex = 0;
	private bool _isActive = false;
	private Dictionary<string, List<MenuItem>> _childrenByParentId = new();
	private Dictionary<string, MenuItem> _itemsById = new();
	private Stack<string> _menuStack = new();

	private Menu() {}
	public Menu(MenuDataset dataset)
	{
    	BuildIndex(dataset);
		ResetMenuState();
	}

	public override void _Process(double delta)
	{
		if(!_isActive) return;

		HandleBackInput();
		HandleNavigationInput();
		HandleConfirmInput();	
	}

	public List<MenuItem> GetCurrentMenu()
	{
		return _childrenByParentId.GetValueOrDefault(_currentMenuId, new()); 
	}
	public int GetCurrentIndex() => _currentIndex;
	public bool IsActive() => _isActive;
	public void SetActive(bool active)
	{
		if(_isActive == active) return;

		if (active)
		{
			ResetMenuState();	
		}

		Visible = active;
		_isActive = active;
	}

	protected virtual void ExecuteFunction(string functionCode) {}
	protected virtual void UpdateUiProperties() {}

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

		UpdateUiProperties();
	}
	private void NavigateBack()
	{
		if (_menuStack.Count == 0) return;
		
		_currentMenuId = _menuStack.Pop();
		_currentIndex = 0;

		UpdateUiProperties();
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
		var items = GetCurrentMenu();
		if (items.Count == 0) return;

		bool down = Input.IsActionJustPressed(GlobalVariables.MoveDownAction);
		bool up = Input.IsActionJustPressed(GlobalVariables.MoveUpAction);

		if (down)
			_currentIndex = (_currentIndex + 1) % items.Count;
		else if (up)
			_currentIndex = (_currentIndex - 1 + items.Count) % items.Count;

		if (down || up) UpdateUiProperties();
	}
	private void HandleConfirmInput()
	{
		if (!Input.IsActionJustPressed(GlobalVariables.InteractAction)) return;

		var items = GetCurrentMenu();
		if (items.Count == 0) return;

		var item = items[_currentIndex];

		switch (item.Action)
		{
			case MenuAction.OPEN_SUBMENU:
				NavigateToMenu(item.Id);
				break;
			case MenuAction.LOAD_SCENE:
				EmitSignal(SignalName.LoadSceneRequested, item.ActionTarget);
				break;
			case MenuAction.EXECUTE:
				ExecuteFunction(item.ActionTarget);
				break;	
		}
	}
	private void ResetMenuState()
	{
		_menuStack.Clear();
		_currentMenuId = "";
    	_currentIndex = 0;
	}
}

