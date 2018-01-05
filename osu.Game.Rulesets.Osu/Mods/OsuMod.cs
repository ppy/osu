// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using OpenTK;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoFail : ModNoFail
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot) }).ToArray();
    }

    public class OsuModEasy : ModEasy
    {
    }

    public class OsuModHidden : ModHidden, IApplicableToDrawableHitObjects
    {
        public override string Description => @"Play with no approach circles and fading notes for a slight score advantage.";
        public override double ScoreMultiplier => 1.06;

        private const double fade_in_duration_multiplier = 0.4;
        private const double fade_out_duration_multiplier = 0.3;

        private float preEmpt => DrawableOsuHitObject.TIME_PREEMPT;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableOsuHitObject>())
            {
                d.ApplyCustomUpdateState += ApplyHiddenState;
                d.FadeInDuration = preEmpt * fade_in_duration_multiplier;
            }
        }

        protected void ApplyHiddenState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var fadeOutStartTime = d.HitObject.StartTime - preEmpt + d.FadeInDuration;
            var fadeOutDuration = preEmpt * fade_out_duration_multiplier;

            // new duration from completed fade in to end (before fading out)
            var longFadeDuration = ((d.HitObject as IHasEndTime)?.EndTime ?? d.HitObject.StartTime) - fadeOutStartTime;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we don't want to see the approach circle
                    circle.ApproachCircle.Hide();

                    // fade out immediately after fade in.
                    using (drawable.BeginAbsoluteSequence(fadeOutStartTime, true))
                        circle.FadeOut(fadeOutDuration);
                    break;
                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(fadeOutStartTime, true))
                    {
                        slider.Body.FadeOut(longFadeDuration, Easing.Out);

                        // delay a bit less to let the sliderball fade out peacefully instead of having a hard cut
                        using (slider.BeginDelayedSequence(longFadeDuration - fadeOutDuration, true))
                            slider.Ball.FadeOut(fadeOutDuration);
                    }

                    break;
                case DrawableSpinner spinner:
                    // hide elements we don't care about.
                    spinner.Disc.Hide();
                    spinner.Ticks.Hide();
                    spinner.Background.Hide();

                    using (spinner.BeginAbsoluteSequence(fadeOutStartTime + longFadeDuration, true))
                    {
                        spinner.FadeOut(fadeOutDuration);

                        // speed up the end sequence accordingly
                        switch (state)
                        {
                            case ArmedState.Hit:
                                spinner.ScaleTo(spinner.Scale * 1.2f, fadeOutDuration * 2, Easing.Out);
                                break;
                            case ArmedState.Miss:
                                spinner.ScaleTo(spinner.Scale * 0.8f, fadeOutDuration * 2, Easing.In);
                                break;
                        }

                        spinner.Expire();
                    }

                    break;
            }
        }
    }

    public class OsuModHardRock : ModHardRock, IApplicableToHitObject<OsuHitObject>
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public void ApplyToHitObject(OsuHitObject hitObject)
        {
            hitObject.Position = new Vector2(hitObject.Position.X, OsuPlayfield.BASE_SIZE.Y - hitObject.Y);

            var slider = hitObject as Slider;
            if (slider == null)
                return;

            var newControlPoints = new List<Vector2>();
            slider.ControlPoints.ForEach(c => newControlPoints.Add(new Vector2(c.X, OsuPlayfield.BASE_SIZE.Y - c.Y)));

            slider.ControlPoints = newControlPoints;
            slider.Curve?.Calculate(); // Recalculate the slider curve
        }
    }

    public class OsuModSuddenDeath : ModSuddenDeath
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot) }).ToArray();
    }

    public class OsuModDaycore : ModDaycore
    {
        public override double ScoreMultiplier => 0.5;
    }

    public class OsuModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class OsuModRelax : ModRelax
    {
        public override string Description => "You don't need to click.\nGive your clicking/tapping finger a break from the heat of things.";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot) }).ToArray();
    }

    public class OsuModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.5;
    }

    public class OsuModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class OsuModFlashlight : ModFlashlight
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class OsuModPerfect : ModPerfect
    {
    }

    public class OsuModSpunOut : Mod
    {
        public override string Name => "Spun Out";
        public override string ShortenedName => "SO";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_spunout;
        public override string Description => @"Spinners will be automatically completed";
        public override double ScoreMultiplier => 0.9;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(OsuModAutopilot) };
    }

    public class OsuModAutopilot : Mod
    {
        public override string Name => "Autopilot";
        public override string ShortenedName => "AP";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_autopilot;
        public override string Description => @"Automatic cursor movement - just follow the rhythm.";
        public override double ScoreMultiplier => 0;
        public override bool Ranked => false;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModSpunOut), typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModNoFail), typeof(ModAutoplay) };
    }

    public class OsuModAutoplay : ModAutoplay<OsuHitObject>
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot), typeof(OsuModSpunOut) }).ToArray();

        protected override Score CreateReplayScore(Beatmap<OsuHitObject> beatmap) => new Score
        {
            Replay = new OsuAutoGenerator(beatmap).Generate()
        };
    }

    public class OsuModTarget : Mod
    {
        public override string Name => "Target";
        public override string ShortenedName => "TP";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_target;
        public override string Description => @"";
        public override double ScoreMultiplier => 1;
    }
}
