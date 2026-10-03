using Godot;

public partial class AudioManager
{
	private static AudioManager _instance;
	public static AudioManager Instance => _instance ??= new AudioManager();

	public float GetMasterVolume()
	{
		float db = AudioServer.GetBusVolumeDb(0);
    	float normalized = Mathf.DbToLinear(db);
    	return normalized * 100f;
	}
	public void SetMasterVolume(double value)
	{
    	float normalized = (float)value / 100f;
    	float db = Mathf.LinearToDb(normalized);
    	AudioServer.SetBusVolumeDb(0, db);
	}
}
