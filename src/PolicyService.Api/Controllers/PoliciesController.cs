using Microsoft.AspNetCore.Mvc;
using PolicyService.Api.Contracts.Policies;
using PolicyService.Application.Policies.GetByReference;

namespace PolicyService.Api.Controllers;

[ApiController]
[Route("policies")]
public sealed class PoliciesController : ControllerBase
{
    private readonly GetPolicyByReferenceHandler _getByReferenceHandler;

    public PoliciesController(
        GetPolicyByReferenceHandler getByReferenceHandler)
    {
        ArgumentNullException.ThrowIfNull(getByReferenceHandler);

        _getByReferenceHandler = getByReferenceHandler;
    }

    [HttpGet("{reference}")]
    [ProducesResponseType(
        typeof(PolicyResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyResponse>> GetByReference(
        string reference,
        CancellationToken cancellationToken)
    {
        var query = new GetPolicyByReferenceQuery(reference);

        var result = await _getByReferenceHandler
            .Handle(query, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            var error = result.Errors.Single();

            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Policy not found",
                detail: error.Message,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = error.Code
                });
        }

        return Ok(
            PolicyResponse.FromDomain(result.Value));
    }
}