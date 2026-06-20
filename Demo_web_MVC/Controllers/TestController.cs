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
    // SIÊU KHÓ - khách rất tốt nhưng có dấu hiệu bất thường mạnh
    // =========================
    (
        "TC01 - VIP rất tốt nhưng đơn tăng đột biến cực mạnh",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 420,
            TotalOrders = 25,
            OrdersLast24h = 1,
            OrdersLast7d = 2,
            CancelledOrders = 1,
            CancelRate = 0.04f,
            CurrentOrderValue = 6500000,
            AvgOrderValue = 800000,
            IsCod = 1,
            CodOrderCount = 12,
            PhoneUsedCount = 1,
            AddressUsedCount = 1,
            ItemCount = 5,
            TotalQuantity = 10,
            StatusChangeCount = 2,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f,
            IsVip = 1,
            CompletedOrderCount = 24,
            CompletionRate = 0.96f
        },
        "High"
    ),

    // =========================
    // SIÊU KHÓ - tài khoản mới, đơn cao nhưng chưa đủ để High
    // =========================
    (
        "TC02 - Tài khoản mới, đơn cao nhưng số lượng ít và đã hoàn thành 1 đơn",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 5,
            TotalOrders = 1,
            OrdersLast24h = 1,
            OrdersLast7d = 1,
            CancelledOrders = 0,
            CancelRate = 0f,
            CurrentOrderValue = 4200000,
            AvgOrderValue = 900000,
            IsCod = 1,
            CodOrderCount = 1,
            PhoneUsedCount = 1,
            AddressUsedCount = 1,
            ItemCount = 1,
            TotalQuantity = 1,
            StatusChangeCount = 1,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f,
            IsVip = 0,
            CompletedOrderCount = 1,
            CompletionRate = 1f
        },
        "Medium"
    ),

    // =========================
    // SIÊU KHÓ - cancelRate đáng chú ý nhưng đơn hiện tại nhỏ
    // =========================
    (
        "TC03 - Khách cũ, cancelRate trung bình cao nhưng đơn hiện tại rất nhỏ",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 180,
            TotalOrders = 12,
            OrdersLast24h = 1,
            OrdersLast7d = 2,
            CancelledOrders = 4,
            CancelRate = 0.333f,
            CurrentOrderValue = 450000,
            AvgOrderValue = 900000,
            IsCod = 1,
            CodOrderCount = 10,
            PhoneUsedCount = 1,
            AddressUsedCount = 1,
            ItemCount = 2,
            TotalQuantity = 2,
            StatusChangeCount = 3,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 1,
            CancelRateLast7d = 0.5f,
            IsVip = 0,
            CompletedOrderCount = 8,
            CompletionRate = 0.667f
        },
        "Medium"
    ),

    // =========================
    // SIÊU KHÓ - tài khoản mới đặt dồn dập nhưng giá trị thấp
    // =========================
    (
        "TC04 - Tài khoản mới, đặt nhiều đơn trong ngày nhưng đơn hiện tại nhỏ",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 6,
            TotalOrders = 3,
            OrdersLast24h = 4,
            OrdersLast7d = 4,
            CancelledOrders = 0,
            CancelRate = 0f,
            CurrentOrderValue = 600000,
            AvgOrderValue = 500000,
            IsCod = 1,
            CodOrderCount = 3,
            PhoneUsedCount = 1,
            AddressUsedCount = 1,
            ItemCount = 2,
            TotalQuantity = 3,
            StatusChangeCount = 1,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f,
            IsVip = 0,
            CompletedOrderCount = 3,
            CompletionRate = 1f
        },
        "Medium"
    ),

    // =========================
    // SIÊU KHÓ - khách tốt nhưng số lượng/item lớn
    // =========================
    (
        "TC05 - Khách tốt, nhiều item và số lượng lớn nhưng giá trị không cao",
        new OrderRiskTrainingData
        {
            AccountAgeDays = 300,
            TotalOrders = 18,
            OrdersLast24h = 1,
            OrdersLast7d = 2,
            CancelledOrders = 1,
            CancelRate = 0.056f,
            CurrentOrderValue = 1300000,
            AvgOrderValue = 1100000,
            IsCod = 1,
            CodOrderCount = 12,
            PhoneUsedCount = 1,
            AddressUsedCount = 1,
            ItemCount = 7,
            TotalQuantity = 18,
            StatusChangeCount = 2,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0f,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0f,
            IsVip = 1,
            CompletedOrderCount = 17,
            CompletionRate = 0.944f
        },
        "Medium"
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
