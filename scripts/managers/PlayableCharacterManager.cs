using Godot;

public partial class PlayableCharacterManager : Node, IInputContextHandler
{
	[Export] public PlayableCharacterConfig CharacterConfig { get; set; }
	[Export] private PlayableCharacter _player;

	public static PlayableCharacterManager Instance { get; private set; }
	public InputContext Context => InputContext.GAMEPLAY;

	private PlayableCharacterManager() {}

	public override void _Ready() 
	{
		Instance = this;

		_player.ApplyConfig(CharacterConfig);
		_player.SetState(EntityState.HIDDEN);
		
		InputContextManager.Instance.Register(this);
	}
	public override void _ExitTree()
	{
		InputContextManager.Instance?.Unregister(this);
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
	public void OnSceneChanged()
	{
		_player.SetState(EntityState.HIDDEN);
	}

	private void HandleReturningToGameplay(InputContext previousContext)
	{
		switch (previousContext)
		{
			case InputContext.CUTSCENE or InputContext.SCENE_TRANSITION:
				ApplyCharacterConfig();
				_player.SetState(EntityState.ACTIVE);
				break;
			default: // DIALOG, INVENTORY, MENU, NONE
				_player.SetState(EntityState.ACTIVE);
				break;
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
			case InputContext.SCENE_TRANSITION:
				SnapshotCharacterConfig();
				_player.SetState(EntityState.FROZEN);
				break;
			default: // DIALOG, INVENTORY, MENU
				_player.SetState(EntityState.FROZEN);
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
