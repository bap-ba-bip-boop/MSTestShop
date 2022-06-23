using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MSTestShop.Settings;
using MvcSuperShop.Controllers;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Profiles;
using MvcSuperShop.Services;
using System;
using System.Security.Claims;

namespace UnitTests.TestInfrastructure.Attributes;

public class HomeControllerMoqAutoDataAttribute : AutoDataAttribute
{
    public HomeControllerMoqAutoDataAttribute() : base(
        () =>
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            fixture.Inject(
                new ApplicationDbContext(
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: nameof(HomeControllerMoqAutoDataAttribute))
                        .Options
                )
            );

            fixture.Register(() => Options.Create(fixture.Create<HomeControllerSettings>()));

            fixture.Register(() => new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<CategoryProfile>();
                    cfg.AddProfile<ProductProfile>();
                }
                ).CreateMapper()
            );

            var email = "temp@temp.se";
            var userId = fixture.Create<Guid>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email)
            }, "TestAuthentication"));

            fixture.Inject(
                user
            );
            
            var controllerContext = new ControllerContext();
            controllerContext.HttpContext = new DefaultHttpContext
            {
                User = user
            };

            fixture.Inject(controllerContext);

            fixture.Inject(new Mock<ICategoryService>());
            fixture.Inject(new Mock<IProductService>());


            var sut = new HomeController(
                fixture.Create<Mock<ICategoryService>>().Object,
                fixture.Create<Mock<IProductService>>().Object,
                fixture.Create<IMapper>(),
                fixture.Create<ApplicationDbContext>(),
                fixture.Create<IOptions<HomeControllerSettings>>()
                )
            {
                ControllerContext = controllerContext
            };

            fixture.Inject(sut);

            return fixture;
        }
    )
    {

    }
}
