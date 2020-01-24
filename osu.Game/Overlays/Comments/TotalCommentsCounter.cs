// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Comments
{
    public class TotalCommentsCounter : CompositeDrawable
    {
        public readonly BindableInt Current = new BindableInt();

        private readonly SpriteText counter;
        private readonly OsuSpriteText text;
        private readonly Box pillBackground;
        private readonly OverlayColourScheme colourScheme;

        public TotalCommentsCounter(OverlayColourScheme colourScheme)
        {
            this.colourScheme = colourScheme;

            RelativeSizeAxes = Axes.X;
            Height = 50;
            AddInternal(new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Margin = new MarginPadding { Left = 50 },
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 20, italics: true),
                        Text = @"Comments"
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            pillBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            counter = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold)
                            }
                        },
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            text.Colour = colours.ForOverlayElement(colourScheme, 0.4f, 0.8f);
            pillBackground.Colour = colours.ForOverlayElement(colourScheme, 0.1f, 0.1f);
            counter.Colour = colours.ForOverlayElement(colourScheme, 0.1f, 0.6f);
        }

        protected override void LoadComplete()
        {
            Current.BindValueChanged(value => counter.Text = value.NewValue.ToString("N0"), true);
            base.LoadComplete();
        }
    }
}
