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
    public class RetryHandler : DelegatingHandler
    {
        public int Delay { get; set; }

        public RetryHandler(int delay, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            Delay = delay;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
                }

                //Console.WriteLine("Retrying.");
                await Task.Delay(Delay);
            }
        }
    }

    public class PalindromeClient : HttpClient
    {
        private string endpoint = "/palindromme";

        private readonly PalindromeClientConsoleLogger logger;

        public PalindromeClient(string url, PalindromeClientConsoleLogger logger) 
            : base(new RetryHandler(delay: 1000, new HttpClientHandler()))
        {
            BaseAddress = new Uri(url);
            this.logger = logger;
        }

        private Result result;

        public async Task<Result> SendPalindromeFilesAsync(IEnumerable<string> files)
        {
            result = new Result();

            // httpClient will repeat request and stop it right after 10 minutes 
            CancellationTokenSource c = new CancellationTokenSource(1000 * 60 * 60 * 10);

            List<Task> tasks = new List<Task>();
            foreach (string fn in files)
            {
                Task task = SendPalindromeFile(fn, c.Token);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            //int filesPerTask = 2;

            //List<string> lst = files.ToList();

            //int tasksCount = (lst.Count / filesPerTask) + 1;

            //CancellationTokenSource c = new CancellationTokenSource(5000);

            //ParallelOptions po = new ParallelOptions { CancellationToken = c.Token };
            //ParallelLoopResult r = Parallel.For(0, tasksCount, async (x) => await SendPalindromeFilesRangeAsync(x, filesPerTask, lst, default));
            //if (!r.IsCompleted)
            //{

            //}

            return result;
        }

        private object loggerLock = new object();
        private async Task SendPalindromeFilesRangeAsync(int index, int filesPerTask, List<string> files, CancellationToken token)
        {
            int start = index * filesPerTask;
            int count = (start + filesPerTask >= files.Count) ? (files.Count - start) : filesPerTask;
            
            for (int i = start; i < start + count; i++)
            {
                string fileName = files[i];

                await SendPalindromeFile(fileName, token);
            }
        }

        private async Task SendPalindromeFile(string fileName, CancellationToken token)
        {
            string content = File.ReadAllText(fileName);

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri(endpoint, UriKind.Relative),
                Method = HttpMethod.Get,
                Content = new StringContent(content)
            };

            HttpResponseMessage response = await SendAsync(request, token);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();

                if (!String.IsNullOrEmpty(responseString))
                {
                    ApiServerResponse? responseObject = JsonSerializer.Deserialize<ApiServerResponse>(responseString);

                    if (responseObject != null)
                    {
                        OnRequestSuccess(fileName, responseObject);

                        return;
                    }
                }
            }

            OnRequestFailure(fileName, response);
        }

        private void OnRequestSuccess(string sourceFile, ApiServerResponse responseObject)
        {
            if (responseObject.IsPalindrome)
            {
                Interlocked.Increment(ref result.FilesArePalindromes);

                lock (loggerLock)
                {
                    logger.LogSuccess(sourceFile);

                    logger.Flush();
                }
            }
            else
            {
                lock (loggerLock)
                {
                    logger.LogFailure(sourceFile);

                    logger.Flush();
                }
            }

            Interlocked.Increment(ref result.TotalProceededFiles);
        }

        private void OnRequestFailure(string fileName, HttpResponseMessage response)
        {
            //logger.LogFailure(sourceFile);
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
