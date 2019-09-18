// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Select.Options
{
    public class BeatmapOptionsButton : OsuClickableContainer
    {
        private const float width = 130;

        private readonly Box background;
        private readonly Box flash;
        private readonly SpriteIcon iconText;
        private readonly OsuSpriteText firstLine;
        private readonly OsuSpriteText secondLine;
        private readonly Container box;

        public Color4 ButtonColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }

        public IconUsage Icon
        {
            get => iconText.Icon;
            set => iconText.Icon = value;
        }

        public string FirstLineText
        {
            get => firstLine.Text;
            set => firstLine.Text = value;
        }

        public string SecondLineText
        {
            get => secondLine.Text;
            set => secondLine.Text = value;
        }

        public Key? HotKey;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            flash.FadeTo(0.1f, 1000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            flash.FadeTo(0, 1000, Easing.OutQuint);
            return base.OnMouseUp(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            flash.ClearTransforms();
            flash.Alpha = 0.9f;
            flash.FadeOut(800, Easing.OutExpo);

            return base.OnClick(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.Key == HotKey)
            {
                Click();
                return true;
            }

            return false;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => box.ReceivePositionalInputAt(screenSpacePos);

        public BeatmapOptionsButton()
        {
            Width = width;
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                box = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Shear = new Vector2(0.2f, 0f),
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.2f),
                        Roundness = 5,
                        Radius = 8,
                    },
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            EdgeSmoothness = new Vector2(1.5f, 0),
                            Colour = Color4.Black,
                        },
                        flash = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            EdgeSmoothness = new Vector2(1.5f, 0),
                            Blending = BlendingParameters.Additive,
                            Colour = Color4.White,
                            Alpha = 0,
                        },
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        iconText = new SpriteIcon
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Size = new Vector2(30),
                            Shadow = true,
                            Icon = FontAwesome.Solid.TimesCircle,
                            Margin = new MarginPadding
                            {
                                Bottom = 5,
                            },
                        },
                        firstLine = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold),
                            Text = @"",
                        },
                        secondLine = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold),
                            Text = @"",
                        },
                    },
                },
            };
        }
    }
}
