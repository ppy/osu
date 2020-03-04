// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Profile.Sections
{
    public class CounterPill : CircularContainer
    {
        private const int duration = 200;

        public readonly BindableInt Current = new BindableInt();

        private OsuSpriteText counter;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AutoSizeAxes = Axes.Both;
            Alpha = 0;
            Masking = true;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background6
                },
                counter = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                    Colour = colourProvider.Foreground1
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

            counter.Text = value.NewValue.ToString("N0");
            this.FadeIn(duration, Easing.OutQuint);
        }
    }
}
