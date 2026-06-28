using Godot;

[GlobalClass]
public partial class DialogueItem : Resource
{
	[Export] public DialogueKind Kind { get; set; }
	[Export] public string Title { get; set; }
	[Export] public string Description { get; set; }
	[Export] public string Portrait { get; set; }
	[Export] public string CG { get; set; }
	[Export] public DialogueDecisionTree Options { get; set; }
}
