using System.Text.RegularExpressions;
using UnityEngine;

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

namespace Luddy.Children
{
    public class Children
    {
        public static void ClearChildren(GameObject parent)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                parent.transform.GetChild(i);
            }
        }

        public static void ClearChildren(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                parent.GetChild(i);
            }
        }
    }
}
