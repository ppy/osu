// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    public abstract class APIDownloadRequest : APIRequest
    {
        private string filename;

        protected override WebRequest CreateWebRequest()
        {
            var request = new FileWebRequest(filename = Path.GetTempFileName(), Uri);
            request.DownloadProgress += request_Progress;
            return request;
        }

        private void request_Progress(long current, long total) => API.Schedule(() => Progressed?.Invoke(current, total));

        protected APIDownloadRequest()
        {
            base.Success += onSuccess;
        }

        private void onSuccess()
        {
            Success?.Invoke(filename);
        }

        public event APIProgressHandler Progressed;

        public new event APISuccessHandler<string> Success;
    }
}
