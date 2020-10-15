using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Collections;

namespace osu.Game.Screens.Mvis.Modules
{
    public class CollectionHelper : Component
    {
        private BindableList<BeatmapCollection> collections = new BindableList<BeatmapCollection>();

        [Resolved]
        private CollectionManager collectionManager { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> b { get; set; }

        private List<BeatmapSetInfo> beatmapList = new List<BeatmapSetInfo>();

        private int currentPosition = 0;
        private int maxCount = 0;

        [BackgroundDependencyLoader]
        private void load()
        {
            collections.BindTo(collectionManager.Collections);

            foreach (var c in collections)
            {
                Logger.Log($"+++收藏夹{c.Name}中的内容");
                foreach (var item in c.Beatmaps)
                {
                    Logger.Log($"+++++{item}");
                    Logger.Log($"++++++++++{item.BeatmapSet}");
                }
            }
        }

        public void RandomSelectBeatmap()
        {
            var currentCollection = collectionManager.Collections.First();

            var beatmap = NextBeatmap(currentCollection, b.Value);

            b.Value = beatmap;
        }

        protected override void LoadComplete()
        {
            updateBeatmaps(collectionManager.Collections.First());
            base.LoadComplete();
        }

        private WorkingBeatmap NextBeatmap(BeatmapCollection collection, WorkingBeatmap prevBeatmap)
        {
            var info = prevBeatmap.BeatmapInfo;
            WorkingBeatmap NewBeatmap = null;

            //更新当前位置和最大位置
            currentPosition = collection.Beatmaps.IndexOf(info);
            maxCount = collection.Beatmaps.Count;

            void NextBeatmap()
            {
                //当前位置往后移一个
                currentPosition++;
                //如果当前位置超过了最大位置或者不在范围内，那么回到第一个
                if (currentPosition >= maxCount || currentPosition < 0) currentPosition = 0;

                //获取下一张图
                NewBeatmap = beatmaps.GetWorkingBeatmap(collection.Beatmaps.ElementAt(currentPosition));

                //如果下一张图和之前的图是一个曲子，继续寻找下一个
                if ( NewBeatmap.BeatmapSetInfo.Hash == prevBeatmap.BeatmapSetInfo.Hash )
                    NextBeatmap();
            }

            NextBeatmap();
            return NewBeatmap;
        }

        ///<summary>
        ///用来更新`beatmapList`(谱面列表)
        ///</summary>
        private void updateBeatmaps(BeatmapCollection collection)
        {
            //清理现有的谱面列表
            beatmapList.Clear();

            foreach( var item in collection.Beatmaps )
            {
                //获取当前BeatmapSet
                var currentSet = item.BeatmapSet;
            
                //进行比对，如果beatmapList中不存在，则添加。
                if ( ! beatmapList.Contains(currentSet) )
                    beatmapList.Add(currentSet);
            }
        }
    }
}