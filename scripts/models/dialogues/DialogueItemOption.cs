using Godot;

public partial class DialogueItemOption : Resource
{
    [Export] public string Label {get; set;}
    [Export] public DialogueAction Action {get; set;}
    [Export] public string ActionTarget {get; set;}
}
