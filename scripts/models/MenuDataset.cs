using Godot;
using Godot.Collections;


[GlobalClass]
public partial class MenuDataset : Resource
{
	[Export] public Array<MenuItem> Items { get; set; } = new();
}
