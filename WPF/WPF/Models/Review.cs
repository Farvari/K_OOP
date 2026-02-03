using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPF.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        [Column("ReviewID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int ReviewerID { get; set; }

        [Required]
        public decimal Rating { get; set; }

        [MaxLength(100)]
        public string RatingTags { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("ReviewerID")]
        public virtual User Reviewer { get; set; }
    }
}

