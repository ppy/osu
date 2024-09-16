// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class WindowSizeIndicator : CompositeDrawable
    {
        private BindableSize sizeBindable = new BindableSize();

        private TournamentSpriteText winWidthText = null!;
        private TournamentSpriteText winHeightText = null!;

        public WindowSizeIndicator(BindableSize bSize)
        {
            sizeBindable = bSize;
            sizeBindable.BindValueChanged(bindSizeChanged);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
            AlwaysPresent = true;

            InternalChildren = new Drawable[]
            {
                new EmptyBox(cornerRadius: 10)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.15f,
                    Height = 0.15f,
                    Colour = Color4.Black.Opacity(0.6f),
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Height = 40,
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.X,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.RulerHorizontal,
                                    Size = new Vector2(24),
                                },
                                winWidthText = new TournamentSpriteText
                                {
                                    Text = sizeBindable.Value.Width.ToString(),
                                    Colour = TournamentGame.TEXT_COLOUR,
                                    Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Height = 40,
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.X,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.RulerVertical,
                                    Size = new Vector2(24),
                                },
                                winHeightText = new TournamentSpriteText
                                {
                                    Text = sizeBindable.Value.Height.ToString(),
                                    Colour = TournamentGame.TEXT_COLOUR,
                                    Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                                },
                            }
                        },
                    }
                },
            };
        }

        private void bindSizeChanged(ValueChangedEvent<Size> e)
        {
            Scheduler.Add(() =>
            {
                winWidthText.Text = e.NewValue.Width.ToString();
                winHeightText.Text = e.NewValue.Height.ToString();
            });
        }
    }
}
