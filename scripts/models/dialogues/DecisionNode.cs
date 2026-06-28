using Godot;
using Godot.Collections;
public partial class DecisionNode : Resource
{
    [Export] public DialogueItemOption Option { get; set; }
    [Export] public Array<DecisionNode> Children { get; set; } = new();

    public bool IsLeaf => Children is null || Children.Count == 0;
}
