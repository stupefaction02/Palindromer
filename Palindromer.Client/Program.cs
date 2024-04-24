using Palindromer.Client;

internal class Program
{
    private static string host = "http://localhost:5000";

    // put console or a file 
    private static PalindromeClientConsoleLogger logger = new PalindromeClientConsoleLogger();

    private static void Main(string[] args)
    {
        if (args.Length != 0)
        {
            string dirPath = args[0];

            if (!String.IsNullOrEmpty(dirPath) & Directory.Exists(dirPath))
            {
                // get only .txt files
                IEnumerable<string> files = Directory.EnumerateFiles(dirPath)
                    .Where(f => Path.GetExtension(f) == ".txt");

                Console.WriteLine($"Start processing directory {dirPath}.");

                PalindromeClient client = new PalindromeClient(host, logger);
                PalindromeClient.Result result = null;

                try
                {
                    // here starts async methods chain
                    result = client.SendPalindromeFilesAsync(files).GetAwaiter().GetResult();

                    Console.WriteLine($"Done. Procceded files: {result.TotalProceededFiles}, Palindromes: {result.FilesArePalindromes}");
                }
                catch (AggregateException exceptions)
                {
                    foreach (Exception ex in exceptions.InnerExceptions)
                    {
                        if (ex is HttpRequestException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("ERROR! ");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"{host} is down. Please try again later.");
                            break;
                        }
                    }
                }

                Console.WriteLine($"Press any key.");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR! ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Provide valid directory path!");
            return;
        }

        Console.WriteLine("Usage: .exe [DIRPATH]");
    }
}