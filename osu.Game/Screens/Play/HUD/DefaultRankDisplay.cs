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
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultRankDisplay : Container, ISerialisableDrawable
    {
        [SettingSource(typeof(GameplayRankDisplayStrings), nameof(GameplayRankDisplayStrings.RankDisplay), nameof(GameplayRankDisplayStrings.RankDisplayDescription))]
        public Bindable<RankDisplayMode> RankDisplay { get; } = new Bindable<RankDisplayMode>();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        public bool UsesFixedAnchor { get; set; }

        private readonly UpdateableRank rank;

        public DefaultRankDisplay()
        {
            Size = new Vector2(70, 35);

            InternalChildren = new Drawable[]
            {
                rank = new UpdateableRank(Scoring.ScoreRank.X)
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rank.Rank = scoreProcessor.Rank.Value;
            switch (RankDisplay.Value)
            {
                case RankDisplayMode.Standard:
                    rank.Rank = scoreProcessor.Rank.Value;
                    break;

                case RankDisplayMode.MinimumAchievable:
                    rank.Rank = scoreProcessor.MinimumRank.Value;
                    break;

                case RankDisplayMode.MaximumAchievable:
                    rank.Rank = scoreProcessor.MaximumRank.Value;
                    break;
            }
            RankDisplay.BindValueChanged(mode =>
            {
                switch (mode.OldValue)
                {
                    case RankDisplayMode.Standard:
                        scoreProcessor.Rank.UnbindBindings();
                        break;

                    case RankDisplayMode.MinimumAchievable:
                        scoreProcessor.MinimumRank.UnbindBindings();
                        break;

                    case RankDisplayMode.MaximumAchievable:
                        scoreProcessor.MaximumRank.UnbindBindings();
                        break;
                }
                switch (mode.NewValue)
                {
                    case RankDisplayMode.Standard:
                        rank.Rank = scoreProcessor.Rank.Value;
                        scoreProcessor.Rank.BindValueChanged(v => rank.Rank = v.NewValue);
                        break;

                    case RankDisplayMode.MinimumAchievable:
                        rank.Rank = scoreProcessor.MinimumRank.Value;
                        scoreProcessor.MinimumRank.BindValueChanged(v => rank.Rank = v.NewValue);
                        break;

                    case RankDisplayMode.MaximumAchievable:
                        rank.Rank = scoreProcessor.MaximumRank.Value;
                        scoreProcessor.MaximumRank.BindValueChanged(v => rank.Rank = v.NewValue);
                        break;
                }
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
