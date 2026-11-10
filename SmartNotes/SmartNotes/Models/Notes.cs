using System.ComponentModel.DataAnnotations;

namespace SmartNotes.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "PleaseEnter A Title")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Please enter note content.")]
        public string Content { get; set; }

    }
}
