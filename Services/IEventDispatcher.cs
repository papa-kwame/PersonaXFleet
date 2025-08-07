namespace PersonaXFleet.Services
{
    public interface IEventDispatcher
    {
        void Dispatch<TEvent>(TEvent @event);
    }

}
