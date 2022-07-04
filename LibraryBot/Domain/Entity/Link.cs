using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Entity
{
    public class Link //Таблица с ссылками которые приходят в запросе от сервера, там может быть ссылка на автора или на скачивание книг
    {
        public Guid Id { get; set; } //Айди для поиска
        public string? Href { get; set; } //Ссылка
        public string? Type { get; set; } //Тип ссылки 
        public string? Title { get; set; } //Название, для чего ссылка(Не всегда приходит поэтому может быть пустым)
        public string? Rel { get; set; } //Дополнительная ссылка если прошлая не сработала
    }
}
