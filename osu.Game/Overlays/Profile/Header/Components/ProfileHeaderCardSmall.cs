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
        protected OsuSpriteText Label { get; private set; }
        protected OsuSpriteText Value { get; private set; }

        protected IconUsage Icon
        {
            get => icon.Icon;
            set => icon.Icon = value;
        }

        private readonly Box background;

        private readonly SpriteIcon icon;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

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
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(4),
                            Children = new Drawable[]
                            {
                                icon = new SpriteIcon
                                {
                                    Size = new Vector2(24),
                                    Colour = Color4.White,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        Label = new OsuSpriteText
                                        {
                                            Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold),
                                            Colour = Color4.White,
                                        },
                                        Value = new OsuSpriteText
                                        {
                                            Font = OsuFont.Style.Caption2,
                                            Colour = Color4.White,
                                        },
                                    },
                                },
                            },
                        },
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 86,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background3;
        }
    }
}
