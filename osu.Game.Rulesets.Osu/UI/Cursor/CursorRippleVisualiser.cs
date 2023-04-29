// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager? rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.ShowCursorRipples, showRipples);
        }

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (showRipples.Value)
            {
                var ripple = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.CursorRipple), _ => new DefaultCursorRipple())
                {
                    Blending = BlendingParameters.Additive,
                    Position = e.MousePosition
                };

                AddInternal(ripple);

                ripple.ScaleTo(0.05f)
                      .ScaleTo(0.5f, 700, Easing.Out)
                      .FadeTo(0.2f)
                      .FadeOut(700)
                      .Expire();
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
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
