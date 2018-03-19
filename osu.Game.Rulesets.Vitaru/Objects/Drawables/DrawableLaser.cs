using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Vitaru.Judgements;
using osu.Game.Rulesets.Vitaru.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Vitaru.Scoring;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.UI;
using Symcol.Core.GameObjects;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawableLaser : DrawableVitaruHitObject
    {
        private readonly ScoringMetric currentScoringMetric = VitaruSettings.VitaruConfigManager.GetBindable<ScoringMetric>(VitaruSetting.ScoringMetric);
        private VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);

        //Set to "true" when a judgement should be returned
        private bool returnJudgement;

        private bool returnedJudgement;

        public bool ReturnGreat = false;

        //Can be set for the Graze ScoringMetric
        public int ScoreZone;

        //Should be set to true when a character is hit
        public bool Hit;

        //Incase we want to be deleted in the near future
        public double LaserDeleteTime = -1;        

        public SymcolHitbox Hitbox;
        private LaserPiece laserPiece;

        private readonly DrawablePattern drawablePattern;
        public readonly Laser Laser;

        private const float fade_in_time = 200;
        private const float fade_out_time = 200;

        private bool started;
        private bool loaded;

        public DrawableLaser(Container parent, Laser laser, DrawablePattern drawablePattern) : base(laser, parent)
        {
            AlwaysPresent = true;
            Alpha = 0;

            Anchor = Anchor.TopLeft;
            Origin = Anchor.BottomCentre;

            Laser = laser;
            this.drawablePattern = drawablePattern;

            Size = new Vector2(Laser.LaserSize.X / 2, Laser.LaserSize.Y / 8);
            Rotation = MathHelper.RadiansToDegrees(Laser.LaserAngleRadian);
        }

        public DrawableLaser(Container parent, Laser laser) : base(laser, parent)
        {
            AlwaysPresent = true;
            Alpha = 0;

            Anchor = Anchor.TopLeft;
            Origin = Anchor.BottomCentre;

            Laser = laser;

            Size = new Vector2(Laser.LaserSize.X / 2, Laser.LaserSize.Y / 8);
            Rotation = MathHelper.RadiansToDegrees(Laser.LaserAngleRadian);
        }

        /// <summary>
        /// Called 1 second before the bullet's starttime
        /// </summary>
        private void load()
        {
            if (!loaded)
            {
                loaded = true;

                Children = new Drawable[]
                {
                    laserPiece = new LaserPiece(this),
                    Hitbox = new SymcolHitbox(new Vector2(Laser.LaserSize.X / 2, Laser.LaserSize.Y / 8), Shape.Rectangle)
                    {
                        Team = Laser.Team,
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
                LaserDeleteTime = -1;
                Alpha = 0;

                Remove(laserPiece);
                laserPiece.Dispose();
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
                Hitbox.HitDetection = true;
                started = true;
                this.FadeInFromZero(fade_in_time);
                this.ResizeTo(Laser.LaserSize, fade_in_time);
                laserPiece.ResizeTo(Laser.LaserSize, fade_in_time);
                Hitbox.ResizeTo(Laser.LaserSize, fade_in_time);
            }
        }

        public void End()
        {
            if (started)
            {
                started = false;
                this.FadeOutFromOne(fade_out_time);
                this.ResizeTo(new Vector2(Laser.LaserSize.X / 2, Laser.LaserSize.Y), fade_out_time);
                laserPiece.ResizeTo(new Vector2(Laser.LaserSize.X / 2, Laser.LaserSize.Y), fade_out_time);
                Hitbox.ResizeTo(new Vector2(Laser.LaserSize.X / 2, Laser.LaserSize.Y), fade_out_time);
                LaserDeleteTime = Time.Current + fade_out_time;
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

            else if (Hit && !returnedJudgement)
            {
                if (!Laser.DummyMode)
                    AddJudgement(new VitaruJudgement { Result = HitResult.Miss });
                returnedJudgement = true;
            }

            else if (ReturnGreat)
            {
                AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                unload();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (Time.Current >= Laser.StartTime | Laser.DummyMode)
                load();

            if (LaserDeleteTime <= Time.Current && LaserDeleteTime != -1 || Time.Current < Laser.StartTime && !Laser.DummyMode)
                unload();

            if (Time.Current >= Laser.StartTime && Time.Current < Laser.EndTime)
                start();

            if (Time.Current >= Laser.EndTime)
                End();
        }
    }
}
