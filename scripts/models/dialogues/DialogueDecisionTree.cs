using Godot;
using Godot.Collections;

public partial class DialogueDecisionTree : Resource
{
    [Export] public Array<DecisionNode> Roots { get; set; } = new();
}
