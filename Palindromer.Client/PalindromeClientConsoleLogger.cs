namespace Palindromer.Client
{
    public class PalindromeClientConsoleLogger
    {
        public void LogSuccess(string sourcePalFile)
        {
            Console.Write($"{sourcePalFile} ---------- ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"YES\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogFailure(string sourcePalFile)
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
    }
}
