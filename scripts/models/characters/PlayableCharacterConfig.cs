using Godot;	

[GlobalClass]
public partial class PlayableCharacterConfig : Resource
{
	// Spawn properties
	[Export] public float PosX { get; set; } = GlobalVariables.DefaultPosX;
	[Export] public float PosY { get; set; } = GlobalVariables.DefaultPosY;
	
	// Status properties
	[Export] public float Speed { get; set; } = GlobalVariables.DefaultSpeed;
	[Export] public float RunSpeed { get; set; } = GlobalVariables.DefaultRunSpeed;
	
	// Interaction properties
	[Export] public FacingDirection FacingDirection { get; set; } = GlobalVariables.DefaultFacingDirection;
	[Export] public float InteractionRange { get; set; } = GlobalVariables.DefaultInteractionRange;

	// Animation properties
	[Export] public string AnimWalkUp { get; set; } = GlobalVariables.DefaultAnimWalkUp;
	[Export] public string AnimWalkDown { get; set; } = GlobalVariables.DefaultAnimWalkDown;
	[Export] public string AnimWalkLeft { get; set; } = GlobalVariables.DefaultAnimWalkLeft;
	[Export] public string AnimWalkRight { get; set; } = GlobalVariables.DefaultAnimWalkRight;
	[Export] public string AnimRunUp { get; set; } = GlobalVariables.DefaultAnimRunUp;
	[Export] public string AnimRunDown { get; set; } = GlobalVariables.DefaultAnimRunDown;
	[Export] public string AnimRunLeft { get; set; } = GlobalVariables.DefaultAnimRunLeft;
	[Export] public string AnimRunRight { get; set; } = GlobalVariables.DefaultAnimRunRight;
	[Export] public string AnimIdleUp { get; set; } = GlobalVariables.DefaultAnimIdleUp;
	[Export] public string AnimIdleDown { get; set; } = GlobalVariables.DefaultAnimIdleDown;
	[Export] public string AnimIdleLeft { get; set; } = GlobalVariables.DefaultAnimIdleLeft;
	[Export] public string AnimIdleRight { get; set; } = GlobalVariables.DefaultAnimIdleRight;
}
