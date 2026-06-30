using Godot;
using Godot.Collections;

public partial class MenuManager : Node, IInputContextHandler
{
	[Export] public Dictionary<MenuType, MenuDataset> MenuDatasets { get; private set; }
	[Export] public MenuType MenuType { get; set; } = MenuType.NONE;

	public static MenuManager Instance { get; private set; }
	
	private MenuType _activeMenuType = MenuType.NONE;
	private Dictionary<MenuType, Menu> _menuInstances;

	public InputContext Context => InputContext.MENU;

	private MenuManager() {}

	public override void _Ready()
	{
		Instance = this;

		foreach (var kvp in MenuDatasets)
		{
			MenuType type = kvp.Key;
			MenuDataset dataset = kvp.Value;

			Menu menu = MenuFactory.CreateMenu(type, dataset);
			menu.SetState(EntityState.HIDDEN);

			_menuInstances.Add(type, menu);
			AddChild(menu);
		}

		InputContextManager.Instance.Register(this);
	}
	public override void _ExitTree()
	{
		InputContextManager.Instance?.Unregister(this);
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
	public void OnSceneChanged()
	{
		foreach (var menu in _menuInstances.Values)
		{
			menu.SetState(EntityState.HIDDEN);
			AddChild(menu);
		}
	}

	private void HandleReturningToMenu(InputContext previousContext)
	{
		if(_activeMenuType != MenuType.NONE)
			_menuInstances[_activeMenuType].SetState(EntityState.HIDDEN);

		_activeMenuType = MenuType;
		
		switch(previousContext)
		{
			case InputContext.GAMEPLAY or
				 InputContext.CUTSCENE or
				 InputContext.NONE:
				_menuInstances[_activeMenuType].SetState(EntityState.ACTIVE);
				_menuInstances[_activeMenuType].ResetMenuNavigation();
				break;
			case InputContext.INVENTORY:
				_menuInstances[_activeMenuType].SetState(EntityState.ACTIVE);
				break;
		}
	}
	private void HandleLeavingMenu(InputContext currentContext)
	{
		switch (currentContext)
		{
			case InputContext.GAMEPLAY:
				_menuInstances[_activeMenuType].SetState(EntityState.HIDDEN);
				_activeMenuType = MenuType.NONE;
				break;
			case InputContext.INVENTORY or InputContext.SCENE_TRANSITION:
				_menuInstances[_activeMenuType].SetState(EntityState.FROZEN);
				break;
		}
	}
}
