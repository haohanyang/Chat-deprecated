using Microsoft.AspNetCore.Mvc;

namespace Chat.Areas.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    // GET: api/Auth
    [HttpGet]
    public IEnumerable<string> Index()
    {
        return new[] { "value1", "value2" };
    }

    // GET: api/Auth/5
    [HttpGet("{id}", Name = "Get")]
    public string Get(int id)
    {
        return "value";
    }

    // POST: api/Auth
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT: api/Auth/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE: api/Auth/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}