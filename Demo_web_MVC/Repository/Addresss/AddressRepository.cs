    using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models.ViewModel.Address;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Addresss
{
    public class AddressRepository : IAddressRepository
    {
        public readonly AppDatabase _context;
        public readonly ILogger<AddressRepository> _logger;
        public AddressRepository(AppDatabase context, ILogger<AddressRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IEnumerable<AddressViewModel>> GetAllByUserIdAsync(int userId)
        {
            try
            {
                var addresses = await _context.Addresses.AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    AddressLine = a.AddressLine,
                    City = a.City,
                    Country = a.Country,
                    IsDefault = a.IsDefault,
                    RecipientName = a.RecipientName,
                    PhoneNumber = a.PhoneNumber
                }).ToListAsync();
                return addresses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách địa chỉ của userId: {UserId}", userId);
                throw;
            }
        }
        public async Task<AddressViewModel?> GetByIdAsync(int addressId, int userid)
        {
            try
            {
                _logger.LogInformation("===== REPOSITORY GET BY ID =====");

                _logger.LogInformation($"addressId = {addressId}");
                _logger.LogInformation($"userid = {userid}");

                var query = _context.Addresses
                    .AsNoTracking()
                    //.IgnoreQueryFilters() // bật dòng này để test query filter
                    .Where(a => a.Id == addressId && a.UserId == userid);

                _logger.LogInformation("Đang query database...");

                var result = await query
                    .Select(a => new AddressViewModel
                    {
                        Id = a.Id,
                        AddressLine = a.AddressLine,
                        City = a.City,
                        Country = a.Country,
                        IsDefault = a.IsDefault,
                        RecipientName = a.RecipientName,
                        PhoneNumber = a.PhoneNumber
                    })
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    _logger.LogWarning("Database trả về NULL");
                }
                else
                {
                    _logger.LogInformation($"Tìm thấy address Id = {result.Id}");
                    _logger.LogInformation($"RecipientName = {result.RecipientName}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching address with ID {AddressId} for user {UserId}",
                    addressId,
                    userid);

                return null;
            }
        }
        public async Task<bool> CreateAsync(int userId, AddressViewModel model)
        {
            try
            {
                var existingDefaultAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

                var newAddress = new Models.Address
                {
                    UserId = userId,
                    AddressLine = model.AddressLine,
                    City = model.City,
                    Country = model.Country,
                    IsDefault = model.IsDefault,
                    RecipientName = model.RecipientName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow
                };
                if (model.IsDefault && existingDefaultAddress != null)
                {
                    existingDefaultAddress.IsDefault = false;
                }
                await _context.Addresses.AddAsync(newAddress);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for user {UserId}", userId);
                _logger.LogError(ex.Message);
                return false;
            }

        }
        public async Task<bool> UpdateAsync(int userId, int addressId, AddressViewModel model)
        {
            try
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
                if (address == null)
                {
                    return false;
                }
                if (model.IsDefault)
                {
                    var existingDefaultAddress = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault && a.Id != addressId);

                    if (existingDefaultAddress != null)
                    {
                        existingDefaultAddress.IsDefault = false;
                    }
                }
                address.AddressLine = model.AddressLine;
                address.City = model.City;
                address.Country = model.Country;
                address.IsDefault = model.IsDefault;
                address.RecipientName = model.RecipientName;
                address.PhoneNumber = model.PhoneNumber;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address with ID {AddressId} for user {UserId}", addressId, userId);
                _logger.LogError(ex.Message);
                return false;
            }
        }
        public async Task<bool> DeleteAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
                if (address == null)
                {
                    return false;
                }
                var wasDefault = address.IsDefault;
                _context.Addresses.Remove(address);
                if (wasDefault)
                {
                    var anotherAddress = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.UserId == userId && a.Id != addressId);

                    if (anotherAddress != null)
                    {
                        anotherAddress.IsDefault = true;
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address with ID {AddressId} for user {UserId}", addressId, userId);
                _logger.LogError(ex.Message);
                return false;
            }
        }
        public async Task<AddressViewModel?> GetDefaultAddressAsync(int userId)
        {
            try
            {
                return await _context.Addresses.AsNoTracking()
                .Where(a => a.UserId == userId && a.IsDefault)
                .Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    AddressLine = a.AddressLine,
                    City = a.City,
                    Country = a.Country,
                    IsDefault = a.IsDefault,
                    RecipientName = a.RecipientName,
                    PhoneNumber = a.PhoneNumber
                }).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching default address for user {UserId}", userId);
                _logger.LogError(ex.Message);
                return null;
            }
        }
        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
                if (address == null)
                {
                    return false;
                }
                if (address.IsDefault)
                {
                    return true;
                }
                var userAddresses = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();

                foreach (var item in userAddresses)
                {
                    item.IsDefault = false;
                }
                address.IsDefault = true;
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address with ID {AddressId} for user {UserId}", addressId, userId);
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}
