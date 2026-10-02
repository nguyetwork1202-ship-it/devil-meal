using Godot;

public partial class MenuComponent : Node
{
	protected EntityState _currentState;
	protected Menu _parentMenu;
	
	public EntityState State => _currentState;
	public void SetState(EntityState newState)
	{
		if(_currentState == newState) return;
		
		_currentState = newState;
		OnStateChanged(newState);
	}

	public override void _Ready()
	{
		_currentState = EntityState.PAUSED;
		_parentMenu = GetParent<Menu>();
	}
	public override void _Process(double delta)
	{
		if(State != EntityState.ACTIVE) return;
		OnActiveProcess(delta);
	}

	public virtual Rect2 GetFrame()
	{
		return new Rect2(Vector2.Zero, Vector2.Zero);
	}

	protected virtual void OnStateChanged(EntityState newState) {}
	protected virtual void OnActiveProcess(double delta) {}
}
