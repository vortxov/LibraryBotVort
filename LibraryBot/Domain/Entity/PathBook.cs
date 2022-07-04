using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBot.Domain.Entity
{
    public class PathBook
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public long Length { get; set; }
    }
}
