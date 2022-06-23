using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Context;

namespace MvcSuperShop.Services;

public  class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IPricingService _pricingService;
    private readonly IMapper _mapper;

    public ProductService(ApplicationDbContext context, IPricingService pricingService,  IMapper mapper)
    {
        _context = context;
        _pricingService = pricingService;
        _mapper = mapper;
    }
    public IEnumerable<ProductServiceModel> GetNewProducts(int cnt, CurrentCustomerContext context)
    {
        var products = _context.Products!
                    .Include(e => e.Category)
                    .Include(e => e.Manufacturer)
                    .OrderByDescending(e => e.AddedUtc)
                    .Take(cnt);
        var mappedProducts = _mapper.Map<IEnumerable<ProductServiceModel>>(
                products
            );
        var result = _pricingService.CalculatePrices(
            mappedProducts,
            context
        );
        return result;
    }
}