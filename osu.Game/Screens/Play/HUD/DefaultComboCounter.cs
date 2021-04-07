// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultComboCounter : RollingCounter<int>, IComboCounter
    {
        private readonly Vector2 offset = new Vector2(20, 5);

        [Resolved(canBeNull: true)]
        private HUDOverlay hud { get; set; }

        public DefaultComboCounter()
        {
            Current.Value = DisplayedCount = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours) => Colour = colours.BlueLighter;

        protected override void Update()
        {
            base.Update();

            if (hud?.ScoreCounter.Drawable is DefaultScoreCounter score)
            {
                // for now align with the score counter. eventually this will be user customisable.
                Position = Parent.ToLocalSpace(score.ScreenSpaceDrawQuad.TopRight) + offset;
            }
        }

        protected override string FormatCount(int count)
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
