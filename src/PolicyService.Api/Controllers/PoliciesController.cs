using Microsoft.AspNetCore.Mvc;
using PolicyService.Application.Policies.Cancel;
using PolicyService.Api.Contracts.Policies;
using PolicyService.Application.Policies.GetByReference;
using PolicyService.Application.Policies.Sell;

namespace PolicyService.Api.Controllers;

[ApiController]
[Route("policies")]
public sealed class PoliciesController : ControllerBase
{
    private readonly GetPolicyByReferenceHandler _getByReferenceHandler;
    private readonly SellPolicyHandler _sellHandler;
    private readonly CancelPolicyHandler _cancelHandler;

    public PoliciesController(
        GetPolicyByReferenceHandler getByReferenceHandler,
        SellPolicyHandler sellHandler,
        CancelPolicyHandler cancelPolicyHandler)
    {
        ArgumentNullException.ThrowIfNull(getByReferenceHandler);
        ArgumentNullException.ThrowIfNull(sellHandler);
        ArgumentNullException.ThrowIfNull(cancelPolicyHandler);

        _getByReferenceHandler = getByReferenceHandler;
        _sellHandler = sellHandler;
        _cancelHandler = cancelPolicyHandler;
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

    [HttpPost]
    [ProducesResponseType(
    typeof(PolicyResponse),
    StatusCodes.Status201Created)]
    [ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyResponse>> Sell(
    SellPolicyRequest request,
    CancellationToken cancellationToken)
    {
        var command = new SellPolicyCommand(
            Reference: request.Reference,
            Type: request.Type,
            StartDate: request.StartDate,
            Amount: request.Amount,
            AutoRenew: request.AutoRenew,
            Policyholders: request.Policyholders
                .Select(policyholder =>
                    new PolicyholderInput(
                        policyholder.FirstName,
                        policyholder.LastName,
                        policyholder.DateOfBirth))
                .ToArray(),
            Property: new InsuredPropertyInput(
                request.Property.AddressLine1,
                request.Property.AddressLine2,
                request.Property.AddressLine3,
                request.Property.Postcode),
            PaymentReference: request.Payment.Reference,
            PaymentType: request.Payment.Type,
            CardNumber: request.Payment.CardNumber);

        var result = await _sellHandler
            .Handle(command, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            var error = result.Errors.Single();

            var isConflict =
                error.Code == "policy.reference.conflict";

            return Problem(
                statusCode: isConflict
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status400BadRequest,
                title: isConflict
                    ? "Policy reference conflict"
                    : "Policy validation failed",
                detail: error.Message,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = error.Code
                });
        }

        var response = PolicyResponse.FromDomain(
            result.Value);

        return CreatedAtAction(
            nameof(GetByReference),
            new
            {
                reference = result.Value.Reference
            },
            response);
    }

    [HttpPost("{reference}/cancellation")]
    [ProducesResponseType(
    typeof(PolicyResponse),
    StatusCodes.Status200OK)]
    [ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
    [ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyResponse>> Cancel(
    string reference,
    CancelPolicyRequest request,
    CancellationToken cancellationToken)
    {
        var command = new CancelPolicyCommand(
            Reference: reference,
            RefundReference: request.RefundReference);

        var result = await _cancelHandler
            .Handle(command, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            var error = result.Errors.Single();

            var (statusCode, title) = error.Code switch
            {
                "policy.not_found" => (
                    StatusCodes.Status404NotFound,
                    "Policy not found"),

                "policy.already_cancelled" => (
                    StatusCodes.Status409Conflict,
                    "Policy cancellation conflict"),

                _ => (
                    StatusCodes.Status400BadRequest,
                    "Policy cancellation failed")
            };

            return Problem(
                statusCode: statusCode,
                title: title,
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