// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;

namespace osu.Game.Overlays.Profile.Sections
{
    public class CounterPill : CircularContainer
    {
        public readonly BindableInt Current = new BindableInt();

        private OsuSpriteText counter;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AutoSizeAxes = Axes.Both;
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
                    Margin = new MarginPadding { Horizontal = 10, Bottom = 1 },
                    Font = OsuFont.GetFont(size: 11.2f, weight: FontWeight.Bold),
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
            counter.Text = value.NewValue.ToLocalisableString("N0");
        }
    }
}
