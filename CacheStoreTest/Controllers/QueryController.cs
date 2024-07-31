using Microsoft.AspNetCore.Mvc;

namespace CacheStoreTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : Controller
    {
        private readonly DatabaseQueryService _databaseQueryService;

        public QueryController(DatabaseQueryService databaseQueryService)
        {
            _databaseQueryService = databaseQueryService;
        }

        [HttpGet("MeasureQueryTime")]
        public async Task<IActionResult> MeasureQueryTime()
        {
            string result = await _databaseQueryService.MeasureQueryTimeAsync();
            return Ok(result);
        }

        [HttpGet("MeasureQueryTimeWithRedis")]
        public async Task<IActionResult> MeasureQueryTimeWithRedis()
        {
            string result = await _databaseQueryService.MeasureQueryTimeWithRedisCacheAsync();
            return Ok(result);
        }

        [HttpGet("MeasureQueryTimeWithGarnet")]
        public async Task<IActionResult> MeasureQueryTimeWithGarnet()
        {
            string result = await _databaseQueryService.MeasureQueryTimeWithGarnetCacheAsync();
            return Ok(result);
        }

        [HttpGet("GetAllQueryTime")]
        public async Task<IActionResult> GetAllQueryTime()
        {
            List<string> result = await _databaseQueryService.GetAllQueryTimeAsync();
            return Ok(result);
        }
    }
}
