using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ApiFunction.Util;

public static class PasswordUtil
{
    public static bool Validate(string password)
    {
        if (password.Length < 6)
            return false;

        if (!Regex.IsMatch(password, @"[$^*.\[\]{}()?\-""!@#%&/\\,><':;|_~`=+]+"))
            return false;

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return false;

        if (!Regex.IsMatch(password, @"[a-z]"))
            return false;

        if (!Regex.IsMatch(password, @"\d"))
            return false;

        return true;
    }

    public static string GenerateRandomPassword()
    {
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numberChars = "0123456789";
        const string specialChars = "!@#$%^&*()";

        var passwordChars = new List<char>();
        int length = RandomNumberGenerator.GetInt32(8, 13); // Generate a password of length 8-12

        // Ensure the password has at least one of each required character type
        passwordChars.Add(lowerChars[RandomNumberGenerator.GetInt32(lowerChars.Length)]);
        passwordChars.Add(upperChars[RandomNumberGenerator.GetInt32(upperChars.Length)]);
        passwordChars.Add(numberChars[RandomNumberGenerator.GetInt32(numberChars.Length)]);
        passwordChars.Add(specialChars[RandomNumberGenerator.GetInt32(specialChars.Length)]);

        var allChars = lowerChars + upperChars + numberChars + specialChars;

        // Fill the rest of the password with random characters from all sets
        for (int i = passwordChars.Count; i < length; i++)
        {
            passwordChars.Add(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
        }

        // Shuffle the characters to make the password less predictable
        for (int i = 0; i < passwordChars.Count; i++)
        {
            int j = RandomNumberGenerator.GetInt32(i, passwordChars.Count);
            (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
        }

        return new string(passwordChars.ToArray());
    }

}