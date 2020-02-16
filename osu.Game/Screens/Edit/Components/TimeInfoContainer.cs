// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using System;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components
{
    public class TimeInfoContainer : BottomBarContainer
    {
        private readonly OsuSpriteText trackTimer;

        [Resolved]
        private IAdjustableClock adjustableClock { get; set; }

        public TimeInfoContainer()
        {
            Children = new Drawable[]
            {
                trackTimer = new OsuSpriteText
                {
                    Origin = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.Y,
                    Font = OsuFont.GetFont(size: 22, fixedWidth: true),
                    Y = 0.5f,
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            trackTimer.Text = TimeSpan.FromMilliseconds(adjustableClock.CurrentTime).ToString(@"mm\:ss\:fff");
        }
    }
}