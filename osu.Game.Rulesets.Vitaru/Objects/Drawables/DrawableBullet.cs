using osu.Framework.Graphics;
using OpenTK;
using System;
using osu.Game.Rulesets.Vitaru.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Vitaru.Judgements;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.Scoring;
using osu.Game.Rulesets.Vitaru.UI;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using Symcol.Core.GameObjects;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawableBullet : DrawableVitaruHitObject
    { 
        public static int BulletCount;

        private readonly ScoringMetric currentScoringMetric = VitaruSettings.VitaruConfigManager.GetBindable<ScoringMetric>(VitaruSetting.ScoringMetric);
        private readonly VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);

        //Used like a multiple (useful for spells in multiplayer)
        public static float BulletSpeedModifier = 1;

        //Playfield size + Margin of 10 on each side
        public Vector4 BulletBounds = new Vector4(-10, -10, 520, 830);

        //Result of bulletSpeed + bulletAngle math, should never be modified outside of this class
        public Vector2 BulletVelocity;

        //Set to "true" when a judgement should be returned
        private bool returnJudgement;

        public bool ReturnGreat = false;

        //Can be set for the Graze ScoringMetric
        public int ScoreZone;

        //Should be set to true when a character is hit
        public bool Hit;

        //Incase we want to be deleted in the near future
        public double BulletDeleteTime = -1;

        private readonly DrawablePattern drawablePattern;
        public readonly Bullet Bullet;

        public Action OnHit;

        public SymcolHitbox Hitbox;

        private BulletPiece bulletPiece;

        private bool started;
        private bool loaded;

        public DrawableBullet(Container parent, Bullet bullet, DrawablePattern drawablePattern) : base(bullet, parent)
        {
            AlwaysPresent = true;
            Alpha = 0;

            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;

            BulletCount++;

            Bullet = bullet;
            this.drawablePattern = drawablePattern;

            if (currentGameMode == VitaruGamemode.Dodge)
                BulletBounds = new Vector4(-10, -10, 522, 394);
        }

        public DrawableBullet(Container parent, Bullet bullet) : base(bullet, parent)
        {
            AlwaysPresent = true;
            Alpha = 0;

            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;

            BulletCount++;

            Bullet = bullet;

            if (currentGameMode == VitaruGamemode.Dodge)
                BulletBounds = new Vector4(-10, -10, 522, 394);
        }

        /// <summary>
        /// Called 1 second before the bullet's starttime
        /// </summary>
        private void load()
        {
            if (!loaded)
            {
                loaded = true;

                Size = new Vector2(Bullet.BulletDiameter);
                Scale = new Vector2(0.1f);

                Children = new Drawable[]
                {
                    bulletPiece = new BulletPiece(this),
                    Hitbox = new SymcolHitbox(new Vector2(Bullet.BulletDiameter), Shape.Circle)
                    {
                        Team = Bullet.Team,
                        HitDetection = false
                    }
                };
            }
        }

        /// <summary>
        /// Called to unload the bullet for storage
        /// </summary>
        private void unload()
        {
            if (loaded)
            {
                loaded = false;
                started = false;
                returnJudgement = false;
                BulletDeleteTime = -1;
                Alpha = 0;

                Remove(bulletPiece);
                bulletPiece.Dispose();
                Remove(Hitbox);
                Hitbox.Dispose();
                ParentContainer.Remove(this);
                Dispose();
            }
        }

        /// <summary>
        /// Called once when the bullet starts
        /// </summary>
        private void start()
        {
            if (!started)
            {
                Position = Bullet.Position;
                Hitbox.HitDetection = true;
                started = true;
                this.FadeInFromZero(100);
                this.ScaleTo(Vector2.One, 100);
                BulletVelocity = getBulletVelocity();
            }
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            base.CheckForJudgements(userTriggered, timeOffset);

            if (returnJudgement)
            {
                if (currentScoringMetric == ScoringMetric.ScoreZones)
                {
                    switch (VitaruPlayfield.VitaruPlayer.ScoreZone)
                    {
                        case 0:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Miss });
                            break;
                        case 100:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Ok });
                            break;
                        case 200:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Good });
                            break;
                        case 300:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                    }
                }
                else if (currentScoringMetric == ScoringMetric.InverseCatch)
                {
                    switch (VitaruPlayfield.VitaruPlayer.ScoreZone)
                    {
                        case 0:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Miss });
                            break;
                        case 100:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                        case 200:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                        case 300:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                    }
                }
                else if (currentScoringMetric == ScoringMetric.Graze)
                {
                    switch (ScoreZone)
                    {
                        case 0:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Miss });
                            break;
                        case 50:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Meh });
                            break;
                        case 100:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Ok });
                            break;
                        case 200:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Good });
                            break;
                        case 300:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                    }
                }
            }

            else if (Hit)
            {
                if (!Bullet.DummyMode)
                    AddJudgement(new VitaruJudgement { Result = HitResult.Miss });
                unload();
            }

            else if (ReturnGreat)
            {
                AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                unload();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if(isDisposing)
                BulletCount--;
        }

        private Vector2 getBulletVelocity()
        {
            Vector2 velocity = new Vector2(Bullet.BulletSpeed * (float)Math.Cos(Bullet.BulletAngleRadian), Bullet.BulletSpeed * (float)Math.Sin(Bullet.BulletAngleRadian));
            return velocity;
        }

        protected override void Update()
        {
            base.Update();

            if (OnHit != null && Hit)
            {
                OnHit();
                OnHit = null;
            }

            if (Position.Y >= BulletBounds.Y | Position.X >= BulletBounds.X | Position.Y <= BulletBounds.W | Position.X <= BulletBounds.Z && Time.Current >= Bullet.StartTime | Bullet.DummyMode || !Bullet.ObeyBoundries && Time.Current >= Bullet.StartTime | Bullet.DummyMode)
                load();

            if (BulletDeleteTime <= Time.Current && BulletDeleteTime != -1 || Time.Current < Bullet.StartTime && !Bullet.DummyMode)
                unload();

            if (Time.Current >= Bullet.StartTime)
            {
                start();

                float frameTime = (float)Clock.ElapsedFrameTime;
                this.MoveToOffset(new Vector2(BulletVelocity.X * BulletSpeedModifier * frameTime, BulletVelocity.Y * BulletSpeedModifier * frameTime));

                if (Bullet.ObeyBoundries && Position.Y < BulletBounds.Y | Position.X < BulletBounds.X | Position.Y > BulletBounds.W | Position.X > BulletBounds.Z && !returnJudgement)
                {
                    returnJudgement = true;
                    BulletDeleteTime = Time.Current + TIME_FADEOUT / 12;
                    this.FadeOutFromOne(TIME_FADEOUT / 12);
                }
            }
        }
    }
}
