using Godot;


[GlobalClass]
public partial class Item : Resource
{
    [Export] public string Code { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public string Name { get; set; } = "";
}
