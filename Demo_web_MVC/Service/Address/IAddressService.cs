using Demo_web_MVC.Models.ViewModel.Address;

namespace Demo_web_MVC.Service.Address
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressViewModel>> GetAllByUserId(int userId);
        Task<AddressViewModel?> GetById(int addressId, int userId);
        Task<bool> Create(int userId, AddressViewModel model);
        Task<bool> Update(int userId, int addressId, AddressViewModel model);
        Task<bool> Delete(int userId, int addressId);
        Task<bool> SetDefaultAddress(int userId, int addressId);
        Task<AddressViewModel?> GetDefaultAddress(int userId);
    }
}
