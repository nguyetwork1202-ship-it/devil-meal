using Godot;
using System.Collections.Generic;

public partial class ScreenModeSelector : MenuComponent
{
	// === Text ===
	[Export] private Font _font;
	[Export] private int _fontSize;
	[Export] private Color _textColor = Colors.White;
	[Export] private Color _selectedTextColor = Colors.Black;

	// === Option ===
	[Export] private float _optionWidth;
	[Export] private float _optionHeight;
	[Export] private float _optionSpacing = 0f;
	[Export] private Color _optionColor = Colors.White;

	// === Frame ===
	[Export] private Color _frameColor = Colors.White;
	[Export] private float _frameWidth = 2f;

	private List<DisplayServer.WindowMode> _modes = new()
	{
		DisplayServer.WindowMode.Windowed,
		DisplayServer.WindowMode.Fullscreen
	};
	private List<string> _labels = new()
	{
		"WINDOWED",
		"FULLSCREEN"
	};

	private DisplayServer.WindowMode _currentMode = DisplayServer.WindowMode.Windowed;
	private int _currentIndex = 0;

    public override void _Ready()
    {
		_currentMode = SettingsManager.Instance.GetScreenMode();
    }
    public override void _Draw()
    {
        for(int i = 0; i < _modes.Count; i++)
		{
			Vector2 optionPosition = new Vector2(Position.X + (_optionWidth + _optionSpacing) * i, Position.Y);
			Rect2 optionRect = new Rect2(optionPosition, new Vector2(_optionWidth, _optionHeight));
			string label = _labels[i];

			if(_modes[i].Equals(_currentMode))
			{
				DrawSelectedOption(optionRect, label);
			}
			else
			{
				DrawOption(optionRect, label);
			}

			if(i == _currentIndex)
			{
				DrawSelectionFrame(optionRect);
			}
		}

    }

    public override Rect2 GetComponentFrame()
    {
        return new Rect2(Vector2.Zero, Vector2.Zero);
    }
    protected override void OnActiveProcess(double delta)
    {
        HandleMovement();
		HandleSelection();
    }

	private void DrawOption(Rect2 optionRect, string optionText)
	{
		Vector2 textPosition = CalculateTextPosition(optionRect, optionText);
		DrawString(_font, textPosition, optionText, HorizontalAlignment.Left, -1, _fontSize, _textColor);
	}
	private void DrawSelectedOption(Rect2 optionRect, string optionText)
	{
		DrawRect(optionRect, _optionColor);
		Vector2 textPosition = CalculateTextPosition(optionRect, optionText);
		DrawString(_font, textPosition, optionText, HorizontalAlignment.Left, -1, _fontSize, _selectedTextColor);
	}
	private void DrawSelectionFrame(Rect2 optionRect)
	{
		DrawRect(optionRect, _frameColor, false, _frameWidth);
	}
	private Vector2 CalculateTextPosition(Rect2 rect, string text)
	{
		Vector2 textSize = _font.GetStringSize(text, HorizontalAlignment.Left, -1, _fontSize);
		return new Vector2(
        	rect.Position.X + (rect.Size.X - textSize.X) / 2,
        	rect.Position.Y + (rect.Size.Y - textSize.Y) / 2 + _font.GetAscent(_fontSize)
    	);
	}

	private void HandleMovement()
	{
		float horizontalMovement = Input.GetAxis(GlobalVariables.MoveLeftAction, GlobalVariables.MoveRightAction);
		_currentIndex += (int)horizontalMovement;
		
		if(_currentIndex >= _modes.Count) _currentIndex = _modes.Count - 1;
		if(_currentIndex < 0) _currentIndex = 0;
	}
	private void HandleSelection()
	{
		if(Input.IsActionJustPressed(GlobalVariables.AcceptAction))
		{
			_currentMode = _modes[_currentIndex];
			SettingsManager.Instance.ApplyScreenMode(_currentMode);
		}
	}
}
