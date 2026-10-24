// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Audio;

namespace osu.Game.Skinning.Select
{
    public partial class LegacyOldBackButtonPiece : CompositeDrawable
    {
        private Drawable? sprite;
        private Drawable? glow;

        private SkinnableSound hoverSound = null!;
        private SkinnableSound clickSound = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            AutoSizeAxes = Axes.Both;

            sprite = skin.GetAnimation("menu-back", true, true, applyConfigFrameRate: true);

            if (sprite != null)
            {
                glow = skin.GetAnimation("menu-back", true, true, applyConfigFrameRate: true)!;
                glow.Alpha = 0f;
                glow.Blending = BlendingParameters.Additive;

                InternalChildren = new[] { sprite, glow };
            }

            AddInternal(hoverSound = new SkinnableSound(new SampleInfo(@"back-button-hover", @"menuclick")));
            AddInternal(clickSound = new SkinnableSound(new SampleInfo(@"back-button-click", @"menuback")));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverSound.Play();
            updateDisplay();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateDisplay();
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            clickSound.Play();
            return base.OnClick(e);
        }

        private void updateDisplay()
        {
            glow?.FadeTo(IsHovered ? 0.4f : 0f, 250);
        }
    }
}
