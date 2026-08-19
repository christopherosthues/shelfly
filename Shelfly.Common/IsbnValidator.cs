namespace Shelfly.Common;

public static class IsbnValidator
{
    public static bool IsValid(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return false;
        }

        string digits = isbn.Replace("-", "").Replace(" ", "");

        if (digits.Length == 10)
        {
            return ValidateIsbn10(digits);
        }

        if (digits.Length == 13)
        {
            return ValidateIsbn13(digits);
        }

        return false;
    }

    private static bool ValidateIsbn10(string digits)
    {
        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            char c = digits[i];

            if (c >= '0' && c <= '9')
            {
                sum += (c - '0') * (10 - i);
            }
            else if (i == 9 && (c == 'X' || c == 'x'))
            {
                sum += 10;
            }
            else
            {
                return false;
            }
        }

        char checkChar = digits[9];
        int checkValue = checkChar >= '0' && checkChar <= '9' ? (checkChar - '0') :
                         checkChar == 'X' || checkChar == 'x' ? 10 : -1;

        return checkValue >= 0 && sum % 11 == checkValue;
    }

    private static bool ValidateIsbn13(string digits)
    {
        int sum = 0;
        int multiplier = 1;

        for (int i = 0; i < 12; i++)
        {
            char c = digits[i];

            if (c >= '0' && c <= '9')
            {
                sum += (c - '0') * multiplier;
                multiplier = multiplier == 1 ? 3 : 1;
            }
            else
            {
                return false;
            }
        }

        char checkChar = digits[12];

        if (checkChar >= '0' && checkChar <= '9')
        {
            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (checkChar - '0');
        }

        return false;
    }
}
