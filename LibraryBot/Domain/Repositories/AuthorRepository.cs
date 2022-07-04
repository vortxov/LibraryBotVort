using LibraryBot.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Repositories
{
    public class AuthorRepository
    {
        AppDbContext app; //Переменная для содержания класс с бд

        public AuthorRepository(AppDbContext app) //При создание класс требуется класс с бд
        {
            this.app = app; //В переменную кидается класс с бд
        }


        public async Task DeleteAuthor(Guid id) //Функция удаления запроса клиента(поиск запроса идет по айди)
        {
            app.Authors.Remove(GetAuthorById(id)); //GetRequestClientById(id) ищет запрос по айди и передает функции Remove строку в бд для удаления
            app.SaveChanges();//Сохраняем изменения в дб, если не сохранить то не удалится 
        }

        public Author GetAuthorById(Guid id)//Функция получения одной строки в бд(поиск запроса идет по айди)
        {
            return app.Authors.FirstOrDefault(x => x.Id == id);//FirstOrDefault(x => x.Id == id) функция выдаст первую попавшиюся строку
                                                             //если условия будут выполнятся то есть айди строки будет равен айди поиска до выдаст строку
                                                             //Если не найдет такой строки то выдаст пустоту то есть null
                                                             //Include(x => x.Pages) Функция для поключения страницы с другой таблицы,
                                                             //без нее pages будет пустым
        }

        public Author GetAuthorByName(string name)//Функция получения одной строки в бд(поиск запроса идет по айди)
        {
            return app.Authors.FirstOrDefault(x => x.Name == name);
        }

        public IQueryable<Author> GetAuthors() //Функция для получения всех строк запроса
        {
            return app.Authors.Include(x => x.Books).ThenInclude(x => x.PathBooks).Include(x => x.Books).ThenInclude(x => x.Genre);
        }

        public async Task SaveAuthor(Author entity) //Функция для сохранение строки в таблицу
        {
            if (app.Authors.FirstOrDefault(x => x.Id == entity.Id) == null) //Проверка есть ли такая строка уже, если возращает пустоту(то есть null) то сохраняем
            {
                await app.Authors.AddAsync(entity); //Добавляем строку в таблицу
                app.SaveChanges(); //сохраняем изменения бд
            }
        }

        public async Task UpdateAuthor(Author entity) //Функция обновление строки, то изменяем данные строки, если нет строки то добавляет ее
        {
            app.Authors.Update(entity); //Апдейт строки
            app.SaveChanges(); //Сохранение изменений
        }
    }
}
