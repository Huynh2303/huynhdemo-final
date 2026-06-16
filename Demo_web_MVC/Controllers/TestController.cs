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

        public IActionResult TestPublicDatasetLikeCases()
        {
            var cases = new List<OrderRiskTrainingData>
    {
        
        new OrderRiskTrainingData
        {
            AccountAgeDays = 120,
            TotalOrders = 1,
            OrdersLast24h = 0,
            OrdersLast7d = 1,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 180000,
            AvgOrderValue = 180000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 2,
            TotalQuantity = 3,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 2: Khách mua nhiều lần, lịch sử ổn
        // Pattern: khách cũ, nhiều invoice, không hủy
        // Kỳ vọng: Low
        new OrderRiskTrainingData
        {
            AccountAgeDays = 240,
            TotalOrders = 12,
            OrdersLast24h = 0,
            OrdersLast7d = 2,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 420000,
            AvgOrderValue = 390000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 4,
            TotalQuantity = 6,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 3: Khách mua sỉ nhẹ
        // UCI Online Retail có nhiều khách wholesale, nên quantity có thể cao hơn
        // Kỳ vọng: Low hoặc Medium nhẹ
        new OrderRiskTrainingData
        {
            AccountAgeDays = 300,
            TotalOrders = 20,
            OrdersLast24h = 0,
            OrdersLast7d = 3,
            CancelledOrders = 1,
            CancelRate = 0.05f,
            CurrentOrderValue = 1200000,
            AvgOrderValue = 900000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 8,
            TotalQuantity = 20,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 4: Khách cũ, có 1 đơn bị hủy trong lịch sử
        // Kỳ vọng: Low
        new OrderRiskTrainingData
        {
            AccountAgeDays = 180,
            TotalOrders = 10,
            OrdersLast24h = 0,
            OrdersLast7d = 2,
            CancelledOrders = 1,
            CancelRate = 0.1f,
            CurrentOrderValue = 350000,
            AvgOrderValue = 400000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 3,
            TotalQuantity = 5,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 5: Khách có nhiều đơn gần đây nhưng chưa hủy nhiều
        // Pattern: khách mua thường xuyên trong tuần
        // Kỳ vọng: Low hoặc Medium nhẹ
        new OrderRiskTrainingData
        {
            AccountAgeDays = 90,
            TotalOrders = 18,
            OrdersLast24h = 1,
            OrdersLast7d = 6,
            CancelledOrders = 1,
            CancelRate = 0.056f,
            CurrentOrderValue = 560000,
            AvgOrderValue = 450000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 5,
            TotalQuantity = 8,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 1,
            CancelRateLast7d = 0.167f
        },

        // Case 6: Hóa đơn có giá trị cao hơn trung bình
        // Dataset thật có khách mua nhiều mặt hàng trong một invoice
        // Kỳ vọng: Medium
        new OrderRiskTrainingData
        {
            AccountAgeDays = 150,
            TotalOrders = 8,
            OrdersLast24h = 0,
            OrdersLast7d = 2,
            CancelledOrders = 1,
            CancelRate = 0.125f,
            CurrentOrderValue = 1800000,
            AvgOrderValue = 450000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 12,
            TotalQuantity = 30,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 7: Khách có tỷ lệ hủy trung bình
        // Trong UCI, hóa đơn bắt đầu bằng C thường được xem là cancellation/return
        // Kỳ vọng: Medium
        new OrderRiskTrainingData
        {
            AccountAgeDays = 200,
            TotalOrders = 9,
            OrdersLast24h = 0,
            OrdersLast7d = 4,
            CancelledOrders = 3,
            CancelRate = 0.333f,
            CurrentOrderValue = 480000,
            AvgOrderValue = 430000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 4,
            TotalQuantity = 7,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 2,
            CancelRateLast7d = 0.5f
        },

        // Case 8: Khách có nhiều đơn hủy trong lịch sử
        // Kỳ vọng: High
        new OrderRiskTrainingData
        {
            AccountAgeDays = 160,
            TotalOrders = 10,
            OrdersLast24h = 0,
            OrdersLast7d = 5,
            CancelledOrders = 5,
            CancelRate = 0.5f,
            CurrentOrderValue = 520000,
            AvgOrderValue = 400000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 5,
            TotalQuantity = 9,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 3,
            CancelRateLast7d = 0.6f
        },

        // Case 9: Nhiều đơn trong 24h, giống khách đặt nhiều invoice gần nhau
        // Kỳ vọng: High theo rule của hệ thống bạn
        new OrderRiskTrainingData
        {
            AccountAgeDays = 60,
            TotalOrders = 6,
            OrdersLast24h = 3,
            OrdersLast7d = 6,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 300000,
            AvgOrderValue = 280000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 3,
            TotalQuantity = 4,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 10: Khách mới, mua đơn nhỏ, không COD
        // Kỳ vọng: Low
        new OrderRiskTrainingData
        {
            AccountAgeDays = 5,
            TotalOrders = 1,
            OrdersLast24h = 1,
            OrdersLast7d = 1,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 220000,
            AvgOrderValue = 220000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 2,
            TotalQuantity = 2,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 11: Khách mới, đơn lớn/số lượng cao, nhưng dataset public không có COD
        // Kỳ vọng: Medium, vì không COD nhưng giá trị và số lượng cao
        new OrderRiskTrainingData
        {
            AccountAgeDays = 6,
            TotalOrders = 1,
            OrdersLast24h = 1,
            OrdersLast7d = 1,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 2500000,
            AvgOrderValue = 2500000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 15,
            TotalQuantity = 40,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 12: Khách mua sỉ ổn định, nhiều đơn nhưng hủy thấp
        // Kỳ vọng: Low hoặc Medium nhẹ
        new OrderRiskTrainingData
        {
            AccountAgeDays = 365,
            TotalOrders = 45,
            OrdersLast24h = 1,
            OrdersLast7d = 7,
            CancelledOrders = 2,
            CancelRate = 0.044f,
            CurrentOrderValue = 1500000,
            AvgOrderValue = 1300000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 10,
            TotalQuantity = 35,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 1,
            CancelRateLast7d = 0.143f
        }
    };

            var results = cases.Select((input, index) =>
            {
                var prediction = _orderRiskPredictor.Predict(input);

                var decision = _orderRiskPredictor.GetRiskDecision(input, prediction);

                return new
                {
                    Case = index + 1,
                    Source = "Mapped from public real retail dataset patterns",
                    MissingFieldsDefaultedToZero = new[]
                    {
                        "IsCod",
                        "CodOrderCount",
                        "PhoneUsedCount",
                        "AddressUsedCount",
                        "StatusChangeCount"
                    },
                    Expected = index switch
                    {
                        0 => "Low",
                        1 => "Low",
                        2 => "Low hoặc Medium nhẹ",
                        3 => "Low",
                        4 => "Low hoặc Medium nhẹ",
                        5 => "Medium",
                        6 => "Medium",
                        7 => "High",
                        8 => "High",
                        9 => "Low",
                        10 => "Medium",
                        11 => "Low hoặc Medium nhẹ",
                        _ => ""
                    },
                    Input = input,
                    Prediction = new
                    {
                        prediction.IsRisk,
                        prediction.Score,
                        RiskLevel = decision.RiskLevel,
                        Suggestion = decision.Suggestion,
                        Reasons = decision.Reasons
                    }
                };
            });

            return Json(results);
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
    }
}
