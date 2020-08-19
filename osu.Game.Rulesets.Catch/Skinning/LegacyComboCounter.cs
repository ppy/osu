// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning
{
    public class LegacyComboCounter : CompositeDrawable, ICatchComboCounter
    {
        private readonly ISkin skin;

        private readonly string fontName;
        private readonly float fontOverlap;

        private readonly LegacyRollingCounter counter;
        private LegacyRollingCounter lastExplosion;

        public LegacyComboCounter(ISkin skin, string fontName, float fontOverlap)
        {
            this.skin = skin;

            this.fontName = fontName;
            this.fontOverlap = fontOverlap;

            AutoSizeAxes = Axes.Both;

            Alpha = 0f;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0.8f);

            InternalChild = counter = new LegacyRollingCounter(skin, fontName, fontOverlap)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        private IBindable<bool> isBreakTime;

        [Resolved(canBeNull: true)]
        private GameplayBeatmap beatmap { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isBreakTime = beatmap?.IsBreakTime.GetBoundCopy();
            isBreakTime?.BindValueChanged(b =>
            {
                if (b.NewValue)
                    this.FadeOut(400.0, Easing.OutQuint);
            });
        }

        public void DisplayInitialCombo(int combo) => updateCombo(combo, null, true);
        public void UpdateCombo(int combo, Color4? hitObjectColour) => updateCombo(combo, hitObjectColour, false);

        private void updateCombo(int combo, Color4? hitObjectColour, bool immediate)
        {
            // Combo fell to zero, roll down and fade out the counter.
            if (combo == 0)
            {
                counter.Current.Value = 0;
                if (lastExplosion != null)
                    lastExplosion.Current.Value = 0;

                this.FadeOut(immediate ? 0.0 : 400.0, Easing.Out);
                return;
            }

            // There may still be previous transforms being applied, finish them and remove explosion.
            FinishTransforms(true);
            if (lastExplosion != null)
                RemoveInternal(lastExplosion);

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

            var explosion = new LegacyRollingCounter(skin, fontName, fontOverlap)
            {
                Alpha = 0.65f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(1.5f),
                Colour = hitObjectColour ?? Color4.White,
                Depth = 1f,
            };

            AddInternal(explosion);

            explosion.SetCountWithoutRolling(combo);
            explosion.ScaleTo(1.9f, 400.0, Easing.Out)
                     .FadeOut(400.0)
                     .Expire(true);

            lastExplosion = explosion;
        }

        private class LegacyRollingCounter : RollingCounter<int>
        {
            private readonly ISkin skin;

            private readonly string fontName;
            private readonly float fontOverlap;

            protected override bool IsRollingProportional => true;

            public LegacyRollingCounter(ISkin skin, string fontName, float fontOverlap)
            {
                this.skin = skin;
                this.fontName = fontName;
                this.fontOverlap = fontOverlap;
            }

            public override void Increment(int amount) => Current.Value += amount;

            protected override double GetProportionalDuration(int currentValue, int newValue)
            {
                return Math.Abs(newValue - currentValue) * 75.0;
            }

            protected override OsuSpriteText CreateSpriteText()
            {
                return new LegacySpriteText(skin, fontName)
                {
                    Spacing = new Vector2(-fontOverlap, 0f)
                };
            }
        }
    }
}
