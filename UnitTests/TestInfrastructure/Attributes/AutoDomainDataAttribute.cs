using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MSTestShop.Settings;
using MvcSuperShop.Controllers;
using MvcSuperShop.Data;
using MvcSuperShop.Services;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace UnitTests.TestInfrastructure.Attributes;

public class AutoDomainDataAttribute : AutoDataAttribute
{
    public AutoDomainDataAttribute() : base(
        () =>
        {
            var fixture = new Fixture()
            .Customize(new AutoMoqCustomization());

            fixture.Inject(
                new ApplicationDbContext(
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "AutoDomain Database")
                        .Options
                )
            );


            return fixture;
        }
    )
    {

    }
}