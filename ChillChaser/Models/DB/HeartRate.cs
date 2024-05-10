using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ChillChaser.Models.DB
{
    [Index(nameof(UserId), nameof(DateTime), IsUnique = false, Name = "datetime_userid_idx")]
    [Index(nameof(DateTime), IsUnique = false, Name = "datetime_idx")]
    public class HeartRate
    {
        public int Id { get; set; }
        public int Bpm { get; set; }
        public DateTime DateTime { get; set; }
        public string UserId { get; set; }
        public CCUser User { get; set; }
    }
}