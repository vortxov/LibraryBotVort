using LibraryBot.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibraryBot.Service
{
    public class PageParse //Класс для получения данных с xml файла который приходит с сайта
    {
        public static Page Parse(string strXml) //Функция для этого Она создает класс page и заполняет этот класс данными
        {
            XmlDocument xDoc = new XmlDocument(); //Класс для хранения xml файла
            Page page = new Page(); //page который будем заполнять 
            Genres Gen; //Список жанров пойдет сюда
            string id = null; //Айди автора, помогает для пойска нужного автора в авторах книг 

            try
            {
                xDoc.LoadXml(strXml); //Загружаем xml файл в класс для хранения

                XmlElement? xRoot = xDoc.DocumentElement; //Получаем элементы из файла

                List<Link> links = new List<Link>(); //Список ссылок которые будут заполняться и пойду в page 
                List<Entry> entries = new List<Entry>(); //Список книг которые будут заполняться и пойду в page 

                foreach (XmlElement xnode in xRoot)  //Цикл для проверки и манипуляций всех элементов в xml файле
                {
                    if (xnode.Name == "title") //Проверяем если элемент title
                        page.Title = xnode.InnerText; //Если да то в page title пишем что находится в этом элементе
                    if (xnode.Name == "id") //Тоже самое но с айди
                       id = xnode.InnerText;
                    else if (xnode.Name == "link")   //Проверяет на ссылку
                    {
                        Link link = new Link(); //создаем ссылку 
                        link.Href = xnode.Attributes["href"]?.Value; //заполняем саму ссылку
                        link.Rel = xnode.Attributes["rel"]?.Value; //дополнительная ссылка
                        link.Title = xnode.Attributes["title"]?.Value;//Название ссылки
                        link.Type = xnode.Attributes["type"]?.Value;//Тип ссылки

                        links.Add(link); //Добавляем ссылку в список ссылок 
                    }
                    else if (xnode.Name == "entry")//Проверяем на entry
                    {
                        Entry entry = new Entry();//Создаем пустой entry
                        foreach (XmlNode childnode in xnode.ChildNodes)//Проверяем элементы внутри элемента entry(В одном элементе может быть много других елементов дочерних)
                        {
                            if (childnode.Name == "title") //Проверяем на title 
                                entry.Title = childnode.InnerText; //И заносим уже в пустой entry а не page
                            else if (childnode.Name == "author")
                            {
                                if (childnode.ChildNodes[1].InnerText.Split('/').Last() == id.Split(':')[2] || id.Split(':')[1] == "search") //По id проверяем айди автора,
                                                                                                                                             //если айди тот же то это наш автор
                                    entry.Author = childnode.ChildNodes[0].InnerText; //Записываем автора
                            }
                            else if (childnode.Name == "id")
                            {
                                entry.IdBook = childnode.InnerText.Split(':').Last(); //Записываем айди самой книги и вносим 
                            }
                            else if (childnode.Name == "content")
                            {
                                entry.Content = childnode.InnerText; //Записываем айди самой книги и вносим 
                            }
                            else if (childnode.Name == "link")
                            {
                                Link link = new Link();
                                link.Href = childnode.Attributes["href"]?.Value;
                                link.Rel = childnode.Attributes["rel"]?.Value;
                                link.Title = childnode.Attributes["title"]?.Value;
                                link.Type = childnode.Attributes["type"]?.Value;

                                entry.Links.Add(link);
                            }
                            else if (childnode.Name == "dc:issued") //Элемент с годом выпуска
                                entry.Year = childnode.InnerText;
                            else if (childnode.Name == "category") //Жанры книги
                            {
                                Gen = new Genres(); //Создаем пустой жанр
                                Gen.Genre = childnode.Attributes["label"]?.Value; //Записываем жанр из элемент в жанр
                                entry.Genre.Add(Gen); //Добавляем жанр в книгу
                            }
                        }
                        entries.Add(entry); //Добавляем книгу в лист книг
                    }
                }

                page.Links = links; //Записываем лист ссылок в лист с ссылками в page
                page.Entries = entries;//Записываем лист книг в лист с книгами в page

                return page; //Возвращаем page откуда шел запрос
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
