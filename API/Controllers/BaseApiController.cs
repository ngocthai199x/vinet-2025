using API.RequestHelpers;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected async Task<ActionResult> CreatePageResult<T>(IGenericRepository<T> repo, ISpecification<T> spec, int pageIndex, int pageSize) where T: BaseEntity
        {
            var items = await repo.ListAsync(spec);
            var count = await repo.CountAsyn(spec);
            var pagination = new Pagination<T>(pageIndex, pageSize,count, items);
            return Ok(pagination);
        }
    }
}
