using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Entity
{
    public class Entry //Таблица с характеристиками книг
    {
        public Guid Id { get; set; } //Айди для поиска 
        public string? Updated { get; set; } //Последнее облавление книги, но оно почему то не используется нормально на сайте поэтому я не использовал, оставил чтобы была
        public string? IdBook { get; set; } //айди книги на сайте, чтобы найти в списки книг автора
        public string? Year { get; set; } //Год выпуска книги
        public string? Title { get; set; } //Название книги
        public string? Author { get; set; } //Автор
        public string? Content { get; set; }
        public List<Genres> Genre { get; set; } = new List<Genres>(); //Список жанров
        public List<Link> Links { get; set; } = new List<Link>(); //Список ссылок
    }
}
