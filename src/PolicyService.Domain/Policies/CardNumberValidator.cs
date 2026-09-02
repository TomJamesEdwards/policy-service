namespace PolicyService.Domain.Policies;

internal static class CardNumberValidator
{
    private const int MinimumLength = 12;
    private const int MaximumLength = 19;

    internal static bool IsValid(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return false;
        }

        var sum = 0;
        var digitCount = 0;
        var shouldDouble = false;

        for (var index = cardNumber.Length - 1; index >= 0; index--)
        {
            var character = cardNumber[index];

            if (character is ' ' or '-')
            {
                continue;
            }

            if (!char.IsAsciiDigit(character))
            {
                return false;
            }

            var digit = character - '0';

            if (shouldDouble)
            {
                digit *= 2;

                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            digitCount++;
            shouldDouble = !shouldDouble;
        }

        return digitCount is >= MinimumLength and <= MaximumLength
            && sum % 10 == 0;
    }
}