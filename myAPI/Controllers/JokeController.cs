using Microsoft.AspNetCore.Mvc;
using System.Web.Http.Cors;

namespace myAPI.Controllers
{
    [EnableCors("*","*","*")]
    [ApiController]
    [Route("[controller]")]
    public class JokeController : Controller
    {
        private IConfiguration _config;
        public JokeController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                var req = new HttpRequestMessage();
                req.RequestUri = new Uri(_config["JokeURI"]);
                req.Headers.Add("Accept", "application/json");
                var response = await client.SendAsync(req);

                var data = await response.Content.ReadAsStringAsync();
                var rc = $"{{\"JokeURI\" : \"{_config["JokeURI"]}\", \"result\": {data}}}";

                return Ok(rc);
            }
        }
    }
}
