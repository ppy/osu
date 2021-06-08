// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class LegacyCatcher : CompositeDrawable, ICatcherPiece
    {
        public Bindable<CatcherAnimationState> CurrentState { get; } = new Bindable<CatcherAnimationState>();

        public Texture CurrentTexture => (currentDrawable as TextureAnimation)?.CurrentFrame ?? (currentDrawable as Sprite)?.Texture;

        private readonly Dictionary<CatcherAnimationState, Drawable> drawables = new Dictionary<CatcherAnimationState, Drawable>();

        private Drawable currentDrawable;

        public LegacyCatcher()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, Bindable<CatcherAnimationState> currentState)
        {
            CurrentState.BindTo(currentState);

            AddRangeInternal(new[]
            {
                drawables[CatcherAnimationState.Idle] = getDrawableFor(@"fruit-catcher-idle"),
                drawables[CatcherAnimationState.Fail] = getDrawableFor(@"fruit-catcher-fail"),
                drawables[CatcherAnimationState.Kiai] = getDrawableFor(@"fruit-catcher-kiai"),
            });
            currentDrawable = drawables[CatcherAnimationState.Idle];

            foreach (var d in drawables.Values)
            {
                d.Anchor = Anchor.TopCentre;
                d.Origin = Anchor.TopCentre;
                d.RelativeSizeAxes = Axes.Both;
                d.Size = Vector2.One;
                d.FillMode = FillMode.Fit;
                d.Alpha = 0;
            }

            Drawable getDrawableFor(string name) =>
                skin.GetAnimation(name, true, true, true) ??
                skin.GetAnimation(@"fruit-ryuuta", true, true, true) ??
                skin.GetAnimation(@"fruit-catcher-idle", true, true, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentState.BindValueChanged(state =>
            {
                currentDrawable.Alpha = 0;
                currentDrawable = drawables[state.NewValue];
                currentDrawable.Alpha = 1;

                (currentDrawable as IFramedAnimation)?.GotoFrame(0);
            }, true);
        }
    }
}
