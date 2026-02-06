using cafeteriaKFE.Core.Products.Request;
using cafeteriaKFE.Core.Products.Response;
using cafeteriaKFE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cafeteriaKFE.Controllers;

[Authorize(Roles = "Admin")]
public sealed class ProductController : Controller
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    // GET: /Product
    [HttpGet]
    public async Task<IActionResult> All()
    {
        var list = await _service.GetAllAsync();
        return View(list);
    }

    // GET: /Product/Create
    [HttpGet]
    public IActionResult Create()
    {
        LoadProductTypes();
        return View();
    }


    // POST: /Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductResponse model)
    {
        if (!ModelState.IsValid)
        {
            LoadProductTypes(model.ProductTypeId);
            return View(model);
        }

        var result = await _service.CreateAsync(model);

        if (result.Succeeded)
        {
            TempData["Success"] = "Producto creado correctamente.";
            return RedirectToAction("All", "Product");
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError("", err.Description);

        LoadProductTypes(model.ProductTypeId);
        return RedirectToAction("All", "Product");
    }


    // GET: /Product/Update/5
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var p = await _service.GetByIdAsync(id);
        if (p is null) return NotFound();

        var vm = new UpdateProductResponse
        {
            ProductId = p.ProductId,
            ProductTypeId = p.ProductTypeId,
            Name = p.Name,
            BarCode = p.BarCode,
            BasePrice = p.BasePrice
        };

        LoadProductTypes(vm.ProductTypeId);
        return View("Update", vm);
    }


    // POST: /Product/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateProductResponse model)
    {
        if (!ModelState.IsValid)
        {
            LoadProductTypes(model.ProductTypeId);
            return View("Update", model);
        }

        var result = await _service.UpdateAsync(model);

        if (result.Succeeded)
        {
            TempData["Success"] = "Producto actualizado correctamente.";
            return RedirectToAction("All", "Product");
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError("", err.Description);

        LoadProductTypes(model.ProductTypeId);
        return RedirectToAction("All", "Product");
    }


    // POST: /Product/Delete/5  (soft delete)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.SoftDeleteAsync(id);

        if (!result.Succeeded)
            return NotFound();

        TempData["Success"] = "Producto eliminado correctamente.";
        return RedirectToAction("All", "Product");
    }

    // GET: /Product/Reports/Sales
    [HttpGet]
    [Route("Product/Reports/Sales")] // Explicitly define the route
    public IActionResult Sales()
    {
        return View("Reports/Sales");
    }

    private static readonly (int Id, string Name)[] ProductTypes =
{
    (2, "Temperatures"),   // ajusta el texto a como lo quieras mostrar
    (3, "Syrups"),
    (4, "Whipped Cream"),
    (5, "Food")
};

    private void LoadProductTypes(int? selectedId = null)
    {
        ViewBag.ProductTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            ProductTypes.Select(x => new { x.Id, x.Name }),
            "Id",
            "Name",
            selectedId
        );
    }

}
