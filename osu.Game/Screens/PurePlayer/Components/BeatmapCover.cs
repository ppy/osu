using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Mvis.UI.Objects;
using System.Threading;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Colour;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.PurePlayer.Components
{
    public class BeatmapCover : Container
    {

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        private Container coverContainer;

        private BeatmapBackground cover;

        private CancellationTokenSource ChangeCoverTask;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                coverContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            b.BindValueChanged(OnBeatmapChanged, true);
        }

        private void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> value)
        {
            var b = value.NewValue;

            LoadComponentAsync(new BeatmapBackground(b)
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
                coverContainer.Add(cover);

                this.Schedule(() => cover?.FadeIn(300));
                this.Schedule(() => oldCover?.FadeIn(300));
                oldCover = null;
            },
            (ChangeCoverTask = new CancellationTokenSource()).Token);
        }
    }
}