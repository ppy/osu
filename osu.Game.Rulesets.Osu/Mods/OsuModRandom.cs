// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Mod that randomises the positions of the <see cref="HitObject"/>s
    /// </summary>
    public class OsuModRandom : ModRandom, IApplicableToBeatmap
    {
        public override string Description => "It never gets boring!";

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModTarget)).ToArray();

        private static readonly float playfield_diagonal = OsuPlayfield.BASE_SIZE.LengthFast;

        private Random? rng;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!(beatmap is OsuBeatmap osuBeatmap))
                return;

            Seed.Value ??= RNG.Next();

            rng = new Random((int)Seed.Value);

            var positionInfos = OsuHitObjectGenerationUtils.GeneratePositionInfos(osuBeatmap.HitObjects);

            float rateOfChangeMultiplier = 0;

            foreach (var positionInfo in positionInfos)
            {
                // rateOfChangeMultiplier only changes every 5 iterations in a combo
                // to prevent shaky-line-shaped streams
                if (positionInfo.HitObject.IndexInCurrentCombo % 5 == 0)
                    rateOfChangeMultiplier = (float)rng.NextDouble() * 2 - 1;

                if (positionInfo == positionInfos.First())
                {
                    positionInfo.DistanceFromPrevious = (float)(rng.NextDouble() * OsuPlayfield.BASE_SIZE.X / 2);
                    positionInfo.RelativeAngle = (float)(rng.NextDouble() * 2 * Math.PI - Math.PI);
                }
                else
                {
                    positionInfo.RelativeAngle = rateOfChangeMultiplier * 2 * (float)Math.PI * Math.Min(1f, positionInfo.DistanceFromPrevious / (playfield_diagonal * 0.5f));
                }
            }

            osuBeatmap.HitObjects = OsuHitObjectGenerationUtils.RepositionHitObjects(positionInfos);
        }
    }
}
