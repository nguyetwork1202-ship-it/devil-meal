using Godot;

public partial class Option : Label
{
	[Export] public MenuAction Action { get; set; }
	[Export] public string ActionTarget { get; set; }
}
