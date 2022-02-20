using System.Threading;

namespace KiwiBot
{
    internal class Program
    {
        private const int _sleepTime = 1000 * 3; // 3 seconds

        static void Main(string[] args)
        {
            //Instantiate the KiwiBot
            Kiwi bot = new Kiwi(true); // true for production purposes


            // execute an infinite loop to read the queue every fixed time
            do
            {
                bot.ReadQueue();
                Thread.Sleep(_sleepTime);
            } while (true);
        }
    }
}
