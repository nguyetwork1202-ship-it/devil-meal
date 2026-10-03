using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class OptionList : MenuComponent
{
	[Export] private float _frameWidth = 1f;
	[Export] private Color _frameColor = Colors.White;
	
	private List<Option> _options;
	private Rect2 _selectedOptionFrame;
	private Rect2 _frame;
	private int _currentIndex = 0;

	// === Execution Codes ===
	private static string EXIT_GAME_CODE = "EXIT_GAME";
	private static string RESUME_GAME_CODE = "RESUME_GAME";

	public override void _Ready()
	{
		_options = new List<Option>();
		foreach (Node child in GetChildren())
		{
			if (child is Option option)
			{
				_options.Add(option);
			}
		}

		Option longestOption = _options.OrderByDescending(l => l.Text.Length).FirstOrDefault();
		_selectedOptionFrame = longestOption != null ? longestOption.GetRect() : new Rect2(Vector2.Zero, Vector2.Zero);
		_frame = longestOption != null
			? new Rect2(longestOption.GetRect().Position, longestOption.GetRect().Size * new Vector2(1, _options.Count))
			: new Rect2(Vector2.Zero, Vector2.Zero);
	}
	public override void _Draw()
	{
		if(_currentState != EntityState.ACTIVE) return;
		DrawRect(_selectedOptionFrame, _frameColor, false, _frameWidth);
	}

	public override Rect2 GetFrame()
	{
		return _frame;
	}
	protected override void OnActiveProcess(double delta)
	{
		if(_options.Count == 0) return;

		HandleMovement();
		HandleSelection();
	}

	private void HandleMovement()
	{
		float movement = Input.GetAxis(GlobalVariables.MoveUpAction, GlobalVariables.MoveDownAction);
		_currentIndex = (_currentIndex + _options.Count + (int)movement) % _options.Count;
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
