using LibraryBot.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Repositories
{
    public class BookRepository
    {
        AppDbContext app; //Переменная для содержания класс с бд

        public BookRepository(AppDbContext app) //При создание класс требуется класс с бд
        {
            this.app = app; //В переменную кидается класс с бд
        }


        public async Task DeleteBook(Guid id) //Функция удаления запроса клиента(поиск запроса идет по айди)
        {
            app.Books.Remove(GetBookById(id)); //GetRequestClientById(id) ищет запрос по айди и передает функции Remove строку в бд для удаления
            app.SaveChanges();//Сохраняем изменения в дб, если не сохранить то не удалится 
        }

        public Book GetBookById(Guid id)//Функция получения одной строки в бд(поиск запроса идет по айди)
        {
            return app.Books.Include(x => x.Authors).Include(x => x.Genre).Include(x => x.PathBooks).FirstOrDefault(x => x.Id == id);//FirstOrDefault(x => x.Id == id) функция выдаст первую попавшиюся строку
                                                                                            //если условия будут выполнятся то есть айди строки будет равен айди поиска до выдаст строку
                                                                                            //Если не найдет такой строки то выдаст пустоту то есть null
                                                                                            //Include(x => x.Pages) Функция для поключения страницы с другой таблицы,
                                                                                            //без нее pages будет пустым
        }

        public Book GetBookByTitle(string title)//Функция получения одной строки в бд(поиск запроса идет по айди)
        {
            return app.Books.Include(x => x.Authors).Include(x => x.Genre).Include(x => x.PathBooks).FirstOrDefault(x => x.Title == title);
        }

        public IQueryable<Book> GetBooks() //Функция для получения всех строк запроса
        {
            return app.Books.Include(x => x.Authors).Include(x => x.Genre).Include(x => x.PathBooks);
        }

        public async Task SaveBook(Book entity) //Функция для сохранение строки в таблицу
        {
            if (app.Books.FirstOrDefault(x => x.Id == entity.Id) == null) //Проверка есть ли такая строка уже, если возращает пустоту(то есть null) то сохраняем
            {
                await app.Books.AddAsync(entity); //Добавляем строку в таблицу
                app.SaveChanges(); //сохраняем изменения бд
            }
        }

        public async Task UpdateBook(Book entity) //Функция обновление строки, то изменяем данные строки, если нет строки то добавляет ее
        {
            app.Books.Update(entity); //Апдейт строки
            app.SaveChanges(); //Сохранение изменений
        }
    }
}
