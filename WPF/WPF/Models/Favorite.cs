using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPF.Models
{
    [Table("Favorite")]
    public class Favorite
    {
        [Key, Column(Order = 0)]
        public int UserID { get; set; }

        public int? FavUserID { get; set; }

        [Key, Column(Order = 1)]
        [Required]
        public int FavItemID { get; set; }

        [Required]
        public DateTime AddedAt { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("FavUserID")]
        public virtual User FavUser { get; set; }

        [ForeignKey("FavItemID")]
        public virtual Item FavItem { get; set; }
    }
}

