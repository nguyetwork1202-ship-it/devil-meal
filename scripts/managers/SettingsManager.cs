using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SettingsManager : Node
{
	private const string SETTINGS_PATH = "user://settings.cfg";
	private const string DISPLAY_SECTION = "display";
	private const string DISPLAY_MODE = "mode";
	private const string AUDIO_SECTION = "audio";
	private const string MASTER_BUS = "master";
	private const string INPUT_SECTION = "input";

    private ConfigFile _cfg = new();
	private Dictionary<string, Key> _keyMap = new();

	public static SettingsManager Instance { get; private set; }

	private SettingsManager() {}

    public override void _Ready()
    {
		Instance = this;

        Error er = _cfg.Load(SETTINGS_PATH);
		if(er != Error.Ok)
		{
			CreateDefaultFile();
		}
		
		int wdModeInt = (int)_cfg.GetValue(DISPLAY_SECTION, DISPLAY_MODE);
		DisplayServer.WindowMode wdMode = (DisplayServer.WindowMode)wdModeInt;
		ApplyScreenMode(wdMode);

		float linear = (float)_cfg.GetValue(AUDIO_SECTION, MASTER_BUS);
		SetMasterVolume(linear);

        LoadKeybinds();
    }

	private void CreateDefaultFile()
	{
		// Display
		_cfg.SetValue(DISPLAY_SECTION, DISPLAY_MODE, (int)DisplayServer.WindowMode.Windowed);

		// Audio
		_cfg.SetValue(AUDIO_SECTION, MASTER_BUS, 1.0f);

		// Input (physical keycode)
		_cfg.SetValue(INPUT_SECTION, GlobalVariables.MoveUpAction, (int)Key.Up);
		_cfg.SetValue(INPUT_SECTION, GlobalVariables.MoveDownAction, (int)Key.Down);
		_cfg.SetValue(INPUT_SECTION, GlobalVariables.MoveLeftAction, (int)Key.Left);
		_cfg.SetValue(INPUT_SECTION, GlobalVariables.MoveRightAction, (int)Key.Right);

		_cfg.SetValue(INPUT_SECTION, GlobalVariables.RunAction, (int)Key.Shift);
		_cfg.SetValue(INPUT_SECTION, GlobalVariables.AcceptAction, (int)Key.Z);
		_cfg.SetValue(INPUT_SECTION, GlobalVariables.CancelAction, (int)Key.X);
		_cfg.SetValue(INPUT_SECTION, GlobalVariables.OpenInventoryAction, (int)Key.I);

		_cfg.Save(SETTINGS_PATH);
	}

    // === Screen mode ===
    public void ApplyScreenMode(DisplayServer.WindowMode mode)
    {
        DisplayServer.WindowSetMode(mode);
        _cfg.SetValue(DISPLAY_SECTION, DISPLAY_MODE, (int)mode);
        _cfg.Save(SETTINGS_PATH);
    }

	// === Audio ===
	public float GetMasterVolume()
    {
		float db = AudioServer.GetBusVolumeDb(0);
        return Mathf.DbToLinear(db);
    }
	public void SetMasterVolume(float linear)
    {
        AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(linear));
        _cfg.SetValue(AUDIO_SECTION, MASTER_BUS, linear);
        _cfg.Save(SETTINGS_PATH);
    }

    // === Control mapping ===
	public bool Rebind(string action, Key physicalKey)
	{
		if (!_keyMap.ContainsKey(action)) return false;

		bool used = _keyMap.Any(kv => kv.Value == physicalKey);
		if (used) return false;

		_keyMap[action] = physicalKey;

		InputMap.ActionEraseEvents(action);
		InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = physicalKey });
		
		SaveKeybinds();
		
		return true;
	}
	private void LoadKeybinds()
	{
		foreach (var action in _cfg.GetSectionKeys(INPUT_SECTION))
		{
			if (!InputMap.HasAction(action)) continue;

			Key key = (Key)(int)_cfg.GetValue(INPUT_SECTION, action);
			InputMap.ActionEraseEvents(action);
			InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });

			_keyMap.Add(action, key);
		}
	}
	private void SaveKeybinds()
	{
		foreach (var action in InputMap.GetActions())
		{
			string name = action.ToString();
			if (!_keyMap.Keys.Contains(name)) continue;

			foreach (var ev in InputMap.ActionGetEvents(name))
			{
				if (ev is InputEventKey key)
				{
					_cfg.SetValue(INPUT_SECTION, name, (int)key.PhysicalKeycode);
					break;
				}
			}
		}

		_cfg.Save(SETTINGS_PATH);
	}
}
