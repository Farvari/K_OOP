using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPF.Models
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int ReporterID { get; set; }

        [Required]
        public int ReportedID { get; set; }

        public int? ItemID { get; set; }

        public int? ResolverID { get; set; }

        [Required]
        [MaxLength(60)]
        public string ReportTitle { get; set; }

        [MaxLength(200)]
        [Column("AttachmetsPath")]
        public string AttachmentsPath { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(40)]
        public string Status { get; set; }

        [Required]
        public DateTime SentAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [ForeignKey("ReporterID")]
        public virtual User Reporter { get; set; }

        [ForeignKey("ReportedID")]
        public virtual User ReportedUser { get; set; }

        [ForeignKey("ResolverID")]
        public virtual User Resolver { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }
    }
}

