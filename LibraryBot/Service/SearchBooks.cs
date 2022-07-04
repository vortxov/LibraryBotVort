using Leaf.xNet;
using LibraryBot.Domain;
using LibraryBot.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Service
{
    public class SearchBooks
    {
        private DataManager dataManager = new DataManager(); //Создаем класс для работы с бд
        public PageParse pageParse { get; set; }
        private string RootOPDS = /*"https://www.flibusta.site";*/  "http://flibustaongezhld6dibs2dps6vm4nvqg2kp7vgowbu76tzopgnhazqd.onion"; //Ссылки сайта
                                                                                                                                                    //(не все всегда рабочие поэтому надо менять иногда
        private string AuthorSearch = "/search?searchType=authors&searchTerm="; //Дополнение к ссылке для поиска автора
        private string BookSearch = "/search?searchType=books&searchTerm="; //Дополнение к ссылке для поиска книг


        public void SearchFullBook()
        {
            for (int i = 0; i < 32; i++)
            {
                char a = 'а';
                BooksOnAuthor(((char)((int)a + i)).ToString());
            }

            for (int i = 0; i < 26; i++)
            {
                char a = 'a';
                BooksOnAuthor(((char)((int)a + i)).ToString());
            }
        }




        private async void BooksOnAuthor(string symbol) //Функция для получения листа книг с помощью запросов
        {
            int NumberPageListAuthor = 0; //Номер страницы на сайте( по одному запросу на сайт можем получить несколько страниц, так как один xml файл хранит всего 20 авторов

            while (true) //Бесконечный цикл
            {
                try
                {
                    string xmlPage = GetAuthor(symbol, NumberPageListAuthor); //Получаем xml файл с листом авторов
                    var page = PageParse.Parse(xmlPage);   //Обрабатывайм xml файл и получаем страницу
                    NumberPageListAuthor++; //Добавляем один к номеру страниц


                    if (page.Entries.Count == 0) //Проверяем количество авторов, если 0 то выходим
                        break;


                    foreach (var authorPage in page.Entries) //Цикл проходит всех авторов и получает все книги у автора
                    {
                        var author = dataManager.author.GetAuthorByName(authorPage.Title);
                        if (author == null)
                        {
                            author = new Author();
                            author.Name = authorPage.Title;
                            await dataManager.author.SaveAuthor(author);
                        }
                        var NumberPage = 0; //Номер страницы для получения книг
                        while (true) //Бесконечный цикл
                        {
                            try
                            {
                                var xmlPageAuthor = OpdsRequest(RootOPDS + authorPage.Links[0].Href + "/alphabet/" + NumberPage).Result; //Получаем xml файл с книгами по алфавиту начиная с 0 страницы
                                var pageBooks = PageParse.Parse(xmlPageAuthor); //Получаем страницу с книгами
                                if (pageBooks.Entries.Count > 0) //Если книг большо 0
                                {
                                    foreach (var bookPage in pageBooks.Entries)
                                    {
                                        var book = new Book();

                                        var bookDb = dataManager.book.GetBookByTitle(bookPage.Title);

                                        if (bookDb == null)
                                        {
                                            book.Title = bookPage.Title;
                                            book.Year = bookPage.Year;

                                            book.Content = "";
                                            if (bookPage.Content != null)
                                            {
                                                var contents = bookPage.Content.Split(@"</p>");

                                                for (int i = 0; i < contents.Length - 1; i++)
                                                {
                                                    book.Content += contents[i].Split(">")[1] + "\n";
                                                }
                                            }

                                            foreach (var genre in bookPage.Genre)
                                            {
                                                var genreDb = dataManager.genre.GetGenresByName(genre.Genre);
                                                if (genreDb != null)
                                                {
                                                    book.Genre.Add(genreDb);
                                                }
                                                else
                                                {
                                                    book.Genre.Add(genre);
                                                }
                                            }

                                            book.Authors.Add(author);

                                            var path = Guid.NewGuid().ToString();

                                            var LinkFb2 = bookPage.Links.FirstOrDefault(x => x.Type == "application/fb2+zip");
                                            if (LinkFb2 != null)
                                            {
                                                try
                                                {
                                                    var pathFb2 = path + ".fb2";
                                                    DownloadFile(LinkFb2.Href, pathFb2);
                                                    PathBook pathBook = new PathBook();
                                                    var fileSize = new System.IO.FileInfo(@"C:\Users\ilega\OneDrive\Рабочий стол\LibraryBot\LibraryBot\Files\" + pathFb2).Length;
                                                    pathBook.Length = fileSize;
                                                    pathBook.Path = pathFb2;
                                                    pathBook.Type = "FB2";

                                                    book.PathBooks.Add(pathBook);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine(ex.Message);
                                                }
                                            }
                                            var LinkEPUB = bookPage.Links.FirstOrDefault(x => x.Type == "application/epub+zip");
                                            if (LinkEPUB != null)
                                            {
                                                try
                                                {
                                                    var pathepub = path + ".epub";
                                                    DownloadFile(LinkEPUB.Href, pathepub);
                                                    PathBook pathBook = new PathBook();
                                                    pathBook.Path = pathepub;
                                                    pathBook.Type = "EPUB";

                                                    book.PathBooks.Add(pathBook);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine(ex.Message);
                                                }
                                            }
                                            var LinkMOBI = bookPage.Links.FirstOrDefault(x => x.Type == "application/x-mobipocket-ebook");
                                            if (LinkMOBI != null)
                                            {
                                                try
                                                {
                                                    var pathMobi = path + ".mobi";
                                                    DownloadFile(LinkMOBI.Href, pathMobi);
                                                    PathBook pathBook = new PathBook();
                                                    pathBook.Path = pathMobi;
                                                    pathBook.Type = "MOBI";

                                                    book.PathBooks.Add(pathBook);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine(ex.Message);
                                                }
                                            }

                                            if (book.PathBooks.Count != 0)
                                            {
                                                await dataManager.book.SaveBook(book);
                                                Console.WriteLine("Add " + book.Title);
                                            }
                                        }
                                        else
                                        {
                                            var aub = bookDb.Authors.FirstOrDefault(x => x.Name == author.Name);
                                            if (aub == null)
                                            {
                                                book.Authors.Add(author);
                                                await dataManager.book.UpdateBook(book);
                                                Console.WriteLine("Update " + book.Title);
                                            }
                                        }
                                    }
                                    NumberPage++; //Добавляем один к номеру страниц
                                }
                                else //Если книг в странице нет 
                                    break; //Выходим из цикла
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                continue;
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        public async void SearchNewBook() //Функция для получения листа книг с помощью запросов
        {
            var NumberPage = 0; //Номер страницы для получения книг
            while (true) //Бесконечный цикл
            {
                try
                {
                    var xmlPageNew = GetNew(NumberPage); //Получаем xml файл с книгами по алфавиту начиная с 0 страницы
                    var pageBooks = PageParse.Parse(xmlPageNew); //Получаем страницу с книгами
                    if (pageBooks.Entries.Count > 0) //Если книг большо 0
                    {
                        foreach (var bookPage in pageBooks.Entries)
                        {
                            var author = dataManager.author.GetAuthorByName(bookPage.Author);
                            if (author == null)
                            {
                                author = new Author();
                                author.Name = bookPage.Author;
                                await dataManager.author.SaveAuthor(author);
                            }

                            var book = new Book();

                            var bookDb = dataManager.book.GetBookByTitle(bookPage.Title);

                            if (bookDb == null)
                            {
                                book.Title = bookPage.Title;
                                book.Year = bookPage.Year;

                                book.Content = "";
                                if (bookPage.Content != null)
                                {
                                    var contents = bookPage.Content.Split(@"</p>");

                                    for (int i = 0; i < contents.Length - 1; i++)
                                    {
                                        book.Content += contents[i].Split(">")[1] + "\n";
                                    }
                                }

                                foreach (var genre in bookPage.Genre)
                                {
                                    var genreDb = dataManager.genre.GetGenresByName(genre.Genre);
                                    if (genreDb != null)
                                    {
                                        book.Genre.Add(genreDb);
                                    }
                                    else
                                    {
                                        book.Genre.Add(genre);
                                    }
                                }

                                book.Authors.Add(author);

                                var path = Guid.NewGuid().ToString();

                                var LinkFb2 = bookPage.Links.FirstOrDefault(x => x.Type == "application/fb2+zip");
                                if (LinkFb2 != null)
                                {
                                    try
                                    {
                                        var pathFb2 = path + ".fb2";
                                        DownloadFile(LinkFb2.Href, path);
                                        PathBook pathBook = new PathBook();
                                        pathBook.Path = pathFb2;
                                        pathBook.Type = "FB2";

                                        book.PathBooks.Add(pathBook);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                var LinkEPUB = bookPage.Links.FirstOrDefault(x => x.Type == "application/epub+zip");
                                if (LinkEPUB != null)
                                {
                                    try
                                    {
                                        var pathepub = path + ".epub";
                                        DownloadFile(LinkEPUB.Href, path);
                                        PathBook pathBook = new PathBook();
                                        pathBook.Path = pathepub;
                                        pathBook.Type = "EPUB";

                                        book.PathBooks.Add(pathBook);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                var LinkMOBI = bookPage.Links.FirstOrDefault(x => x.Type == "application/x-mobipocket-ebook");
                                if (LinkMOBI != null)
                                {
                                    try
                                    {
                                        var pathMobi = path + ".mobi";
                                        DownloadFile(LinkMOBI.Href, path);
                                        PathBook pathBook = new PathBook();
                                        pathBook.Path = pathMobi;
                                        pathBook.Type = "MOBI";

                                        book.PathBooks.Add(pathBook);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }

                                if (book.PathBooks.Count != 0)
                                {
                                    await dataManager.book.SaveBook(book);
                                    Console.WriteLine("Add " + book.Title);
                                }
                            }
                            else
                            {
                                var aub = book.Authors.FirstOrDefault(x => x.Name == author.Name);
                                if (aub == null)
                                {
                                    book.Authors.Add(author);
                                    await dataManager.book.UpdateBook(book);
                                    Console.WriteLine("Update " + book.Title);
                                }
                            }
                        }
                        NumberPage++; //Добавляем один к номеру страниц
                    }
                    else //Если книг в странице нет 
                        break; //Выходим из цикла
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }

        }

        private void DownloadFile(string href, string path) //Функция для получения xml файла с книгами c сайта 
        {
            using (var request = new HttpRequest()) //Создаем класс для запросов 
            {
                request.Proxy = Socks5ProxyClient.Parse("127.0.0.1:9150"); //Создаем прокси для запроса
                request.Post(RootOPDS + "/" + href).ToFile(@"C:\Users\ilega\OneDrive\Рабочий стол\LibraryBot\LibraryBot\Files\" + path); 

            }
        }

        private string GetNew(int indexPage) //Функция для получения xml файла с книгами c сайта 
        {
            return OpdsRequest(RootOPDS + "/opds/new/" + indexPage.ToString() + "/new").Result; //OpdsRequest - функция которая отправляет запрос сайту
                                                                                                                       //RootOPDS + "/opds" + BookSearch + book + "&pageNumber=" + indexPage.ToString() - ссылка для запроса
        }

        private string GetBook(string book, int indexPage) //Функция для получения xml файла с книгами c сайта 
        {
            return OpdsRequest(RootOPDS + "/opds" + BookSearch + book + "&pageNumber=" + indexPage.ToString()).Result; //OpdsRequest - функция которая отправляет запрос сайту
                                                                                                                       //RootOPDS + "/opds" + BookSearch + book + "&pageNumber=" + indexPage.ToString() - ссылка для запроса
        }

        private string GetAuthor(string author, int indexPage) //Функция для получения xml файла с авторами c сайта 
        {
            return OpdsRequest(RootOPDS + "/opds" + AuthorSearch + author.Split(' ')[0] + "&pageNumber=" + indexPage.ToString()).Result; //OpdsRequest - функция которая отправляет запрос сайту
        }                               //RootOPDS + "/opds" + AuthorSearch + author.Split(' ')[0] + "&pageNumber=" + indexPage.ToString() - ссылка для запроса


        public async Task<string> OpdsRequest(string url) //функция которая отправляет запрос сайту
        {
            using (var request = new HttpRequest()) //Создаем класс для отправление запросов на сайт
            {
                request.Proxy = Socks5ProxyClient.Parse("127.0.0.1:9150"); //Подключаем прокси к классу (Прокси это типо обход блокировки сайта в нашей стране)
                var res = request.Post(url).ToString(); //Отправляем запрос сайта, url - ссылка на сайт в который нужно отправить запрос

                return res; //Возвращаем xml файл 
            }
        }
    }
}
