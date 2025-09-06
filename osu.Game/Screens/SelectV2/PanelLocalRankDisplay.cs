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
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osuTK;
using Realms;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelLocalRankDisplay : CompositeDrawable
    {
        private BeatmapInfo? beatmap;

        public BeatmapInfo? Beatmap
        {
            get => beatmap;
            set
            {
                beatmap = value;

                if (IsLoaded)
                    updateSubscription();
            }
        }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IDisposable? scoreSubscription;

        private readonly UpdateableRank updateable;

        public bool HasRank => updateable.Rank != null;

        public PanelLocalRankDisplay(BeatmapInfo? beatmap = null)
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = updateable = new UpdateableRank(animate: false)
            {
                Size = new Vector2(40, 20),
                Alpha = 0,
            };

            Beatmap = beatmap;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ => updateSubscription(), true);
        }

        private void updateSubscription()
        {
            scoreSubscription?.Dispose();

            if (beatmap == null)
                return;

            scoreSubscription = realm.RegisterForNotifications(r =>
                    r.GetAllLocalScoresForUser(api.LocalUser.Value.Id)
                     .Filter($@"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} == $0"
                             + $" && {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $1", beatmap.ID, ruleset.Value.ShortName),
                localScoresChanged);
        }

        private void localScoresChanged(IRealmCollection<ScoreInfo> sender, ChangeSet? changes)
        {
            // This subscription may fire from changes to linked beatmaps, which we don't care about.
            // It's currently not possible for a score to be modified after insertion, so we can safely ignore callbacks with only modifications.
            if (changes?.HasCollectionChanges() == false)
                return;

            ScoreInfo? topScore = sender.MaxBy(info => (info.TotalScore, -info.Date.UtcDateTime.Ticks));
            updateable.Rank = topScore?.Rank;
            updateable.Alpha = topScore != null ? 1 : 0;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            scoreSubscription?.Dispose();
        }
    }
}
