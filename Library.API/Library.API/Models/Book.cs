using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [Required, MaxLength(500)]
        public string Description { get; set; }
        [ForeignKey("AuthorId")]
        public Author Author { get; set; }
        public Guid AuthorID { get; set; }
    }
}
