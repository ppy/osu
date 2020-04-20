// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Mvis.Buttons
{
    public class ToggleableButton : OsuAnimatedButton
    {
        public Bindable<bool> ToggleableValue = new Bindable<bool>();
        private readonly Box bgBox;
        protected readonly SpriteIcon icon;
        protected readonly bool defaultValue;

        [Resolved]
        private OsuColour colour { get; set; }

        public ToggleableButton()
        {
            Size = new Vector2(30, 30);

            Children = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = Color4Extensions.FromHex("#5a5a5a"),
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ToggleableValue.Value = defaultValue;
            this.Action = () =>
            {
                Toggle();
            };
        }

        public void Toggle()
        {
            switch ( ToggleableValue.Value )
            {
                case true:
                    ToggleableValue.Value = false;
                    bgBox.FadeColour( Color4Extensions.FromHex("#5a5a5a"), 500, Easing.OutQuint );
                    break;

                case false:
                    ToggleableValue.Value = true;
                    bgBox.FadeColour( colour.Green, 500, Easing.OutQuint );
                    break;
            }
        }
    }
}
