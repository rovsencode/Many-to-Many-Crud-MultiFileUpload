using FirelloProject.DAL;
using FirelloProject.Extentions;
using FirelloProject.Models;
using FirelloProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FirelloProject.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext appDbContext, IWebHostEnvironment env)
        {
            _appDbContext = appDbContext;
            _env = env;
        }

        public IActionResult Index()
        {
            return View(_appDbContext.Products.Include(p => p.ProductImages).Include(p=>p.Category).ToList());
        }
        public IActionResult Create()
        {

            ViewBag.Categories= new SelectList(_appDbContext.Categories.ToList(), "ID", "Name");
            return View();
        }

        [HttpPost]

        public IActionResult Create(ProductCreateVM productCreateVM)
        {
            ViewBag.Categories = _appDbContext.Categories.ToList();
            if (!ModelState.IsValid) return View();
            List<ProductImage> productImages = new();
            foreach (var photo in productCreateVM.Photos)
            {
                if (!photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "only image ");
                    return View();
                }
                if (photo.CheckImageSize(500))
                {
                    ModelState.AddModelError("Photo", "olcu boyukdur ");
                    return View();
                }
                ProductImage productImage = new();
                productImage.ImageUrl = photo.SaveImage(_env,"img",photo.FileName);
                productImages.Add(productImage);


            }

            Product newproduct = new();
            newproduct.Name = productCreateVM.Name;
            newproduct.Price = productCreateVM.Price;
            newproduct.CategoryID = productCreateVM.CategoryId;
            newproduct.ProductImages = productImages;
            _appDbContext.Products.Add(newproduct);
            _appDbContext.SaveChanges();
         
            return RedirectToAction("Index");
        }
        public IActionResult Detail(int id)
        {
            if (id == null) return NotFound();
            Product product = _appDbContext.Products.Include(p=>p.ProductImages).Include(p=>p.Category).SingleOrDefault(p=>p.ID==id);
           if(product== null) return NotFound();
           return View(product);
        }
    }
}
