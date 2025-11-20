using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    private readonly ILogger<ValuesController> _logger;

    public ValuesController(ILogger<ValuesController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<string>> Get()
    {
        _logger.LogInformation("GET api/values called");
        return Ok(new[] { "value1", "value2", "value3" });
    }

    [HttpGet("{id}")]
    public ActionResult<string> Get(int id)
    {
        _logger.LogInformation("GET api/values/{Id} called", id);
        return Ok($"value{id}");
    }

    [HttpPost]
    public ActionResult<string> Post([FromBody] string value)
    {
        _logger.LogInformation("POST api/values called with value: {Value}", value);
        return Created($"/api/values/1", value);
    }

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] string value)
    {
        _logger.LogInformation("PUT api/values/{Id} called with value: {Value}", id, value);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("DELETE api/values/{Id} called", id);
        return NoContent();
    }
}
