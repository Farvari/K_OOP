using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPF.Models
{
    [Table("Images")]
    public class Image
    {
        [Key]
        [Column("ImgID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImgID { get; set; }

        [Required]
        [Column("ItemID")]
        public int ItemID { get; set; }

        [MaxLength(200)]
        [Column("ImgPath")]
        public string ImgPath { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }
    }
}

