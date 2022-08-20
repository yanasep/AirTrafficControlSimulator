public class MessageHub
{
    public MessageHub()
    {
        Event = new EventHub();
        Module = new ModuleHub();
    }

    public EventHub Event { get; }
    public ModuleHub Module { get; }
}