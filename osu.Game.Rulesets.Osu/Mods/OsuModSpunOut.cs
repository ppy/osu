// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSpunOut : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Spun Out";
        public override string Acronym => "SO";
        public override IconUsage? Icon => OsuIcon.ModSpunout;
        public override ModType Type => ModType.Automation;
        public override string Description => @"Spinners will be automatically completed.";
        public override double ScoreMultiplier => 0.9;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(OsuModAutopilot) };

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var hitObject in drawables)
            {
                if (hitObject is DrawableSpinner spinner)
                {
                    spinner.Disc.Enabled = false;
                    spinner.OnUpdate += autoSpin;
                }
            }
        }

        private void autoSpin(Drawable drawable)
        {
            if (drawable is DrawableSpinner spinner)
            {
                if (spinner.Disc.Valid)
                    spinner.Disc.Rotate(180 / MathF.PI * (float)spinner.Clock.ElapsedFrameTime / 40);
                if (!spinner.SpmCounter.IsPresent)
                    spinner.SpmCounter.FadeIn(spinner.HitObject.TimeFadeIn);
            }
        }
    }
}
