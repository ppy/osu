// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Online.API.Requests;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Sections
{
    public partial class RanksSection : ProfileSection
    {
        public override LocalisableString Title => UsersStrings.ShowExtraTopRanksTitle;

        public override string Identifier => @"top_ranks";

        private readonly PaginatedScoreContainer pinnedScores;

        [Resolved]
        private ScorePinningManager? pinningManager { get; set; }

        public RanksSection()
        {
            Children = new Drawable[]
            {
                pinnedScores = new PaginatedScoreContainer(ScoreType.Pinned, User, UsersStrings.ShowExtraTopRanksPinnedTitle),
                new PaginatedScoreContainer(ScoreType.Best, User, UsersStrings.ShowExtraTopRanksBestTitle),
                new PaginatedScoreContainer(ScoreType.Firsts, User, UsersStrings.ShowExtraTopRanksFirstTitle)
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (pinningManager != null)
                pinningManager.PinsChanged += onPinsChanged;
        }

        private void onPinsChanged() => pinnedScores.Refresh(clearPrevious: false);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (pinningManager != null)
                pinningManager.PinsChanged -= onPinsChanged;
        }
    }
}
