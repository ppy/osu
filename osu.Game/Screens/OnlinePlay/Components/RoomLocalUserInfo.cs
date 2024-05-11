// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class RoomLocalUserInfo : OnlinePlayComposite
    {
        private OsuSpriteText attemptDisplay;

        [Resolved]
        private OsuColour colours { get; set; }

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

            MaxAttempts.BindValueChanged(_ => updateAttempts());
            UserScore.BindValueChanged(_ => updateAttempts(), true);
        }

        private void updateAttempts()
        {
            if (MaxAttempts.Value != null)
            {
                attemptDisplay.Text = $"Maximum attempts: {MaxAttempts.Value:N0}";

                if (UserScore.Value != null)
                {
                    int remaining = MaxAttempts.Value.Value - UserScore.Value.PlaylistItemAttempts.Sum(a => a.Attempts);
                    attemptDisplay.Text += $" ({remaining} remaining)";

                    if (remaining == 0)
                        attemptDisplay.Colour = colours.RedLight;
                }
            }
            else
            {
                attemptDisplay.Text = string.Empty;
            }
        }
    }
}
