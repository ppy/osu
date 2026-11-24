// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public abstract partial class ProfileHeaderCardSmall : Container
    {
        private const int icon_size = 24;
        private const int spacing = 4;
        private const int right_width = 86;

        protected OsuSpriteText Label { get; }
        protected OsuSpriteText Value { get; }

        protected IconUsage Icon
        {
            get => icon.Icon;
            set => icon.Icon = value;
        }

        private readonly Box background;

        private readonly SpriteIcon icon;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        protected ProfileHeaderCardSmall()
        {
            RelativeSizeAxes = Axes.X;
            Height = 60;
            Masking = true;
            CornerRadius = 10;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.25f),
                Offset = new Vector2(1),
                Radius = 3,
            };

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.4f)),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 8, Vertical = 10 },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Child = icon = new SpriteIcon
                            {
                                Size = new Vector2(icon_size),
                                Colour = Color4.White,
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Left = icon_size + spacing,
                                Right = spacing + right_width,
                            },
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    Label = new OsuSpriteText
                                    {
                                        Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold),
                                    },
                                    Value = new OsuSpriteText
                                    {
                                        Font = OsuFont.Style.Caption2,
                                    },
                                },
                            },
                        },
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = right_width,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Background3;

            Label.Colour = colourProvider.Content1;
            Value.Colour = colourProvider.Content2;
        }
    }
}
