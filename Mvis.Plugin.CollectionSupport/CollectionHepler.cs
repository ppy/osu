using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using M.DBus.Tray;
using M.DBus.Utils.Canonical.DBusMenuFlags;
using Mvis.Plugin.CollectionSupport.Config;
using Mvis.Plugin.CollectionSupport.DBus;
using Mvis.Plugin.CollectionSupport.Sidebar;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Overlays;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Game.Screens.LLin.SideBar.Settings.Items;

namespace Mvis.Plugin.CollectionSupport
{
    public class CollectionHelper : BindableControlledPlugin, IProvideAudioControlPlugin
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

        public int CurrentPosition
        {
            get => currentPosition;
            set
            {
                currentPosition = value;

                if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
                {
                    dBusObject.Position = value;
                }
            }
        }

        private int currentPosition = -1;
        private int maxCount;
        public Bindable<BeatmapCollection> CurrentCollection = new Bindable<BeatmapCollection>();

        protected override Drawable CreateContent() => new PlaceHolder();

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit() => true;

        public override int Version => 8;

        public override PluginSidebarPage CreateSidebarPage()
            => new CollectionPluginPage(this);

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new CollectionHelperConfigManager(storage);

        public CollectionHelper()
        {
            Name = "收藏夹";
            Description = "将收藏夹作为歌单播放音乐!";
            Author = "mf-osu";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload
            });
        }

        private bool trackChangedAfterDisable = true;

        [Resolved]
        private OsuGame game { get; set; }

        private CollectionDBusObject dBusObject;

        private readonly SimpleEntry trayEntry = new SimpleEntry
        {
            Label = "收藏夹（未选择任何收藏夹）",
            ChildrenDisplay = ChildrenDisplayType.SSubmenu
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (CollectionHelperConfigManager)DependenciesContainer.Get<LLinPluginManager>().GetConfigManager(this);
            config.BindWith(CollectionSettings.EnablePlugin, Value);
            b.BindValueChanged(v =>
            {
                updateCurrentPosition();
                if (!IsCurrent) trackChangedAfterDisable = true;
            });

            PluginManager.RegisterDBusObject(dBusObject = new CollectionDBusObject());

            if (LLin != null)
            {
                LLin.Resuming += UpdateBeatmaps;
                LLin.Exiting += onMvisExiting;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CurrentCollection.BindValueChanged(OnCollectionChanged);

            collectionManager.Collections.CollectionChanged += triggerRefresh;
        }

        private void onMvisExiting()
        {
            PluginManager.UnRegisterDBusObject(new CollectionDBusObject());

            if (!Disabled.Value)
                PluginManager.RemoveDBusMenuEntry(trayEntry);

            resetDBusMessage();
        }

        public void Play(WorkingBeatmap b) => changeBeatmap(b);

        public void NextTrack() => changeBeatmap(getBeatmap(beatmapList, b.Value, true));

        public void PrevTrack() =>
            changeBeatmap(getBeatmap(beatmapList, b.Value, true, -1));

        public void TogglePause()
        {
            if (drawableTrack.IsRunning)
                drawableTrack.Stop();
            else
                drawableTrack.Start();
        }

        public override bool Disable()
        {
            this.MoveToX(-10, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint);

            resetDBusMessage();
            PluginManager.RemoveDBusMenuEntry(trayEntry);

            return base.Disable();
        }

        public override bool Enable()
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
            {
                dBusObject.Position = currentPosition;
                dBusObject.CollectionName = CurrentCollection.Value?.Name.Value ?? "-";
                PluginManager.AddDBusMenuEntry(trayEntry);
            }

            return base.Enable();
        }

        private void resetDBusMessage()
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
            {
                dBusObject.Position = -1;
                dBusObject.CollectionName = string.Empty;
            }
        }

        public void Seek(double position) => b.Value.Track.Seek(position);

        private DrawableTrack drawableTrack;

        public DrawableTrack GetCurrentTrack() => drawableTrack ??= new DrawableTrack(b.Value.Track);

        private bool isCurrent;

        public bool IsCurrent
        {
            get => isCurrent;
            set
            {
                if (trackChangedAfterDisable && value)
                {
                    drawableTrack = new DrawableTrack(b.Value.Track);
                    drawableTrack.Completed += () =>
                    {
                        if (IsCurrent) Schedule(NextTrack);
                    };
                    trackChangedAfterDisable = false;
                }

                isCurrent = value;
            }
        }

        private void changeBeatmap(WorkingBeatmap working)
        {
            if (Disabled.Value) return;

            b.Disabled = false;
            b.Value = working;
            b.Disabled = IsCurrent;
            drawableTrack = new DrawableTrack(b.Value.Track);
            drawableTrack.Completed += () =>
            {
                if (IsCurrent) Schedule(NextTrack);
            };
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
        private WorkingBeatmap getBeatmap(List<BeatmapSetInfo> list, WorkingBeatmap prevBeatmap, bool updateCurrentPosition = false, int displace = 1)
        {
            var prevSet = prevBeatmap.BeatmapSetInfo;

            //更新当前位置和最大位置
            if (updateCurrentPosition)
                CurrentPosition = list.IndexOf(prevSet);

            maxCount = list.Count;

            //当前位置往指定位置移动
            CurrentPosition += displace;

            //如果当前位置超过了最大位置或者不在范围内，那么回到第一个
            if (CurrentPosition >= maxCount || CurrentPosition < 0)
            {
                if (displace > 0) CurrentPosition = 0;
                else CurrentPosition = maxCount - 1;
            }

            //从list获取当前位置所在的BeatmapSetInfo, 然后选择该BeatmapSetInfo下的第一个WorkingBeatmap
            //最终赋值给NewBeatmap
            var newBeatmap = list.Count > 0
                ? beatmaps.GetWorkingBeatmap(list.ElementAt(CurrentPosition).Beatmaps.First())
                : b.Value;
            return newBeatmap;
        }

        ///<summary>
        ///用来更新<see cref="beatmapList"/>
        ///</summary>
        private void updateBeatmaps(BeatmapCollection collection)
        {
            //清理现有的谱面列表
            beatmapList.Clear();
            trayEntry.Children.Clear();

            if (collection == null) return;

            foreach (var item in collection.Beatmaps)
            {
                //获取当前BeatmapSet
                var currentSet = item.BeatmapSet;
                //进行比对，如果beatmapList中不存在，则添加。
                if (!beatmapList.Contains(currentSet))
                    beatmapList.Add(currentSet);

                if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
                {
                    var subEntry = new SimpleEntry
                    {
                        Label = item.BeatmapSet.Metadata.GetDisplayTitleRomanisable().GetPreferred(true),
                        OnActive = () =>
                        {
                            Schedule(() => Play(beatmaps.GetWorkingBeatmap(item)));
                        }
                    };

                    if (trayEntry.Children.All(s => s.Label != subEntry.Label))
                        trayEntry.Children.Add(subEntry);
                }
            }

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
                dBusObject.CollectionName = collection.Name.Value;

            updateCurrentPosition(true);
            trayEntry.Label = $"收藏夹（{collection.Name}）";
        }

        private SimpleEntry currentSubEntry;

        private void updateCurrentPosition(bool triggerDBusSubmenu = false)
        {
            CurrentPosition = beatmapList.IndexOf(b.Value.BeatmapSetInfo);

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
            {
                if (currentSubEntry != null)
                    currentSubEntry.ToggleState = 0;

                var targetEntry = trayEntry.Children.FirstOrDefault(s =>
                    s.Label == b.Value.BeatmapSetInfo.Metadata.GetDisplayTitleRomanisable().GetPreferred(true));

                if (targetEntry != null)
                    targetEntry.ToggleState = 1;

                currentSubEntry = targetEntry;

                if (triggerDBusSubmenu)
                    trayEntry.TriggerPropertyChangedEvent();
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
            if (collectionManager != null)
                collectionManager.Collections.CollectionChanged -= triggerRefresh;

            if (LLin != null) LLin.Resuming -= UpdateBeatmaps;

            base.Dispose(isDisposing);
        }
    }
}
