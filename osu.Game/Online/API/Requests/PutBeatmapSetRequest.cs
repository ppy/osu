// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.IO.Network;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class PutBeatmapSetRequest : APIRequest<PutBeatmapSetResponse>
    {
        protected override string Uri
        {
            get
            {
                // can be removed once the service has been successfully deployed to production
                if (API!.Endpoints.BeatmapSubmissionServiceUrl == null)
                    throw new NotSupportedException("Beatmap submission not supported in this configuration!");

                return $@"{API!.Endpoints.BeatmapSubmissionServiceUrl}/beatmapsets";
            }
        }

        protected override string Target => throw new NotSupportedException();

        [JsonProperty("beatmapset_id")]
        public uint? BeatmapSetID { get; init; }

        [JsonProperty("beatmaps_to_create")]
        public uint BeatmapsToCreate { get; init; }

        [JsonProperty("beatmaps_to_keep")]
        public uint[] BeatmapsToKeep { get; init; } = [];

        [JsonProperty("target")]
        public BeatmapSubmissionTarget SubmissionTarget { get; init; }

        private PutBeatmapSetRequest()
        {
        }

        public static PutBeatmapSetRequest CreateNew(uint beatmapCount, BeatmapSubmissionTarget target) => new PutBeatmapSetRequest
        {
            BeatmapsToCreate = beatmapCount,
            SubmissionTarget = target,
        };

        public static PutBeatmapSetRequest UpdateExisting(uint beatmapSetId, IEnumerable<uint> beatmapsToKeep, uint beatmapsToCreate, BeatmapSubmissionTarget target) => new PutBeatmapSetRequest
        {
            BeatmapSetID = beatmapSetId,
            BeatmapsToKeep = beatmapsToKeep.ToArray(),
            BeatmapsToCreate = beatmapsToCreate,
            SubmissionTarget = target,
        };

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            req.ContentType = @"application/json";
            req.AddRaw(JsonConvert.SerializeObject(this));
            return req;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BeatmapSubmissionTarget
    {
        [LocalisableDescription(typeof(BeatmapSubmissionStrings), nameof(BeatmapSubmissionStrings.BeatmapSubmissionTargetWIP))]
        WIP,

        [LocalisableDescription(typeof(BeatmapSubmissionStrings), nameof(BeatmapSubmissionStrings.BeatmapSubmissionTargetPending))]
        Pending,
    }
}
