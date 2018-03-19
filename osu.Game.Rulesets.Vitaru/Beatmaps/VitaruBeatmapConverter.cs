using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Vitaru.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using System;
using osu.Game.Audio;
using System.Linq;
using osu.Game.Rulesets.Vitaru.Settings;

namespace osu.Game.Rulesets.Vitaru.Beatmaps
{
    internal class VitaruBeatmapConverter : BeatmapConverter<VitaruHitObject>
    {
        private VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);

        public static List<HitObject> HitObjectList = new List<HitObject>();

        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasPosition) };

        private float ar;
        private float cs;

        protected override IEnumerable<VitaruHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
        {
            var curveData = original as IHasCurve;
            var endTimeData = original as IHasEndTime;
            var positionData = original as IHasPosition;
            var comboData = original as IHasCombo;

            List<SampleInfo> samples = original.Samples;

            int difficulty = 2;
            if (currentGameMode == VitaruGamemode.Dodge)
                difficulty = 1;

            ar = calculateAr(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate);
            cs = calculateCs(beatmap.BeatmapInfo.BaseDifficulty.CircleSize);

            bool isLine = samples.Any(s => s.Name == SampleInfo.HIT_WHISTLE);
            bool isTriangleWave = samples.Any(s => s.Name == SampleInfo.HIT_FINISH);
            bool isCoolWave = samples.Any(s => s.Name == SampleInfo.HIT_CLAP);

            Pattern p;
            Pattern a;

            if (curveData != null)
            {
                if (isLine)
                {
                    p = new Pattern
                    {
                        Ar = ar,
                        Cs = cs,
                        StartTime = original.StartTime,
                        Position = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        PatternID = 2,
                        PatternAngleDegree = 180,
                        PatternSpeed = 0.25f,
                        PatternDifficulty = difficulty,
                        PatternBulletDiameter = 8f * difficulty,
                        PatternTeam = 1,
                        EnemyHealth = 60,
                        ControlPoints = curveData.ControlPoints,
                        CurveType = curveData.CurveType,
                        Distance = curveData.Distance,
                        RepeatSamples = curveData.RepeatSamples,
                        RepeatCount = curveData.RepeatCount,
                        NewCombo = comboData?.NewCombo ?? false,
                        IsSlider = true,
                    };
                }
                else if (isTriangleWave)
                {
                    p = new Pattern
                    {
                        Ar = ar,
                        Cs = cs,
                        StartTime = original.StartTime,
                        Position = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        PatternID = 3,
                        PatternAngleDegree = 180,
                        PatternSpeed = 0.25f,
                        PatternDifficulty = difficulty,
                        PatternBulletDiameter = 8f * difficulty,
                        PatternTeam = 1,
                        EnemyHealth = 60,
                        ControlPoints = curveData.ControlPoints,
                        CurveType = curveData.CurveType,
                        Distance = curveData.Distance,
                        RepeatSamples = curveData.RepeatSamples,
                        RepeatCount = curveData.RepeatCount,
                        NewCombo = comboData?.NewCombo ?? false,
                        IsSlider = true,
                    };
                }
                else if (isCoolWave)
                {
                    p = new Pattern
                    {
                        Ar = ar,
                        Cs = cs,
                        StartTime = original.StartTime,
                        Position = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        PatternID = 4,
                        PatternAngleDegree = 180,
                        PatternSpeed = 0.25f,
                        PatternDifficulty = difficulty,
                        PatternBulletDiameter = 6f * difficulty,
                        PatternTeam = 1,
                        EnemyHealth = 60,
                        ControlPoints = curveData.ControlPoints,
                        CurveType = curveData.CurveType,
                        Distance = curveData.Distance,
                        RepeatSamples = curveData.RepeatSamples,
                        RepeatCount = curveData.RepeatCount,
                        NewCombo = comboData?.NewCombo ?? false,
                        IsSlider = true,
                    };
                }
                else
                {
                    p = new Pattern
                    {
                        Ar = ar,
                        Cs = cs,
                        StartTime = original.StartTime,
                        Position = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        PatternID = 1,
                        PatternAngleDegree = 180,
                        PatternSpeed = 0.20f,
                        PatternDifficulty = difficulty,
                        PatternBulletDiameter = 8f * difficulty,
                        PatternTeam = 1,
                        EnemyHealth = 60,
                        ControlPoints = curveData.ControlPoints,
                        CurveType = curveData.CurveType,
                        Distance = curveData.Distance,
                        RepeatSamples = curveData.RepeatSamples,
                        RepeatCount = curveData.RepeatCount,
                        NewCombo = comboData?.NewCombo ?? false,
                        IsSlider = true,
                    };
                }
            }
            else if (endTimeData != null)
            {
                p = new Pattern
                {
                    Ar = ar,
                    Cs = cs,
                    StartTime = original.StartTime,
                    Position = positionData?.Position ?? Vector2.Zero,
                    Samples = original.Samples,
                    IsSpinner = true,
                    PatternSpeed = 0.25f,
                    PatternBulletDiameter = 8f * difficulty,
                    PatternTeam = 1,
                    EnemyHealth = 120,
                    PatternDamage = 5,
                    PatternID = 5,
                    EndTime = endTimeData.EndTime,
                    PatternDifficulty = 2 * difficulty,
                };
            }
            else
            {
                if (isLine)
                {
                    p = new Pattern
                    {
                        Ar = ar,
                        Cs = cs,
                        StartTime = original.StartTime,
                        Position = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        PatternID = 2,
                        PatternAngleDegree = 180,
                        PatternSpeed = 0.2f,
                        PatternDifficulty = difficulty * 2,
                        PatternDamage = 8,
                        PatternBulletDiameter = 10f * difficulty,
                        PatternTeam = 1,
                        NewCombo = comboData?.NewCombo ?? false
                    };
                }
                else if (isTriangleWave)
                {
                    p = new Pattern
                    {
                        Ar = ar,
                        Cs = cs,
                        StartTime = original.StartTime,
                        Position = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        PatternID = 3,
                        PatternAngleDegree = 180,
                        PatternSpeed = 0.3f,
                        PatternDifficulty = difficulty,
                        PatternBulletDiameter = 10f * difficulty,
                        PatternTeam = 1,
                        NewCombo = comboData?.NewCombo ?? false
                    };
                }
                else if (isCoolWave)
                {
                    p = new Pattern
                    {
                        Ar = ar,
                        Cs = cs,
                        StartTime = original.StartTime,
                        Position = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        PatternID = 4,
                        PatternAngleDegree = 180,
                        PatternSpeed = 0.18f,
                        PatternDifficulty = difficulty,
                        PatternBulletDiameter = 10f * difficulty,
                        PatternTeam = 1,
                        NewCombo = comboData?.NewCombo ?? false
                    };
                }
                else
                {
                    p = new Pattern
                    {
                        Ar = ar,
                        Cs = cs,
                        StartTime = original.StartTime,
                        Position = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        PatternID = 1,
                        PatternAngleDegree = 180,
                        PatternSpeed = 0.28f,
                        PatternDifficulty = difficulty,
                        PatternBulletDiameter = 12f * difficulty,
                        PatternTeam = 1,
                        NewCombo = comboData?.NewCombo ?? false
                    };
                }
            }

            a = p;
            HitObjectList.Add(a);
            yield return p;
        }

        private float calculateAr(float ar)
        {
            if (ar >= 5)
            {
                this.ar = 1200 - ((ar - 5) * 150);
                return this.ar;
            }
            else
            {
                this.ar = 1800 - (ar * 120);
                return this.ar;
            }
        }

        private float calculateCs(float cs)
        {
            this.cs = cs / 4;
            return this.cs;
        }
    }
}
