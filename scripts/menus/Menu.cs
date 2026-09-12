using System.Collections.Generic;
using Godot;

public partial class Menu : Node2D
{
	[Signal] public delegate void LoadSceneEventHandler(string scenePath);
	[Signal] public delegate void OpenMenuEventHandler(string menuId);
	[Signal] public delegate void CloseMenuEventHandler();

	// Data
	[Export] protected MenuDataset _dataset;
	[Export] protected CanvasLayer _layer;

	// Text properties
	[Export] protected Font _font;
	[Export] protected int _fontSize;
	[Export] protected float _lineSpacing;
	[Export] protected float _maxHeight;
	[Export] protected HorizontalAlignment _textAlignment = HorizontalAlignment.Left;
	[Export] protected Color _defaultColor = Colors.White;

	// Box properties
	[Export] private float _boxPadding;
	[Export] private float _boxOutlineWidth = 1f;
	[Export] protected Color _boxOutlineColor = Colors.White;

	protected string _currentMenuId = string.Empty;
	protected Vector2 _longestLabelSize = Vector2.Zero;
	protected Vector2 _boxSize = Vector2.Zero;
	protected int _currentIndex = 0;
	protected EntityState _currentState = EntityState.ACTIVE;
	protected bool _isDestroyable = false;
	protected Dictionary<string, List<MenuItem>> _childrenByParentId = new();
	protected Dictionary<string, MenuItem> _itemsById = new();
	protected Stack<string> _menuStack = new();

	public override void _Ready()
	{
		CalculateBoxSize();
		
		BuildIndex();
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

	// === Getters ===
	public List<MenuItem> CurrentMenu => _childrenByParentId.GetValueOrDefault(_currentMenuId, new()); 
	public int CurrentIndex => _currentIndex;
	public EntityState State => _currentState;

	// === State ===
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

	private void CalculateBoxSize()
	{
		_longestLabelSize = FindLongestLabelSize();
		_boxSize.X = _longestLabelSize.X + _boxPadding * 2;
		_boxSize.Y = _longestLabelSize.Y + _lineSpacing;
	}
	private Vector2 FindLongestLabelSize()
	{
		Vector2 longestLabelSize = Vector2.Zero;

		foreach (var item in _dataset.Items)
		{
			Vector2 size = _font.GetStringSize(item.Label, _textAlignment, -1, _fontSize);
			if (size.X > longestLabelSize.X)
				longestLabelSize = size;
		}

		return longestLabelSize;
	}
	private void BuildIndex()
    {
        _childrenByParentId.Clear();
        _itemsById.Clear();

		foreach (var item in _dataset.Items)
        {
            _itemsById[item.Id] = item;

            string parent = item.ParentId ?? "";
            if (!_childrenByParentId.ContainsKey(parent))
                _childrenByParentId[parent] = new List<MenuItem>();
            _childrenByParentId[parent].Add(item);
        }
    }
	
	// === Navigation ===
	private void NavigateToMenu(string menuId)
	{
    	_menuStack.Push(_currentMenuId);
    	_currentMenuId = menuId;
    	_currentIndex = 0;

		CalculateBoxSize();

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

	// === Input Handling ===
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
			case MenuAction.EXECUTE:
				ExecuteFunction(item.ActionTarget);
				break;
			case MenuAction.LOAD_SCENE:
				EmitSignal(SignalName.LoadScene, item.ActionTarget);
				break;
			case MenuAction.OPEN_MENU:
				EmitSignal(SignalName.OpenMenu, item.ActionTarget);
				break;		
			case MenuAction.OPEN_SUBMENU:
				NavigateToMenu(item.Id);
				break;	
		}
	}
	protected virtual void ExecuteFunction(string functionCode) {}

	// === UI ===
	public override void _Draw()
	{
		List<MenuItem> items = CurrentMenu;
		if (items.Count == 0) return;

		float lineHeight = _lineSpacing + _font.GetHeight(_fontSize);
    	if (lineHeight <= 0) return;

		int itemsPerPage = Mathf.Max(1, Mathf.FloorToInt(_maxHeight / lineHeight));

		int page = _currentIndex / itemsPerPage;
		int startIndex = page * itemsPerPage;
		int endIndex = Mathf.Min(startIndex + itemsPerPage, items.Count);

		for (int i = startIndex; i < endIndex; i++)
		{
			var item = items[i];
			int visibleRow = i - startIndex;

			var position = new Vector2(0, visibleRow * lineHeight);

			if (i == _currentIndex)
				DrawSelectedOption(position, item.Label);
			else
				DrawOption(position, item.Label);
		}
	}
	protected virtual void DrawOption(Vector2 position, string label)
	{
		DrawString(_font, position, label, _textAlignment, _longestLabelSize.X, _fontSize, _defaultColor);
	}
	protected virtual void DrawSelectedOption(Vector2 position, string label)
	{
		DrawSelectedBox(position);	
		DrawOption(position, label);
	}
	private void DrawSelectedBox(Vector2 position)
	{
		float ascent = _font.GetAscent(_fontSize);
		Vector2 rectPosition = new Vector2(position.X - _boxPadding,
			position.Y - ascent - _lineSpacing / 2);

		Rect2 rect = new Rect2(rectPosition, _boxSize);
		DrawRect(rect, _boxOutlineColor, filled: false, width: _boxOutlineWidth);
	}
}
