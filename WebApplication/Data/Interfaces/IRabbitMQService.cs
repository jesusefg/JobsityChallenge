namespace WebApplication.Data.Interfaces
{
    public interface IRabbitMQService
    {
        void Connect();

        void SendMessageToKiwi(string message);
    }
}
