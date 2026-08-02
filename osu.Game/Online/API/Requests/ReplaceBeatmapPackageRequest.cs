// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.Http;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class ReplaceBeatmapPackageRequest : APIUploadRequest
    {
        protected override string Uri
        {
            get
            {
                // can be removed once the service has been successfully deployed to production
                if (API!.Endpoints.BeatmapSubmissionServiceUrl == null)
                    throw new NotSupportedException("Beatmap submission not supported in this configuration!");

                return $@"{API!.Endpoints.BeatmapSubmissionServiceUrl}/beatmapsets/{BeatmapSetID}";
            }
        }

        protected override string Target => throw new NotSupportedException();

        public uint BeatmapSetID { get; }

        private readonly byte[] oszPackage;

        public ReplaceBeatmapPackageRequest(uint beatmapSetID, byte[] oszPackage)
        {
            this.oszPackage = oszPackage;
            BeatmapSetID = beatmapSetID;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();
            request.AddFile(@"beatmapArchive", oszPackage);
            request.Method = HttpMethod.Put;
            request.Timeout = 600_000;
            return request;
        }
    }
}
