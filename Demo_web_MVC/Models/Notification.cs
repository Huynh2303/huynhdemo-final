namespace Demo_web_MVC.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string Type { get; set; } = null!;

        public int? ReferenceId { get; set; }

        public string? Url { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
