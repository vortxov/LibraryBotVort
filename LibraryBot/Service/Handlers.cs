using LibraryBot.Domain;
using LibraryBot.Domain.Entity;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace LibraryBot.Service;

public class Handlers
{
    private static DataManager dataManager = new DataManager(); 

    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) 
    {
        var ErrorMessage = exception switch 
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}", 
            _ => exception.ToString() 
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask; 
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!), 
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!), 
            UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!), 
            UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler; 
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

    private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message) //��������� ����� ��������� ������
    {
        if (message.Type != MessageType.Text) //���� ��� ��������� �� ����� �� �������
            return;

        var action = message.Text!.Split(' ')[0] switch //��������� ��� �������� � ������ ���������
        {
            "/help" => Usage(botClient, message), //���� �������� /help �� ���������� ��������� ����������� ����
            "/start" => HelloView(botClient, message),
            "/random" => RandomBook(botClient, message),
            "/pocketbook" => LittleBook(botClient, message),
            "/help@FunLibrary_bot" => Usage(botClient, message), //���� �������� /help �� ���������� ��������� ����������� ����
            "/start@FunLibrary_bot" => HelloView(botClient, message),
            "/random@FunLibrary_bot" => RandomBook(botClient, message),
            "/pocketbook@FunLibrary_bot" => LittleBook(botClient, message),
            _ => AnswerMessage(botClient, message) //���� �� help �� ���� ������ ����� �� �������
        };
        Message sentMessage = await action;



        static async Task<Message> Usage(ITelegramBotClient botClient, Message message) //���������� ��������� �� ����
        {
            const string usage = "�� ����� ����������� �������� ��� ���?\n" +
                "������ ��� �������� �� ���� �����������, ��� ���������� Telegram. \n" +
                "��� ����� ������������ �����? \n" +
                "� ������ ����� ��������� �� ������ �������� �������� ������������/������/����� � ��������� ������ ����. � ����� ��� ������ ������ � ������� ������� ����, � ������� �� ��������� ������ �������. \n" +
                "��� �� � ������ ����� ��������� �� ������ ������� id ���� � ��� �� ��������/������/����, ����� ������ ��������� �������������. \n" +
                "������������� ������ ������ ������ �������� �������� ���� �������� � ���, �� ������ ������������ ������ ��������� ����� ������-�������, ������� ��� �������� � ��������/������/���� � ���� �������� ��������� � ����� ����, ������ ��� ������. ������������ ��� �� ����������� ������ ��������� ����. \n" +
                "��������� ������� ���� \n" +
                "������� ����� ������������ ��� � ���� ����, ��� � �������� �� ����� ���� �������. ���� ������� ������ ��������� \n" +
                "/start � ������ ����, ����������� \n" +
                "/random-��������� ����� \n" +
                "/help- ���������� \n" +
                "/pocketbook � ���������, �������� �����  \n" +
                "��� �������� ���� � ���? \n" +
                "���� ��������� � ��� ��� ��, ��� �������� ������������, ���� ������������� ������. ������ ��������� ��������� ���������.\n";

            await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: usage,
                                                      replyMarkup: new ReplyKeyboardRemove());

            FileStream fileStream = new(@"C:\Users\ilega\OneDrive\������� ����\LibraryBot\LibraryBot\usage\video_2022-06-09_16-54-36.mp4", FileMode.Open, FileAccess.Read, FileShare.Read);

            using (var stream = System.IO.File.OpenRead(@"C:\Users\ilega\OneDrive\������� ����\LibraryBot\LibraryBot\usage\video_2022-06-09_16-54-36.mp4"))
            {
                return await botClient.SendVideoAsync(chatId: message.Chat.Id,
                                                        video: stream,
                                                        replyMarkup: new ReplyKeyboardRemove());
            }
        }

        static async Task<Message> HelloView(ITelegramBotClient botClient, Message message) //���������� ��������� �� ����
        {
            const string usage = "������, � ���� ��������� ����������!\n��� �������� �������?"; //TODO:�������� ��������� ��� ������������



            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: usage,
                                                        replyMarkup: new ReplyKeyboardRemove());
        }
    }
    private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery) //�������� ������ � ������ 
    {

        var action = callbackQuery.Data.Split(':')[0] switch //����� ������ ������ �� ��������� ������ � ��������� 
        {
            "download" => BotDownloadBookFormat(botClient, callbackQuery) //���� ������ ����� �������� donwload �� ��������� � ������� ���������� �����
        };



    }


    private static async Task<Message> BotDownloadBookFormat(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var KeyboardData = callbackQuery.Data.Split(':'); //����� ������ ������ �� ��������� ������
        try
        {
            

            using (FileStream fs = System.IO.File.Open(@"C:\Users\ilega\OneDrive\������� ����\LibraryBot\LibraryBot\Files\" + KeyboardData[1], FileMode.Open)) //��������� ���� ������� ��������
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, KeyboardData[1].Split("/").Last()); //�������� ���� ������ ������� �������� ����

                return await botClient.SendDocumentAsync(chatId: callbackQuery.From.Id, //�������� �������� � ��� ��� ������� ������ callbackQuery.From.Id ���� ���� � �������� ������ ���������
                                                      document: inputOnlineFile); //��� �������� ������� ��������
            }


        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message); //���� ���� ������ �� ������� ��
        }

    }


    private static async Task<string> MessageBookInfo(Book book) //������� ��� �������� ��������� � ������������ � �����
    {
        //TODO: �������� �� ��������
        string ViewBook = book.Title + "\n"; //������� ������� � ������� (�������� �����, �����, ���, �����

        foreach (var author in book.Authors)
        {
            ViewBook += author.Name + "  ";
        }

        ViewBook += "\n";
        if (book.Year != null)
        {
            ViewBook += "�������: " + book.Year + "\n";
                }
        ViewBook += "����: ";

        foreach (var item in book.Genre)
        {
            ViewBook += item.Genre + " "; //������ ��������� �����
        }

        ViewBook += "\n\n " + book.Content;

        var fileSize = book.PathBooks.FirstOrDefault(x => x.Type == "FB2");
        if(fileSize != null)
        {
            ViewBook += "\n\n������ �����: " + (fileSize.Length / 1024).ToString() + " KB";
        }

        return ViewBook; //��������� ��������� �� �������
    }

    private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery) //������� ���������� �� ��������� � ���� � ���������� ������ ����
    {
        if (inlineQuery.Query != "") //������ � ���� �� ������ ���� ������
        {
            List<Book> listB = new List<Book>();   //������� ������ ���� �� ����������

            var Books = dataManager.book.GetBooks();
            listB.AddRange(Books.Where(x => x.Title.ToLower().Contains(inlineQuery.Query.ToLower())));
            var authors = dataManager.author.GetAuthors();

            foreach (var author in authors)
            {
                if (author.Name.ToLower().Contains(inlineQuery.Query.ToLower()))
                {
                    listB.AddRange(author.Books);
                }
            }

            var Genre = dataManager.genre.GetGenres();
            foreach (var item in Genre)
            {
                if (item.Genre.ToLower().Contains(inlineQuery.Query.ToLower()))
                {
                    listB.AddRange(item.Books);
                }
            }


            //offset ��� �������� ������� ��������� ����� ��� ��� ���������� ����� ������, ���� ��� ����� ������� �� �� �����, ������ �������,
            //���� ���� �� ������� �� ���������� ���� �� ����� �������
            if (listB.Count < 50 && inlineQuery.Offset == "") //��������� ���������� ���� ���� ������ 50 � offset ����� ������� �� ���� ������
            {
                await AnswerInline(botClient, inlineQuery, "", listB); //������� ������ ���� �������
            }
            else
            {
                string offset = ""; //������� ���� offset � ������ ��� ������
                if (inlineQuery.Offset == "") //��������� ����� offset ������ ���� ����� ����������
                {
                    offset = "1"; //������ offset ������� ������ 

                    

                    await AnswerInline(botClient, inlineQuery, offset, listB); //������� ������ ���� �������
                }
                else
                {
                    offset = inlineQuery.Offset; //�������� offset ���������� ��������

                    for (int i = 0; i < int.Parse(offset); i++) //������ ������� �� ������ ���� �� ����� ������� ��� ��������
                    {
                        if (listB.Count > 50) //��������� ������ �� 50 ���� 
                            listB.RemoveRange(0, 50); //���� ������ �� ������� 50 ����
                    }

                    if (listB.Count <= 50) //���� ����� ��� ������ 50 ���� 
                    {
                        offset = ""; //offset ������ ������� ������� 
                    }
                    else
                    {
                        offset = (int.Parse(offset) + 1).ToString(); //����� ���������� ���� � offset
                    }

                    await AnswerInline(botClient, inlineQuery, offset, listB); //������� ������ ���� �������
                }

            }

        }
    }


    private static async Task AnswerInline(ITelegramBotClient botClient, InlineQuery inlineQuery, string offset, List<Book> listBook) //������� ��� ������ � ��������� 
    {
        List<InlineQueryResult> results = new List<InlineQueryResult>(); //������� ������ � ������� ����� ��������� ������ �� ������

        int RangeCount; //���������� ������� ������� ������ ������� ���� ������ � ������ �� ������ ������
        if (listBook.Count < 50) //���� � ������ ���� ������ 50 ���� 
            RangeCount = listBook.Count; //�� ������� ���������� ���� ����
        else
            RangeCount = 50; //����� 50

        listBook.GetRange(0, RangeCount);
        var list = listBook.Distinct().ToList();

        foreach (var entry in list.GetRange(0, RangeCount)) //���� �������� ��������� ��� ����������� ������� ������� ������ 50 ��������� �� ������ ��� ���� ������ ���� ������ 50
        {
            var fileSize = entry.PathBooks.FirstOrDefault(x => x.Type == "FB2");
            var item = new InlineQueryResultArticle( //������� ������� � ��������� ���
                    id: entry.Id.ToString(), //��������� ���� ��������
                    title: entry.Title + "(" + (fileSize.Length / 1024).ToString() + " KB)", //��������� ���� ������� ����� ������ � ������ ��� �������
                    inputMessageContent: new InputTextMessageContent(
                        MessageBookInfo(entry).Result  //������� ��������� ������� ������ ������ ��� ������� �� �����
                    )


                );

            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < entry.PathBooks.Count; i++)
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(entry.PathBooks[i].Type, "download:" + entry.PathBooks[i].Path) });
            }

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

            item.Description = "";
            foreach (var author in entry.Authors)
            {
                item.Description += author.Name + "  ";
            }

            item.ReplyMarkup = inlineKeyboard; //��������� ������ � ��������
            results.Add(item); //��������� ������� � ������ 
            if (results.Count == 50) //���� ����� ��������� ����� 50 �� ��������� ����
                break;
        }

        await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id, //������� ������ �������  inlineQuery.Id - ���� ��������� � ����, ��� ��������� ������ ��� � ������� �������
                                               results: results, //������� ������ ��������� � ������� � ������
                                               isPersonal: true, //���������� ������ ��� ��� ��������� 
                                               cacheTime: -1, //cacheTime ����������� ������� �������� ��� ������ ���������� �������, 0 �� ����������
                                               nextOffset: offset);
    }

    private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult) //������� �� ����� ����
    {
        Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        return Task.CompletedTask;
    }

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update) //������� �� ����� ����
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }


    static async Task<Message> AnswerMessage(ITelegramBotClient botClient, Message message) //����� �� ������� ���������
    {
        if (message.ViaBot != null) //�������� ����� �� ���� ��� ���
        {
            return null; //���� ����� �� ������ ������� � ������� �� ���������
        }


        return await SerchBooks(botClient, message); //��������� ������� ������ ���� � ��������� ����� 
    }

    static async Task<Message> RandomBook(ITelegramBotClient botClient, Message message) //����� �� ������� ���������
    {
        var Books = dataManager.book.GetBooks().ToList();
        
        var random = new Random();

        var book = Books[random.Next(Books.Count() - 1)];

        List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

        for (int i = 0; i < book.PathBooks.Count; i++)
        {
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(book.PathBooks[i].Type, "download:" + book.PathBooks[i].Path) });
        }

        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);


        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, //Chat.Id ���� ���� ������ ��� ��������� ������
                                                        text: await MessageBookInfo(book), //��������� ������� ������� �������
                                                        replyMarkup: inlineKeyboard); //������� �� ������ ��������� ���� ����
    }


    static async Task<Message> LittleBook(ITelegramBotClient botClient, Message message) //����� �� ������� ���������
    {
        var Books = dataManager.book.GetBooks().ToList();
        List<Book> books = new List<Book>();

        foreach(var item in Books)
        {
            var size = item.PathBooks.FirstOrDefault(x => x.Type == "FB2" && x.Length / 1024 < 200);
            if(size != null)
            {
                books.Add(item);
            }
        }

        var random = new Random();

        var book = books[random.Next(books.Count() - 1)];

        List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

        for (int i = 0; i < book.PathBooks.Count; i++)
        {
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(book.PathBooks[i].Type, "download:" + book.PathBooks[i].Path) });
        }

        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);


        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, //Chat.Id ���� ���� ������ ��� ��������� ������
                                                        text: await MessageBookInfo(book), //��������� ������� ������� �������
                                                        replyMarkup: inlineKeyboard); //������� �� ������ ��������� ���� ����
    }

    static async Task<Message> SerchBooks(ITelegramBotClient botClient, Message message) //������� ������ ���� 
    {
        List<Book> listB = new List<Book>();   //������� ������ ���� �� ����������

        var Books = dataManager.book.GetBooks();
        listB.AddRange(Books.Where(x => x.Title.ToLower().Contains(message.Text.ToLower())));
        var authors = dataManager.author.GetAuthors();

        foreach (var author in authors)
        {
            if (author.Name.ToLower().Contains(message.Text.ToLower()))
            {
                listB.AddRange(author.Books);
            }    
        }

        var Genre = dataManager.genre.GetGenres();
        foreach (var item in Genre)
        {
            if(item.Genre.ToLower().Contains(message.Text.ToLower()))
            {
                listB.AddRange(item.Books);
            }
        }

        var list = listB.Distinct().ToList();


        if (list.Count != 0) //��������� �� ������ �� ���� 
        {
            //���� �� ������ 
            InlineKeyboardMarkup inlineKeyboard = new(
             new[]
             {
                    new []
                    {
                        InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("������ ����", message.Text) //������� ������ ������� ����� ���������� ��������� � ���� ����� ������� ������
                    }
             });

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, //�������� �������� ������� �������� ������, message.Chat.Id ���� ���� ������ ��� ��������� ������
                                           text: "�� ������ ������� ���� ������� " + list.Count + " ����.", //����� ������� ������� ���, ������� ������� ���� �����
                                           replyMarkup: inlineKeyboard); //���������� ������ 
        }
        else
        { //���� �� ����� ����� �� �������� ��� ���������
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, //Chat.Id ���� ���� ������ ��� ��������� ������
                                                        text: "�� ������ ������� �� ���� ������� ����. ��������� ������.", //��������� ������� ������� �������
                                                        replyMarkup: new ReplyKeyboardRemove()); //������� �� ������ ��������� ���� ����
        }
    }
}
