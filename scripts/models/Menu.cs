using Godot;
using Godot.Collections;


[GlobalClass]
public partial class Menu : Resource
{
	[Export] public Array<MenuItem> Items { get; set; } = new();
}
