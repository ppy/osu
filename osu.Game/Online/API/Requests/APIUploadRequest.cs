// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public abstract class APIUploadRequest : APIRequest
    {
        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();
            request.UploadProgress += onUploadProgress;
            return request;
        }

        private void onUploadProgress(long current, long total)
        {
            Debug.Assert(API != null);
            API.Schedule(() => Progressed?.Invoke(current, total));
        }

        public event APIProgressHandler? Progressed;
    }
}
