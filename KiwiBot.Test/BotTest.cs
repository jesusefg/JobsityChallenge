using Xunit;

namespace KiwiBot.Test
{
    public class BotTest
    {
        private readonly Kiwi bot = new Kiwi(false); // set false for testing purposes

        [Fact]
        public void Test_Bot_Read_From_Queue()
        {
            Assert.True(bot.ReadQueue());
        }

        [Fact]
        public void Test_Bot_Write_To_Queue()
        {
            Assert.True(bot.WriteQueue("testRoom", "testMessage"));
        }

        [Fact]
        public void Test_Stock_Quote_API()
        {
            string parameter = "aapl.us";

            Assert.NotNull(bot.GetStockQuote(parameter));
        }

        [Fact]
        public void Test_Stock_Quote_API_With_Wrong_Parameter()
        {
            string parameter = "wrong.parameter";

            Assert.Null(bot.GetStockQuote(parameter));
        }
    }
}
