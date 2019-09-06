// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public class OsuCursor : SkinReloadableDrawable
    {
        private const float size = 28;

        private bool cursorExpand;

        private Bindable<float> cursorScale;
        private Bindable<bool> autoCursorScale;
        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private Container expandTarget;
        private Drawable scaleTarget;

        public OsuCursor()
        {
            Origin = Anchor.Centre;

            Size = new Vector2(size);
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            cursorExpand = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.CursorExpand)?.Value ?? true;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IBindable<WorkingBeatmap> beatmap)
        {
            InternalChild = expandTarget = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Child = scaleTarget = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.Cursor), _ => new DefaultCursor(), confineMode: ConfineMode.NoScaling)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            };

            this.beatmap.BindTo(beatmap);
            this.beatmap.ValueChanged += _ => calculateScale();

            cursorScale = config.GetBindable<float>(OsuSetting.GameplayCursorSize);
            cursorScale.ValueChanged += _ => calculateScale();

            autoCursorScale = config.GetBindable<bool>(OsuSetting.AutoCursorSize);
            autoCursorScale.ValueChanged += _ => calculateScale();

            calculateScale();
        }

        private void calculateScale()
        {
            float scale = cursorScale.Value;

            if (autoCursorScale.Value && beatmap.Value != null)
            {
                // if we have a beatmap available, let's get its circle size to figure out an automatic cursor scale modifier.
                scale *= 1f - 0.7f * (1f + beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize - BeatmapDifficulty.DEFAULT_DIFFICULTY) / BeatmapDifficulty.DEFAULT_DIFFICULTY;
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

        private class DefaultCursor : CompositeDrawable
        {
            public DefaultCursor()
            {
                RelativeSizeAxes = Axes.Both;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderThickness = size / 6,
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
                                BorderThickness = size / 3,
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
                    }
                };
            }
        }
    }
}
