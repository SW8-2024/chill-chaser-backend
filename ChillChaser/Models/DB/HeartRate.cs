using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChillChaser.Models.DB
{
    public class HeartRate
    {
        public int Id { get; set; }
        public int Bpm { get; set; }
        public DateTime DateTime { get; set; }
        public string UserId { get; set; }
        public CCUser User { get; set; }
    }
}