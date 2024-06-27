// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultRankDisplay : Container, ISerialisableDrawable
    {
        [SettingSource(typeof(GameplayRankDisplayStrings), nameof(GameplayRankDisplayStrings.RankDisplay), nameof(GameplayRankDisplayStrings.RankDisplayDescription))]
        public Bindable<RankDisplayMode> RankDisplay { get; } = new Bindable<RankDisplayMode>();

        private Bindable<ScoreRank> rankBinding = new Bindable<ScoreRank>();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        public bool UsesFixedAnchor { get; set; }

        private readonly UpdateableRank rank;

        public DefaultRankDisplay()
        {
            Size = new Vector2(70, 35);

            InternalChildren = new Drawable[]
            {
                rank = new UpdateableRank(ScoreRank.X)
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            RankDisplay.BindValueChanged(mode =>
            {
                rankBinding.UnbindBindings();
                switch (mode.NewValue)
                {
                    case RankDisplayMode.Standard:
                        rankBinding.BindTarget = scoreProcessor.Rank;
                        break;

                    case RankDisplayMode.MinimumAchievable:
                        rankBinding.BindTarget = scoreProcessor.MinimumRank;
                        break;

                    case RankDisplayMode.MaximumAchievable:
                        rankBinding.BindTarget = scoreProcessor.MaximumRank;
                        break;
                }
                rankBinding.BindValueChanged(v => rank.Rank = v.NewValue, true);
            }, true);
        }

        public enum RankDisplayMode
        {
            [LocalisableDescription(typeof(GameplayRankDisplayStrings), nameof(GameplayRankDisplayStrings.RankDisplayModeStandard))]
            Standard,

            [LocalisableDescription(typeof(GameplayRankDisplayStrings), nameof(GameplayRankDisplayStrings.RankDisplayModeMax))]
            MaximumAchievable,

            [LocalisableDescription(typeof(GameplayRankDisplayStrings), nameof(GameplayRankDisplayStrings.RankDisplayModeMin))]
            MinimumAchievable
        }
    }
}
