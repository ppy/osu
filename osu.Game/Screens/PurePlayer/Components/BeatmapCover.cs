using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Mvis.UI.Objects;
using System.Threading;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Colour;
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
                new Box
                {
                    Depth = float.MaxValue,
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#444")),
                },
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
            ChangeCoverTask?.Cancel();

            LoadComponentAsync(new BeatmapBackground(value.NewValue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
            }, newCover =>
            {
                var oldCover = cover ?? null;
                oldCover?.Delay(300).FadeOut(300);
                oldCover?.Expire();

                cover = newCover;
                coverContainer.Add(cover);

                this.Schedule(() => cover?.FadeIn(300));
                oldCover = null;
            },
            (ChangeCoverTask = new CancellationTokenSource()).Token);
        }
    }
}