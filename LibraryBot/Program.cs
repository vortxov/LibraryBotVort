using LibraryBot.Service;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace LibraryBot;

public static class Program
{
    private static TelegramBotClient? Bot;
    public static Handlers handlers { get; set; }
    public static SearchBooks searchBooks { get; set; }

    public static async Task Main()
    {
        var builder = new ConfigurationBuilder();
        builder.AddJsonFile("appsettings.json", optional: false);

        var configuration = builder.Build(); 

        Bot = new TelegramBotClient(configuration["Bot:Token"]);

        User me = await Bot.GetMeAsync();
        Console.Title = me.Username ?? "My awesome Bot";

        using var cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
        Bot.StartReceiving(Handlers.HandleUpdateAsync,
                           Handlers.HandleErrorAsync,
                           receiverOptions,
                           cts.Token);

        Console.WriteLine($"Start listening for @{me.Username}");

        while (true)
        {
            var cns = Console.ReadLine();
            switch (cns)
            {
                case "Full":
                    {
                        Console.WriteLine("Start Search Full");
                        var search = new SearchBooks();
                        search.SearchFullBook();
                        Console.WriteLine("End Search Full");
                        break;
                    }
                case "New":
                    {
                        Console.WriteLine("Start Search New");
                        var search = new SearchBooks();
                        search.SearchNewBook();
                        Console.WriteLine("End Search New");
                        break;
                    }
                case "Exit":
                    {
                        cts.Cancel();
                        return;
                    }
            }
        }

    }
}
