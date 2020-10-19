// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableScoreCounter : SkinnableDrawable, IScoreCounter
    {
        public Bindable<double> Current { get; } = new Bindable<double>();

        private Bindable<ScoringMode> scoreDisplayMode;

        public Bindable<int> RequiredDisplayDigits { get; } = new Bindable<int>();

        public SkinnableScoreCounter()
            : base(new HUDSkinComponent(HUDSkinComponents.ScoreCounter), _ => new DefaultScoreCounter())
        {
            CentreComponent = false;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            scoreDisplayMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);
            scoreDisplayMode.BindValueChanged(scoreMode =>
            {
                switch (scoreMode.NewValue)
                {
                    case ScoringMode.Standardised:
                        RequiredDisplayDigits.Value = 6;
                        break;

                    case ScoringMode.Classic:
                        RequiredDisplayDigits.Value = 8;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(scoreMode));
                }
            }, true);
        }

        private IScoreCounter skinnedCounter;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            skinnedCounter = Drawable as IScoreCounter;

            skinnedCounter?.Current.BindTo(Current);
            skinnedCounter?.RequiredDisplayDigits.BindTo(RequiredDisplayDigits);
        }
    }
}
