using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace myApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string JokeURI { get; set; }
        public string Joke { get; set; }
        public IConfiguration _config { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            JokeURI = _config["JokeURI"];
            Joke = "Loading...";
        }

        public void OnGet()
        {
            bool first = true;
            try
            {
                //if (first)
                //{
                //    first = false;
                //    return;
                //}

                using (var client = new HttpClient())
                {
                    var req = new HttpRequestMessage();
                    req.RequestUri = new Uri(JokeURI);
                    //req.Headers.Add("Accept", "application/json");
                    var response = client.SendAsync(req).Result;

                    var data = response.Content.ReadAsStringAsync().Result;

                    dynamic json = JToken.Parse(data);

                    Joke = $"{json.result.joke}";
                    JokeURI = json.JokeURI;
                }

            }
            catch (Exception ex)
            {
                Joke = ex.Message;
            }
        }
    }
}
