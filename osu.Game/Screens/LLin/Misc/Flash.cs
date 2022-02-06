using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.LLin.Misc
{
    public class Flash : BeatSyncedContainer
    {
        private readonly Box flashBox = new Box
        {
            RelativeSizeAxes = Axes.Both,
            Alpha = 0,
            Colour = Colour4.White.Opacity(0.4f)
        };

        public ColourInfo BoxColor
        {
            get => flashBox.Colour;
            set => flashBox.Colour = value;
        }

        public Flash()
        {
            Child = flashBox;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            switch (timingPoint.TimeSignature.Numerator)
            {
                case 4:
                    if ((beatIndex % 4 == 0 && beatIndex / 4 > 0) || effectPoint.KiaiMode)
                        flashBox.FadeOutFromOne(1000);
                    break;

                case 3:
                    if ((beatIndex % 3 == 0 && beatIndex / 3 > 0) || effectPoint.KiaiMode)
                        flashBox.FadeOutFromOne(1000);
                    break;
            }
        }
    }
}
