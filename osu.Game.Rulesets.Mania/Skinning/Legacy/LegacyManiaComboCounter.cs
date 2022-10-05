// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public class LegacyManiaComboCounter : LegacyComboCounter
    {
        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            Y = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.ComboPosition)?.Value ?? 0;

            DisplayedCountText.Anchor = Anchor.Centre;
            DisplayedCountText.Origin = Anchor.Centre;

            PopOutCountText.Anchor = Anchor.Centre;
            PopOutCountText.Origin = Anchor.Centre;
            PopOutCountText.Colour = skin.GetManiaSkinConfig<Color4>(LegacyManiaSkinConfigurationLookups.ComboBreakColour)?.Value ?? Color4.Red;
        }

        protected override void OnCountIncrement()
        {
            base.OnCountIncrement();

            PopOutCountText.Hide();
            DisplayedCountText.ScaleTo(new Vector2(1f, 1.4f))
                              .ScaleTo(new Vector2(1f), 300, Easing.Out)
                              .FadeIn(120);
        }

        protected override void OnCountChange()
        {
            base.OnCountChange();

            PopOutCountText.Hide();
            DisplayedCountText.ScaleTo(1f);
        }

        protected override void OnCountRolling()
        {
            if (DisplayedCount > 0)
            {
                PopOutCountText.Text = FormatCount(DisplayedCount);
                PopOutCountText.FadeTo(0.8f).FadeOut(200)
                               .ScaleTo(1f).ScaleTo(4f, 200);

                DisplayedCountText.FadeTo(0.5f, 300);
            }

            base.OnCountRolling();
        }
    }
}
