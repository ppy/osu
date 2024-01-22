// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public partial class OsuCursor : SkinReloadableDrawable
    {
        public const float SIZE = 28;

        private bool cursorExpand;

        private SkinnableDrawable cursorSprite;
        private Container cursorScaleContainer = null!;

        private SkinnableCursor skinnableCursor => (SkinnableCursor)cursorSprite.Drawable;

        public IBindable<float> CursorScale => cursorScale;

        private readonly Bindable<float> cursorScale = new BindableFloat(1);

        private Bindable<float> userCursorScale = null!;
        private Bindable<bool> autoCursorScale = null!;

        [Resolved(canBeNull: true)]
        private GameplayState state { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        public OsuCursor()
        {
            Origin = Anchor.Centre;

            Size = new Vector2(SIZE);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = CreateCursorContent();

            userCursorScale = config.GetBindable<float>(OsuSetting.GameplayCursorSize);
            userCursorScale.ValueChanged += _ => cursorScale.Value = CalculateCursorScale();

            autoCursorScale = config.GetBindable<bool>(OsuSetting.AutoCursorSize);
            autoCursorScale.ValueChanged += _ => cursorScale.Value = CalculateCursorScale();

            cursorScale.BindValueChanged(e => cursorScaleContainer.Scale = new Vector2(e.NewValue), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            cursorScale.Value = CalculateCursorScale();
        }

        protected virtual Drawable CreateCursorContent() => cursorScaleContainer = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Child = cursorSprite = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.Cursor), _ => new DefaultCursor(), confineMode: ConfineMode.NoScaling)
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
            },
        };

        protected virtual float CalculateCursorScale()
        {
            float scale = userCursorScale.Value;

            if (autoCursorScale.Value && state != null)
            {
                // if we have a beatmap available, let's get its circle size to figure out an automatic cursor scale modifier.
                scale *= GetScaleForCircleSize(state.Beatmap.Difficulty.CircleSize);
            }

            return scale;
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            cursorExpand = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.CursorExpand)?.Value ?? true;
        }

        public void Expand()
        {
            if (!cursorExpand) return;

            skinnableCursor.Expand();
        }

        public void Contract() => skinnableCursor.Contract();

        /// <summary>
        /// Get the scale applicable to the ActiveCursor based on a beatmap's circle size.
        /// </summary>
        public static float GetScaleForCircleSize(float circleSize) =>
            1f - 0.7f * (1f + circleSize - BeatmapDifficulty.DEFAULT_DIFFICULTY) / BeatmapDifficulty.DEFAULT_DIFFICULTY;

        private partial class DefaultCursor : SkinnableCursor
        {
            public DefaultCursor()
            {
                RelativeSizeAxes = Axes.Both;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChildren = new[]
                {
                    ExpandTarget = new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderThickness = SIZE / 6,
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
                                BorderThickness = SIZE / 3,
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
                        },
                    },
                    new Circle
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(0.14f),
                        Colour = new Color4(34, 93, 204, 255),
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Radius = 8,
                            Colour = Color4.White,
                        },
                    },
                };
            }
        }
    }
}
