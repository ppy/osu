using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.LLin.Misc
{
    public class BeatmapCover : CompositeDrawable
    {
        private readonly WorkingBeatmap b;

        private Drawable cover;

        private CancellationTokenSource changeCoverTask;

        public bool BackgroundBox = true;

        public bool UseBufferedBackground;
        public float TimeBeforeWrapperLoad = 500;

        public BeatmapCover(WorkingBeatmap beatmap)
        {
            RelativeSizeAxes = Axes.Both;

            b = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (BackgroundBox)
            {
                AddInternal(new Box
                {
                    Depth = float.MaxValue,
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#444")),
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UpdateBackground(b);
        }

        public void UpdateBackground(WorkingBeatmap beatmap)
        {
            changeCoverTask?.Cancel();
            if (IsDisposed) return;

            changeCoverTask = new CancellationTokenSource();

            if (beatmap == null)
            {
                cover?.FadeOut(300);
                return;
            }

            if (!UseBufferedBackground)
            {
                LoadComponentAsync(new DelayedLoadUnloadWrapper(() =>
                {
                    var c = new Cover(beatmap)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0
                    };

                    c.OnLoadComplete += d => d.FadeIn(300);

                    return c;
                }, TimeBeforeWrapperLoad)
                {
                    Anchor = Anchor,
                    Origin = Origin
                }, newCover =>
                {
                    var oldCover = cover;
                    oldCover?.FadeOut(300);
                    oldCover?.Expire();

                    cover = newCover;
                    AddInternal(cover);
                }, changeCoverTask.Token);
            }
            else
            {
                LoadComponentAsync(new BeatmapBackground(beatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                }, newCover =>
                {
                    var oldCover = cover;
                    oldCover?.FadeOut(300);
                    oldCover?.Expire();

                    cover = newCover;
                    AddInternal(cover);

                    Schedule(() => cover?.FadeIn(300));
                }, changeCoverTask.Token);
            }
        }

        public class Cover : Sprite
        {
            private readonly WorkingBeatmap b;

            public Cover(WorkingBeatmap beatmap = null)
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;

                b = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = b?.Background;
            }
        }
    }
}
