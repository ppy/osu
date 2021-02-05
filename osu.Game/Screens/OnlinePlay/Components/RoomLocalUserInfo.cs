// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class RoomLocalUserInfo : OnlinePlayComposite
    {
        private OsuSpriteText attemptDisplay;

        public RoomLocalUserInfo()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    attemptDisplay = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14)
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            MaxAttempts.BindValueChanged(attempts =>
            {
                attemptDisplay.Text = attempts.NewValue == null
                    ? string.Empty
                    : $"Maximum attempts: {attempts.NewValue:N0}";
            }, true);
        }
    }
}
