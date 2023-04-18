using System;
using System.Collections.Generic;
using System.Linq;
using M.DBus.Tray;
using M.DBus.Utils.Canonical.DBusMenuFlags;
using Mvis.Plugin.CollectionSupport.Config;
using Mvis.Plugin.CollectionSupport.DBus;
using Mvis.Plugin.CollectionSupport.Sidebar;
using Mvis.Plugin.CollectionSupport.Utils;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Screens.LLin.Misc;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Game.Screens.LLin.SideBar.Settings.Items;
using Realms;

namespace Mvis.Plugin.CollectionSupport
{
    public partial class CollectionHelper : BindableControlledPlugin, IProvideAudioControlPlugin
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IDisposable? realmSubscription;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private Bindable<WorkingBeatmap> b { get; set; } = null!;

        [Resolved]
        private MusicController controller { get; set; } = null!;

        private readonly List<IBeatmapSetInfo> beatmapList = new List<IBeatmapSetInfo>();

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

        public override int Version => 10;

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

        private readonly CollectionDBusObject dBusObject = new CollectionDBusObject();

        private readonly SimpleEntry trayEntry = new SimpleEntry
        {
            Label = "收藏夹（未选择任何收藏夹）",
            ChildrenDisplay = ChildrenDisplayType.Submenu
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (CollectionHelperConfigManager)DependenciesContainer.Get<LLinPluginManager>().GetConfigManager(this);
            config.BindWith(CollectionSettings.EnablePlugin, Enabled);
            b.BindValueChanged(v =>
            {
                updateCurrentPosition();
                if (!IsCurrent) trackChangedAfterDisable = true;
            });

            PluginManager!.RegisterDBusObject(dBusObject);

            if (LLin != null)
            {
                LLin.Resuming += UpdateBeatmaps;
                LLin.Exiting += onMvisExiting;
            }

            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>().OrderBy(c => c.Name), onCollectionChange);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CurrentCollection.BindValueChanged(OnCollectionChanged);
        }

        private void onMvisExiting()
        {
            PluginManager!.UnRegisterDBusObject(dBusObject);

            if (!Disabled.Value)
                PluginManager.RemoveDBusMenuEntry(trayEntry);

            resetDBusMessage();
        }

        public void Play(WorkingBeatmap b) => changeBeatmap(b);

        public bool NextTrack()
        {
            changeBeatmap(getBeatmap(beatmapList, b.Value, true));

            return beatmapList.Count != 0;
        }

        public bool PrevTrack()
        {
            changeBeatmap(getBeatmap(beatmapList, b.Value, true, -1));

            return beatmapList.Count != 0;
        }

        public bool TogglePause()
        {
            if (drawableTrack.IsRunning)
                drawableTrack.Stop();
            else
                drawableTrack.Start();

            return true;
        }

        public override bool Disable()
        {
            this.MoveToX(-10, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint);

            resetDBusMessage();
            PluginManager!.RemoveDBusMenuEntry(trayEntry);

            return base.Disable();
        }

        public override bool Enable()
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
            {
                dBusObject!.Position = currentPosition;
                dBusObject.CollectionName = CurrentCollection.Value?.Name ?? "-";
                PluginManager!.AddDBusMenuEntry(trayEntry);
            }

            return base.Enable();
        }

        private void resetDBusMessage()
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
            {
                dBusObject!.Position = -1;
                dBusObject.CollectionName = string.Empty;
            }
        }

        public bool Seek(double position)
        {
            b.Value.Track.Seek(position);

            return true;
        }

        private DrawableTrack drawableTrack = null!;

        public DrawableTrack GetCurrentTrack() => drawableTrack;

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
                        if (IsCurrent) Schedule(() => NextTrack());
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
                if (IsCurrent) Schedule(() => NextTrack());
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
        private WorkingBeatmap getBeatmap(List<IBeatmapSetInfo> list, WorkingBeatmap prevBeatmap, bool updateCurrentPosition = false, int displace = 1)
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
                ? beatmaps.GetWorkingBeatmap(list.ElementAt(CurrentPosition).Beatmaps.First().AsBeatmapInfo())
                : b.Value;
            return newBeatmap;
        }

        [Resolved]
        private BeatmapHashResolver hashResolver { get; set; } = null!;

        ///<summary>
        ///用来更新<see cref="beatmapList"/>
        ///</summary>
        private void updateBeatmaps(BeatmapCollection collection)
        {
            //清理现有的谱面列表
            beatmapList.Clear();
            trayEntry.Children.Clear();

            if (collection?.BeatmapMD5Hashes == null) return;

            foreach (string hash in collection.BeatmapMD5Hashes)
            {
                var item = hashResolver.ResolveHash(hash);

                //获取当前BeatmapSet
                var currentSet = item?.BeatmapSet;

                if (currentSet == null)
                {
                    Logger.Log($"{hash}解析到的谱面是null，将不会继续处理此Hash");
                    continue;
                }

                //进行比对，如果beatmapList中不存在，则添加。
                if (!beatmapList.Contains(currentSet))
                    beatmapList.Add(currentSet);

                if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
                {
                    var subEntry = new SimpleEntry
                    {
                        Label = item!.BeatmapSet?.Metadata.GetDisplayTitleRomanisable().GetPreferred(true) ?? item.ToString(),
                        OnActive = () =>
                        {
                            Schedule(() => Play(beatmaps.GetWorkingBeatmap(item.AsBeatmapInfo())));
                        }
                    };

                    if (trayEntry.Children.All(s => s.Label != subEntry.Label))
                        trayEntry.Children.Add(subEntry);
                }
            }

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
                dBusObject.CollectionName = collection.Name;

            updateCurrentPosition(true);
            trayEntry.Label = $"收藏夹（{collection.Name}）";
        }

        private SimpleEntry? currentSubEntry;

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

        public List<BeatmapCollection> AvaliableCollections { get; private set; } = new List<BeatmapCollection>();

        public static readonly BeatmapCollection DEFAULT_COLLECTION = new BeatmapCollection("未选择任何收藏夹");

        private void onCollectionChange(IRealmCollection<BeatmapCollection> collections, ChangeSet? changes, Exception error)
        {
            AvaliableCollections = collections.AsEnumerable().Select(c => c).ToList();

            if (CurrentCollection.Value != null)
            {
                var collectionMatch = AvaliableCollections.Find(c => c.ID == CurrentCollection.Value.ID);

                CurrentCollection.Value = collectionMatch ?? DEFAULT_COLLECTION;
            }
        }

        private void OnCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            updateBeatmaps(CurrentCollection.Value);
        }

        protected override void Dispose(bool isDisposing)
        {
            //collectionManager.Collections.CollectionChanged -= triggerRefresh;

            if (LLin != null)
                LLin.Resuming -= UpdateBeatmaps;

            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
        }
    }
}
