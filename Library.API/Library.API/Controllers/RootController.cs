using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.DTOs;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api")]
    public class RootController : Controller
    {
        public IUrlHelper UrlHelper { get; }

        public RootController(IUrlHelper urlHelper)
        {
            UrlHelper = urlHelper;
        }


        // GET: api/<controller>
        [HttpGet(Name = "GetRoot")]
        public IActionResult Get([FromHeader(Name = "Accept")]string mediaType)
        {
            if (mediaType == "application/vnd.netXworks.hateoas+json")
            {
                var links = new List<LinkDto>();

                links.Add(new LinkDto(UrlHelper.Link("GetRoot", new { }), "self", "GET"));

                links.Add(new LinkDto(UrlHelper.Link("GetAuthors", new { }), "authors", "GET"));

                links.Add(new LinkDto(UrlHelper.Link("CreateAuthor", new { }), "create_author", "POST"));

                return Ok(links);
            }

            return NoContent();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
