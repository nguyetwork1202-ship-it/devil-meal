using Godot;

public partial class DialogueItem : Resource
{
	[Export] public string Title;
	[Export] public string Description;
	[Export] public string Portrait;
	[Export] public string CG;
	[Export] public DialogueDecisionTree Options;
}
