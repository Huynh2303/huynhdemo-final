namespace Demo_web_MVC.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

       // public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
