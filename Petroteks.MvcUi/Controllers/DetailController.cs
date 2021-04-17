﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Petroteks.Bll.Abstract;
using Petroteks.Bll.Concreate;
using Petroteks.Entities.Concreate;
using Petroteks.MvcUi.ExtensionMethods;
using Petroteks.MvcUi.Models;
using Petroteks.MvcUi.Services;
using Petroteks.MvcUi.StringInfos;

namespace Petroteks.MvcUi.Controllers
{
    public class DetailController : GlobalController
    {
        private readonly IProductService productService;
        private readonly ICategoryService categoryService;
        private readonly IBlogService blogService;
        private readonly IRouteTable routeTable;
        private readonly ICacheService cacheService;

        public DetailController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            productService = serviceProvider.GetService<IProductService>();
            categoryService = serviceProvider.GetService<ICategoryService>();
            blogService = serviceProvider.GetService<IBlogService>();
            routeTable = serviceProvider.GetService<IRouteTable>();
            cacheService = serviceProvider.GetService<ICacheService>();
        }
         
        [Route("Kategori-Detay/{categoryName}-{page:int}-{category:int}")]
        public IActionResult CategoryDetailOld(string categoryName, int page = 1, int category = 0)
        {
            return RedirectToAction("CategoryDetail", "Detail",
                new
                {
                    area = "",
                    pageTag = routeTable.Get(EntityName.Category, PageType.Detail),
                    pageType = routeTable.Get(EntityName.Category, PageType.Normal),
                    id = category,
                    categoryName = categoryName
                });
        }
        [Route("{pageType}/{pageTag}/{id:int}/{categoryName}")]
        public async Task<IActionResult> CategoryDetail(string pageType, string pageTag, int id)
        {
            if (routeTable.Exists(pageTag, EntityName.Category, PageType.Detail) && routeTable.Exists(pageType, EntityName.Category, PageType.Normal))
            {
                Category Category = await cacheService.GetAsync($"{CacheInfo.Category}-{id}-{CurrentWebsite.id}", () =>
                     categoryService.GetAllLanguageCategory(x => x.id == id && x.WebSiteid == CurrentWebsiteId && x.IsActive == true)
                 );

                if (Category != null)
                {
                    List<Category> subCategories = await cacheService.GetAsync($"{CacheInfo.SubCategoryies}-{id}-{CurrentWebsite.id}-{Category.Languageid.Value}", () =>
                     categoryService.GetMany(x => x.WebSite == CurrentWebsite && x.Parentid == Category.id && x.IsActive == true, Category.Languageid.Value).OrderByDescending(x => x.Priority).ToList()
                     );
                    ICollection<Product> products = await cacheService.GetAsync($"{CacheInfo.SubProducts}-{id}-{Category.Languageid.Value}", () =>
                      productService.GetMany(x => x.Categoryid == Category.id && x.IsActive == true, Category.Languageid.Value)
                    );
                    if (Category.Languageid != CurrentLanguage.id)
                    {
                        LoadLanguage(true, Category.Languageid);
                    }

                    return View(new ProductListViewModel()
                    {
                        Products = products,
                        CurrentCategory = Category,
                        SubCategories = subCategories
                    });
                }
                else
                {
                    return RedirectToAction("CategoryNotFound");
                }
            }
            else
            {
                return NotFoundPage();
            }
        }

        [Route("Urun-Detay/{produtname}-{id:int}")]
        [HttpGet]
        public IActionResult ProductDetailOld(int id, string produtname)
        {
            return RedirectToAction("ProductDetail", "Detail", 
                new {
                    area = "",
                    pageTag = routeTable.Get(EntityName.Product, PageType.Detail),
                    id = id,
                    produtname = produtname 
                });
        }
        [Route("{pageTag}/{id:int}/{produtname}")]
        [HttpGet]
        public async Task<IActionResult> ProductDetail(string pageTag, int id)
        {
            if (routeTable.Exists(pageTag, EntityName.Product, PageType.Detail))
            {
                Product product = await cacheService.GetAsync($"{CacheInfo.Product}-{id}", () =>
                     productService.GetAllLanguageProduct(x => x.id == id && x.IsActive == true)
                 );
                if (product != null)
                {
                    Category category = await cacheService.GetAsync($"{CacheInfo.Category}-product-{id}", () =>
                          categoryService.GetAllLanguageCategory(x => x.IsActive && x.WebSiteid == CurrentWebsite.id && x.id == product.Categoryid)
                     );
                    if (category != null)
                    {
                        if (product?.Languageid != CurrentLanguage.id)
                        {
                            LoadLanguage(true, product.Languageid);
                        }

                        return View(product);
                    }
                }
                return RedirectToAction("ProductNotFound");
            }
            else
            {
                return NotFoundPage();
            }
        }

        [Route("Blog/{blogPageName}/{id:int}/{title}")]
        public async Task<IActionResult> BlogDetail(string blogPageName, int id)
        {
            if (routeTable.Exists(blogPageName, EntityName.Blog, PageType.Detail))
            {
                Blog findedBlog = await cacheService.GetAsync($"{CacheInfo.Blog}-{id}-{CurrentWebsite.id}", () =>
                    blogService.GetAllLanguageBlog(x => x.id == id && x.IsActive == true && x.WebSiteid == CurrentWebsiteId)
                );

                if (findedBlog != null)
                {

                    if (findedBlog?.Languageid != CurrentLanguage.id)
                    {
                        LoadLanguage(true, findedBlog.Languageid);
                    }

                    return View(findedBlog);
                }
            }
            return RedirectToAction("BlogNotFound");
        }

        [Route("404-Product-Not-Found.html")]
        [HttpGet]
        public IActionResult ProductNotFound()
        {
            return View();
        }
        [Route("404-Category-Not-Found.html")]
        [HttpGet]
        public IActionResult CategoryNotFound()
        {
            return View();
        }

        [Route("404-Blog-Not-Found.html")]
        [HttpGet]
        public IActionResult BlogNotFound()
        {
            return View();
        }
    }
}


