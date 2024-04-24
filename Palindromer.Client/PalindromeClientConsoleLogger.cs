

namespace Palindromer.Client
{
    public class PalindromeClientConsoleLogger
    {
        public void LogOnPalindrome(string sourcePalFile)
        {
            Console.Write($"{sourcePalFile} ---------- ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"YES\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogOnNotPalindrome(string sourcePalFile)
        {
            Console.Write($"{sourcePalFile} ---------- ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"NO\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void Flush()
        {
            Console.Out.Flush();
        }

        public void LogOnRequestCancelled(string fileName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Aborting request. Give time for one request was exceeded. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"file: {fileName}");
        }

        public void LogOnTooManyRequests(string fileName)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("Too Many Requests 413. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"file: {fileName}. Retrying...\n");
        }
    }
}
