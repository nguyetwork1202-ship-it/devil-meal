using System.Collections.Generic;
using Godot;

public partial class InputContextManager : Node
{
	[Export] public InputContext InitialContext { get; private set; } = InputContext.GAMEPLAY;

	public static InputContextManager Instance { get; private set; }
    public InputContext Current => Instance._stack.Count > 0 ? Instance._stack.Peek() : Instance.InitialContext;

	private readonly List<IInputContextHandler> _handlers = new();
    private readonly Stack<InputContext> _stack = new();

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;
        _stack.Push(InitialContext);
    }

    public void Register(IInputContextHandler handler)
    {
        if (handler == null || _handlers.Contains(handler)) return;
        _handlers.Add(handler);
        handler.OnInputContextChanged(InputContext.NONE, Current);
    }
    public void Unregister(IInputContextHandler handler)
    {
        _handlers.Remove(handler);
    }

    public void Push(InputContext context)
    {
        InputContext previous = Current;
        if (context == previous) return;
        _stack.Push(context);
        Broadcast(previous);
    }
    public void Pop(InputContext expected)
    {
        if (_stack.Count <= 1) return;
        if (_stack.Peek() != expected) return;
        InputContext previous = Current;
        _stack.Pop();
        Broadcast(previous);
    }
    private void Broadcast(InputContext previous)
    {
        InputContext current = Current;
        foreach (IInputContextHandler handler in _handlers.ToArray())
        {
            handler.OnInputContextChanged(previous, current);
        }
    }
}