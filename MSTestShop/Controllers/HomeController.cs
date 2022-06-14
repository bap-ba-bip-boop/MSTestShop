using Microsoft.AspNetCore.Mvc;
using MvcSuperShop.Models;
using System.Diagnostics;
using AutoMapper;
using MvcSuperShop.Data;
using MvcSuperShop.Services;
using MvcSuperShop.ViewModels;
using Microsoft.Extensions.Options;
using MSTestShop.Settings;

namespace MvcSuperShop.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IOptions<HomeControllerSettings> _settings;

        public HomeController(ICategoryService categoryService, IProductService productService, IMapper mapper, ApplicationDbContext context, IOptions<HomeControllerSettings> settings)
        :base(context)
        {
            _categoryService = categoryService;
            _productService = productService;
            _mapper = mapper;
            _settings = settings;
        }

        public IActionResult Index()
        {
            var trendigCategoriesAmount = _settings.Value.TrendigCategoriesAmount;
            var productAmount = _settings.Value.ProductAmount;
            var model = new HomeIndexViewModel
            {
                TrendingCategories = _mapper.Map<List<CategoryViewModel>>(_categoryService.GetTrendingCategories(trendigCategoriesAmount)),
                NewProducts = _mapper.Map<List<ProductBoxViewModel>>(_productService.GetNewProducts(productAmount, GetCurrentCustomerContext()))
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}