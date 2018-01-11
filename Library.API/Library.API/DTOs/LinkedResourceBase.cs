using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.DTOs
{
    public abstract class LinkedResourceBase
    {
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}
