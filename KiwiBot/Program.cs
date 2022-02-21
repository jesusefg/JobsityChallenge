using System.Threading;

namespace KiwiBot
{
    internal class Program
    {
        private const int _sleepTime = 1000 * 300; // 300 seconds

        static void Main(string[] args)
        {
            //Instantiate the KiwiBot
            Kiwi bot = new Kiwi(true); // true for production purposes

            bot.ReadQueue();

            // execute an infinite loop to prevent the application for closing
            // and terminting the consumer process
            do
            {
                Thread.Sleep(_sleepTime);
            } while (true);
        }
    }
}
