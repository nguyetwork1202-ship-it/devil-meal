using System.Collections.Generic;
using Godot;

public abstract partial class Menu : Node
{
	[Signal] public delegate void LoadSceneRequestedEventHandler(string scenePath);

	private string _currentMenuId = "";
	private int _currentIndex = 0;
	private Dictionary<string, List<MenuItem>> _childrenByParentId = new();
	private Dictionary<string, MenuItem> _itemsById = new();
	private Stack<string> _menuStack = new();

	public override void _Process(double delta)
	{
		HandleBackInput();
		HandleMovementInput();
		HandleConfirmInput();	
	}

	public void LoadMenu(MenuDataset dataset)
	{
    	BuildIndex(dataset);
    	_menuStack.Clear();
		_currentMenuId = "";
    	_currentIndex = 0;
	}
	public List<MenuItem> GetCurrentMenu()
	{
		return _childrenByParentId.GetValueOrDefault(_currentMenuId, new()); 
	}

	protected abstract void ExecuteFunction(string functionCode);

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
	}
	private void NavigateBack()
	{
		if (_menuStack.Count == 0) return;
		_currentMenuId = _menuStack.Pop();
		_currentIndex = 0;
	}

	private void HandleBackInput()
	{
		if (Input.IsActionJustPressed(GlobalVariables.BackAction))
		{
			NavigateBack();
		}
	}
	private void HandleMovementInput()
	{
		var items = GetCurrentMenu();
    	if (items.Count == 0) return;

		if (Input.IsActionJustPressed(GlobalVariables.MoveDownAction))
		{
			_currentIndex = (_currentIndex + 1) % items.Count;
		}
		else if (Input.IsActionJustPressed(GlobalVariables.MoveUpAction))
		{
			_currentIndex = (_currentIndex - 1 + items.Count) % items.Count;
		}
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
}

