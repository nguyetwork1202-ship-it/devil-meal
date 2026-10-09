using System;
using System.Linq;
using Godot;

public partial class OptionList : MenuComponent
{
	// === Options ===
	[Export] private Option[] _options;
	[Export] private int _optionPerPage;

	// === Text ===
	[Export] private Font _font;
	[Export] private int _fontSize;
	[Export] private float _lineSpacing;
	[Export] private HorizontalAlignment _textAlignment = HorizontalAlignment.Left;
	[Export] private Color _textColor = Colors.White;

	// === Frame ===
	[Export] private float _frameWidth = 2f;
	[Export] private Color _frameColor = Colors.White;
	[Export] private float _framePadding = 5f;

	// === Execution Codes ===
	private static string EXIT_GAME_CODE = "EXIT_GAME";
	private static string RESUME_GAME_CODE = "RESUME_GAME";

	private Vector2 _optionFrameSize = Vector2.Zero;
	private Rect2 _frame = new Rect2(Vector2.Zero, Vector2.Zero);
	private int _currentIndex = 0;

	public override void _Ready()
	{
		if(_options.Length == 0) return;
		_optionPerPage = Math.Min(_optionPerPage, _options.Length);

		Option longestOption = _options.OrderByDescending(o => o.Text.Length).FirstOrDefault();
		Vector2 longestOptionSize = _font.GetStringSize(longestOption.Text, _textAlignment, -1, _fontSize);

		_optionFrameSize.X = longestOptionSize.X + _framePadding * 2;
		_optionFrameSize.Y = longestOptionSize.Y + _lineSpacing;

		float frameWidth = _optionFrameSize.X;
		float frameHeight = longestOptionSize.Y * _optionPerPage + _lineSpacing * (_optionPerPage - 1) + _framePadding * 2;
		_frame = new Rect2(Position, new Vector2(frameWidth, frameHeight));

	}
	public override void _Draw()
	{
		if (_currentState == EntityState.INACTIVE || _options.Length == 0) return;

		float lineHeight = _lineSpacing + _font.GetHeight(_fontSize);

		int page = _currentIndex / _optionPerPage;
		int startIndex = page * _optionPerPage;
		int endIndex = Math.Min(startIndex + _optionPerPage, _options.Length);

		for (int i = startIndex; i < endIndex; i++)
		{
			Option option = _options[i];
			int visibleRow = i - startIndex;

			Vector2 position = new Vector2(0, visibleRow * lineHeight);
			DrawOption(position, option.Text);

			if (_currentState == EntityState.ACTIVE && i == _currentIndex) 
			{
				DrawSelectionFrame(position);
			}
		}
	}

	private void DrawOption(Vector2 position, string text)
	{
		DrawString(_font, position, text, _textAlignment, _optionFrameSize.X, _fontSize, _textColor);
	}
	private void DrawSelectionFrame(Vector2 position)
	{
		float ascent = _font.GetAscent(_fontSize);
		Vector2 rectPosition = new Vector2(position.X - _framePadding,
			position.Y - ascent - _lineSpacing / 2);
		Rect2 rect = new Rect2(rectPosition, _optionFrameSize);
		DrawRect(rect, _frameColor, false, _frameWidth);
	}

	public override Rect2 GetComponentFrame()
	{
		return _frame;
	}
	protected override void OnStateChanged(EntityState newState)
	{
		if(newState == EntityState.ACTIVE)
		{
			_currentIndex = 0;
		}
	}
	protected override void OnActiveProcess(double delta)
	{
		if(_options.Length == 0) return;

		HandleMovement();
		HandleSelection();
	}

	private void HandleMovement()
	{
		float movement = Input.GetAxis(GlobalVariables.MoveUpAction, GlobalVariables.MoveDownAction);
		_currentIndex = (_currentIndex + _options.Length + (int)movement) % _options.Length;
	}
	private void HandleSelection()
	{
		if(!Input.IsActionJustPressed(GlobalVariables.AcceptAction)) return;
		
		Option selectedOption = _options[_currentIndex];

        switch (selectedOption.Action)
		{
			case MenuAction.EXCECUTE:
				ExecuteAction(selectedOption.ActionTarget);
				break;
			case MenuAction.LOAD_SCENE:
				LoadScene(selectedOption.ActionTarget);
				break;
			case MenuAction.OPEN_MENU:
				OpenMenu(selectedOption.ActionTarget);
				break;
		}
	}

	private void ExecuteAction(string actionCode)
	{
		if(actionCode == EXIT_GAME_CODE)
		{
			SignalBus.Instance.EmitSignal(SignalBus.SignalName.ExitGame);
			return;
		}

		if(actionCode == RESUME_GAME_CODE)
		{
			SignalBus.Instance.EmitSignal(SignalBus.SignalName.CloseMenu);
			return;
		}
	}
	private void LoadScene(string scenePath)
	{
		SignalBus.Instance.EmitSignal(SignalBus.SignalName.LoadScene, scenePath);
	}
	private void OpenMenu(string menuTypeStr) 
	{
		MenuType menuType = MenuTypeExtensions.FromString(menuTypeStr);
		SignalBus.Instance.EmitSignal(SignalBus.SignalName.OpenMenu, Variant.From(menuType));
	}
}
