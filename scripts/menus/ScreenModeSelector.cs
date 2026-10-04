using Godot;
using System;
using System.Collections.Generic;

public partial class ScreenModeSelector : MenuComponent
{
	// === Button ===
	[Export] private float _buttonVerticalPadding;
	[Export] private float _buttonHorizontalPadding;
	[Export] private float _buttonSpacing;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Toàn màn hình
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);

		// Cửa sổ thường
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
	}
}
