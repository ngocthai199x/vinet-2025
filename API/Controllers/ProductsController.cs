using API.RequestHelpers;
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
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public ProductsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery]ProductSpecParams specParams)
        {
            var spec = new ProductSpecification(specParams);
            return await CreatePageResult(unitOfWork.Repository<Product>(), spec, specParams.PageIndex, specParams.PageSize); 
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await unitOfWork.Repository<Product>().GetByIdAsync(id);
            if(product == null) return NotFound();
            return product;
        }
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            unitOfWork.Repository<Product>().Add(product);
            if(await unitOfWork.Complete())
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
            unitOfWork.Repository<Product>().Update(product);

            if(await unitOfWork.Complete())
            {
                return NoContent();
            }
            return BadRequest("Problem in updating product");
            
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await unitOfWork.Repository<Product>().GetByIdAsync(id);
            if(product == null) return NotFound();
            unitOfWork.Repository<Product>().Remove(product);
            if(await unitOfWork.Complete())
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
            return Ok(await unitOfWork.Repository<Product>().ListAsync(spec));
        }
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            //TODO: implement method
            var spec = new TypeListSpecification();
            return Ok(await unitOfWork.Repository<Product>().ListAsync(spec));
        }
        private bool ProductExists(int id)
        {
            return unitOfWork.Repository<Product>().Exists(id);
        }
    }
}
