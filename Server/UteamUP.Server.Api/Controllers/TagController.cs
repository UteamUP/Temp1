using UteamUP.Server.Repository.GenericRepository.Interfaces;

namespace UteamUP.Server.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TagController : ControllerBase
{
    private readonly ILogger<PlanController> _logger;
    private readonly ITagRepository _tag;
    private readonly IMUserRepository _user;
    private readonly IRepository<Tag> _tagRepository;

    
    public TagController(ILogger<PlanController> logger, ITagRepository tag, IMUserRepository user, IRepository<Tag> tagRepository)
    {
        _logger = logger;
        _tag = tag;
        _user = user;
        _tagRepository = tagRepository;
    }
    
    // Get Tag by name
    [HttpGet("name/{name}/tenant/{tenantId}")]
    public async Task<IActionResult> GetTagByNameAsync(string name, int tenantId)
    {
        Console.WriteLine("Trying to find tag by name: " + name + " and tenant id: " + tenantId);
        // Get the tag
        return Ok(await _tag.GetTagByNameAndTenantIdAsync(name, tenantId));
    }
    
    // Get Tag by name and tenant id
    [HttpGet("{name}/tenant/{tenantId}")]
    public async Task<IActionResult> GetTagByNameAndTenantIdAsync(string name, int tenantId)
    {
        // Get the tag
        return Ok(await _tag.GetTagByNameAndTenantIdAsync(name, tenantId));
    }
    
    // Get all tags by tenant id
    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetAllTagsByTenantIdAsync(int tenantId, [FromQuery] string filter = "", [FromQuery] string sort = "Id asc", [FromQuery] int skip = 0, [FromQuery] int top = 10)
    {
        Console.WriteLine("The filter is : " + filter);
        var result = await _tag.GetAllTagsByTenantIdAsync(tenantId, filter, sort, skip, top);
        if (result == null)
        {
            _logger.Log(LogLevel.Error, $"{nameof(GetAllTagsByTenantIdAsync)}: Something went wrong while retrieving the tags");
            return BadRequest("Something went wrong while retrieving the tags, please review logs for more information");
        }
        return Ok(result);
    }

    // Create the tag
    [HttpPost]
    public async Task<IActionResult> CreateTagAsync([FromBody] Tag tag)
    {
        var result = await _tag.CreateAsync(tag);
        if (string.IsNullOrWhiteSpace(result.Name)) return Ok(new Tag());
        return Ok(result);
    }
    
    // Get tag by name and location name
    [HttpGet("{name}/{locationName}")]
    public async Task<IActionResult> GetTagByNameAndLocationNameAsync(string name, string locationName)
    {
        // Get the tag
        return Ok(await _tag.GetTagByNameAndLocationNameAsync(name, locationName));
    }
    
    
}