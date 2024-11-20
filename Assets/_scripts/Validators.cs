using System.Text.RegularExpressions;

namespace Luddy.Validators
{
    public class Validators
    {
        public static bool ValidateInputs(string email, string password)
        {
            return !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password);
        }

        public static bool IsValidEmail(string email)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }

        public static bool IsValidName(string name)
        {
            string namePattern = @"^[a-zA-Z0-9._%+-]{3,}$";
            return Regex.IsMatch(name, namePattern);
        }
    }
}
