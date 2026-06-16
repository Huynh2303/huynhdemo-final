namespace Demo_web_MVC.Models.ViewModel.Admin
{
    public class UserManagementViewModel
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int LockedUsers { get; set; }

        public PaginatedList<UserItemViewModel>? Users { get; set; }
    }
}
