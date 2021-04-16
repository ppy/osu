using System;
using Mvis.Plugin.RulesetPanel.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.RulesetPanel.Objects
{
    public class SpaceParticlesContainer : ParticlesContainer
    {
        /// <summary>
        /// Adjusts the speed of all the particles.
        /// </summary>
        private const int absolute_time = 5000;

        /// <summary>
        /// The maximum scale of a single particle.
        /// </summary>
        private const float particle_max_scale = 3;

        protected override Drawable CreateParticle() => new Particle();

        private class Particle : Circle
        {
            [Resolved]
            private RulesetPanelConfigManager config { get; set; }

            private readonly Bindable<bool> useCustomColour = new Bindable<bool>();
            private readonly Bindable<int> red = new Bindable<int>(0);
            private readonly Bindable<int> green = new Bindable<int>(0);
            private readonly Bindable<int> blue = new Bindable<int>(0);

            private Vector2 finalPosition;
            private double lifeTime;
            private float finalScale;

            public Particle()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.Both;
                Size = new Vector2(2);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                config.BindWith(RulesetPanelSetting.Red, red);
                config.BindWith(RulesetPanelSetting.Green, green);
                config.BindWith(RulesetPanelSetting.Blue, blue);
                config.BindWith(RulesetPanelSetting.UseCustomColour, useCustomColour);

                red.BindValueChanged(_ => updateColour());
                green.BindValueChanged(_ => updateColour());
                blue.BindValueChanged(_ => updateColour());
                useCustomColour.BindValueChanged(_ => updateColour(), true);
            }

            private void updateColour()
            {
                if (!useCustomColour.Value)
                {
                    Colour = Color4.White;
                    return;
                }

                this.FadeColour(new Colour4(red.Value / 255f, green.Value / 255f, blue.Value / 255f, 1));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Reuse();
            }

            public void Reuse()
            {
                Alpha = 0;
                Position = new Vector2(RNG.NextSingle(-0.5f, 0.5f), RNG.NextSingle(-0.5f, 0.5f));
                calculateValues();

                this.FadeIn(lifeTime > 500 ? 500 : lifeTime);
                this.MoveTo(finalPosition, lifeTime);
                this.ScaleTo(finalScale, lifeTime).Finally(_ => Reuse());
            }

            private void calculateValues()
            {
                float finalX;
                float finalY;
                float ratio;

                if (Math.Abs(X) > Math.Abs(Y))
                {
                    ratio = Math.Abs(X) / 0.5f;
                    finalX = X > 0 ? 0.5f : -0.5f;
                    finalY = Y / ratio;
                }
                else
                {
                    ratio = Math.Abs(Y) / 0.5f;
                    finalY = Y > 0 ? 0.5f : -0.5f;
                    finalX = X / ratio;
                }

                finalPosition = new Vector2(finalX, finalY);

                float depth = RNG.NextSingle(0.25f, 1);
                Scale = new Vector2(depth);

                lifeTime = absolute_time * (1 - ratio) / depth;
                finalScale = 1 + ((particle_max_scale - 1) * depth * (1 - ratio));
            }
        }
    }
}
