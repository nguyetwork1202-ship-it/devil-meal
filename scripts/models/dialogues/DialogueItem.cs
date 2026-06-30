using Godot;
using Godot.Collections;


[GlobalClass]
public partial class DialogueItem : Resource
{
	[Export] public string Title { get; set; }
	[Export] public string Description { get; set; }
	[Export] public string Portrait { get; set; }
	[Export] public Texture2D CG { get; set; }
	[Export] public Array<DecisionNode> Options { get; set; } = new();
}
