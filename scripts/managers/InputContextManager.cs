using System.Collections.Generic;

public partial class InputContextManager
{
	private static InputContextManager _instance;
    
    public static InputContextManager Instance => _instance ??= new InputContextManager();
    public InputContext Current => Instance._stack.Count > 0 ? Instance._stack.Peek() : InputContext.NONE;

	private readonly List<IInputContextHandler> _handlers = new();
    private readonly Stack<InputContext> _stack = new();

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
    public void OnSceneChanged(InputContext initialContext)
    {
        InputContext previous = Current;

        foreach (var handler in _handlers.ToArray())
            handler.OnSceneChanged();
            
        _stack.Clear();
        _stack.Push(initialContext);

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