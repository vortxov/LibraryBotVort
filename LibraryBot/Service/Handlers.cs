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

    private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message) //Проверяем какое сообжение пришло
    {
        if (message.Type != MessageType.Text) //Если тип сообщения не текст то выходим
            return;

        var action = message.Text!.Split(' ')[0] switch //Проверяем что написано в начале сообщения
        {
            "/help" => Usage(botClient, message), //Если написано /help то отображаем иструкцию пользование бота
            "/start" => HelloView(botClient, message),
            "/random" => RandomBook(botClient, message),
            "/pocketbook" => LittleBook(botClient, message),
            "/help@FunLibrary_bot" => Usage(botClient, message), //Если написано /help то отображаем иструкцию пользование бота
            "/start@FunLibrary_bot" => HelloView(botClient, message),
            "/random@FunLibrary_bot" => RandomBook(botClient, message),
            "/pocketbook@FunLibrary_bot" => LittleBook(botClient, message),
            _ => AnswerMessage(botClient, message) //Если не help то идем искать книги по запросу
        };
        Message sentMessage = await action;



        static async Task<Message> Usage(ITelegramBotClient botClient, Message message) //Отображаем иструкцию по боту
        {
            const string usage = "На каких устройствах работает наш бот?\n" +
                "Данный бот работает на всех устройствах, где установлен Telegram. \n" +
                "Как найти определенную книгу? \n" +
                "В строке ввода сообщения вы можете написать Название произведения/автора/жанра и отправить запрос боту. В ответ вам придет кнопка с готовым списком книг, в котором вы выбираете нужный вариант. \n" +
                "Так же в строке ввода сообщения вы можете указать id бота и так же название/автора/жанр, тогда список отразится автоматически. \n" +
                "Использование Инлайн режима Помимо отправки запросов боту напрямую в чат, вы можете использовать вашего помощника через инлайн-запросы, написав его юзернейм и название/автора/жанр в поле отправки сообщений в любом чате, группе или канале. Пользователю так же отобразится список доступных книг. \n" +
                "Доступные команды бота \n" +
                "Команды можно использовать как в меню бота, так и прописав их через слэш вручную. Ниже приведён список доступных \n" +
                "/start – запуск бота, приветствие \n" +
                "/random-случайная книга \n" +
                "/help- инструкция \n" +
                "/pocketbook – карманная, короткая книга  \n" +
                "Как добавить бота в чат? \n" +
                "Бота добавляем в чат так же, как обычного пользователя, ниже предоставлены скрины. Первым действием добавляем участника.\n";

            await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: usage,
                                                      replyMarkup: new ReplyKeyboardRemove());

            FileStream fileStream = new(@"C:\Users\ilega\OneDrive\Рабочий стол\LibraryBot\LibraryBot\usage\video_2022-06-09_16-54-36.mp4", FileMode.Open, FileAccess.Read, FileShare.Read);

            using (var stream = System.IO.File.OpenRead(@"C:\Users\ilega\OneDrive\Рабочий стол\LibraryBot\LibraryBot\usage\video_2022-06-09_16-54-36.mp4"))
            {
                return await botClient.SendVideoAsync(chatId: message.Chat.Id,
                                                        video: stream,
                                                        replyMarkup: new ReplyKeyboardRemove());
            }
        }

        static async Task<Message> HelloView(ITelegramBotClient botClient, Message message) //Отображаем иструкцию по боту
        {
            const string usage = "Привет, я твоя карманная библиотека!\nЧто почитаем сегодня?"; //TODO:Написать иструкцию для пользователя



            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: usage,
                                                        replyMarkup: new ReplyKeyboardRemove());
        }
    }
    private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery) //Проверка данных в кнопке 
    {

        var action = callbackQuery.Data.Split(':')[0] switch //Делим данные кнопки на несколько частей и проверяем 
        {
            "download" => BotDownloadBookFormat(botClient, callbackQuery) //если первая часть содержит donwload то переходит к функции скачивания книги
        };



    }


    private static async Task<Message> BotDownloadBookFormat(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var KeyboardData = callbackQuery.Data.Split(':'); //Делим данные кнопки на несколько частей
        try
        {
            

            using (FileStream fs = System.IO.File.Open(@"C:\Users\ilega\OneDrive\Рабочий стол\LibraryBot\LibraryBot\Files\" + KeyboardData[1], FileMode.Open)) //Открываем файл который получили
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, KeyboardData[1].Split("/").Last()); //Передаем файл классу который передаст боту

                return await botClient.SendDocumentAsync(chatId: callbackQuery.From.Id, //Передаем документ в тот чат который указан callbackQuery.From.Id айди чата с которого пришло сообщение
                                                      document: inputOnlineFile); //Сам документ который передаем
            }


        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message); //Если были ошибки то выводит их
        }

    }


    private static async Task<string> MessageBookInfo(Book book) //Функция для создания сообщения с иноформацией о книге
    {
        //TODO: картинку бы наверное
        string ViewBook = book.Title + "\n"; //Создаем строчку с данными (Название книги, автор, год, жанры

        foreach (var author in book.Authors)
        {
            ViewBook += author.Name + "  ";
        }

        ViewBook += "\n";
        if (book.Year != null)
        {
            ViewBook += "Издание: " + book.Year + "\n";
                }
        ViewBook += "Жанр: ";

        foreach (var item in book.Genre)
        {
            ViewBook += item.Genre + " "; //Циклом добавляем жанры
        }

        ViewBook += "\n\n " + book.Content;

        var fileSize = book.PathBooks.FirstOrDefault(x => x.Type == "FB2");
        if(fileSize != null)
        {
            ViewBook += "\n\nРазмер файла: " + (fileSize.Length / 1024).ToString() + " KB";
        }

        return ViewBook; //Возращаем сообщение по запросу
    }

    private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery) //Функция вызывается по обращению к боту и отображает список книг
    {
        if (inlineQuery.Query != "") //Запрос к боту не должен быть пустым
        {
            List<Book> listB = new List<Book>();   //Создаем пустой лист со страницами

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


            //offset это значение которое указывает нужно или нет продолжать вывод списка, если оно равно пустоте то не нужно, список окончен,
            //если чему то другому то продолжаем пока не будет пустоте
            if (listB.Count < 50 && inlineQuery.Offset == "") //Проверяем количество книг если меньше 50 и offset равен пустоте то идем дальше
            {
                await AnswerInline(botClient, inlineQuery, "", listB); //Выводим список книг клиенту
            }
            else
            {
                string offset = ""; //Создаем свой offset и делаем его пустым
                if (inlineQuery.Offset == "") //Проверяем какой offset пришел если пусто продолжаем
                {
                    offset = "1"; //Делаем offset равному одному 

                    

                    await AnswerInline(botClient, inlineQuery, offset, listB); //Выводим список книг клиенту
                }
                else
                {
                    offset = inlineQuery.Offset; //Получаем offset полученный запросом

                    for (int i = 0; i < int.Parse(offset); i++) //Циклом удаляем со списка книг те книги которые уже показаны
                    {
                        if (listB.Count > 50) //Проверяем больше ли 50 книг 
                            listB.RemoveRange(0, 50); //Если больше то удаляем 50 книг
                    }

                    if (listB.Count <= 50) //Если равно или меньше 50 книг 
                    {
                        offset = ""; //offset делаем равному пустоте 
                    }
                    else
                    {
                        offset = (int.Parse(offset) + 1).ToString(); //Иначе прибавляем один к offset
                    }

                    await AnswerInline(botClient, inlineQuery, offset, listB); //Выводим список книг клиенту
                }

            }

        }
    }


    private static async Task AnswerInline(ITelegramBotClient botClient, InlineQuery inlineQuery, string offset, List<Book> listBook) //Функция для ответа к обращению 
    {
        List<InlineQueryResult> results = new List<InlineQueryResult>(); //Создаем список в который будут заносится данные об книгах

        int RangeCount; //Переменная которая поможет узнать сколько книг войдут в список на данный момент
        if (listBook.Count < 50) //Если в списке книг меньше 50 книг 
            RangeCount = listBook.Count; //То заносим количество этих книг
        else
            RangeCount = 50; //Иначе 50

        listBook.GetRange(0, RangeCount);
        var list = listBook.Distinct().ToList();

        foreach (var entry in list.GetRange(0, RangeCount)) //Цикл создание элементов для отображения клиенту берутся первые 50 элементов со списка или весь список если меньше 50
        {
            var fileSize = entry.PathBooks.FirstOrDefault(x => x.Type == "FB2");
            var item = new InlineQueryResultArticle( //Создаем элемент и заполняем его
                    id: entry.Id.ToString(), //Заполняем айди элемента
                    title: entry.Title + "(" + (fileSize.Length / 1024).ToString() + " KB)", //Запонляем тест который будет ввиден в начале для клиента
                    inputMessageContent: new InputTextMessageContent(
                        MessageBookInfo(entry).Result  //Создаем сообщение который увидет клиент при нажатие на книгу
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

            item.ReplyMarkup = inlineKeyboard; //Добавляем кнопки к элементу
            results.Add(item); //Добавляем элемент к списку 
            if (results.Count == 50) //Если чисто элементов равно 50 то закончить цикл
                break;
        }

        await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id, //выводим данные клиенту  inlineQuery.Id - айди обращение к боту, кто обратился только тот и получит обратно
                                               results: results, //Выводит список элементов с данными о книгах
                                               isPersonal: true, //Отправляет только тем кто обратился 
                                               cacheTime: -1, //cacheTime ограничение времени ожидания для вывода результата запроса, 0 не ограничено
                                               nextOffset: offset);
    }

    private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult) //Функция не нужна пока
    {
        Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        return Task.CompletedTask;
    }

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update) //Функция не нужна пока
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }


    static async Task<Message> AnswerMessage(ITelegramBotClient botClient, Message message) //Ответ на обычное сообщение
    {
        if (message.ViaBot != null) //Проверка ответ на бота или нет
        {
            return null; //Если ответ то просто выходит и игнорим на сообщение
        }


        return await SerchBooks(botClient, message); //Выполняем функцию поиска книг и возращаем ответ 
    }

    static async Task<Message> RandomBook(ITelegramBotClient botClient, Message message) //Ответ на обычное сообщение
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


        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, //Chat.Id айди чата откуда был отправлен запрос
                                                        text: await MessageBookInfo(book), //Сообщение который напишет клиенту
                                                        replyMarkup: inlineKeyboard); //убирает не нужный интерфейс если есть
    }


    static async Task<Message> LittleBook(ITelegramBotClient botClient, Message message) //Ответ на обычное сообщение
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


        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, //Chat.Id айди чата откуда был отправлен запрос
                                                        text: await MessageBookInfo(book), //Сообщение который напишет клиенту
                                                        replyMarkup: inlineKeyboard); //убирает не нужный интерфейс если есть
    }

    static async Task<Message> SerchBooks(ITelegramBotClient botClient, Message message) //Функция поиска книг 
    {
        List<Book> listB = new List<Book>();   //Создаем пустой лист со страницами

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


        if (list.Count != 0) //Проверяем не пустой ли лист 
        {
            //Если не пустой 
            InlineKeyboardMarkup inlineKeyboard = new(
             new[]
             {
                    new []
                    {
                        InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Список книг", message.Text) //Создаем кнопку которая будет отправлять обращение к боту чтобы вывести список
                    }
             });

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, //Отвечает клиентку который отправил запрос, message.Chat.Id айди чата откуда был отправлен запрос
                                           text: "По вашему запросу было найдено " + list.Count + " книг.", //Текст который выведет бот, напишет сколько книг нашел
                                           replyMarkup: inlineKeyboard); //Отправляет кнопку 
        }
        else
        { //Если не нашел книги то отправит это сообщение
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, //Chat.Id айди чата откуда был отправлен запрос
                                                        text: "По вашему запросу не было найдено книг. Проверьте запрос.", //Сообщение который напишет клиенту
                                                        replyMarkup: new ReplyKeyboardRemove()); //убирает не нужный интерфейс если есть
        }
    }
}
