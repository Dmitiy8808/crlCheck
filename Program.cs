using Org.BouncyCastle.X509;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;




namespace CrlCheck
{
    public class Program
    {
         
        static async Task Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly, optional: false);

            IConfigurationRoot config = builder.Build();
            

            string[] crlLinks = {"http://ca.1c.ru/cdp/1cca1_2019.crl", "http://ca.1c.ru/cdp/1cca2_2020.crl", "http://ca.1c.ru/cdp/1cca2_2022.crl",   
                                    "http://ca.1c.ru/cdp/1cca1_2020.crl", "http://ca.1c.ru/cdp/1cca1_2022.crl"};
            while (true)
            {
                
                foreach (var link in crlLinks)
                {
                    var bufStream = GetCrlStream(link);
                    var crlDate = GetCrlInfo(bufStream.Result);
                    if (DateTime.Parse(crlDate) < DateTime.Now)
                    {
                        await SendMessage(config["User1"], link);
                        await SendMessage(config["User2"], link);
                    }
                }
                
                Thread.Sleep(300000);
            }

        }
        
        public static string GetCrlInfo(byte[] buf)
        {
            try
            {
                X509CrlParser xx = new X509CrlParser();
                X509Crl ss = xx.ReadCrl(buf);
                var nextupdate = ss.NextUpdate;
                return nextupdate.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
            
        }

        public static async Task SendMessage(ChatId chatId, string link)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly, optional: false);

            IConfigurationRoot config = builder.Build();

            var token = config["ACCESS_TOKEN"];

            if (token != null)
            {
                var botClient = new TelegramBotClient(token);
                Message message = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text:   $"Проверьте список отзыва {link}");
            }

        }

        public static async Task<byte[]> GetCrlStream(string link)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(link);
            return response;
        }

    }
}



