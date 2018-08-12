// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using OpenTK;
using osu.Game.Rulesets.Scoring;
using System;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTaiko : Mod, IApplicableToDrawableHitObjects, IApplicableToHitObject
    {
        public override string Name => "Taiko";
        public override string ShortenedName => "TK";
        public override FontAwesome Icon => FontAwesome.fa_osu_taiko_o;
        public override string Description => "Hit blue with left, hit red with right";
        public override double ScoreMultiplier => 1;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => true;


        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            OsuColour colours = new OsuColour();
            foreach (var d in drawables.OfType<DrawableOsuHitObject>())
            {
                applyColour(d);
                DrawableSlider ds = d as DrawableSlider;
                if (ds != null)
                {
                    applyColour(ds.HeadCircle);
                }
            }

            void applyColour(DrawableOsuHitObject d)
            {
                if (d.HitObject.Samples.Any(s => s.Name == SampleInfo.HIT_CLAP || s.Name == SampleInfo.HIT_WHISTLE))
                {
                    d.AccentColour = colours.BlueDarker;
                }
                else
                {
                    d.AccentColour = colours.PinkDarker;
                }
            }
        }
        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            applyHitAction(osuObject);
            foreach (var nestedHitObject in hitObject.NestedHitObjects)
            {
                OsuHitObject nh = nestedHitObject as OsuHitObject;
                if (nh != null)
                    applyHitAction(nh);
            }

            void applyHitAction(OsuHitObject h)
            {
                if (h.Samples.Any(s => s.Name == SampleInfo.HIT_CLAP || s.Name == SampleInfo.HIT_WHISTLE))
                {
                    h.HitActions = new OsuAction[] { OsuAction.LeftButton };
                }
                else
                {
                    h.HitActions = new OsuAction[] { OsuAction.RightButton };
                }
            }
        }
    }
}
