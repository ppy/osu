using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Vitaru.Scoring;

namespace osu.Game.Rulesets.Vitaru.UI
{
    public class ComboFire : Container
    {
        public ComboFire()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            Height = 500;

            RelativeSizeAxes = Axes.X;
            Masking = false;
        }

        protected override void Update()
        {
            base.Update();

            float spawn = 0;

            if (0 <= (float)Clock.ElapsedFrameTime / 1000 * VitaruScoreProcessor.Combo)
                spawn = (float)RNG.NextDouble(0, (float)Clock.ElapsedFrameTime / 1000 * VitaruScoreProcessor.Combo);

            if (spawn > 1f)
                addFireParticle();
        }

        private void addFireParticle()
        {
            Add(new FireParticle(RelativeToAbsoluteFactor.X)
            {
                Size = new Vector2((float)RNG.NextDouble(40, 120)),
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.Centre
            });
        }
    }

    public class FireParticle : Triangle
    {
        private readonly float randomMovementYValue;
        private readonly float randomMovementXValue;
        private readonly float width;

        public FireParticle(float width)
        {
            this.width = width;

            Position = new Vector2((float)RNG.NextDouble(0, width), 60);
            Colour = Interpolation.ValueAt(RNG.NextSingle(), Color4.Red, Color4.Yellow, 0, 1);

            randomMovementYValue = -1 * ((float)RNG.NextDouble(10, 40) * 2);
            randomMovementXValue = (float)RNG.NextDouble(-10, 10);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            float randomScaleValue = (float)RNG.NextDouble(50, 100) / 500;
            Scale = new Vector2(randomScaleValue);
            this.ScaleTo(new Vector2(0.001f), 3000);
        }

        protected override void Update()
        {
            base.Update();

            this.MoveToOffset(new Vector2(randomMovementXValue * ((float)Clock.ElapsedFrameTime / 1000), randomMovementYValue * ((float)Clock.ElapsedFrameTime / 1000)));

            if (Position.X > width + 60 || Scale.X <= 0.005f || Position.X < -60)
                Expire();
        }
    }
}
