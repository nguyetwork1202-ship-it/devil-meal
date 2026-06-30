public partial class MenuFactory
{
	private MenuFactory() {}
	public static Menu CreateMenu(MenuType type, MenuDataset dataset) => type switch
	{
		//MenuType.START => new StartMenu(dataset),
		//MenuType.PAUSE => new PauseMenu(dataset),
		//MenuType.GAME_OVER => new GameOverMenu(dataset),
		_ => new Menu(dataset)
	};
}