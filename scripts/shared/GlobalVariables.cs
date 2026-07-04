public partial class GlobalVariables
{
	// Actions
	public const string MoveUpAction = "move_up";
	public const string MoveDownAction = "move_down";
	public const string MoveLeftAction = "move_left";
	public const string MoveRightAction = "move_right";
	public const string RunAction = "run";
	public const string AcceptAction = "accept";
	public const string CancelAction = "cancel";
	public const string BackAction = "back";
	public const string OpenInventoryAction = "open_inventory";
	public const string OpenPauseMenu = "open_pause_menu";

	// Masks
	public const uint InteractionMask = 1;
	public const uint SolidMask = 2;

	// Default statuses
	public const float DefaultPosX = 144;
	public const float DefaultPosY = 66;

	public const float DefaultSpeed = 80.0f;
	public const float DefaultRunSpeed = 120.0f;

	public const FacingDirection DefaultFacingDirection = FacingDirection.DOWN;
	public const float DefaultInteractionRange = 16.0f;

	// Default animations
	public const string DefaultAnimWalkUp = "walk_up";
	public const string DefaultAnimWalkDown = "walk_down";
	public const string DefaultAnimWalkLeft = "walk_left";
	public const string DefaultAnimWalkRight = "walk_right";

	public const string DefaultAnimRunUp = "run_up";
	public const string DefaultAnimRunDown = "run_down";
	public const string DefaultAnimRunLeft = "run_left";
	public const string DefaultAnimRunRight = "run_right";

	public const string DefaultAnimIdleUp = "idle_up";
	public const string DefaultAnimIdleDown = "idle_down";
	public const string DefaultAnimIdleLeft = "idle_left";
	public const string DefaultAnimIdleRight = "idle_right";

}
