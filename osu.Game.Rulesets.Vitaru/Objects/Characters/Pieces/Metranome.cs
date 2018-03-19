using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Menu;
using System;

namespace osu.Game.Rulesets.Vitaru.Objects.Characters.Pieces
{
    public class Metranome : BeatSyncedContainer
    {
        private readonly Sprite sign;
        private readonly LogoVisualisation visualizer;

        public Metranome()
        {
            AlwaysPresent = true;
            Size = new Vector2(120);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 0;

            Children = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.9f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,

                    Child = new Triangles
                    {
                        ColourDark = Color4.Pink,
                        ColourLight = Color4.Cyan,
                        RelativeSizeAxes = Axes.Both
                    }
                },
                sign = new Sprite
                {
                    Colour = Color4.Cyan,
                    RelativeSizeAxes = Axes.Both,
                    Texture = VitaruRuleset.VitaruTextures.Get("sign"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                visualizer = new LogoVisualisation
                {
                    Colour = Color4.DeepPink,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.96f)
                }
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            float amplitudeAdjust = Math.Min(1, 0.4f + amplitudes.Maximum);

            const double beat_in_time = 60;

            this.ScaleTo(1 - 0.05f * amplitudeAdjust, beat_in_time, Easing.Out);
            using (BeginDelayedSequence(beat_in_time))
                this.ScaleTo(1, timingPoint.BeatLength * 2, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            sign.RotateTo(-(float)(Clock.CurrentTime / 1000 * 90) / 2);
        }
    }
}
