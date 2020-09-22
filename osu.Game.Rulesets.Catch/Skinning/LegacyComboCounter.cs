// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using static osu.Game.Skinning.LegacySkinConfiguration;

namespace osu.Game.Rulesets.Catch.Skinning
{
    /// <summary>
    /// A combo counter implementation that visually behaves almost similar to osu!stable's combo counter.
    /// </summary>
    public class LegacyComboCounter : CompositeDrawable, ICatchComboCounter
    {
        private readonly LegacyRollingCounter counter;

        private readonly LegacyRollingCounter explosion;

        public LegacyComboCounter(ISkin skin)
        {
            var fontName = skin.GetConfig<LegacySetting, string>(LegacySetting.ComboPrefix)?.Value ?? "score";
            var fontOverlap = skin.GetConfig<LegacySetting, float>(LegacySetting.ComboOverlap)?.Value ?? -2f;

            AutoSizeAxes = Axes.Both;

            Alpha = 0f;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0.8f);

            InternalChildren = new Drawable[]
            {
                explosion = new LegacyRollingCounter(skin, fontName, fontOverlap)
                {
                    Alpha = 0.65f,
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1.5f),
                },
                counter = new LegacyRollingCounter(skin, fontName, fontOverlap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        public void DisplayInitialCombo(int combo) => updateCombo(combo, null, true);
        public void UpdateCombo(int combo, Color4? hitObjectColour) => updateCombo(combo, hitObjectColour, false);

        private void updateCombo(int combo, Color4? hitObjectColour, bool immediate)
        {
            // There may still be existing transforms to the counter (including value change after 250ms),
            // finish them immediately before new transforms.
            counter.FinishTransforms();

            // Combo fell to zero, roll down and fade out the counter.
            if (combo == 0)
            {
                counter.Current.Value = 0;
                explosion.Current.Value = 0;

                this.FadeOut(immediate ? 0.0 : 400.0, Easing.Out);
                return;
            }

            this.FadeIn().Delay(1000.0).FadeOut(300.0);

            // For simplicity, in the case of rewinding we'll just set the counter to the current combo value.
            immediate |= Time.Elapsed < 0;

            if (immediate)
            {
                counter.SetCountWithoutRolling(combo);
                return;
            }

            counter.ScaleTo(1.5f).ScaleTo(0.8f, 250.0, Easing.Out)
                   .OnComplete(c => c.SetCountWithoutRolling(combo));

            counter.Delay(250.0).ScaleTo(1f).ScaleTo(1.1f, 60.0).Then().ScaleTo(1f, 30.0);

            explosion.Colour = hitObjectColour ?? Color4.White;

            explosion.SetCountWithoutRolling(combo);
            explosion.ScaleTo(1.5f).ScaleTo(1.9f, 400.0, Easing.Out)
                     .FadeOutFromOne(400.0);
        }
    }
}
