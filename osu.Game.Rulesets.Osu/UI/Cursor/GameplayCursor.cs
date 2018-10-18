// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Skinning;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public class GameplayCursor : CursorContainer, IKeyBindingHandler<OsuAction>
    {
        protected override Drawable CreateCursor() => new OsuCursor();

        protected override Container<Drawable> Content => fadeContainer;

        private readonly Container<Drawable> fadeContainer;

        public GameplayCursor()
        {
            InternalChild = fadeContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new CursorTrail { Depth = 1 }
                }
            };
        }

        private int downCount;

        private const float pressed_scale = 1.2f;
        private const float released_scale = 1f;

        private float targetScale => downCount > 0 ? pressed_scale : released_scale;

        public bool OnPressed(OsuAction action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    downCount++;
                    ActiveCursor.ScaleTo(released_scale).ScaleTo(targetScale, 100, Easing.OutQuad);
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
                        ActiveCursor.ScaleTo(targetScale, 200, Easing.OutQuad);
                    break;
            }

            return false;
        }

        public override bool HandlePositionalInput => true; // OverlayContainer will set this false when we go hidden, but we always want to receive input.

        protected override void PopIn()
        {
            fadeContainer.FadeTo(1, 300, Easing.OutQuint);
            ActiveCursor.ScaleTo(targetScale, 400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            fadeContainer.FadeTo(0.05f, 450, Easing.OutQuint);
            ActiveCursor.ScaleTo(targetScale * 0.8f, 450, Easing.OutQuint);
        }

        public class OsuCursor : Container
        {
            private Drawable cursorContainer;

            private Bindable<double> cursorScale;
            private Bindable<bool> autoCursorScale;
            private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

            public OsuCursor()
            {
                Origin = Anchor.Centre;
                Size = new Vector2(42);
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config, IBindableBeatmap beatmap)
            {
                Child = cursorContainer = new SkinnableDrawable("cursor", _ => new CircularContainer
                {
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
                }, restrictSize: false)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                };

                this.beatmap.BindTo(beatmap);
                this.beatmap.ValueChanged += v => calculateScale();

                cursorScale = config.GetBindable<double>(OsuSetting.GameplayCursorSize);
                cursorScale.ValueChanged += v => calculateScale();

                autoCursorScale = config.GetBindable<bool>(OsuSetting.AutoCursorSize);
                autoCursorScale.ValueChanged += v => calculateScale();

                calculateScale();
            }

            private void calculateScale()
            {
                float scale = (float)cursorScale.Value;

                if (autoCursorScale && beatmap.Value != null)
                {
                    // if we have a beatmap available, let's get its circle size to figure out an automatic cursor scale modifier.
                    scale *= (float)(1 - 0.7 * (1 + beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize - BeatmapDifficulty.DEFAULT_DIFFICULTY) / BeatmapDifficulty.DEFAULT_DIFFICULTY);
                }

                cursorContainer.Scale = new Vector2(scale);
            }
        }
    }
}
