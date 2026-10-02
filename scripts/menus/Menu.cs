using Godot;
using System.Collections.Generic;

public partial class Menu : Node2D
{
	[Signal] public delegate void CloseMenuEventHandler();

	[Export] private CanvasLayer _layer;

	[Export] private float _frameWidth = 1f;
	[Export] private Color _frameColor = Colors.White;

	private List<MenuComponent> _components;
	private EntityState _currentState = EntityState.ACTIVE;
	private int _currentIndex = 0;
	private bool _isFocused = false;
	private bool _isDestroyable = false;

	public override void _Ready()
	{
		 _components = new List<MenuComponent>();
        foreach (Node child in GetChildren())
        {
            if (child is MenuComponent component)
            {
                _components.Add(component);
            }
        }

		if(_components.Count == 1)
		{
			ActivateCurrentComponent();
		}

		UILayerManager.Instance.Push(_layer);
	}
	public override void _Process(double delta)
	{
		if(_currentState != EntityState.ACTIVE ||
		 _components.Count == 0 ||
		 _isFocused) return;

		HandleMovement();
		HandleSelection();
		HandleExit();
	}
    public override void _Draw()
    {
		if(_components.Count <= 1) return;
		Rect2 rect = _components[_currentIndex].GetFrame();
		DrawRect(rect, _frameColor, false, _frameWidth);
    }

	// === State ===
	public EntityState State => _currentState;
	public void SetState(EntityState state)
	{
		if(_currentState == state) return;
		_currentState = state;

		Visible = state == EntityState.ACTIVE;

		if(state == EntityState.INACTIVE)
			UILayerManager.Instance.Remove(_layer);	
	}

	// === Component Management ===
	private void ActivateCurrentComponent()
	{
		_components[_currentIndex].SetState(EntityState.ACTIVE);
		_isFocused = true;
	}
	private void HandleMovement()
	{
		bool down = Input.IsActionJustPressed(GlobalVariables.MoveDownAction);
		bool up = Input.IsActionJustPressed(GlobalVariables.MoveUpAction);

		if(down)
		{
			_currentIndex = (_currentIndex + 1) % _components.Count;
		}
		else if(up) 
		{
			_currentIndex = (_currentIndex - 1 + _components.Count) % _components.Count;
		}

		if (down || up) QueueRedraw();
	}
	private void HandleSelection()
	{
		if (Input.IsActionJustPressed(GlobalVariables.AcceptAction))
		{
			ActivateCurrentComponent();
		}
	}
	private void HandleExit()
	{
		if (Input.IsActionJustPressed(GlobalVariables.CancelAction) && _isDestroyable)
		{
			EmitSignal(SignalName.CloseMenu);
		}
	}

	// === Component Callback ===
	public void OnComponentUnfocused()
	{
		_isFocused = false;
	}
}
