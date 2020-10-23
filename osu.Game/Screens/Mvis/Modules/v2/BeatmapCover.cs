using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Mvis.UI.Objects;
using System.Threading;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Colour;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class BeatmapCover : Container
    {
        private WorkingBeatmap b;

        private BeatmapBackground cover;

        private CancellationTokenSource ChangeCoverTask;

        public bool BackgroundBox = true;

        public BeatmapCover(WorkingBeatmap beatmap)
        {
            b = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            if ( BackgroundBox )
                AddInternal(new Box
                {
                    Depth = float.MaxValue,
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#444")),   
                });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateBackground(b);
        }

        public void updateBackground(WorkingBeatmap beatmap)
        {
            ChangeCoverTask?.Cancel();

            if ( beatmap == null)
            {
                 cover?.FadeOut(300);
                 return;
            }

            LoadComponentAsync(new BeatmapBackground(beatmap)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
            }, newCover =>
            {
                var oldCover = cover ?? null;
                oldCover?.FadeOut(300);
                oldCover?.Expire();

                cover = newCover;
                Add(cover);

                Schedule(() => cover?.FadeIn(300));
                oldCover = null;
            },
            (ChangeCoverTask = new CancellationTokenSource()).Token);
        }
    }
}