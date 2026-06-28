using Godot;
using Godot.Collections;

public partial class MenuManager : Node, IInputContextHandler
{
	[Export] public Dictionary<MenuType, MenuDataset> MenuDatasets { get; private set; }
	[Export] public MenuType MenuType { get; set; }

	public static MenuManager Instance { get; private set; }
	private Dictionary<MenuType, Menu> _menuInstances;

	public InputContext Context => InputContext.MENU;

	public override void _Ready()
	{
		Instance = this;

		foreach (var kvp in MenuDatasets)
		{
			MenuType type = kvp.Key;
			MenuDataset dataset = kvp.Value;

			Menu menu = MenuFactory.CreateMenu(type, dataset);
			_menuInstances.Add(type, menu);

			AddChild(menu);
		}

		InputContextManager.Instance.Register(this);
	}
	public override void _ExitTree()
	{
		InputContextManager.Instance.Unregister(this);
	}

	public void OnInputContextChanged(InputContext previousContext, InputContext currentContext)
    {
		if(previousContext == Context)
		{
			HandleLeavingMenu(currentContext);
		}

		if(currentContext == Context)
		{
			HandleReturningToMenu(previousContext);
		}	
    }

	private void HandleLeavingMenu(InputContext currentContext)
	{
		switch (currentContext)
		{
			default:
				_menuInstances[MenuType].SetActive(false);			
				break;
		}
	}
	private void HandleReturningToMenu(InputContext previousContext)
	{
		switch(previousContext)
		{
			default:
				_menuInstances[MenuType].SetActive(true);
				break;
		}
	}

}
