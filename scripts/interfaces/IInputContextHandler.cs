public interface IInputContextHandler
{
    InputContext Context { get; }
    void OnInputContextChanged(InputContext context);
}
