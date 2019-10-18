using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class AuthorsResourceParameters
    {
        const int MAX_PAGE_SIZE = 20;
        public int PageNumber { get; set; } = 1;
        private int pageSize = 10;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = (value > MAX_PAGE_SIZE) ? MAX_PAGE_SIZE : value;
        }

        public string Genre { get; set; }

        public string SearchQuery { get; set; }

        public string OrderBy { get; set; } = "Name";

        public string Fields { get; set; }
    }
}
