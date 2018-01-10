using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.DTOs
{
    public class BookUpdateDto : BookEditBase
    {
        [Required(ErrorMessage = "You must enter a book description!")]
        public override string Description
        {
            get => base.Description; set => base.Description = value;
        }
    }
}
