namespace Palindromer.Server.Services
{
    /// <summary>
    /// Checks if a given string a palindrome
    /// </summary>
    public class PalindromeService
    {
        public bool CheckPalindrome(string str)
        {
            str = str.Trim();

            int len = str.Length;
            bool isPalindrome = true;

            for (int i = 0; i < len / 2; i++)
            {
                if (str[i] != str[len - 1 - i])
                {
                    isPalindrome = false;
                    break;
                }
            }

            return isPalindrome;
        }
    }
}
