using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    public abstract class APIDownloadRequest : APIRequest
    {
        protected override WebRequest CreateWebRequest()
        {
            var request = new WebRequest(Uri);
            request.DownloadProgress += request_Progress;
            return request;
        }

        private void request_Progress(long current, long total) => API.Scheduler.Add(delegate { Progress?.Invoke(current, total); });

        protected APIDownloadRequest()
        {
            base.Success += onSuccess;
        }

        private void onSuccess()
        {
            Success?.Invoke(WebRequest.ResponseData);
        }

        public event APIProgressHandler Progress;

        public new event APISuccessHandler<byte[]> Success;
    }
}