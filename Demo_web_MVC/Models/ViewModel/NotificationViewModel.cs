namespace Demo_web_MVC.Models.ViewModel
{
    public class NotificationViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string Type { get; set; } = null!;

        public string? Url { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
