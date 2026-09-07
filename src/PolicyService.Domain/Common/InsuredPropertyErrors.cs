using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

internal static class InsuredPropertyErrors
{
    internal static readonly DomainError AddressLineOneRequired = new(
        Code: "property.address_line_1.required",
        Message: "Address line 1 is required.");

    internal static readonly DomainError PostcodeRequired = new(
        Code: "property.postcode.required",
        Message: "Postcode is required.");
}