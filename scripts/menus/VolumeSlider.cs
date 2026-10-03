using System;
using Godot;

public partial class VolumeSlider : MenuComponent
{
	[Export] private HSlider _slider;
	[Export] private Label _valueLabel;
	[Export] private float _speed;
	[Export] private float _framePadding;

	private Rect2 _frame;

	public override void _Ready()
	{
		_slider.MinValue = 0;
		_slider.MaxValue = 100;

		_slider.Value = AudioManager.Instance.GetMasterVolume();
		_slider.ValueChanged += OnSliderValueChanged;
		
		_valueLabel.Text = $"{_slider.Value}%";
		_frame = _slider.GetRect().Grow(_framePadding);
	}

    public override Rect2 GetFrame()
	{
		return _frame;
	}
    protected override void OnActiveProcess(double delta)
	{
		float movement = Input.GetAxis(GlobalVariables.MoveUpAction, GlobalVariables.MoveDownAction);
		_slider.Value += movement * _speed * delta;
	}

	private void OnSliderValueChanged(double value)
	{
		AudioManager.Instance.SetMasterVolume(value);
		_valueLabel.Text = $"{value}%";
	}
}
