// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public partial class TotalCommentsCounter : CompositeDrawable
    {
        public readonly BindableInt Current = new BindableInt();

        private OsuSpriteText counter;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            Height = 50;
            AddInternal(new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Margin = new MarginPadding { Left = WaveOverlayContainer.HORIZONTAL_PADDING },
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 20, italics: true),
                        Colour = colourProvider.Light1,
                        Text = CommentsStrings.Title
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
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
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                                Colour = colourProvider.Foreground1
                            }
                        },
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            Current.BindValueChanged(value => counter.Text = value.NewValue.ToString("N0"), true);
            base.LoadComplete();
        }
    }
}
