using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

internal static class InsuredPropertyErrors
{
    internal static readonly DomainError AddressLineOneRequired = new(
        "property.address_line_1.required",
        "Address line 1 is required.");

    internal static readonly DomainError PostcodeRequired = new(
    "property.postcode.required",
    "Postcode is required.");
}