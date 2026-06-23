public interface IInputContextHandler
{
    InputContext Context { get; }
    void OnInputContextChanged(InputContext previousContext, InputContext currentContext);
}
