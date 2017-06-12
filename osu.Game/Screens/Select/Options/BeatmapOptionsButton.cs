﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Screens.Select.Options
{
    public class BeatmapOptionsButton : ClickableContainer
    {
        private const float width = 130;

        private readonly Box background;
        private readonly Box flash;
        private readonly TextAwesome iconText;
        private readonly OsuSpriteText firstLine;
        private readonly OsuSpriteText secondLine;
        private readonly Container box;

        public Color4 ButtonColour
        {
            get { return background.Colour; }
            set { background.Colour = value; }
        }

        public FontAwesome Icon
        {
            get { return iconText.Icon; }
            set { iconText.Icon = value; }
        }

        public string FirstLineText
        {
            get { return firstLine.Text; }
            set { firstLine.Text = value; }
        }

        public string SecondLineText
        {
            get { return secondLine.Text; }
            set { secondLine.Text = value; }
        }

        public Key? HotKey;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            flash.FadeTo(0.1f, 1000, EasingTypes.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            flash.FadeTo(0, 1000, EasingTypes.OutQuint);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnClick(InputState state)
        {
            flash.ClearTransforms();
            flash.Alpha = 0.9f;
            flash.FadeOut(800, EasingTypes.OutExpo);

            return base.OnClick(state);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat && args.Key == HotKey)
            {
                OnClick(state);
                return true;
            }

            return false;
        }

        protected override bool InternalContains(Vector2 screenSpacePos) => box.Contains(screenSpacePos);

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
                            BlendingMode = BlendingMode.Additive,
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
                        iconText = new TextAwesome
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            TextSize = 30,
                            Shadow = true,
                            Icon = FontAwesome.fa_close,
                            Margin = new MarginPadding
                            {
                                Bottom = 5,
                            },
                        },
                        firstLine = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Font = @"Exo2.0-Bold",
                            Text = @"",
                        },
                        secondLine = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Font = @"Exo2.0-Bold",
                            Text = @"",
                        },
                    },
                },
            };
        }
    }
}
