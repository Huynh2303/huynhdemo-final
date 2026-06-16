namespace Demo_web_MVC.Models
{
    public class UserImage
    {
        public int Id { get; set; }

        public int UserId { get; set; } // FK

        public string Url { get; set; } = null!;

        // Navigation
        public User User { get; set; } = null!;
    }
}
