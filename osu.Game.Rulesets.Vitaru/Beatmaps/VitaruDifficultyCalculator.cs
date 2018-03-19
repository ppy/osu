using osu.Game.Beatmaps;
using osu.Game.Rulesets.Vitaru.Objects;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Vitaru.Beatmaps
{
    /// <summary>
    /// Most of this is copied from OsuDifficultyCalculator ATM
    /// </summary>
    public class VitaruDifficultyCalculator : DifficultyCalculator<VitaruHitObject>
    {
        private const double star_scaling_factor = 0.0675;

        internal List<VitaruHitObjectDifficulty> DifficultyHitObjects = new List<VitaruHitObjectDifficulty>();

        public VitaruDifficultyCalculator(Beatmap beatmap, Mod[] mods) : base(beatmap, mods) { }

        protected override void PreprocessHitObjects()
        {
            //foreach (Pattern h in Beatmap.HitObjects)
                //h.Curve?.Calculate();
        }

        protected override BeatmapConverter<VitaruHitObject> CreateBeatmapConverter(Beatmap beatmap) => new VitaruBeatmapConverter();

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            // Fill our custom DifficultyHitObject class, that carries additional information
            DifficultyHitObjects.Clear();

            foreach (VitaruHitObject hitObject in Beatmap.HitObjects)
                DifficultyHitObjects.Add(new VitaruHitObjectDifficulty(hitObject));

            // Sort DifficultyHitObjects by StartTime of the HitObjects - just to make sure.
            DifficultyHitObjects.Sort((a, b) => a.BaseHitObject.StartTime.CompareTo(b.BaseHitObject.StartTime));

            if (!CalculateStrainValues()) return 0;

            double speedDifficulty = CalculateDifficulty(DifficultyType.Speed) * 0.75f;
            double aimDifficulty = CalculateDifficulty(DifficultyType.Aim) * 1.5f;

            double speedStars = Math.Sqrt(speedDifficulty) * star_scaling_factor;
            double aimStars = Math.Sqrt(aimDifficulty) * star_scaling_factor;

            double starRating = aimStars + speedStars + Math.Abs(aimStars - speedStars) / 2;

            if (categoryDifficulty != null)
            {
                categoryDifficulty.Add("Aim", aimStars);
                categoryDifficulty.Add("Speed", speedStars);
            }

            return starRating;
        }

        protected bool CalculateStrainValues()
        {
            // Traverse hitObjects in pairs to calculate the strain value of NextHitObject from the strain value of CurrentHitObject and environment.
            using (List<VitaruHitObjectDifficulty>.Enumerator hitObjectsEnumerator = DifficultyHitObjects.GetEnumerator())
            {

                if (!hitObjectsEnumerator.MoveNext()) return false;

                VitaruHitObjectDifficulty current = hitObjectsEnumerator.Current;

                // First hitObject starts at strain 1. 1 is the default for strain values, so we don't need to set it here. See DifficultyHitObject.
                while (hitObjectsEnumerator.MoveNext())
                {
                    var next = hitObjectsEnumerator.Current;
                    next?.CalculateStrains(current, TimeRate);
                    current = next;
                }

                return true;
            }
        }

        protected const double STRAIN_STEP = 200;
        protected const double DECAY_WEIGHT = 0.75;

        protected double CalculateDifficulty(DifficultyType type)
        {
            double actualStrainStep = STRAIN_STEP * TimeRate;

            List<double> highestStrains = new List<double>();
            double intervalEndTime = actualStrainStep;
            double maximumStrain = 0;

            VitaruHitObjectDifficulty previousHitObject = null;
            foreach (VitaruHitObjectDifficulty hitObject in DifficultyHitObjects)
            {
                while (hitObject.BaseHitObject.StartTime > intervalEndTime)
                {
                    highestStrains.Add(maximumStrain);

                    // The maximum strain of the next interval is not zero by default! We need to take the last hitObject we encountered, take its strain and apply the decay
                    // until the beginning of the next interval.
                    if (previousHitObject == null)
                    {
                        maximumStrain = 0;
                    }
                    else
                    {
                        double decay = Math.Pow(VitaruHitObjectDifficulty.DECAY_BASE[(int)type], (intervalEndTime - previousHitObject.BaseHitObject.StartTime) / 1000);
                        maximumStrain = previousHitObject.Strains[(int)type] * decay;
                    }

                    // Go to the next time interval
                    intervalEndTime += actualStrainStep;
                }

                // Obtain maximum strain
                maximumStrain = Math.Max(hitObject.Strains[(int)type], maximumStrain);

                previousHitObject = hitObject;
            }

            // Build the weighted sum over the highest strains for each interval
            double difficulty = 0;
            double weight = 1;
            highestStrains.Sort((a, b) => b.CompareTo(a)); // Sort from highest to lowest strain.

            foreach (double strain in highestStrains)
            {
                difficulty += weight * strain;
                weight *= DECAY_WEIGHT;
            }

            return difficulty;
        }

        public enum DifficultyType
        {
            Speed = 0,
            Aim,
        };
    }
}
