using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Entity
{
    public class Book
    {
        public Guid Id { get; set; }
        public string? Year { get; set; } 
        public string Title { get; set; }
        public string? Content { get; set; }
        public List<Genres> Genre { get; set; } = new List<Genres>();
        public List<Author> Authors { get; set; } = new List<Author>();
        public List<PathBook> PathBooks { get; set; } = new List<PathBook>();
    }
}
