// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Buttons
{
    public class OverlayLockButton : OsuAnimatedButton
    {
        private Box bgBox;

        public Bindable<bool> LockEnabled = new Bindable<bool>();

        public OverlayLockButton()
        {
            Size = new Vector2(50, 30);

            Children = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = Color4Extensions.FromHex("#5a5a5a"),
                },
                new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Icon = FontAwesome.Solid.Lock,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LockEnabled.Value = false;
            this.Action = () =>
            {
                ToggleLock();
            };
            this.Delay(1000).FadeOut(500, Easing.OutQuint);
        }

        public void ToggleLock()
        {
            switch ( LockEnabled.Value )
            {
                case true:
                    LockEnabled.Value = false;
                    bgBox.FadeColour( Color4Extensions.FromHex("#5a5a5a"), 500, Easing.OutQuint );
                    break;

                case false:
                    LockEnabled.Value = true;
                    bgBox.FadeColour( Color4.DarkGreen, 500, Easing.OutQuint );
                    break;
            }
        }
    }
}
