
using Godot;
using Godot.Collections;

public partial class Dialogue : Node2D
{
	[Export] public Array<DialogueItem> Items { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
