namespace Demo_web_MVC.Models.ViewModel
{
    public class UserRoleViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public string SelectedRole { get; set; }
        
        public List<string> AvailableRoles { get; set; } = new();
    }
}
