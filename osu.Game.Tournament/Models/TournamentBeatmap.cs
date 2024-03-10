// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Tournament.Models
{
    public class TournamentBeatmap : IBeatmapInfo, IBeatmapSetOnlineInfo
    {
        public int OnlineID { get; set; }

        public string DifficultyName { get; set; } = string.Empty;

        public double BPM { get; set; }

        public double Length { get; set; }

        public double StarRating { get; set; }

        public int EndTimeObjectCount { get; set; }

        public int TotalObjectCount { get; set; }

        public IBeatmapMetadataInfo Metadata { get; set; } = new BeatmapMetadata();

        public IBeatmapDifficultyInfo Difficulty { get; set; } = new BeatmapDifficulty();

        public BeatmapSetOnlineCovers Covers { get; set; }

        public TournamentBeatmap()
        {
        }

        public TournamentBeatmap(APIBeatmap beatmap)
        {
            OnlineID = beatmap.OnlineID;
            DifficultyName = beatmap.DifficultyName;
            BPM = beatmap.BPM;
            Length = beatmap.Length;
            StarRating = beatmap.StarRating;
            Metadata = beatmap.Metadata;
            Difficulty = beatmap.Difficulty;
            Covers = beatmap.BeatmapSet?.Covers ?? new BeatmapSetOnlineCovers();
            EndTimeObjectCount = beatmap.EndTimeObjectCount;
            TotalObjectCount = beatmap.TotalObjectCount;
        }

        public bool Equals(IBeatmapInfo? other) => other is TournamentBeatmap b && this.MatchesOnlineID(b);

        #region IBeatmapInfo/IBeatmapSetOnlineInfo explicit implementation

        IBeatmapSetInfo IBeatmapInfo.BeatmapSet => throw new NotImplementedException();

        string IBeatmapSetOnlineInfo.Preview => throw new NotImplementedException();

        double IBeatmapSetOnlineInfo.BPM => throw new NotImplementedException();

        int IBeatmapSetOnlineInfo.PlayCount => throw new NotImplementedException();

        int IBeatmapSetOnlineInfo.FavouriteCount => throw new NotImplementedException();

        bool IBeatmapSetOnlineInfo.HasFavourited => throw new NotImplementedException();

        BeatmapSetOnlineAvailability IBeatmapSetOnlineInfo.Availability => throw new NotImplementedException();

        BeatmapSetOnlineGenre IBeatmapSetOnlineInfo.Genre => throw new NotImplementedException();

        BeatmapSetOnlineLanguage IBeatmapSetOnlineInfo.Language => throw new NotImplementedException();

        int? IBeatmapSetOnlineInfo.TrackId => throw new NotImplementedException();

        int[] IBeatmapSetOnlineInfo.Ratings => throw new NotImplementedException();

        BeatmapSetHypeStatus IBeatmapSetOnlineInfo.HypeStatus => throw new NotImplementedException();

        BeatmapSetNominationStatus IBeatmapSetOnlineInfo.NominationStatus => throw new NotImplementedException();

        string IBeatmapInfo.Hash => throw new NotImplementedException();

        string IBeatmapInfo.MD5Hash => throw new NotImplementedException();

        IRulesetInfo IBeatmapInfo.Ruleset => throw new NotImplementedException();

        DateTimeOffset IBeatmapSetOnlineInfo.Submitted => throw new NotImplementedException();

        DateTimeOffset? IBeatmapSetOnlineInfo.Ranked => throw new NotImplementedException();

        DateTimeOffset? IBeatmapSetOnlineInfo.LastUpdated => throw new NotImplementedException();

        BeatmapOnlineStatus IBeatmapSetOnlineInfo.Status => throw new NotImplementedException();

        bool IBeatmapSetOnlineInfo.HasExplicitContent => throw new NotImplementedException();

        bool IBeatmapSetOnlineInfo.HasVideo => throw new NotImplementedException();

        bool IBeatmapSetOnlineInfo.HasStoryboard => throw new NotImplementedException();

        #endregion
    }
}
