using LibraryBot.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Repositories
{
    public class GenreRepository
    {
        AppDbContext app; //Переменная для содержания класс с бд

        public GenreRepository(AppDbContext app) //При создание класс требуется класс с бд
        {
            this.app = app; //В переменную кидается класс с бд
        }


        public async Task DeleteGenres(Guid id) //Функция удаления запроса клиента(поиск запроса идет по айди)
        {
            app.Genres.Remove(GetGenresById(id)); //GetRequestClientById(id) ищет запрос по айди и передает функции Remove строку в бд для удаления
            await app.SaveChangesAsync();//Сохраняем изменения в дб, если не сохранить то не удалится 
        }

        public Genres GetGenresById(Guid id)//Функция получения одной строки в бд(поиск запроса идет по айди)
        {
            return app.Genres.FirstOrDefault(x => x.Id == id);
        }

        public Genres GetGenresByName(string genre)//Функция получения одной строки в бд(поиск запроса идет по айди)
        {
            return app.Genres.FirstOrDefault(x => x.Genre == genre);
        }

        public IQueryable<Genres> GetGenres() //Функция для получения всех строк запроса
        {
            return app.Genres;
        }

        public async Task SaveGenres(Genres entity) //Функция для сохранение строки в таблицу
        {
            if (app.Authors.FirstOrDefault(x => x.Id == entity.Id) == null) //Проверка есть ли такая строка уже, если возращает пустоту(то есть null) то сохраняем
            {
                app.Genres.Add(entity); //Добавляем строку в таблицу
                await app.SaveChangesAsync(); //сохраняем изменения бд
            }
        }

        public async Task UpdateAuthor(Genres entity) //Функция обновление строки, то изменяем данные строки, если нет строки то добавляет ее
        {
            app.Genres.Update(entity); //Апдейт строки
            await app.SaveChangesAsync(); //Сохранение изменений
        }
    }
}
