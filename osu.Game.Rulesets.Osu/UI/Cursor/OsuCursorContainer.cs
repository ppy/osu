// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public partial class OsuCursorContainer : GameplayCursorContainer, IKeyBindingHandler<OsuAction>
    {
        protected override Drawable CreateCursor() => new OsuCursor();

        protected override Container<Drawable> Content => fadeContainer;

        private readonly Container<Drawable> fadeContainer;

        private readonly Bindable<bool> showTrail = new Bindable<bool>(true);

        private readonly Drawable cursorTrail;

        public IBindable<float> CursorScale => cursorScale;

        private readonly Bindable<float> cursorScale = new BindableFloat(1);

        private Bindable<float> userCursorScale;
        private Bindable<bool> autoCursorScale;

        private readonly CursorRippleVisualiser rippleVisualiser;

        public OsuCursorContainer()
        {
            InternalChild = fadeContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    cursorTrail = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.CursorTrail), _ => new DefaultCursorTrail(), confineMode: ConfineMode.NoScaling),
                    rippleVisualiser = new CursorRippleVisualiser(),
                    new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.CursorParticles), confineMode: ConfineMode.NoScaling),
                }
            };
        }

        [Resolved(canBeNull: true)]
        private GameplayState state { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.ShowCursorTrail, showTrail);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            showTrail.BindValueChanged(v => cursorTrail.FadeTo(v.NewValue ? 1 : 0, 200), true);

            userCursorScale = config.GetBindable<float>(OsuSetting.GameplayCursorSize);
            userCursorScale.ValueChanged += _ => calculateScale();

            autoCursorScale = config.GetBindable<bool>(OsuSetting.AutoCursorSize);
            autoCursorScale.ValueChanged += _ => calculateScale();

            CursorScale.BindValueChanged(e =>
            {
                var newScale = new Vector2(e.NewValue);

                ActiveCursor.Scale = newScale;
                rippleVisualiser.CursorScale = newScale;
                cursorTrail.Scale = newScale;
            }, true);

            calculateScale();
        }

        /// <summary>
        /// Get the scale applicable to the ActiveCursor based on a beatmap's circle size.
        /// </summary>
        public static float GetScaleForCircleSize(float circleSize) =>
            1f - 0.7f * (1f + circleSize - BeatmapDifficulty.DEFAULT_DIFFICULTY) / BeatmapDifficulty.DEFAULT_DIFFICULTY;

        private void calculateScale()
        {
            float scale = userCursorScale.Value;

            if (autoCursorScale.Value && state != null)
            {
                // if we have a beatmap available, let's get its circle size to figure out an automatic cursor scale modifier.
                scale *= GetScaleForCircleSize(state.Beatmap.Difficulty.CircleSize);
            }

            cursorScale.Value = scale;

            var newScale = new Vector2(scale);

            ActiveCursor.ScaleTo(newScale, 400, Easing.OutQuint);
            cursorTrail.Scale = newScale;
        }

        private int downCount;

        private void updateExpandedState()
        {
            if (downCount > 0)
                (ActiveCursor as OsuCursor)?.Expand();
            else
                (ActiveCursor as OsuCursor)?.Contract();
        }

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            switch (e.Action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    downCount++;
                    updateExpandedState();
                    break;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
            switch (e.Action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    // Todo: Math.Max() is required as a temporary measure to address https://github.com/ppy/osu-framework/issues/2576
                    downCount = Math.Max(0, downCount - 1);

                    if (downCount == 0)
                        updateExpandedState();
                    break;
            }
        }

        public override bool HandlePositionalInput => true; // OverlayContainer will set this false when we go hidden, but we always want to receive input.

        protected override void PopIn()
        {
            fadeContainer.FadeTo(1, 300, Easing.OutQuint);
            ActiveCursor.ScaleTo(CursorScale.Value, 400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            fadeContainer.FadeTo(0.05f, 450, Easing.OutQuint);
            ActiveCursor.ScaleTo(CursorScale.Value * 0.8f, 450, Easing.OutQuint);
        }

        private partial class DefaultCursorTrail : CursorTrail
        {
            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(@"Cursor/cursortrail");
                Scale = new Vector2(1 / Texture.ScaleAdjust);
            }
        }
    }
}
