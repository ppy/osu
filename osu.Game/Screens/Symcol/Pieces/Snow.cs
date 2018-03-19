using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Symcol.Pieces
{
    public class Snow : BeatSyncedContainer
    {
        public Snow()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = false;
        }

        protected override void Update()
        {
            base.Update();

            float randomPositionXValue = ((float)RNG.NextDouble(0, (int)ScreenSpaceDrawQuad.TopRight.X));
            
            float spawn = (float)RNG.NextDouble(1, 5000 * ((float)Clock.ElapsedFrameTime / 1000));

            if (spawn < 2)
                addSnowParticle(randomPositionXValue);
        }

        private void addSnowParticle(float x)
        {
            Add(new SnowParticle(new Vector2(ScreenSpaceDrawQuad.TopRight.X / 1.8f, ScreenSpaceDrawQuad.BottomLeft.Y / 2))
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.Centre,
                Position = new Vector2 (x, -100)
            });
        }
    }

    public class SnowParticle : Sprite
    {
        private float randomMovementYValue = 1;
        private float randomMovementXValue = 1;
        private float randomRotationValue = 1;
        private bool randomRotateDirection = false;
        private readonly Vector2 screenSize;

        public SnowParticle(Vector2 screenSize)
        {
            //Texture = OsuGame.SymcolTextures.Get("snowflake");

            this.screenSize = screenSize;

            randomMovementYValue = ((float)RNG.NextDouble(10, 40) * 2);
            randomMovementXValue = ((float)RNG.NextDouble(-10, 10) * 2);
            randomRotationValue = ((float)RNG.NextDouble(10, 16)) / 10;
            randomRotateDirection = RNG.NextBool();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            float randomScaleValue = ((float)RNG.NextDouble(50, 100) / 500);
            Scale = new Vector2(randomScaleValue);
        }

        protected override void Update()
        {
            base.Update();

            if (randomRotateDirection)
                this.RotateTo((float)((Clock.CurrentTime / 1000) * 90) * randomRotationValue);
            else
                this.RotateTo(((float)((Clock.CurrentTime / 1000) * 90) * -1) * randomRotationValue);

            this.MoveToOffset(new Vector2(randomMovementXValue * ((float)Clock.ElapsedFrameTime / 1000), randomMovementYValue * ((float)Clock.ElapsedFrameTime / 1000)));

            if (Position.X > screenSize.X + 40 || Position.Y > screenSize.Y + 40 || Position.Y < -100 || Position.X < -40)
                Expire();
        }
    }
}
