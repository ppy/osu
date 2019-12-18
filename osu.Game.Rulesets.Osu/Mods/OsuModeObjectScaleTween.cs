// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Adjusts the size of hit objects during their fade in animation.
    /// </summary>
    public abstract class OsuModeObjectScaleTween : Mod, IReadFromConfig, IApplicableToDrawableHitObjects
    {
        public override ModType Type => ModType.Fun;

        public override double ScoreMultiplier => 1;

        protected virtual float StartScale => 1;

        protected virtual float EndScale => 1;

        private Bindable<bool> increaseFirstObjectVisibility = new Bindable<bool>();

        public override Type[] IncompatibleMods => new[] { typeof(OsuModSpinIn), typeof(OsuModTraceable) };

        public void ReadFromConfig(OsuConfigManager config)
        {
            increaseFirstObjectVisibility = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility);
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables.Skip(increaseFirstObjectVisibility.Value ? 1 : 0))
            {
                switch (drawable)
                {
                    case DrawableSpinner _:
                        continue;

                    default:
                        drawable.ApplyCustomUpdateState += ApplyCustomState;
                        break;
                }
            }
        }

        protected virtual void ApplyCustomState(DrawableHitObject drawable, ArmedState state)
        {
            var h = (OsuHitObject)drawable.HitObject;

            // apply grow effect
            switch (drawable)
            {
                case DrawableSliderHead _:
                case DrawableSliderTail _:
                    // special cases we should *not* be scaling.
                    break;

                case DrawableSlider _:
                case DrawableHitCircle _:
                {
                    using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                        drawable.ScaleTo(StartScale).Then().ScaleTo(EndScale, h.TimePreempt, Easing.OutSine);
                    break;
                }
            }

            // remove approach circles
            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we don't want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                        circle.ApproachCircle.Hide();
                    break;
            }
        }
    }
}
