// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public partial class CursorRippleVisualiser : CompositeDrawable, IKeyBindingHandler<OsuAction>
    {
        private readonly Bindable<bool> showRipples = new Bindable<bool>(true);

        private readonly DrawablePool<CursorRipple> ripplePool = new DrawablePool<CursorRipple>(20);

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager? rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.ShowCursorRipples, showRipples);
        }

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (showRipples.Value)
                AddInternal(ripplePool.Get());

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
        }

        private partial class CursorRipple : PoolableDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;
                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.CursorRipple), _ => new DefaultCursorRipple())
                    {
                        Blending = BlendingParameters.Additive,
                    }
                };
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                ClearTransforms(true);

                this.ScaleTo(0.05f)
                    .ScaleTo(0.5f, 700, Easing.Out)
                    .FadeTo(0.2f)
                    .FadeOut(700)
                    .Expire();
            }
        }

        public partial class DefaultCursorRipple : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new RingPiece(3)
                    {
                        Size = new Vector2(512),
                    }
                };
            }
        }
    }
}
