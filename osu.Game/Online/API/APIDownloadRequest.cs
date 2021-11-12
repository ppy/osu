// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    public abstract class APIDownloadRequest : APIRequest
    {
        private string filename;

        /// <summary>
        /// Used to set the extension of the file returned by this request.
        /// </summary>
        protected virtual string FileExtension { get; } = @".tmp";

        protected APIDownloadRequest()
        {
            base.Success += () => Success?.Invoke(filename);
        }

        protected override WebRequest CreateWebRequest()
        {
            string file = Path.GetTempFileName();

            File.Move(file, filename = Path.ChangeExtension(file, FileExtension));

            var request = new FileWebRequest(filename, Uri);
            request.DownloadProgress += request_Progress;
            return request;
        }

        private void request_Progress(long current, long total) => API.Schedule(() => Progressed?.Invoke(current, total));

        protected void TriggerSuccess(string filename)
        {
            if (this.filename != null)
                throw new InvalidOperationException("Attempted to trigger success more than once");

            this.filename = filename;

            TriggerSuccess();
        }

        public event APIProgressHandler Progressed;

        public new event APISuccessHandler<string> Success;
    }
}
