using ClosedXML.Excel;
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository.OrderRisk;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Birth;
using Demo_web_MVC.Service.Product;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Demo_web_MVC.Controllers
{
    public class TestController : Controller
    {
        private readonly IOrderRiskRepository _orderRepository;

        private readonly OrderRiskModelTrainer _orderRiskModelTrainer;
        private readonly OrderRiskPredictor _orderRiskPredictor;
        private readonly IBirthService _birthService;
        private readonly ILogger<TestController > _logger;
        private readonly AppDatabase _context;
        private readonly IWebHostEnvironment _env;
        private readonly IProductService _productService;
        public TestController(
            IProductService productService,
            IWebHostEnvironment env,
            AppDatabase appDatabase ,ILogger<TestController> logger ,IBirthService birthService,IOrderRiskRepository orderRepository, OrderRiskModelTrainer orderRiskModelTrainer, OrderRiskPredictor orderRiskPredictor)
        {
            _orderRepository = orderRepository;
            _orderRiskModelTrainer = orderRiskModelTrainer;
            _orderRiskPredictor = orderRiskPredictor;
            _birthService = birthService;
            _logger = logger;
            _context = appDatabase;
            _env = env;
            _productService = productService;
        }
       
        public async Task<IActionResult> TestRiskInput(int orderId)
        {
            var data = await _orderRepository.BuildRiskInputAsync(orderId);

            if (data == null)
            {
                return NotFound("Không tìm thấy đơn hàng");
            }

            return Json(data);
        }
        public IActionResult TrainOrderRiskModel()
        {
            var result = _orderRiskModelTrainer.Train();

            return Content(result);
        }

        
        public async Task<IActionResult> TestBirthdayEmail()
        {
            _logger.LogWarning("Bắt đầu");
            
            await _birthService.SendBirthdayEmailsAsync();
            
            return Content("Đã chạy gửi mail sinh nhật");
        }
        [HttpGet]
        public IActionResult ImportExcel()
        {
            return View();
        }
        private int? GetSellerIdFromClaims()
        {
            var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sellerId))
            {
                return null;
            }

            return int.Parse(sellerId);
        }
        
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Chưa chọn file Excel.";
                return RedirectToAction("ImportExcel");
            }

            var sellerId = GetSellerIdFromClaims();
            if (sellerId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            using var stream = excelFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var productSheet = workbook.Worksheet("Products");
            var variantSheet = workbook.Worksheet("Variants");

            var products = new Dictionary<string, ProductViewModel>();

            foreach (var row in productSheet.RowsUsed().Skip(1))
            {
                var productCode = row.Cell(1).GetString().Trim();

                var productVM = new ProductViewModel
                {
                    Name = row.Cell(2).GetString().Trim(),
                    CategoryId = row.Cell(3).GetValue<int>(),
                    Brand = row.Cell(6).GetString().Trim(),
                    Description = row.Cell(7).GetString().Trim(),
                    imageUrl = new List<string>(),
                    Variants = new List<ProductVariantsViewModel>()
                };

                var imageUrls = row.Cell(8).GetString().Trim();

                foreach (var image in imageUrls.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var fileName = CopyImageFromTest(image.Trim(), "products", false);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        productVM.imageUrl.Add(fileName);
                    }
                }

                products[productCode] = productVM;
            }

            foreach (var row in variantSheet.RowsUsed().Skip(1))
            {
                var productCode = row.Cell(1).GetString().Trim();

                if (!products.ContainsKey(productCode))
                    continue;

                var variantVM = new ProductVariantsViewModel
                {
                    Size = row.Cell(3).GetString().Trim(),
                    Color = row.Cell(4).GetString().Trim(),
                    Price = row.Cell(5).GetValue<decimal>(),
                    Stock = row.Cell(6).GetValue<int>(),
                    ImageUrlsVariants = new List<string>()
                };

                var imageUrls = row.Cell(7).GetString().Trim();

                foreach (var image in imageUrls.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var imageUrl = CopyImageFromTest(image.Trim(), "variants", true);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        variantVM.ImageUrlsVariants.Add(imageUrl);
                    }
                }

                products[productCode].Variants.Add(variantVM);
            }

            var count = 0;

            foreach (var product in products.Values)
            {
                await _productService.creat(product, sellerId.Value);
                count++;
            }

            TempData["Success"] = $"Import thành công {count} sản phẩm.";
            return RedirectToAction("ProductsManager");
        }
        private string CopyImageFromTest(string oldImageUrl, string targetFolder, bool returnFullUrl)
        {
            if (string.IsNullOrWhiteSpace(oldImageUrl))
                return "";

            var sourcePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                oldImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!System.IO.File.Exists(sourcePath))
                return "";

            var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}";

            var targetDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                targetFolder);

            Directory.CreateDirectory(targetDirectory);

            var targetPath = Path.Combine(targetDirectory, newFileName);

            System.IO.File.Copy(sourcePath, targetPath, true);

            if (returnFullUrl)
            {
                return $"/uploads/{targetFolder}/{newFileName}";
            }

            return newFileName;
        }














        public async Task<IActionResult> DownloadImages()
        {
            using var httpClient = new HttpClient();

            var keywords = new List<string>
    {
        "smartphone",
        "laptop",
        "tablet",
        "watch",
        "smartwatch",
        "headphones",
        "earbuds",
        "camera",
        "monitor",
        "keyboard",
        "mouse",
        "charger",
        "power bank",
        "speaker",
        "gaming"
    };

            var saveFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "test");

            Directory.CreateDirectory(saveFolder);

            int productCount = 0;
            int imageCount = 0;
            int maxImages = 200;

            foreach (var keyword in keywords)
            {
                if (imageCount >= maxImages)
                    break;

                var json = await httpClient.GetStringAsync(
                    $"https://dummyjson.com/products/search?q={Uri.EscapeDataString(keyword)}");

                var response = JsonSerializer.Deserialize<DummyProductResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                foreach (var product in response?.Products ?? [])
                {
                    if (imageCount >= maxImages)
                        break;

                    if (product.Images == null || product.Images.Count == 0)
                        continue;

                    productCount++;

                    var safeProductName = MakeSafeFileName(product.Title);

                    var productFolder = Path.Combine(
                        saveFolder,
                        $"{productCount}_{safeProductName}");

                    Directory.CreateDirectory(productFolder);

                    for (int i = 0; i < product.Images.Count; i++)
                    {
                        if (imageCount >= maxImages)
                            break;

                        try
                        {
                            var imageUrl = product.Images[i];

                            var bytes = await httpClient.GetByteArrayAsync(imageUrl);

                            var fileName = i == 0
                                ? "main.jpg"
                                : $"variant_{i}.jpg";

                            await System.IO.File.WriteAllBytesAsync(
                                Path.Combine(productFolder, fileName),
                                bytes);

                            imageCount++;
                        }
                        catch
                        {
                            // Bỏ qua ảnh lỗi
                        }
                    }
                }
            }

            return Content($"Đã tải {productCount} sản phẩm, tổng {imageCount} ảnh.");
        }

        private string MakeSafeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '-');
            }

            return name
                .Replace(" ", "-")
                .ToLower();
        }

        public class DummyProductResponse
        {
            public List<DummyProductDto> Products { get; set; } = new();
        }

        public class DummyProductDto
        {
            public int Id { get; set; }

            public string Title { get; set; } = "";

            public List<string> Images { get; set; } = new();
        }
        public IActionResult OrderRiskTestCases()
        {
            var predictor = new OrderRiskPredictor();

            var testCases = new List<(string Name, OrderRiskTrainingData Input, string ExpectedLevel)>
{
    // =========================
    // User 26 - test7, lịch sử tốt nhưng AccountAgeDays đang = 0
    // Nếu user 26 đã là VIP thật thì đổi IsVip = 1
    // =========================

    (
        "DB76 - User 26, đặt nhiều đơn trong ngày, đơn 8.99 triệu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 0,
            TotalOrders = 12,
            OrdersLast24h = 4,
            OrdersLast7d = 5,
            CancelledOrders = 0,
            CancelRate = 0f,

            CurrentOrderValue = 8990000,
            AvgOrderValue = 925000,

            IsCod = 1,
            CodOrderCount = 12,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 8,
            CompletionRate = 0.667f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Medium"
    ),

    (
        "DB75 - User 26, tài khoản mới nhưng có lịch sử hoàn thành, đơn 12 triệu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 0,
            TotalOrders = 11,
            OrdersLast24h = 3,
            OrdersLast7d = 4,
            CancelledOrders = 0,
            CancelRate = 0f,

            CurrentOrderValue = 12000000,
            AvgOrderValue = 925000,

            IsCod = 1,
            CodOrderCount = 11,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 8,
            CompletionRate = 0.727f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Medium"
    ),

    (
        "DB74 - User 1, đặt nhiều đơn gần đây, có lịch sử hủy, đơn 9.5 triệu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 48,
            TotalOrders = 26,
            OrdersLast24h = 3,
            OrdersLast7d = 7,
            CancelledOrders = 6,
            CancelRate = 0.231f,

            CurrentOrderValue = 9500000,
            AvgOrderValue = 0,

            IsCod = 1,
            CodOrderCount = 26,

            ItemCount = 2,
            TotalQuantity = 2,

            IsVip = 0,
            CompletedOrderCount = 0,
            CompletionRate = 0f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Medium"
    ),

    (
        "DB73 - User 26, lịch sử hoàn thành tốt, đơn 12 triệu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 0,
            TotalOrders = 10,
            OrdersLast24h = 2,
            OrdersLast7d = 3,
            CancelledOrders = 0,
            CancelRate = 0f,

            CurrentOrderValue = 12000000,
            AvgOrderValue = 925000,

            IsCod = 1,
            CodOrderCount = 10,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 8,
            CompletionRate = 0.800f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Medium"
    ),

    (
        "DB72 - User 27, tài khoản mới, đơn đầu tiên nhỏ 500k",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 0,
            TotalOrders = 0,
            OrdersLast24h = 0,
            OrdersLast7d = 0,
            CancelledOrders = 0,
            CancelRate = 0f,

            CurrentOrderValue = 500000,
            AvgOrderValue = 0,

            IsCod = 1,
            CodOrderCount = 0,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 0,
            CompletionRate = 0f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Low"
    ),

    (
        "DB71 - User 26, lịch sử tốt, đơn 13.5 triệu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 0,
            TotalOrders = 9,
            OrdersLast24h = 1,
            OrdersLast7d = 2,
            CancelledOrders = 0,
            CancelRate = 0f,

            CurrentOrderValue = 13500000,
            AvgOrderValue = 925000,

            IsCod = 1,
            CodOrderCount = 9,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 8,
            CompletionRate = 0.889f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Medium"
    ),

    (
        "DB62 - User 26, đơn 1.9 triệu, lịch sử hoàn thành tốt",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 0,
            TotalOrders = 8,
            OrdersLast24h = 0,
            OrdersLast7d = 1,
            CancelledOrders = 0,
            CancelRate = 0f,

            CurrentOrderValue = 1900000,
            AvgOrderValue = 925000,

            IsCod = 1,
            CodOrderCount = 8,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 8,
            CompletionRate = 1f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Low"
    ),

    // =========================
    // User 10 - lịch sử hủy / hoàn thành yếu
    // =========================

    (
        "DB61 - User 10, lịch sử yếu, đơn 9 triệu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 38,
            TotalOrders = 11,
            OrdersLast24h = 1,
            OrdersLast7d = 3,
            CancelledOrders = 3,
            CancelRate = 0.273f,

            CurrentOrderValue = 9000000,
            AvgOrderValue = 14950000,

            IsCod = 1,
            CodOrderCount = 11,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 3,
            CompletionRate = 0.273f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Medium"
    ),

    (
        "DB60 - User 10, tỷ lệ hoàn thành thấp, đơn 13.5 triệu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 38,
            TotalOrders = 10,
            OrdersLast24h = 0,
            OrdersLast7d = 2,
            CancelledOrders = 3,
            CancelRate = 0.300f,

            CurrentOrderValue = 13500000,
            AvgOrderValue = 15675000,

            IsCod = 1,
            CodOrderCount = 10,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 2,
            CompletionRate = 0.200f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Medium"
    ),

    // =========================
    // User 1 - nhiều đơn, có hủy, completionRate đang rất yếu
    // =========================

    (
        "DB59 - User 1, đơn rất lớn 42.5 triệu, lịch sử yếu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 48,
            TotalOrders = 25,
            OrdersLast24h = 2,
            OrdersLast7d = 6,
            CancelledOrders = 6,
            CancelRate = 0.240f,

            CurrentOrderValue = 42500000,
            AvgOrderValue = 0,

            IsCod = 1,
            CodOrderCount = 25,

            ItemCount = 2,
            TotalQuantity = 2,

            IsVip = 0,
            CompletedOrderCount = 0,
            CompletionRate = 0f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "High"
    ),

    (
        "DB58 - User 1, đơn 3.65 triệu, lịch sử có hủy",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 48,
            TotalOrders = 24,
            OrdersLast24h = 1,
            OrdersLast7d = 5,
            CancelledOrders = 6,
            CancelRate = 0.250f,

            CurrentOrderValue = 3650000,
            AvgOrderValue = 0,

            IsCod = 1,
            CodOrderCount = 24,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 0,
            CompletionRate = 0f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "Medium"
    ),

    (
        "DB57 - User 1, đơn 18 triệu, lịch sử có hủy và hoàn thành yếu",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 48,
            TotalOrders = 23,
            OrdersLast24h = 0,
            OrdersLast7d = 4,
            CancelledOrders = 6,
            CancelRate = 0.261f,

            CurrentOrderValue = 18001000,
            AvgOrderValue = 0,

            IsCod = 1,
            CodOrderCount = 23,

            ItemCount = 1,
            TotalQuantity = 1,

            IsVip = 0,
            CompletedOrderCount = 0,
            CompletionRate = 0f,

            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f
        },
        "High"
    )
};
            var result = new StringBuilder();

            int passCount = 0;
            int wrongCount = 0;
            int fpHighCount = 0;
            int fnHighCount = 0;

            foreach (var testCase in testCases)
            {
                var prediction = predictor.Predict(testCase.Input);

                // Test kết quả cuối cùng sau rule
                var decision = predictor.GetRiskDecision(testCase.Input, prediction);

                string actualLevel = decision.RiskLevel;
                string expectedLevel = testCase.ExpectedLevel;

                bool expectedHigh = expectedLevel == "High";
                bool actualHigh = actualLevel == "High";

                string resultText;

                if (actualLevel == expectedLevel)
                {
                    resultText = "PASS";
                    passCount++;
                }
                else
                {
                    resultText = "WRONG";
                    wrongCount++;

                    if (!expectedHigh && actualHigh)
                    {
                        fpHighCount++;
                        resultText = "FP_HIGH";
                    }
                    else if (expectedHigh && !actualHigh)
                    {
                        fnHighCount++;
                        resultText = "FN_HIGH";
                    }
                }

                result.AppendLine(testCase.Name);
                result.AppendLine($"ExpectedLevel: {expectedLevel}");
                result.AppendLine($"ActualLevel: {actualLevel}");
                result.AppendLine($"AI Raw PredictedRisk: {prediction.IsRisk}");
                result.AppendLine($"AI Score: {prediction.Score:F6}");

                // Nếu OrderRiskDecision đã có FinalScore thì mở dòng này:
                // result.AppendLine($"Final Score: {decision.FinalScore:F6}");

                result.AppendLine($"Suggestion: {decision.Suggestion}");
                result.AppendLine("Reasons:");

                foreach (var reason in decision.Reasons)
                {
                    result.AppendLine($"- {reason}");
                }

                result.AppendLine($"Result: {resultText}");
                result.AppendLine("------------------------------");
            }

            result.AppendLine("SUMMARY");
            result.AppendLine($"Total: {testCases.Count}");
            result.AppendLine($"PASS: {passCount}");
            result.AppendLine($"WRONG: {wrongCount}");
            result.AppendLine($"FP_HIGH: {fpHighCount}");
            result.AppendLine($"FN_HIGH: {fnHighCount}");

            if (fnHighCount > 0)
            {
                result.AppendLine("Đánh giá: Rule đang bỏ lọt một số case High, cần tăng điểm cho dấu hiệu rủi ro mạnh.");
            }
            else if (fpHighCount >= 3)
            {
                result.AppendLine("Đánh giá: Rule đang đẩy quá nhiều case lên High, cần giảm điểm hoặc tăng ngưỡng High.");
            }
            else if (wrongCount >= 6)
            {
                result.AppendLine("Đánh giá: Rule còn lệch nhiều giữa Low/Medium/High, cần chỉnh lại điểm cộng/trừ.");
            }
            else
            {
                result.AppendLine("Đánh giá: Rule tương đối ổn, có thể tinh chỉnh thêm theo từng case sai.");
            }

            return Content(result.ToString(), "text/plain", Encoding.UTF8);
        }
    }
}
