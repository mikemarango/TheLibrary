using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.DTOs
{
    public class BookCreateDto
    {
        [Required(ErrorMessage = "Please enter a Title"), MaxLength(100, ErrorMessage = "Title should not exceed 100 letters.")]
        public string Title { get; set; }
        [MaxLength(500, ErrorMessage = "The description should not exceed 70 words.")]
        public string Description { get; set; }
    }
}
