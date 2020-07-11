// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
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
                    spinner.HandleUserInput = false;
                    spinner.OnUpdate += onSpinnerUpdate;
                }
            }
        }

        private void onSpinnerUpdate(Drawable drawable)
        {
            var spinner = (DrawableSpinner)drawable;

            spinner.Disc.Tracking = true;
            spinner.Disc.Rotate(MathUtils.RadiansToDegrees((float)spinner.Clock.ElapsedFrameTime * 0.03f));
        }
    }
}
