using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Overlays;

namespace osu.Game.Screens.Mvis.Collections
{
    internal class CollectionHelper : Component
    {
        [Resolved]
        private CollectionManager collectionManager { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> b { get; set; }

        [Resolved]
        private MusicController controller { get; set; }

        private readonly List<BeatmapSetInfo> beatmapList = new List<BeatmapSetInfo>();
        private int currentPosition;
        private int maxCount;
        public Bindable<BeatmapCollection> CurrentCollection = new Bindable<BeatmapCollection>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CurrentCollection.BindValueChanged(OnCollectionChanged);

            collectionManager.Collections.CollectionChanged += triggerRefresh;
        }

        public void PlayNextBeatmap() => Schedule(NextTrack);

        public void Play(WorkingBeatmap b) => changeBeatmap(b);

        private void changeBeatmap(WorkingBeatmap working)
        {
            b.Disabled = false;
            b.Value = working;
            b.Disabled = true;
            controller.Play();
        }

        public void NextTrack() =>
            changeBeatmap(getBeatmap(beatmapList, b.Value, true));

        public void PrevTrack() =>
            changeBeatmap(getBeatmap(beatmapList, b.Value, true, -1));

        /// <summary>
        /// 用于从列表中获取指定的<see cref="WorkingBeatmap"/>。
        /// </summary>
        /// <returns>根据给定位移得到的<see cref="WorkingBeatmap"/></returns>
        /// <param name="list">要给予的<see cref="BeatmapSetInfo"/>列表</param>
        /// <param name="prevBeatmap">上一张图</param>
        /// <param name="updateCurrentPosition">是否更新当前位置</param>
        /// <param name="displace">位移数值，默认为1.</param>
        private WorkingBeatmap getBeatmap(List<BeatmapSetInfo> list, WorkingBeatmap prevBeatmap, bool updateCurrentPosition = false, int displace = 1)
        {
            var prevSet = prevBeatmap.BeatmapSetInfo;

            //更新当前位置和最大位置
            if (updateCurrentPosition)
                currentPosition = list.IndexOf(prevSet);

            maxCount = list.Count;

            //当前位置往指定位置移动
            currentPosition += displace;

            //如果当前位置超过了最大位置或者不在范围内，那么回到第一个
            if (currentPosition >= maxCount || currentPosition < 0)
            {
                if (displace > 0) currentPosition = 0;
                else currentPosition = maxCount - 1;
            }

            //从list获取当前位置所在的BeatmapSetInfo, 然后选择该BeatmapSetInfo下的第一个WorkingBeatmap
            //最终赋值给NewBeatmap
            var newBeatmap = list.Count > 0
                ? beatmaps.GetWorkingBeatmap(list.ElementAt(currentPosition).Beatmaps.First())
                : b.Value;
            return newBeatmap;
        }

        private void playFirstBeatmap(List<BeatmapSetInfo> list)
        {
            if (list.Count == 0) return;

            var newBeatmap = beatmaps.GetWorkingBeatmap(list.FirstOrDefault()?.Beatmaps.First());

            b.Value = newBeatmap;
            controller.Play();
        }

        ///<summary>
        ///用来更新<see cref="beatmapList"/>
        ///</summary>
        private void updateBeatmaps(BeatmapCollection collection)
        {
            //清理现有的谱面列表
            beatmapList.Clear();

            if (collection == null) return;

            foreach (var item in collection.Beatmaps)
            {
                //获取当前BeatmapSet
                var currentSet = item.BeatmapSet;

                //进行比对，如果beatmapList中不存在，则添加。
                if (!beatmapList.Contains(currentSet))
                    beatmapList.Add(currentSet);
            }
        }

        public void UpdateBeatmaps() => updateBeatmaps(CurrentCollection.Value);

        private void triggerRefresh(object sender, NotifyCollectionChangedEventArgs e)
            => updateBeatmaps(CurrentCollection.Value);

        private void OnCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            updateBeatmaps(CurrentCollection.Value);
        }

        protected override void Dispose(bool isDisposing)
        {
            collectionManager.Collections.CollectionChanged -= triggerRefresh;
            base.Dispose(isDisposing);
        }
    }
}
