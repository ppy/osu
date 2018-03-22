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
    public class Pattern : VitaruHitObject, IHasCurve
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
        public SliderCurve Curve { get; } = new SliderCurve();
        public int RepeatCount { get; set; }
        public double Velocity;
        public double SpanDuration => Duration / this.SpanCount();

        public override Vector2 EndPosition => Position + this.CurvePositionAt(1);
        public Vector2 PositionAt(double t) => Position + this.CurvePositionAt(t);

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

            if (IsSlider)
                EndTime = StartTime + this.SpanCount() * Curve.Distance / Velocity;
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
                    totalBullets += (int)(PatternDifficulty * 4);
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
            if (IsSlider)
            {
                for (int repeatIndex = 0, repeat = 0; repeatIndex < RepeatCount + 1; repeatIndex++, repeat++)
                {
                    IEnumerable<Bullet> bullets = createPattern();

                    foreach (Bullet b in bullets)
                    {
                        if (IsSlider)
                        {
                            b.StartTime = StartTime + repeat * SpanDuration;

                            b.Position = Position + Curve.PositionAt(repeat % 2);
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
            else
            {
                IEnumerable<Bullet> bullets = createPattern();

                foreach (Bullet b in bullets)
                {
                    b.NewCombo = NewCombo;
                    b.Ar = Ar;
                    b.Cs = Cs;
                    b.StackHeight = StackHeight;

                    b.NewCombo = NewCombo;
                    b.IndexInCurrentCombo = IndexInCurrentCombo;
                    b.ComboIndex = ComboIndex;
                    b.LastInCombo = LastInCombo;

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
                    return PatternCircle(bulletDiameter);
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
        public List<Bullet> PatternCircle(float diameter)
        {
            List<Bullet> bullets = new List<Bullet>();
            int numberbullets = (int)(PatternDifficulty * 4);
            float directionModifier = (360f / numberbullets);
            float direction = MathHelper.DegreesToRadians(-90);
            directionModifier = MathHelper.DegreesToRadians(directionModifier);
            for (int i = 1; i <= numberbullets; i++)
            {
                patternAngleRadian = patternAngleRadian + (directionModifier * (i - 1));
                bullets.Add(new Bullet
                {
                    StartTime = StartTime,
                    Position = Position,
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
