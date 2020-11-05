using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using System.Threading;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Colour;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Screens.Mvis.UI.Objects;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class BeatmapCover : Container
    {
        private WorkingBeatmap b;

        private Drawable cover;

        private CancellationTokenSource ChangeCoverTask;

        public bool BackgroundBox = true;

        public bool UseBufferedBackground = false;
        public float TimeBeforeWrapperLoad = 500;

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
            ChangeCoverTask = new CancellationTokenSource();

            if ( beatmap == null)
            {
                 cover?.FadeOut(300);
                 return;
            }

            if ( !UseBufferedBackground )
                LoadComponentAsync(new DelayedLoadUnloadWrapper( () =>
                {
                    var c = new Cover(beatmap)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0
                    };

                    c.OnLoadComplete += d => d.FadeIn(300);

                    return c;
                }, TimeBeforeWrapperLoad), newCover =>
                {
                    var oldCover = cover ?? null;
                    oldCover?.FadeOut(300);
                    oldCover?.Expire();

                    cover = newCover;
                    Add(cover);

                    oldCover = null;
                }, ChangeCoverTask.Token);
            else
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
                }, ChangeCoverTask.Token);
        }

        private class Cover : Sprite
        {
            private WorkingBeatmap b;
            public Cover(WorkingBeatmap beatmap = null)
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;

                b = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = b?.Background ?? null;
            }
        }
    }
}