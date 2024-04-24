using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Palindromer.Client.PalindromeClient;

namespace Palindromer.Client
{
    public class PalindromeClient : HttpClient
    {
        private string endpoint = "/palindromme";

        private readonly PalindromeClientConsoleLogger logger;

        private int RepeatInterval = 1000;

        public PalindromeClient(string url, PalindromeClientConsoleLogger logger) 
        {
            BaseAddress = new Uri(url);
            this.logger = logger;
        }

        public async Task<Result> SendPalindromeFilesAsync(IEnumerable<string> files)
        {
            Result result = new Result();

            // httpClient will repeat request and stop it right after 10 minutes 
            CancellationTokenSource c = new CancellationTokenSource(1000 * 60 * 60 * 10);

            List<Task> tasks = new List<Task>();
            foreach (string fn in files)
            {
                Task task = SendPalindromeFile(fn, result, c.Token);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            return result;
        }

        private async Task SendPalindromeFile(string fileName, Result result, CancellationToken cancellationToken)
        {
            string content = File.ReadAllText(fileName);

            while (true)
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri(endpoint, UriKind.Relative),
                    Method = HttpMethod.Get,
                    Content = new StringContent(content)
                };

                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();

                    if (!String.IsNullOrEmpty(responseString))
                    {
                        ApiServerResponse? responseObject = JsonSerializer.Deserialize<ApiServerResponse>(responseString);

                        if (responseObject != null)
                        {
                            OnRequestSuccess(fileName, result, responseObject);

                            return;
                        }
                    }
                }
                else if (cancellationToken.IsCancellationRequested)
                {
                    lock (loggerLock)
                    {
                        logger.LogOnRequestCancelled(fileName);
                    }

                    return;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    lock (loggerLock)
                    {
                        logger.LogOnTooManyRequests(fileName);
                    }
                }

                await Task.Delay(RepeatInterval);
            }
        }

        private void OnRequestSuccess(string sourceFile, Result result, ApiServerResponse responseObject)
        {
            if (responseObject.IsPalindrome)
            {
                Interlocked.Increment(ref result.FilesArePalindromes);

                lock (loggerLock)
                {
                    logger.LogOnPalindrome(sourceFile);

                    logger.Flush();
                }
            }
            else
            {
                lock (loggerLock)
                {
                    logger.LogOnNotPalindrome(sourceFile);

                    logger.Flush();
                }
            }

            Interlocked.Increment(ref result.TotalProceededFiles);
        }

        public class Result
        {
            public int FilesArePalindromes;
            public int TotalProceededFiles;
        }
    }

    public class ApiServerResponse
    {
        [JsonPropertyName("isPalindrome")]
        public bool IsPalindrome { get; set; }
    }
}
