// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osuTK;
using Realms;

namespace osu.Game.Screens.Select.Carousel
{
    public class TopLocalRank : UpdateableRank
    {
        private readonly BeatmapInfo beatmapInfo;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        private IDisposable scoreSubscription;

        public TopLocalRank(BeatmapInfo beatmapInfo)
            : base(null)
        {
            this.beatmapInfo = beatmapInfo;

            Size = new Vector2(40, 20);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ =>
            {
                scoreSubscription?.Dispose();
                scoreSubscription = realm.RegisterForNotifications(r =>
                        r.All<ScoreInfo>()
                         .Filter($"{nameof(ScoreInfo.User)}.{nameof(RealmUser.OnlineID)} == $0"
                                 + $" && {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} == $1"
                                 + $" && {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $2"
                                 + $" && {nameof(ScoreInfo.DeletePending)} == false", api.LocalUser.Value.Id, beatmapInfo.ID, ruleset.Value.ShortName)
                         .OrderByDescending(s => s.TotalScore),
                    (items, changes, ___) =>
                    {
                        Rank = items.FirstOrDefault()?.Rank;
                        // Required since presence is changed via IsPresent override
                        Invalidate(Invalidation.Presence);
                    });
            }, true);
        }

        public override bool IsPresent => base.IsPresent && Rank != null;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            scoreSubscription?.Dispose();
        }
    }
}
