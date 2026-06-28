using Godot;

public partial class MenuItem : Resource
{
	[Export] public string Id { get; set; }
	[Export] public string ParentId { get; set; }
	[Export] public string Label { get; set; }
	[Export] public MenuAction Action { get; set; }
	[Export] public string ActionTarget {get; set;}
}
