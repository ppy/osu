// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Configuration;
using System;

namespace osu.Game.Graphics.Cursor
{
    public class OsuCursorContainer : CursorContainer
    {
        protected override Drawable CreateCursor() => new OsuCursor();

        public OsuCursorContainer()
        {
            Add(new CursorTrail { Depth = 1 });
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            ActiveCursor.Scale = new Vector2(1);
            ActiveCursor.ScaleTo(1.2f, 100, EasingTypes.OutQuad);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (!state.Mouse.HasMainButtonPressed)
                ActiveCursor.ScaleTo(1, 200, EasingTypes.OutQuad);
            return base.OnMouseUp(state, args);
        }

        public class OsuCursor : Container
        {
            private Container cursorContainer;
            private Bindable<double> cursorScale;

            public OsuCursor()
            {
                Origin = Anchor.Centre;
                Size = new Vector2(42);
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                cursorScale = config.GetBindable<double>(OsuConfig.CursorSize);

                Children = new Drawable[]
                {
                    cursorContainer = new CircularContainer
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2((float)cursorScale),
                        Masking = true,
                        BorderThickness = Size.X / 6,
                        BorderColour = Color4.White,
                        EdgeEffect = new EdgeEffect {
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Pink.Opacity(0.5f),
                            Radius = 5,
                        },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            },
                            new CircularContainer
                            {
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                BorderThickness = Size.X / 3,
                                BorderColour = Color4.White.Opacity(0.5f),
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                        AlwaysPresent = true,
                                    },
                                },
                            },
                            new CircularContainer
                            {
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Scale = new Vector2(0.1f),
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.White,
                                    },
                                },
                            },
                        }
                    },
                };
                cursorScale.ValueChanged += scaleChanged;
            }

            private void scaleChanged(object sender, EventArgs e)
            {
                cursorContainer.Scale = new Vector2((float)cursorScale);
            }
        }
    }
}
