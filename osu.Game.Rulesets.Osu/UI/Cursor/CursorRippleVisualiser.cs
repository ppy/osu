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
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Screens.Play;
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

        public Vector2 CursorScale { get; set; } = Vector2.One;

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager? rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.ShowCursorRipples, showRipples);

            AddInternal(ripplePool);
        }

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if ((Clock as IGameplayClock)?.IsRewinding == true)
                return false;

            if (showRipples.Value)
            {
                AddInternal(ripplePool.Get(r =>
                {
                    r.Position = e.MousePosition;
                    r.Scale = CursorScale;
                }));
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
        }

        private partial class CursorRipple : PoolableDrawable
        {
            private Drawable ripple = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;
                Origin = Anchor.Centre;

                InternalChild = ripple = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.CursorRipple), _ => new DefaultCursorRipple())
                {
                    Blending = BlendingParameters.Additive,
                };
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                ClearTransforms(true);

                ripple.ScaleTo(0.1f)
                      .ScaleTo(1, 700, Easing.Out);

                this
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
                        Size = OsuHitObject.OBJECT_DIMENSIONS,
                        Alpha = 0.1f,
                    }
                };
            }
        }
    }
}
