using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.DTOs
{
    public class LinkedCollectionResource<T> : LinkedResourceBase where T : LinkedResourceBase
    {
        public IEnumerable<T> Value { get; set; }

        public LinkedCollectionResource(IEnumerable<T> value)
        {
            Value = value;
        }
    }
}
