using Godot;	

[GlobalClass]
public partial class PlayableCharacterConfig : Resource
{
	// Spawn properties
	[Export] public float PosX { get; set; } = 0;
	[Export] public float PosY { get; set; } = 0;
	[Export] public FacingDirection FacingDirection { get; set; } = FacingDirection.DOWN;

	// Status properties
	[Export] public float Speed { get; set; } = 150.0f;
	[Export] public float RunSpeed { get; set; } = 250.0f;
	[Export] public float InteractionRange { get; set; } = 16.0f;

	// Animation properties
	[Export] public string AnimWalkUp { get; set; } = "mc_walk_up";
	[Export] public string AnimWalkDown { get; set; } = "mc_walk_down";
	[Export] public string AnimWalkLeft { get; set; } = "mc_walk_left";
	[Export] public string AnimWalkRight { get; set; } = "mc_walk_right";
	[Export] public string AnimRunUp { get; set; } = "mc_run_up";
	[Export] public string AnimRunDown { get; set; } = "mc_run_down";
	[Export] public string AnimRunLeft { get; set; } = "mc_run_left";
	[Export] public string AnimRunRight { get; set; } = "mc_run_right";
	[Export] public string AnimIdleUp { get; set; } = "mc_idle_up";
	[Export] public string AnimIdleDown { get; set; } = "mc_idle_down";
	[Export] public string AnimIdleLeft { get; set; } = "mc_idle_left";
	[Export] public string AnimIdleRight { get; set; } = "mc_idle_right";
}
