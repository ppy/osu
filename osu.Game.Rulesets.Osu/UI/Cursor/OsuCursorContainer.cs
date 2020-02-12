// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public class OsuCursorContainer : GameplayCursorContainer, IKeyBindingHandler<OsuAction>
    {
        protected override Drawable CreateCursor() => new OsuCursor();

        protected override Container<Drawable> Content => fadeContainer;

        private readonly Container<Drawable> fadeContainer;

        private readonly Bindable<bool> showTrail = new Bindable<bool>(true);

        private readonly Drawable cursorTrail;

        public Bindable<float> CursorScale;
        private Bindable<float> userCursorScale;
        private Bindable<bool> autoCursorScale;
        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        public OsuCursorContainer()
        {
            InternalChild = fadeContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = cursorTrail = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.CursorTrail), _ => new DefaultCursorTrail(), confineMode: ConfineMode.NoScaling)
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, OsuRulesetConfigManager rulesetConfig, IBindable<WorkingBeatmap> beatmap)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.ShowCursorTrail, showTrail);

            this.beatmap.BindTo(beatmap);
            this.beatmap.ValueChanged += _ => calculateScale();

            userCursorScale = config.GetBindable<float>(OsuSetting.GameplayCursorSize);
            userCursorScale.ValueChanged += _ => calculateScale();

            autoCursorScale = config.GetBindable<bool>(OsuSetting.AutoCursorSize);
            autoCursorScale.ValueChanged += _ => calculateScale();

            CursorScale = new BindableFloat();
            CursorScale.ValueChanged += e => ActiveCursor.Scale = cursorTrail.Scale = new Vector2(e.NewValue);

            calculateScale();
        }

        private void calculateScale()
        {
            float scale = userCursorScale.Value;

            if (autoCursorScale.Value && beatmap.Value != null)
            {
                // if we have a beatmap available, let's get its circle size to figure out an automatic cursor scale modifier.
                scale *= 1f - 0.7f * (1f + beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize - BeatmapDifficulty.DEFAULT_DIFFICULTY) / BeatmapDifficulty.DEFAULT_DIFFICULTY;
            }

            CursorScale.Value = scale;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            showTrail.BindValueChanged(v => cursorTrail.FadeTo(v.NewValue ? 1 : 0, 200), true);
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

        public void OnReleased(OsuAction action)
        {
            switch (action)
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

        private class DefaultCursorTrail : CursorTrail
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
