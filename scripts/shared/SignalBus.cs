using Godot;

public partial class SignalBus : Node
{
	private static SignalBus _instance;
	public static SignalBus Instance => _instance ??= new SignalBus();

	// ===  Inventory Management ===
	[Signal] public delegate void OpenInventoryEventHandler();
	
	// === Menu Management ===
	[Signal] public delegate void OpenMenuEventHandler(MenuType menuType);
	[Signal] public delegate void CloseMenuEventHandler();

	// === Scene Management ===
	[Signal] public delegate void LoadSceneEventHandler(string scenePath);
	[Signal] public delegate void ExitGameEventHandler();
	
	// === Cutscene Management ===
	[Signal] public delegate void PlayDeathAnimationEventHandler(string animationName);
}
