// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Sections
{
    public class CounterPill : CircularContainer
    {
        private const int duration = 200;

        public readonly BindableInt Current = new BindableInt();

        private readonly OsuSpriteText counter;

        public CounterPill()
        {
            AutoSizeAxes = Axes.Both;
            Alpha = 0;
            Masking = true;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.05f)
                },
                counter = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(onCurrentChanged, true);
        }

        private void onCurrentChanged(ValueChangedEvent<int> value)
        {
            if (value.NewValue == 0)
            {
                this.FadeOut(duration, Easing.OutQuint);
                return;
            }

            counter.Text = value.NewValue.ToString();
            this.FadeIn(duration, Easing.OutQuint);
        }
    }
}
