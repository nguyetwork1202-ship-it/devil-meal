using Godot;

public partial class MainMenu : Menu
{	
	private const string _exitGameCode = "EXIT_GAME";
	
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
