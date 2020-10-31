using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Overlays;

namespace osu.Game.Screens.Mvis.Modules
{
    public class CollectionHelper : Component
    {
        [Resolved]
        private CollectionManager collectionManager { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> b { get; set; }

        [Resolved]
        private MusicController controller { get; set; }

        private List<BeatmapSetInfo> beatmapList = new List<BeatmapSetInfo>();

        private BeatmapCollection currentCollection;

        private int currentPosition = 0;
        private int maxCount = 0;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            RefreshBeatmapList();
        }

        public void PlayNextBeatmap() => Schedule(NextTrack);

        public void NextTrack()
        {
            b.Value = GetBeatmap(beatmapList, b.Value, true);
            controller.Play();
        }

        public void PrevTrack()
        {
            b.Value = GetBeatmap(beatmapList, b.Value, true, -1);
            controller.Play();
        }

        /// <summary>
        /// 用于从列表中获取指定的<see cref="WorkingBeatmap"/>。
        /// </summary>
        /// <returns>根据给定位移得到的<see cref="WorkingBeatmap"/></returns>
        /// <param name="list">要给予的<see cref="BeatmapSetInfo"/>列表</param>
        /// <param name="prevBeatmap">上一张图</param>
        /// <param name="updateCurrentPosition">是否更新当前位置</param>
        /// <param name="displace">位移数值，默认为1.</param>
        private WorkingBeatmap GetBeatmap(List<BeatmapSetInfo> list, WorkingBeatmap prevBeatmap, bool updateCurrentPosition = false, int displace = 1)
        {
            var info = prevBeatmap.BeatmapInfo;
            var prevSet = prevBeatmap.BeatmapSetInfo;
            WorkingBeatmap NewBeatmap = null;

            //更新当前位置和最大位置
            if (updateCurrentPosition)
            {
                currentPosition = list.IndexOf(prevSet);
            }
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
            if (list.Count > 0)
                NewBeatmap = beatmaps.GetWorkingBeatmap(list.ElementAt(currentPosition).Beatmaps.First());
            else
                NewBeatmap = b.Value;
            return NewBeatmap;
        }

        private void playFirstBeatmap(List<BeatmapSetInfo> list)
        {
            WorkingBeatmap NewBeatmap;

            if (list.Count == 0) return;

            NewBeatmap = beatmaps.GetWorkingBeatmap(list.ElementAt(0).Beatmaps.First());

            b.Value = NewBeatmap;
            controller.Play();
        }

        public void PlayFirstBeatmap() => playFirstBeatmap(beatmapList);

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

        public void RefreshBeatmapList(BeatmapCollection collection = null)
        {
            if (collection == null)
            {
                currentCollection = null;
                return;
            }

            currentCollection = collection;

            updateBeatmaps(currentCollection);
        }

        public bool currentCollectionContains(WorkingBeatmap b)
        {
            if (beatmapList.Contains(b.BeatmapSetInfo))
                return true;
            else
                return false;
        }
    }
}