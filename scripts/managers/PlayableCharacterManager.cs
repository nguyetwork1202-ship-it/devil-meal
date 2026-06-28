using Godot;

public partial class PlayableCharacterManager : Node, IInputContextHandler
{
	[Export] public PlayableCharacterConfig CharacterConfig { get; set; }
	public static PlayableCharacterManager Instance { get; private set; }
	public InputContext Context => InputContext.GAMEPLAY;

	private PlayableCharacter _player;

	public override void _Ready() 
	{
		Instance = this;

		_player = new PlayableCharacter();
		_player.ApplyConfig(CharacterConfig);
		_player.SetState(EntityState.HIDDEN);
		
		AddChild(_player);
		
		InputContextManager.Instance.Register(this);
	}
    public override void _ExitTree()
	{
		InputContextManager.Instance.Unregister(this);
	}

	public void OnInputContextChanged(InputContext previousContext, InputContext currentContext)
	{
		if (previousContext == Context)
    	{
        	HandleLeavingGameplay(currentContext);
    	}

		if (currentContext == Context)
    	{
        	HandleReturningToGameplay(previousContext);
    	}
	}
	private void HandleLeavingGameplay(InputContext currentContext)
	{
    	switch (currentContext)
    	{
        	case InputContext.CUTSCENE:
            	SnapshotCharacterConfig();
            	_player.SetState(EntityState.HIDDEN);
            	break;
        	case InputContext.TRANSITION:
            	SnapshotCharacterConfig();
            	_player.SetState(EntityState.FROZEN);
            	break;
        	default:
            	_player.SetState(EntityState.FROZEN);
    			break;
    	}
	}
	private void HandleReturningToGameplay(InputContext previousContext)
	{
    	switch (previousContext)
    	{
        	case InputContext.CUTSCENE:
            	ApplyCharacterConfig();
            	_player.SetState(EntityState.ACTIVE);
            	break;
        	default:
            	_player.SetState(EntityState.ACTIVE);
            	break;
    	}
	}
	private void SnapshotCharacterConfig()
	{
		CharacterConfig = _player.getCurrentConfig();
	}
	private void ApplyCharacterConfig()
	{
		_player.ApplyConfig(CharacterConfig);
	}
}
