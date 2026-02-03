using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPF.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("Password")]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Surname { get; set; }

        [Required]
        [MaxLength(12)]
        public string PhoneNum { get; set; }

        public DateTime? RegistrDate { get; set; }

        public decimal? Rating { get; set; }

        [Required]
        public int UserRole { get; set; }

        [Required]
        public int IsBlocked { get; set; }

        [ForeignKey("UserRole")]
        public virtual Role Role { get; set; }

        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<Review> ReviewsReceived { get; set; }
        public virtual ICollection<Review> ReviewsGiven { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<Report> ReportsSent { get; set; }
        public virtual ICollection<Report> ReportsReceived { get; set; }

        public User()
        {
            Items = new HashSet<Item>();
            ReviewsReceived = new HashSet<Review>();
            ReviewsGiven = new HashSet<Review>();
            Favorites = new HashSet<Favorite>();
            ReportsSent = new HashSet<Report>();
            ReportsReceived = new HashSet<Report>();
        }
    }
}

