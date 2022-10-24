// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class LegacyCatcherNew : CompositeDrawable
    {
        [Resolved]
        private Bindable<CatcherAnimationState> currentState { get; set; }

        private readonly Dictionary<CatcherAnimationState, Drawable> drawables = new Dictionary<CatcherAnimationState, Drawable>();

        private Drawable currentDrawable;

        public LegacyCatcherNew()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            foreach (var state in Enum.GetValues(typeof(CatcherAnimationState)).Cast<CatcherAnimationState>())
            {
                AddInternal(drawables[state] = getDrawableFor(state).With(d =>
                {
                    d.Anchor = Anchor.TopCentre;
                    d.Origin = Anchor.TopCentre;
                    d.RelativeSizeAxes = Axes.Both;
                    d.Size = Vector2.One;
                    d.FillMode = FillMode.Fit;
                    d.Alpha = 0;
                }));
            }

            currentDrawable = drawables[CatcherAnimationState.Idle];

            Drawable getDrawableFor(CatcherAnimationState state) =>
                skin.GetAnimation(@$"fruit-catcher-{state.ToString().ToLowerInvariant()}", true, true, true) ??
                skin.GetAnimation(@"fruit-catcher-idle", true, true, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentState.BindValueChanged(state =>
            {
                currentDrawable.Alpha = 0;
                currentDrawable = drawables[state.NewValue];
                currentDrawable.Alpha = 1;

                (currentDrawable as IFramedAnimation)?.GotoFrame(0);
            }, true);
        }
    }
}
