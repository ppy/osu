// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModClassic : Mod, IApplicableToHitObject
    {
        public override string Name => "Classic";

        public override string Acronym => "CL";

        public override double ScoreMultiplier => 1;

        public override IconUsage? Icon => FontAwesome.Solid.History;

        public override string Description => "Feeling nostalgic?";

        public override bool Ranked => false;

        public void ApplyToHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Slider slider:
                    slider.IgnoreJudgement = false;

                    foreach (var head in slider.NestedHitObjects.OfType<SliderHeadCircle>())
                    {
                        head.TrackFollowCircle = false;
                        head.JudgeAsNormalHitCircle = false;
                    }

                    break;
            }
        }
    }
}
