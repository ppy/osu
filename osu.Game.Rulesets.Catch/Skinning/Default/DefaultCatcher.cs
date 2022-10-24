// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public class DefaultCatcher : CompositeDrawable
    {
        public Bindable<CatcherAnimationState> CurrentState { get; } = new Bindable<CatcherAnimationState>();

        private readonly Sprite sprite;

        private readonly Dictionary<CatcherAnimationState, Texture> textures = new Dictionary<CatcherAnimationState, Texture>();

        public DefaultCatcher()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = sprite = new Sprite
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore store, Bindable<CatcherAnimationState> currentState)
        {
            CurrentState.BindTo(currentState);

            textures[CatcherAnimationState.Idle] = store.Get(@"Gameplay/catch/fruit-catcher-idle");
            textures[CatcherAnimationState.Fail] = store.Get(@"Gameplay/catch/fruit-catcher-fail");
            textures[CatcherAnimationState.Kiai] = store.Get(@"Gameplay/catch/fruit-catcher-kiai");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentState.BindValueChanged(state => sprite.Texture = textures[state.NewValue], true);
        }
    }
}
