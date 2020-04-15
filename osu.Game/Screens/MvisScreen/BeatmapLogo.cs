using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.Mvis.UI.Objects.Helpers;
using osu.Game.Screens.Mvis.UI.Objects.MusicVisualizers;
using osu.Game.Screens.Mvis.UI.Objects.MusicVisualizers.Bars;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.UI.Objects
{
    public class BeatmapLogo : CurrentBeatmapProvider
    {
        private const int radius = 350;

        private readonly CircularProgress progressGlow;

        public BeatmapLogo(int barsCount = 120, float barWidth = 3f)
        {
            Origin = Anchor.Centre;
            Size = new Vector2(radius);

            AddRangeInternal(new Drawable[]
            {
                new CircularVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = 120,
                    BarWidth = barWidth,
                    BarsCount = barsCount,
                    CircleSize = radius,
                },
                new CircularVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = 120,
                    Rotation = 120,
                    BarWidth = barWidth,
                    BarsCount = barsCount,
                    CircleSize = radius,
                },
                new CircularVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = 120,
                    Rotation = 240,
                    BarWidth = barWidth,
                    BarsCount = barsCount,
                    CircleSize = radius,
                },
                new UpdateableBeatmapBackground
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                (progressGlow = new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    InnerRadius = 0.02f,
                }).WithEffect(new GlowEffect
                {
                    Colour = Color4.White,
                    Strength = 5,
                    PadExtent = true
                }),
            });
        }

        protected override void Update()
        {
            base.Update();

            var track = Beatmap.Value?.Track;

            progressGlow.Current.Value = track == null ? 0 : (float)(track.CurrentTime / track.Length);
        }

        private class CircularVisualizer : MusicCircularVisualizer
        {
            protected override BasicBar CreateBar() => new CircularBar();
        }
    }
}
