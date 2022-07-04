using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Entity
{
    public class Genres //Таблица для бд с указанием жанра 
    {
        public Guid Id { get; set; } //Айди для бд, чтобы можно было находить нужную строку в таблице
        public string? Genre { get; set; } //Само название жанра 
        public List<Book> Books { get; set; }
    }
}
