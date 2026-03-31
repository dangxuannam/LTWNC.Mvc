using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheapDeal.WebApp.Models
{
    [Table("HangfireJobLog")]
    public class HangfireJobLog
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string JobName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public int? AffectedRows { get; set; }

        public string Message { get; set; }

        // KHÔNG map vào DB — tính toán từ StartTime/EndTime
        [NotMapped]
        public double DurationSeconds
        {
            get
            {
                if (EndTime.HasValue)
                    return (EndTime.Value - StartTime).TotalSeconds;
                return 0;
            }
        }
    }
}