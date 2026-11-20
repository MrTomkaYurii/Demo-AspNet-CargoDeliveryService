namespace BlogManagement.Controllers.v1;

using BlogManagement.Domain.PostAboutDrivers.Features;
using BlogManagement.Domain.PostAboutDrivers.Dtos;
using BlogManagement.Wrappers;
using BlogManagement.Domain;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using MediatR;

[ApiController]
[Route("api/postaboutdrivers")]
[ApiVersion("1.0")]
public sealed class PostAboutDriversController: ControllerBase
{
    private readonly IMediator _mediator;

    public PostAboutDriversController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all PostAboutDrivers.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetPostAboutDrivers")]
    public async Task<IActionResult> GetPostAboutDrivers([FromQuery] PostAboutDriverParametersDto postAboutDriverParametersDto)
    {
        var query = new GetPostAboutDriverList.Query(postAboutDriverParametersDto);
        var queryResponse = await _mediator.Send(query);

        var paginationMetadata = new
        {
            totalCount = queryResponse.TotalCount,
            pageSize = queryResponse.PageSize,
            currentPageSize = queryResponse.CurrentPageSize,
            currentStartIndex = queryResponse.CurrentStartIndex,
            currentEndIndex = queryResponse.CurrentEndIndex,
            pageNumber = queryResponse.PageNumber,
            totalPages = queryResponse.TotalPages,
            hasPrevious = queryResponse.HasPrevious,
            hasNext = queryResponse.HasNext
        };

        Response.Headers.Append("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));

        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a list of all PostAboutDrivers.
    /// </summary>
    [Authorize(Policy = "RequireUserRole")]
    [HttpGet("all", Name = "GetAllPostAboutDrivers")]
    public async Task<IActionResult> GetAllPostAboutDrivers()
    {
        var query = new GetAllPostAboutDrivers.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single PostAboutDriver by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{postAboutDriverId:guid}", Name = "GetPostAboutDriver")]
    public async Task<ActionResult<PostAboutDriverDto>> GetPostAboutDriver(Guid postAboutDriverId)
    {
        var query = new GetPostAboutDriver.Query(postAboutDriverId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new PostAboutDriver record.
    /// </summary>
    [Authorize(Policy = "RequireUserRole")]
    [HttpPost(Name = "AddPostAboutDriver")]
    public async Task<ActionResult<PostAboutDriverDto>> AddPostAboutDriver([FromBody]PostAboutDriverForCreationDto postAboutDriverForCreation)
    {
        var command = new AddPostAboutDriver.Command(postAboutDriverForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetPostAboutDriver",
            new { postAboutDriverId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing PostAboutDriver.
    /// </summary>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPut("{postAboutDriverId:guid}", Name = "UpdatePostAboutDriver")]
    public async Task<IActionResult> UpdatePostAboutDriver(Guid postAboutDriverId, PostAboutDriverForUpdateDto postAboutDriver)
    {
        var command = new UpdatePostAboutDriver.Command(postAboutDriverId, postAboutDriver);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing PostAboutDriver record.
    /// </summary>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpDelete("{postAboutDriverId:guid}", Name = "DeletePostAboutDriver")]
    public async Task<ActionResult> DeletePostAboutDriver(Guid postAboutDriverId)
    {
        var command = new DeletePostAboutDriver.Command(postAboutDriverId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
