// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultComboCounter : RollingCounter<int>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        public DefaultComboCounter()
        {
            Current.Value = DisplayedCount = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, ScoreProcessor scoreProcessor)
        {
            Colour = colours.BlueLighter;
            Current.BindTo(scoreProcessor.Combo);
        }

        protected override LocalisableString FormatCount(int count)
        {
            return $@"{count}x";
        }

        protected override double GetProportionalDuration(int currentValue, int newValue)
        {
            return Math.Abs(currentValue - newValue) * RollingDuration * 100.0f;
        }

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(size: 20f));
    }
}
