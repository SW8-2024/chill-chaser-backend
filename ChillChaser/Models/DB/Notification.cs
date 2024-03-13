using System.ComponentModel.DataAnnotations.Schema;

namespace ChillChaser.Models.DB
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime ReceivedAt { get; set; }
        public int SourceAppId { get; set; }
        public App SourceApp { get; set; }

    }
}
