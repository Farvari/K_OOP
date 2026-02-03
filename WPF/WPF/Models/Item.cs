using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPF.Models
{
    [Table("Items")]
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(60)]
        public string ItemTitle { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        public int? Price { get; set; }

        [MaxLength(65)]
        public string ContactInfo { get; set; }

        [Required]
        [MaxLength(40)]
        public string Status { get; set; }

        [Required]
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<Image> Images { get; set; }

        public Item()
        {
            Favorites = new HashSet<Favorite>();
            Images = new HashSet<Image>();
        }
    }
}

