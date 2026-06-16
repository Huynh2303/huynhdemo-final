using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Service.Address;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Demo_web_MVC.Controllers
{
    [Authorize(Roles = "ADMIN,USER,STAFF")]
    public class AdressController : Controller
    {
        public readonly IAddressService _addressService;
        public readonly ILogger<AdressController> _logger;
        public AdressController(IAddressService addressService, ILogger<AdressController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }
        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }
        public async Task<IActionResult> IndexPartial()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                {
                    return Unauthorized("Không xác định được người dùng.");
                }

                var result = (await _addressService.GetAllByUserId(userId.Value)).ToList();

                _logger.LogInformation(
                    "Lấy danh sách địa chỉ thành công cho userId: {UserId}, Số lượng địa chỉ: {Count}",
                    userId.Value,
                    result.Count
                );

                return PartialView("IndexPartial", result); // trả về partial
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách địa chỉ.");
                return StatusCode(500, "Đã xảy ra lỗi khi tải danh sách địa chỉ.");
            }
        }
        
        public IActionResult CreatePartail()
        {
            return PartialView("CreatePartail", new AddressViewModel());
        }

        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(AddressViewModel model)
        {
            _logger.LogInformation("POST Create Address nhận model: {@Model}", model);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid");
                return PartialView("CreatePartail", model);
            }

            var userId = GetUserIdFromClaims();

            if (userId == null)
            {
                return Unauthorized("Không xác định được người dùng.");
            }

            try
            {
                var result = await _addressService.Create(userId.Value, model);

                if (!result)
                {
                    ModelState.AddModelError("", "Không thể tạo địa chỉ.");

                    return PartialView("CreatePartail", model);
                }

                var data = (await _addressService
                    .GetAllByUserId(userId.Value))
                    .ToList();

                return PartialView("IndexPartial", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi create address");

                return StatusCode(500, "Đã xảy ra lỗi.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserIdFromClaims();

            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _addressService.Delete(userId.Value, id);

                if (!result)
                {
                    return BadRequest("Không thể xóa địa chỉ.");
                }

                var data = (await _addressService
                    .GetAllByUserId(userId.Value))
                    .ToList();

                return PartialView("IndexPartial", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi xóa địa chỉ");

                return StatusCode(500,
                    "Đã xảy ra lỗi.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = GetUserIdFromClaims();

            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _addressService
                    .SetDefaultAddress(userId.Value, id);

                if (!result)
                {
                    return BadRequest(
                        "Không thể đặt mặc định.");
                }

                var data = (await _addressService
                    .GetAllByUserId(userId.Value))
                    .ToList();

                return PartialView("IndexPartial", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi SetDefault");

                return StatusCode(500,
                    "Đã xảy ra lỗi.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("===== BAT DAU EDIT =====");

            var userId = GetUserIdFromClaims();

            _logger.LogInformation($"UserId lay tu claims: {userId}");

            if (userId == null)
            {
                _logger.LogWarning("Nguoi dung chua login hoac claims khong hop le.");

                return Unauthorized("Khong xac dinh duoc nguoi dung.");
            }

            var address = await _addressService.GetById( id,userId.Value);

            _logger.LogInformation($"Tim dia chi voi id = {id}");

            if (address == null)
            {
                _logger.LogWarning("Khong tim thay dia chi.");

                return NotFound();
            }

            _logger.LogInformation("Render partial Edit thanh cong.");

            return PartialView("~/Views/Adress/CreatePartail.cshtml", address);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(int id,AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/Adress/CreatePartail.cshtml", model);
            }

            var userId = GetUserIdFromClaims();

            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _addressService
                    .Update(userId.Value, id, model);

                if (!result)
                {
                    ModelState.AddModelError("",
                        "Không thể cập nhật.");

                    return PartialView("~/Views/Adress/CreatePartail.cshtml", model);
                }

                var data = (await _addressService
                    .GetAllByUserId(userId.Value))
                    .ToList();

                return PartialView("IndexPartial", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Edit Error");

                return StatusCode(500,
                    "Đã xảy ra lỗi.");
            }
        }
    }
}
