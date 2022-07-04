using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Entity
{
    public class Page //Таблица со страницей для манипуляции данными полученые с сайта
    {
        public Guid Id { get; set; }//Айди для поиска
        public string? Title { get; set; } //Название полученного запроса
        public List<Link> Links { get; set; } = new List<Link>();//Лист с ссылками
        public List<Entry> Entries { get; set; } = new List<Entry>();//Лист с книгами
    }
}
