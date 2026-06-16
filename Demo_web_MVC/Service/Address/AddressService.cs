using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Repository.Addresss;

namespace Demo_web_MVC.Service.Address
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger<AddressService> _logger;
        public AddressService(IAddressRepository addressRepository, ILogger<AddressService> logger)
        {
            _addressRepository = addressRepository;
            _logger = logger;
        }
        public async Task<IEnumerable<AddressViewModel>> GetAllByUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }
            return await _addressRepository.GetAllByUserIdAsync(userId);
        }
        public async Task<AddressViewModel?> GetById(int addressId, int userId)
        {
            _logger.LogInformation("===== SERVICE GET BY ID =====");

            _logger.LogInformation($"addressId = {addressId}");
            _logger.LogInformation($"userId = {userId}");

            if (addressId <= 0 || userId <= 0)
            {
                _logger.LogWarning("addressId hoặc userId không hợp lệ");

                throw new ArgumentException("Invalid address ID or user ID.");
            }

            var result = await _addressRepository.GetByIdAsync(addressId, userId);

            if (result == null)
            {
                _logger.LogWarning("Repository trả về NULL");
            }
            else
            {
                _logger.LogInformation($"Tìm thấy address Id = {result.Id}");
            }

            return result;
        }
        public async Task<bool> Create(int userId, AddressViewModel model)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            return await _addressRepository.CreateAsync(userId, model);
        }
        public async Task<bool> Update(int userId, int addressId, AddressViewModel model)
        {
            if (userId <= 0 || addressId <= 0)
            {
                throw new ArgumentException("Invalid user ID or address ID.");
            }
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            return await _addressRepository.UpdateAsync(userId, addressId, model);
        }
        public async Task<bool> Delete( int addressId, int userId)
        {
            if (userId <= 0 || addressId <= 0)
            {
                throw new ArgumentException("Invalid user ID or address ID.");
            }
            return await _addressRepository.DeleteAsync( addressId, userId );
        }
        public async Task<AddressViewModel?> GetDefaultAddress(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }
            return await _addressRepository.GetDefaultAddressAsync(userId);
        }
        public async Task<bool> SetDefaultAddress(int userId, int addressId)
        {
            if (userId <= 0 || addressId <= 0)
            {
                throw new ArgumentException("Invalid user ID or address ID.");
            }
            return await _addressRepository.SetDefaultAddressAsync(userId, addressId);
        }
    }
}
