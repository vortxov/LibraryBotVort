

using LibraryBot.Domain.Repositories;

namespace LibraryBot.Domain
{
    public class DataManager
    {
        public AppDbContext appDbContext { get; set; } //Класс с бд
        public AuthorRepository author { get; set; }
        public BookRepository book { get; set; }
        public GenreRepository genre { get; set; }

        public DataManager()
        {
            appDbContext = new AppDbContext(); //Создаем класс и сохраняем в переменную
            author = new AuthorRepository(appDbContext);
            book = new BookRepository(appDbContext);
            genre = new GenreRepository(appDbContext);
        }
    }
}
