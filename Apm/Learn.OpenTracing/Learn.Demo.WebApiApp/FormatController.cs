using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Learn.Demo.WebApiApp
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormatController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Hello";
        }

        [HttpGet("{helloTo}", Name = "GetFormat")]
        public string Get(string helloTo)
        {
            var formatString = $"Hello, {helloTo}";
            return formatString;
        }
    }
}
