// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osuTK;
using osuTK.Graphics;

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

        private void updateExpandedState()
        {
            if (downCount > 0)
                (ActiveCursor as OsuCursor)?.Expand();
            else
                (ActiveCursor as OsuCursor)?.Contract();
        }

        public bool OnPressed(OsuAction action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    downCount++;
                    updateExpandedState();
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
                        updateExpandedState();
                    break;
            }

            return false;
        }

        public override bool HandlePositionalInput => true; // OverlayContainer will set this false when we go hidden, but we always want to receive input.

        protected override void PopIn()
        {
            fadeContainer.FadeTo(1, 300, Easing.OutQuint);
            ActiveCursor.ScaleTo(1, 400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            fadeContainer.FadeTo(0.05f, 450, Easing.OutQuint);
            ActiveCursor.ScaleTo(0.8f, 450, Easing.OutQuint);
        }

        public class OsuCursor : SkinReloadableDrawable
        {
            private bool cursorExpand;

            private Bindable<double> cursorScale;
            private Bindable<bool> autoCursorScale;
            private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

            private Container expandTarget;
            private Drawable scaleTarget;

            public OsuCursor()
            {
                Origin = Anchor.Centre;
                Size = new Vector2(42);
            }

            protected override void SkinChanged(ISkinSource skin, bool allowFallback)
            {
                cursorExpand = skin.GetValue<SkinConfiguration, bool>(s => s.CursorExpand ?? true);
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config, IBindable<WorkingBeatmap> beatmap)
            {
                InternalChild = expandTarget = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Child = scaleTarget = new SkinnableDrawable("cursor", _ => new CircularContainer
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
                    }
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

                scaleTarget.Scale = new Vector2(scale);
            }

            private const float pressed_scale = 1.2f;
            private const float released_scale = 1f;

            public void Expand()
            {
                if (!cursorExpand) return;
                expandTarget.ScaleTo(released_scale).ScaleTo(pressed_scale, 100, Easing.OutQuad);
            }

            public void Contract() => expandTarget.ScaleTo(released_scale, 100, Easing.OutQuad);
        }
    }
}
