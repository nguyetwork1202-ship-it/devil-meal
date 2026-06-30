using System.Collections.Generic;
using Godot;

public class UILayerManager
{
	private static UILayerManager _instance;
	private int _baseLayer = 10;
	private readonly List<CanvasLayer> _stack = new();
	
	public static UILayerManager Instance => _instance ??= new UILayerManager();
	
	private UILayerManager() {}

	public void Push(CanvasLayer layer)
	{
		layer.Layer = _baseLayer + _stack.Count;
		_stack.Add(layer);
	}
	public void Remove(CanvasLayer layer)
	{
		int idx = _stack.IndexOf(layer);
		if (idx == -1) return;

		_stack.RemoveAt(idx);
		for (int i = idx; i < _stack.Count; i++)
			_stack[i].Layer = _baseLayer + i;
	}
}
