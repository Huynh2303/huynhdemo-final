using Demo_web_MVC.Models.ViewModel.Address;
namespace Demo_web_MVC.Repository.Addresss
{
    public interface IAddressRepository
    {
        Task<IEnumerable<AddressViewModel>> GetAllByUserIdAsync(int userId);
        Task<AddressViewModel?> GetByIdAsync(int addressId , int userid);
        Task<bool> CreateAsync(int userId, AddressViewModel model);
        Task<bool> UpdateAsync(int userId, int addressId, AddressViewModel model);
        Task<bool> DeleteAsync(int userId, int addressId);

        Task<AddressViewModel?> GetDefaultAddressAsync(int userId);
        Task<bool> SetDefaultAddressAsync(int userId, int addressId);
    }
}
