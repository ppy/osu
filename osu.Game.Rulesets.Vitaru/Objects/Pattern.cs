using OpenTK;
using osu.Game.Audio;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Beatmaps;
using System;

namespace osu.Game.Rulesets.Vitaru.Objects
{
    public class Pattern : VitaruHitObject
    {
        public override HitObjectType Type => HitObjectType.Pattern;

        /// <summary>
        /// All Pattern specific stuff
        /// </summary>
        #region Pattern
        public int PatternID { get; set; }
        public float PatternSpeed { get; set; }
        public float PatternDifficulty { get; set; } = 1;
        private float patternAngleRadian { get; set; } = -10;
        public float PatternAngleDegree { get; set; }
        public float PatternBulletDiameter { get; set; } = 4;
        public float PatternDamage { get; set; } = 10;
        private bool dynamicPatternVelocity { get; } = false;
        public int PatternTeam { get; set; }
        private int totalBullets;
        private bool shootPlayer;
        #endregion

        /// <summary>
        /// All Slider specific stuff
        /// </summary>
        #region Slider
        public bool IsSlider { get; set; } = false;
        public List<List<SampleInfo>> RepeatSamples { get; set; } = new List<List<SampleInfo>>();
        private const float base_scoring_distance = 100;
        public double Duration => EndTime - StartTime;
        public readonly SliderCurve Curve = new SliderCurve();
        public int RepeatCount { get; set; } = 1;
        public double Velocity;

        public override Vector2 EndPosition => PositionAt(1);
        public Vector2 PositionAt(double progress) => Curve.PositionAt(ProgressAt(progress));

        public double ProgressAt(double progress)
        {
            double p = progress * RepeatCount % 1;
            if (RepeatAt(progress) % 2 == 1)
                p = 1 - p;
            return p;
        }

        public int RepeatAt(double progress) => (int)(progress * RepeatCount);

        public List<Vector2> ControlPoints
        {
            get { return Curve.ControlPoints; }
            set { Curve.ControlPoints = value; }
        }

        public CurveType CurveType
        {
            get { return Curve.CurveType; }
            set { Curve.CurveType = value; }
        }

        public double Distance
        {
            get { return Curve.Distance; }
            set { Curve.Distance = value; }
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
        }
        #endregion

        /// <summary>
        /// All Spinner specific stuff
        /// </summary>
        #region Spinner
        public bool IsSpinner { get; set; }
        public double EndTime { get; set; }
        #endregion

        #region Bullet Loading
        public int GetTotalBullets()
        {
            switch (PatternID)
            {
                case 1:
                    totalBullets += (int)PatternDifficulty * 2 + 1;
                    break;
                case 2:
                    totalBullets += (int)PatternDifficulty + 1;
                    break;
                case 3:
                    totalBullets += (int)(PatternDifficulty + 2) / 2;
                    break;
                case 4:
                    totalBullets += (int)(PatternDifficulty * 2) + 3;
                    break;
                case 5:
                    totalBullets += (int)(30 * (PatternDifficulty / 3) * (Duration / 1000));
                    break;
            }

            return totalBullets;
        }

        public float EnemyHealth { get; set; } = 40;

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            createBullets();
        }

        private void createBullets()
        {

            var length = Curve.Distance;
            var repeatPointDistance = Math.Min(Distance, length);
            var repeatDuration = length / Velocity;
            int repeatCount = RepeatCount;

            if (IsSlider)
            {
                repeatCount += 1;
                bool sliderStart = false;
                for (var repeat = 0; repeat < repeatCount; repeat++)
                {
                    sliderStart = !sliderStart;
                    for (var d = repeatPointDistance; d <= length; d += repeatPointDistance)
                    {
                        var repeatStartTime = StartTime + repeat * repeatDuration;
                        var distanceProgress = d / length;

                        IEnumerable<Bullet> bullets = createPattern();

                        foreach (Bullet b in bullets)
                        {
                            if (IsSlider)
                            {
                                b.StartTime = repeatStartTime;

                                b.Position = Curve.PositionAt(!sliderStart ? distanceProgress : 0);
                            }

                            b.NewCombo = NewCombo;
                            b.Ar = Ar;
                            b.Cs = Cs;
                            b.StackHeight = StackHeight;

                            b.ShootPlayer = shootPlayer;

                            AddNested(b);
                        }
                    }
                }
            }
            else
            {
                IEnumerable<Bullet> bullets = createPattern();

                foreach (Bullet b in bullets)
                {
                    b.NewCombo = NewCombo;
                    b.Ar = Ar;
                    b.Cs = Cs;
                    b.StackHeight = StackHeight;

                    b.ShootPlayer = shootPlayer;

                    AddNested(b);
                }
            }
        }

        private IEnumerable<Bullet> createPattern()
        {
            if (patternAngleRadian == -10)
                patternAngleRadian = MathHelper.DegreesToRadians(PatternAngleDegree - 90);

            float bulletDiameter = PatternBulletDiameter;
            bulletDiameter += Cs;

            GetTotalBullets();

            switch (PatternID)
            {
                default:
                    shootPlayer = false;
                    return patternWave(bulletDiameter);
                case 1:
                    shootPlayer = false;
                    return patternWave(bulletDiameter);
                case 2:
                    shootPlayer = true;
                    return PatternLine(bulletDiameter);
                case 3:
                    shootPlayer = true;
                    return PatternTriangleWave(bulletDiameter);
                case 4:
                    shootPlayer = false;
                    return PatternCoolWave(bulletDiameter);
                case 5:
                    shootPlayer = true;
                    //should be PatternSpin() once its fixed
                    return patternWave(bulletDiameter);
            }
        }

        /// <summary>
        /// These will be the base patterns
        /// </summary>
        private List<Bullet> patternWave(float diameter)
        {
            List<Bullet> bullets = new List<Bullet>();
            int numberOfBullets = (int)PatternDifficulty * 2 + 1;
            float directionModifier = -0.1f * ((float)(numberOfBullets - 1) / 2);
            for (int i = 1; i <= numberOfBullets; i++)
            {
                float angle = directionModifier + patternAngleRadian;
                bullets.Add(new Bullet
                {
                    StartTime = StartTime,
                    Position = Position,
                    ComboColour = ComboColour,
                    BulletSpeed = PatternSpeed,
                    BulletAngleRadian = angle,
                    BulletDiameter = diameter,
                    BulletDamage = PatternDamage,
                    DynamicBulletVelocity = dynamicPatternVelocity,
                    Team = 1,
                    Ghost = i == ((numberOfBullets - 1) / 2) + 1
                });
                directionModifier += 0.1f;
            }
            return bullets;
        }
        public List<Bullet> PatternLine(float diameter)
        {
            List<Bullet> bullets = new List<Bullet>();
            int numberbullets = (int)PatternDifficulty + 1;
            float speed = PatternSpeed;
            for (int i = 1; i <= numberbullets; i++)
            {
                bullets.Add(new Bullet
                {
                    StartTime = StartTime,
                    Position = Position,
                    ComboColour = ComboColour,
                    BulletSpeed = speed,
                    BulletAngleRadian = patternAngleRadian,
                    BulletDiameter = diameter,
                    BulletDamage = PatternDamage,
                    DynamicBulletVelocity = dynamicPatternVelocity,
                    Team = 1,
                });
                speed += 0.14f;
            }
            return bullets;
        }
        public List<Bullet> PatternCoolWave(float diameter)
        {
            List<Bullet> bullets = new List<Bullet>();
            int numberbullets = (int)(PatternDifficulty * 2) + 3;
            float speedModifier = 0.02f * (PatternDifficulty);
            float directionModifier = -0.15f * (PatternDifficulty);
            for (int i = 1; i <= numberbullets; i++)
            {
                PatternSpeed = PatternSpeed + Math.Abs(speedModifier);
                float angle = directionModifier + patternAngleRadian;
                bullets.Add(new Bullet
                {
                    StartTime = StartTime,
                    Position = Position,
                    ComboColour = ComboColour,
                    BulletSpeed = PatternSpeed,
                    BulletAngleRadian = angle,
                    BulletDiameter = diameter,
                    BulletDamage = PatternDamage,
                    DynamicBulletVelocity = dynamicPatternVelocity,
                    Team = 1,
                });
                speedModifier -= 0.01f;
                directionModifier += 0.075f;
            }
            return bullets;
        }
        public List<Bullet> PatternTriangleWave(float diameter)
        {
            List<Bullet> bullets = new List<Bullet>();
            int numberwaves = (int)(PatternDifficulty + 2) / 2;
            float originalDirection = 0f;
            double duration = Duration / numberwaves;
            for (int i = 1; i <= numberwaves; i++)
            {
                var numberbullets = i;
                var speedModifier = 0.30f - (i - 1) * 0.03f;
                for (int j = 1; j <= numberbullets; j++)
                {
                    float directionModifier = ((j - 1) * 0.1f);
                    var speed = PatternSpeed + speedModifier;
                    float angle = patternAngleRadian + (originalDirection - directionModifier);
                    bullets.Add(new Bullet
                    {
                        StartTime = StartTime,
                        Position = Position,
                        ComboColour = ComboColour,
                        BulletSpeed = speed,
                        BulletAngleRadian = angle,
                        BulletDiameter = diameter,
                        BulletDamage = PatternDamage,
                        DynamicBulletVelocity = dynamicPatternVelocity,
                        Team = 1,
                    });
                }
                originalDirection = 0.05f * i;
            }
            return bullets;
        }

        public List<Bullet> PatternCurve(float diameter)
        {
            List<Bullet> bullets = new List<Bullet>();
            int numberbullets = (int)(PatternDifficulty + 10) / 2;
            float originalDirection = 0.01f * ((float)numberbullets / 2);
            float speedModifier = 0f;
            float directionModifier = 0f;
            for (int i = 1; i <= numberbullets; i++)
            {
                var speed = PatternSpeed + speedModifier;
                patternAngleRadian = patternAngleRadian - originalDirection + directionModifier;
                directionModifier += 0.015f;
                speedModifier -= (i * 0.002f);
                bullets.Add(new Bullet
                {
                    StartTime = StartTime,
                    Position = Position,
                    ComboColour = ComboColour,
                    BulletSpeed = speed,
                    BulletAngleRadian = patternAngleRadian,
                    BulletDiameter = diameter,
                    BulletDamage = PatternDamage,
                    DynamicBulletVelocity = dynamicPatternVelocity,
                    Team = 1,
                });
            }
            return bullets;
        }
        public List<Bullet> PatternCircle(float diameter)
        {
            List<Bullet> bullets = new List<Bullet>();
            int numberbullets = (int)(PatternDifficulty + 1) * 8;
            float directionModifier = (360f / numberbullets);
            directionModifier = MathHelper.DegreesToRadians(directionModifier);
            for (int i = 1; i <= numberbullets; i++)
            {
                patternAngleRadian = patternAngleRadian + (directionModifier * (i - 1));
                bullets.Add(new Bullet
                {
                    StartTime = StartTime,
                    Position = Position,
                    ComboColour = ComboColour,
                    BulletSpeed = PatternSpeed,
                    BulletAngleRadian = patternAngleRadian,
                    BulletDiameter = diameter,
                    BulletDamage = PatternDamage,
                    DynamicBulletVelocity = dynamicPatternVelocity,
                    Team = 1,
                });
            }
            return bullets;
        }

        //Finds what direction the player is
        public float PlayerRelativePositionAngle(Vector2 playerPos, Vector2 enemyPos)
        {
            //Returns a Radian
            var playerAngle = (float)Math.Atan2((playerPos.Y - enemyPos.Y), (playerPos.X - enemyPos.X));
            return playerAngle;
        }
        #endregion
    }
}
