// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableComboCounter : SkinnableDrawable, IComboCounter
    {
        public SkinnableComboCounter()
            : base(new HUDSkinComponent(HUDSkinComponents.ComboCounter), createDefault)
        {
        }

        private IComboCounter skinnedCounter;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            // todo: unnecessary?
            if (skinnedCounter != null)
            {
                Current.UnbindFrom(skinnedCounter.Current);
            }

            base.SkinChanged(skin, allowFallback);

            // temporary layout code, will eventually be replaced by the skin layout system.
            if (Drawable is SimpleComboCounter)
            {
                Drawable.BypassAutoSizeAxes = Axes.X;
                Drawable.Anchor = Anchor.TopRight;
                Drawable.Origin = Anchor.TopLeft;
                Drawable.Margin = new MarginPadding { Top = 5, Left = 20 };
            }
            else
            {
                Drawable.BypassAutoSizeAxes = Axes.X;
                Drawable.Anchor = Anchor.BottomLeft;
                Drawable.Origin = Anchor.BottomLeft;
                Drawable.Margin = new MarginPadding { Top = 5, Left = 20 };
            }

            skinnedCounter = (IComboCounter)Drawable;

            Current.BindTo(skinnedCounter.Current);
        }

        private static Drawable createDefault(ISkinComponent skinComponent) => new SimpleComboCounter();

        public Bindable<int> Current { get; } = new Bindable<int>();

        public void UpdateCombo(int combo, Color4? hitObjectColour = null) => Current.Value = combo;
    }
}
