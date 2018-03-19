using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using osu.Game.Rulesets.Vitaru.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Vitaru.Settings;
using Symcol.Core.GameObjects;
using System;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawableSeekingBullet : DrawableVitaruHitObject
    {
        private readonly VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);

        public VitaruCharacter NearestEnemy;

        private double startTime;

        public readonly SymcolHitbox Hitbox;

        //Result of bulletSpeed + bulletAngle math, should never be modified outside of this class
        private Vector2 bulletVelocity;

        //Incase we want to be deleted in the near future
        public double BulletDeleteTime = -1;

        //Should be set to true when a character is hit
        public bool Hit;

        public readonly SeekingBullet SeekingBullet;

        //Playfield size + Margin of 10 on each side
        public Vector4 BulletBounds = new Vector4(-10, -10, 520, 830);

        public DrawableSeekingBullet(Container parent, SeekingBullet seekingBullet) : base(seekingBullet, parent)
        {
            AlwaysPresent = true;
            Alpha = 0;
            Scale = new Vector2(0.1f);
            Size = new Vector2(20);

            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;

            this.FadeInFromZero(100);
            this.ScaleTo(Vector2.One, 100);

            SeekingBullet = seekingBullet;

            if (currentGameMode == VitaruGamemode.Dodge)
                BulletBounds = new Vector4(-10, -10, 522, 394);

            Children = new Drawable[]
            {
                new SeekingBulletPiece(this),
                Hitbox = new SymcolHitbox(Size, Shape.Rectangle)
            };
        }

        protected override void LoadComplete()
        {
            startTime = Time.Current;
        }

        private void nearestEnemy()
        {
            foreach (Drawable draw in ParentContainer.Children)
            {
                VitaruCharacter enemy = draw as VitaruCharacter;
                if (enemy?.Hitbox != null && enemy.Hitbox.Team != SeekingBullet.Team)
                {
                    if (enemy.Alpha > 0)
                    {
                        float minDist = float.MaxValue;
                        Vector2 pos = enemy.ToSpaceOfOtherDrawable(Vector2.Zero, this) + new Vector2(6);
                        float distance = (float)Math.Sqrt(Math.Pow(pos.X, 2) + Math.Pow(pos.Y, 2));
                        if (distance < minDist)
                        {
                            NearestEnemy = enemy;
                            minDist = distance;
                        }
                    }
                }
            }
        }

        public float enemyRelativePositionAngle()
        {
            //Returns a Radian
            float enemyAngle = (float)Math.Atan2((NearestEnemy.Position.Y - Position.Y), (NearestEnemy.Position.X - Position.X));
            return enemyAngle;
        }

        private Vector2 getBulletVelocity(float angle)
        {
            Vector2 velocity = new Vector2(SeekingBullet.BulletSpeed * (float)Math.Cos(angle), SeekingBullet.BulletSpeed * (float)Math.Sin(angle));
            return velocity;
        }

        private void unload()
        {
            Alpha = 0;
            Expire();
        }

        protected override void Update()
        {
            base.Update();

            if (Hit)
                unload();

            Rotation = Rotation + 0.25f;

            if (BulletDeleteTime <= Time.Current && BulletDeleteTime != -1)
                unload();

            if (SeekingBullet.ObeyBoundries && Position.Y < BulletBounds.Y | Position.X < BulletBounds.X | Position.Y > BulletBounds.W | Position.X > BulletBounds.Z && BulletDeleteTime == -1)
            {
                BulletDeleteTime = Time.Current + TIME_FADEOUT / 12;
                this.FadeOutFromOne(TIME_FADEOUT / 12);
            }

            //IdleTimer
            float frameTime = (float)Clock.ElapsedFrameTime;
            bulletVelocity = getBulletVelocity(MathHelper.DegreesToRadians(SeekingBullet.StartAngle - 90));

            if (startTime + 300 <= Time.Current)
            {
                nearestEnemy();
                if (NearestEnemy != null && !NearestEnemy.Dead)
                {
                    bulletVelocity = getBulletVelocity(enemyRelativePositionAngle());
                    this.MoveToOffset(new Vector2(bulletVelocity.X * DrawableBullet.BulletSpeedModifier * frameTime, bulletVelocity.Y * DrawableBullet.BulletSpeedModifier * frameTime));

                }
                else
                    this.MoveToOffset(new Vector2(bulletVelocity.X * DrawableBullet.BulletSpeedModifier * frameTime, bulletVelocity.Y * DrawableBullet.BulletSpeedModifier * frameTime));
            }
            else
                this.MoveToOffset(new Vector2(bulletVelocity.X * DrawableBullet.BulletSpeedModifier * frameTime, bulletVelocity.Y * DrawableBullet.BulletSpeedModifier * frameTime));

        }
    }
}
