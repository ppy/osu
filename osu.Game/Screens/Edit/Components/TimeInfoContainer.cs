// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using System;
using osu.Framework.Timing;

namespace osu.Game.Screens.Edit.Components
{
    public class TimeInfoContainer : BottomBarContainer
    {
        private const int count_duration = 150;

        private readonly OsuSpriteText trackTimer;

        private readonly IAdjustableClock adjustableClock;

        public TimeInfoContainer(IAdjustableClock adjustableClock)
        {
            this.adjustableClock = adjustableClock;

            Children = new Drawable[]
            {
                trackTimer = new OsuSpriteText
                {
                    Origin = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.Y,
                    TextSize = 22,
                    FixedWidth = true,
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
