// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Catch.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public partial class DefaultCatcher : CompositeDrawable
    {
        public Bindable<CatcherAnimationState> CurrentState { get; } = new Bindable<CatcherAnimationState>();

        private readonly Sprite sprite;

        private readonly Dictionary<CatcherAnimationState, Texture> textures = new Dictionary<CatcherAnimationState, Texture>();

        public DefaultCatcher()
        {
            Anchor = Anchor.TopCentre;
            RelativeSizeAxes = Axes.Both;
            InternalChild = sprite = new Sprite
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit
            };
        }

        protected override void Update()
        {
            base.Update();

            // matches stable's origin position since we're using the same catcher sprite.
            // see LegacyCatcher for more information.
            OriginPosition = new Vector2(DrawWidth / 2, 16f);
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
