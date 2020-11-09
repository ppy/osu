using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class BeatmapList : VisibilityContainer
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; }

        private readonly List<BeatmapSetInfo> beatmapSets;
        private BeatmapPiece currentPiece;
        public BindableBool IsCurrent = new BindableBool();
        private readonly OsuScrollContainer beatmapScroll;
        private readonly FillFlowContainer fillFlow;
        private readonly Cached scrollCache = new Cached();
        private bool firstScroll = true;

        public BeatmapList(List<BeatmapSetInfo> set)
        {
            Padding = new MarginPadding { Vertical = 20 };
            RelativeSizeAxes = Axes.Both;
            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                beatmapScroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RightMouseScrollbar = true,
                    Child = fillFlow = new FillFlowContainer
                    {
                        Padding = new MarginPadding { Horizontal = 35 },
                        Spacing = new Vector2(5),
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    }
                }
            };

            beatmapSets = set;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            addBeatmapSets();

            working.BindValueChanged(OnBeatmapChanged);
            IsCurrent.BindValueChanged(v =>
            {
                foreach (var d in fillFlow)
                {
                    if (d is BeatmapPiece piece)
                        piece.IsCurrent = v.NewValue;
                }

                currentPiece?.TriggerActiveChange();
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!scrollCache.IsValid) scrollToCurrent();
        }

        private void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> v)
        {
            currentPiece?.InActive();

            foreach (var d in fillFlow)
            {
                if (!(d is BeatmapPiece piece)
                    || piece.Beatmap.BeatmapSetInfo.Hash != v.NewValue.BeatmapSetInfo.Hash) continue;

                currentPiece = piece;
                piece.MakeActive();
                break;
            }

            scrollCache.Invalidate();
        }

        private void addBeatmapSets()
        {
            fillFlow.AddRange(beatmapSets.Select(s => new BeatmapPiece(beatmaps.GetWorkingBeatmap(s.Beatmaps.First()))));

            scrollCache.Invalidate();
        }

        private void scrollToCurrent()
        {
            if (!IsCurrent.Value)
            {
                beatmapScroll.ScrollToStart(!firstScroll);
                firstScroll = false;
                scrollCache.Validate();
                return;
            }

            if (currentPiece == null)
            {
                firstScroll = false;
                scrollCache.Validate();
                return;
            }

            var index = fillFlow.IndexOf(currentPiece);

            //如果是第一个，那么滚动到头
            if (index == 0)
            {
                beatmapScroll.ScrollToStart(!firstScroll);
            }
            else
            {
                float distance = (index - 1) * 85 - 1;

                //如果滚动范围超出了beatmapFillFlow的高度，那么滚动到尾
                //n个piece, n-1个间隔
                if (distance + beatmapScroll.DrawHeight > (fillFlow?.Count * 85 - 5))
                    beatmapScroll.ScrollToEnd(!firstScroll);
                else
                    beatmapScroll.ScrollTo(distance, !firstScroll);
            }

            scrollCache.Validate();
            firstScroll = false;
        }

        public void ClearList() =>
            fillFlow.Clear();

        protected override void PopIn()
        {
            this.FadeIn(250);

            //猜测: 因为没显示，所以beatmapScroll因为不Update而无法滚动到指定的位置
            working.TriggerChange();
        }

        protected override void PopOut()
        {
            this.FadeOut(250);
        }
    }
}
