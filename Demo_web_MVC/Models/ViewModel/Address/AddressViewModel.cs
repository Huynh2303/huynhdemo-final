namespace Demo_web_MVC.Models.ViewModel.Address
{
    public class AddressViewModel
    {
        
        public int Id { get; set; }
        public string AddressLine { get; set; } = null!;

        public string City { get; set; } = null!;

        public string Country { get; set; } = null!;

        public bool IsDefault { get; set; }

        public string RecipientName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;


    }
}
