// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class GameplayRankDisplay : CompositeDrawable, IHasCurrentValue<ScoreRank>
    {
        private readonly BindableWithCurrent<ScoreRank> current = new BindableWithCurrent<ScoreRank>();

        public Bindable<ScoreRank> Current
        {
            get => current;
            set => current.Current = value;
        }

        [SettingSource(typeof(GameplayRankDisplayStrings), nameof(GameplayRankDisplayStrings.RankDisplay), nameof(GameplayRankDisplayStrings.RankDisplayDescription))]
        public Bindable<RankDisplayMode> RankDisplay { get; } = new Bindable<RankDisplayMode>();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            RankDisplay.BindValueChanged(mode =>
            {
                Current.UnbindBindings();

                switch (mode.NewValue)
                {
                    case RankDisplayMode.Standard:
                        Current.BindTarget = scoreProcessor.Rank;
                        break;

                    case RankDisplayMode.MinimumAchievable:
                        Current.BindTarget = scoreProcessor.MinimumRank;
                        break;

                    case RankDisplayMode.MaximumAchievable:
                        Current.BindTarget = scoreProcessor.MaximumRank;
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
