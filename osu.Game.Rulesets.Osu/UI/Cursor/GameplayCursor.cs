// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Configuration;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public class GameplayCursor : CursorContainer, IKeyBindingHandler<OsuAction>
    {
        protected override Drawable CreateCursor() => new OsuCursor();

        public GameplayCursor()
        {
            Add(new CursorTrail { Depth = 1 });
        }

        private int downCount;

        public class OsuCursor : Container
        {
            private CircleSizeAdjustContainer adjustContainer;

            private Bindable<double> cursorScale;
            private Bindable<bool> autoCursorScale;

            public OsuCursor()
            {
                Origin = Anchor.Centre;
                Size = new Vector2(42);
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                Children = new Drawable[]
                {
                    adjustContainer = new CircleSizeAdjustContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Child = new CircularContainer
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            BorderThickness = Size.X / 6,
                            BorderColour = Color4.White,
                            EdgeEffect = new EdgeEffectParameters
                            {
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
                    }
                };

                cursorScale = config.GetBindable<double>(OsuSetting.GameplayCursorSize);
                cursorScale.ValueChanged += v => calculateScale();

                autoCursorScale = config.GetBindable<bool>(OsuSetting.AutoCursorSize);
                autoCursorScale.ValueChanged += v => calculateScale();

                calculateScale();
            }

            private void calculateScale()
            {
                adjustContainer.ApplyScale((float)cursorScale.Value, autoCursorScale, 200, Easing.OutQuad);
            }
        }

        public bool OnPressed(OsuAction action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    downCount++;
                    ActiveCursor.ScaleTo(1).ScaleTo(1.2f, 100, Easing.OutQuad);
                    break;
            }

            return false;
        }

        public bool OnReleased(OsuAction action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    if (--downCount == 0)
                        ActiveCursor.ScaleTo(1, 200, Easing.OutQuad);
                    break;
            }

            return false;
        }
    }
}
