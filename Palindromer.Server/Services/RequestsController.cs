namespace Palindromer.Server.Services
{
    public class RequestsController
    {
        public int MaxRequestsCount { get; set; }
        public long RequestsCount = 0;
        private object requestsCountLocker;

        public object Locker { get; set; }

        public RequestsController(int maxRequestsCount)
        {
            this.MaxRequestsCount = maxRequestsCount;
        }

        public bool CheckLimit()
        {
            bool ret = false;

            lock (requestsCountLocker)
            {
                long requestsCount = this.RequestsCount;
                //Console.WriteLine($"requestsCount: {requestsCount}");
                ret = (requestsCount + 1) <= MaxRequestsCount;
            }

            return ret;
        }
    }
}
