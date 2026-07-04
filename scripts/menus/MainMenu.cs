using Godot;

public partial class MainMenu : Menu
{
	[Export] private float _selectedBoxPadding;
	[Export] private Color _selectedBoxColor;
	
	private Vector2 _boxSize;
	
	private const string _exitGameCode = "EXIT_GAME";

	public override void _Ready()
	{
		base._Ready();
		
		_boxSize = _longestLabelSize;
		_boxSize.X += _selectedBoxPadding * 2;
		_boxSize.Y += _lineSpacing;
	}

	protected override void DrawSelectedOption(Vector2 position, string label)
	{
		float ascent = _font.GetAscent(_fontSize);
		Vector2 rectPosition = new Vector2(position.X - _selectedBoxPadding,
		position.Y - ascent - _lineSpacing / 2);
		
		Rect2 rect = new Rect2(rectPosition, _boxSize);
		DrawRect(rect, _selectedBoxColor);

		base.DrawSelectedOption(position, label);
	}
	protected override void ExecuteFunction(string functionCode)
	{
		switch (functionCode)
		{
			case _exitGameCode:
				GetTree().Quit();
				break;
		}
	}
}
