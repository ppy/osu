// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModTraceable : Mod, IReadFromConfig, IApplicableToDrawableHitObjects
    {
        public override string Name => "Traceable";
        public override string Acronym => "TC";
        public override IconUsage Icon => FontAwesome.Brands.SnapchatGhost;
        public override ModType Type => ModType.Fun;
        public override string Description => "Put your faith in the approach circles...";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => new[] { typeof(OsuModHidden), typeof(OsuModSpinIn), typeof(OsuModeObjectScaleTween) };
        private Bindable<bool> increaseFirstObjectVisibility = new Bindable<bool>();

        public void ReadFromConfig(OsuConfigManager config)
        {
            increaseFirstObjectVisibility = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility);
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables.Skip(increaseFirstObjectVisibility.Value ? 1 : 0))
                drawable.ApplyCustomUpdateState += ApplyTraceableState;
        }

        protected void ApplyTraceableState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject drawableOsu))
                return;

            var h = drawableOsu.HitObject;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we only want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                        circle.CirclePiece.Hide();

                    break;

                case DrawableSlider slider:
                    slider.AccentColour.BindValueChanged(_ =>
                    {
                        //will trigger on skin change.
                        slider.Body.AccentColour = slider.AccentColour.Value.Opacity(0);
                        slider.Body.BorderColour = slider.AccentColour.Value;
                    }, true);

                    break;

                case DrawableSpinner spinner:
                    spinner.Disc.Hide();
                    spinner.Background.Hide();
                    break;
            }
        }
    }
}
