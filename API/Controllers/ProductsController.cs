using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    //If not us [ApiController] here the you need to add [FromQuery] in action result argurments
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IGenericRepository<Product> repo;

        public ProductsController(IGenericRepository<Product> repo)
        {
            this.repo = repo;
        }
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
        {
            var spec = new ProductSpecification(brand,type,sort);
            var product = await repo.ListAsync(spec);
            return Ok(product); 
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await repo.GetByIdAsync(id);
            if(product == null) return NotFound();
            return product;
        }
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            repo.Add(product);
            if(await repo.SaveAllAsync())
            {
                return CreatedAtAction("GetProduct", new {id=product.Id},product);
            }
            return BadRequest("Problem create product");
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            if(product.Id!=id || !ProductExists(id))
                return BadRequest("Cannot update this product");
            repo.Update(product);

            if(await repo.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Problem in updating product");
            
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await repo.GetByIdAsync(id);
            if(product == null) return NotFound();
            repo.Remove(product);
            if(await repo.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Problem deleting product");
            
        }
        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            //TODO: implement method
            var spec = new BrandListSpecification();
            return Ok(await repo.ListAsync(spec));
        }
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            //TODO: implement method
            var spec = new TypeListSpecification();
            return Ok(await repo.ListAsync(spec));
        }
        private bool ProductExists(int id)
        {
            return repo.Exists(id);
        }
    }
}
