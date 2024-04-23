using Palindromer.Client;

internal class Program
{
    private static string host = "http://localhost:5000";

    // put console or a file 
    private static PalindromeClientConsoleLogger logger = new PalindromeClientConsoleLogger();

    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            string dirPath = "C:\\Users\\Ivan\\Desktop\\1";// args[0];

            if (!String.IsNullOrEmpty(dirPath) & Directory.Exists(dirPath))
            {
                IEnumerable<string> files = Directory.EnumerateFiles(dirPath);

                Console.WriteLine($"Start processing directory {dirPath}.");

                PalindromeClient client = new PalindromeClient(host, logger);
                PalindromeClient.Result result = null;

                try
                {
                    // here starts async methods chain
                    result = client.SendPalindromeFilesAsync(files).GetAwaiter().GetResult();

                    Console.WriteLine($"Done. Procceded files: {result.TotalProceededFiles}, Palindromes: {result.FilesArePalindromes}");
                }
                catch (HttpRequestException rex)
                {
                    Console.WriteLine($"");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
                

                Console.WriteLine($"Press any key.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Provide valid directory path!");
        }

        Console.WriteLine("Usage: .exe [DIRPATH]");
    }
}