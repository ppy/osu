// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
    public partial class TopLocalRank : CompositeDrawable
    {
        private readonly BeatmapInfo beatmapInfo;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IDisposable? scoreSubscription;

        private readonly UpdateableRank updateable;

        public ScoreRank? DisplayedRank => updateable.Rank;

        public TopLocalRank(BeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;

            AutoSizeAxes = Axes.Both;

            InternalChild = updateable = new UpdateableRank
            {
                Size = new Vector2(40, 20),
                Alpha = 0,
            };
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
                                 + $" && {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.Hash)} == {nameof(ScoreInfo.BeatmapHash)}"
                                 + $" && {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $2"
                                 + $" && {nameof(ScoreInfo.DeletePending)} == false", api.LocalUser.Value.Id, beatmapInfo.ID, ruleset.Value.ShortName),
                    localScoresChanged);
            }, true);

            void localScoresChanged(IRealmCollection<ScoreInfo> sender, ChangeSet? changes)
            {
                // This subscription may fire from changes to linked beatmaps, which we don't care about.
                // It's currently not possible for a score to be modified after insertion, so we can safely ignore callbacks with only modifications.
                if (changes?.HasCollectionChanges() == false)
                    return;

                ScoreInfo? topScore = sender.MaxBy(info => (info.TotalScore, -info.Date.UtcDateTime.Ticks));
                updateable.Rank = topScore?.Rank;
                updateable.Alpha = topScore != null ? 1 : 0;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            scoreSubscription?.Dispose();
        }
    }
}
