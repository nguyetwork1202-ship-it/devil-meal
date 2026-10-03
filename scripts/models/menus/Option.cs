using Godot;

public partial class Option : Resource
{
	[Export] public string Text { get; set; }
	[Export] public MenuAction Action { get; set; }
	[Export] public string ActionTarget { get; set; }
}
