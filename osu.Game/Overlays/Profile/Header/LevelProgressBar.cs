// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class LevelProgressBar : CompositeDrawable, IHasTooltip
    {
        public readonly Bindable<User> User = new Bindable<User>();

        public string TooltipText { get; }

        private Bar levelProgressBar;
        private OsuSpriteText levelProgressText;

        public LevelProgressBar()
        {
            TooltipText = "Progress to next level";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = levelProgressBar = new Bar
                    {
                        RelativeSizeAxes = Axes.Both,
                        BackgroundColour = Color4.Black,
                        Direction = BarDirection.LeftToRight,
                        AccentColour = colours.Yellow
                    }
                },
                levelProgressText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.TopRight,
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold)
                }
            };

            User.BindValueChanged(updateProgress);
        }

        private void updateProgress(ValueChangedEvent<User> user)
        {
            levelProgressBar.Length = user.NewValue?.Statistics?.Level.Progress / 100f ?? 0;
            levelProgressText.Text = user.NewValue?.Statistics?.Level.Progress.ToString("0'%'");
        }
    }
}
