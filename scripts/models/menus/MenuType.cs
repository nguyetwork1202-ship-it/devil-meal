public enum MenuType
{
    MAIN,
    PAUSE,
    GAME_OVER,
    SETTINGS,
    SAVE,
    LOAD,
    NONE
}

public static class MenuTypeExtensions
{
    public static MenuType FromString(string menuType)
    {
        if (string.IsNullOrWhiteSpace(menuType))
        {
            return MenuType.NONE;
        }

        return menuType.Trim().ToUpperInvariant() switch
        {
            "MAIN" => MenuType.MAIN,
            "PAUSE" => MenuType.PAUSE,
            "GAME_OVER" => MenuType.GAME_OVER,
            "SETTINGS" => MenuType.SETTINGS,
            "SAVE" => MenuType.SAVE,
            "LOAD" => MenuType.LOAD,
            _ => MenuType.NONE
        };
    }
}