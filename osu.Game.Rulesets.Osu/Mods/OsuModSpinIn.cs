// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSpinIn : Mod, IApplicableToDrawableHitObjects, IReadFromConfig
    {
        public override string Name => "Spin In";
        public override string Acronym => "SI";
        public override IconUsage Icon => FontAwesome.Solid.Undo;
        public override ModType Type => ModType.Fun;
        public override string Description => "Circles spin in. No approach circles.";
        public override double ScoreMultiplier => 1;

        // todo: this mod should be able to be compatible with hidden with a bit of further implementation.
        public override Type[] IncompatibleMods => new[] { typeof(OsuModeObjectScaleTween), typeof(OsuModHidden), typeof(OsuModTraceable) };

        private const int rotate_offset = 360;
        private const float rotate_starting_width = 2;

        private Bindable<bool> increaseFirstObjectVisibility = new Bindable<bool>();

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
                        drawable.ApplyCustomUpdateState += applyZoomState;
                        break;
                }
            }
        }

        private void applyZoomState(DrawableHitObject drawable, ArmedState state)
        {
            var h = (OsuHitObject)drawable.HitObject;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                    {
                        circle.ApproachCircle.Hide();

                        circle.RotateTo(rotate_offset).Then().RotateTo(0, h.TimePreempt, Easing.InOutSine);
                        circle.ScaleTo(new Vector2(rotate_starting_width, 0)).Then().ScaleTo(1, h.TimePreempt, Easing.InOutSine);

                        // bypass fade in.
                        if (state == ArmedState.Idle)
                            circle.FadeIn();
                    }

                    break;

                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                    {
                        slider.ScaleTo(0).Then().ScaleTo(1, h.TimePreempt, Easing.InOutSine);

                        // bypass fade in.
                        if (state == ArmedState.Idle)
                            slider.FadeIn();
                    }

                    break;
            }
        }
    }
}
