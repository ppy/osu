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

        public CursorRippleVisualiser()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager? rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.ShowCursorRipples, showRipples);
        }

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (showRipples.Value)
            {
                AddInternal(ripplePool.Get(r =>
                {
                    r.Position = e.MousePosition;
                }));
            }

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

                this.ScaleTo(0.1f)
                    .ScaleTo(1, 700, Easing.Out)
                    .FadeOutFromOne(700)
                    .Expire(true);
            }
        }

        public partial class DefaultCursorRipple : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new RingPiece(3)
                    {
                        Size = new Vector2(256),
                        Alpha = 0.2f,
                    }
                };
            }
        }
    }
}
