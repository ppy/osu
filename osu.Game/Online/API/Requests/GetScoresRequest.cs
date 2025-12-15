// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using osu.Framework.IO.Network;
using osu.Game.Extensions;

namespace osu.Game.Online.API.Requests
{
    public class GetScoresRequest : APIRequest<APIScoresCollection>, IEquatable<GetScoresRequest>
    {
        public const int DEFAULT_SCORES_PER_REQUEST = 50;
        public const int MAX_SCORES_PER_REQUEST = 100;

        public int ScoresRequested { get; }

        private readonly IBeatmapInfo beatmapInfo;
        private readonly BeatmapLeaderboardScope scope;
        private readonly IRulesetInfo ruleset;
        private readonly IEnumerable<IMod> mods;

        public GetScoresRequest(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset, BeatmapLeaderboardScope scope = BeatmapLeaderboardScope.Global, IEnumerable<IMod>? mods = null)
        {
            if (beatmapInfo.OnlineID <= 0)
                throw new InvalidOperationException($"Cannot lookup a beatmap's scores without having a populated {nameof(IBeatmapInfo.OnlineID)}.");

            if (scope == BeatmapLeaderboardScope.Local)
                throw new InvalidOperationException("Should not attempt to request online scores for a local scoped leaderboard");

            this.beatmapInfo = beatmapInfo;
            this.scope = scope;
            this.ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            this.mods = mods ?? Array.Empty<IMod>();

            ScoresRequested = this.scope.RequiresSupporter(this.mods.Any()) ? MAX_SCORES_PER_REQUEST : DEFAULT_SCORES_PER_REQUEST;
        }

        protected override string Target => $@"beatmaps/{beatmapInfo.OnlineID}/scores";

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter(@"type", scope.ToString().ToLowerInvariant());
            req.AddParameter(@"mode", ruleset.ShortName);

            foreach (var mod in mods)
                req.AddParameter(@"mods[]", mod.Acronym);

            req.AddParameter(@"limit", ScoresRequested.ToString(CultureInfo.InvariantCulture));
            return req;
        }

        public bool Equals(GetScoresRequest? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return beatmapInfo.Equals(other.beatmapInfo)
                   && scope == other.scope
                   && ruleset.Equals(other.ruleset)
                   && mods.SequenceEqual(other.mods);
        }
    }
}
