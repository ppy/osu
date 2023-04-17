using System.Collections.Generic;
using JetBrains.Annotations;
using M.DBus.Tray;
using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.DBus;
using Mvis.Plugin.CloudMusicSupport.Helper;
using Mvis.Plugin.CloudMusicSupport.Misc;
using Mvis.Plugin.CloudMusicSupport.Sidebar;
using Mvis.Plugin.CloudMusicSupport.UI;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.LLin.Plugins.Types.SettingsItems;

namespace Mvis.Plugin.CloudMusicSupport
{
    public partial class LyricPlugin : BindableControlledPlugin
    {
        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.TargetLayer"/>
        /// </summary>
        public override TargetLayer Target => TargetLayer.Foreground;

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new LyricConfigManager(storage);

        private SettingsEntry[]? entries;

        public bool IsContentLoaded => ContentLoaded;

        public override SettingsEntry[] GetSettingEntries(IPluginConfigManager pluginConfigManager)
        {
            var config = (LyricConfigManager)pluginConfigManager;

            entries = new SettingsEntry[]
            {
                new BooleanSettingsEntry
                {
                    Name = LLinGenericStrings.EnablePlugin,
                    Bindable = config.GetBindable<bool>(LyricSettings.EnablePlugin)
                },
                new BooleanSettingsEntry
                {
                    Icon = FontAwesome.Solid.Save,
                    Name = CloudMusicStrings.SaveLyricOnDownloadedMain,
                    Bindable = config.GetBindable<bool>(LyricSettings.SaveLrcWhenFetchFinish),
                    Description = CloudMusicStrings.SaveLyricOnDownloadedSub
                },
                new BooleanSettingsEntry
                {
                    Icon = FontAwesome.Solid.FillDrip,
                    Name = CloudMusicStrings.DisableShader,
                    Bindable = config.GetBindable<bool>(LyricSettings.NoExtraShadow)
                },
                new NumberSettingsEntry<float>
                {
                    Name = CloudMusicStrings.LyricFadeInDuration,
                    Bindable = config.GetBindable<float>(LyricSettings.LyricFadeInDuration)
                },
                new NumberSettingsEntry<float>
                {
                    Name = CloudMusicStrings.LyricFadeOutDuration,
                    Bindable = config.GetBindable<float>(LyricSettings.LyricFadeOutDuration)
                },
                new BooleanSettingsEntry
                {
                    Name = CloudMusicStrings.LyricAutoScrollMain,
                    Bindable = config.GetBindable<bool>(LyricSettings.AutoScrollToCurrent)
                },
                new ListSettingsEntry<Anchor>
                {
                    Icon = FontAwesome.Solid.Anchor,
                    Name = CloudMusicStrings.LocationDirection,
                    Bindable = config.GetBindable<Anchor>(LyricSettings.LyricDirection),
                    Values = new[]
                    {
                        Anchor.TopLeft,
                        Anchor.TopCentre,
                        Anchor.TopRight,
                        Anchor.CentreLeft,
                        Anchor.Centre,
                        Anchor.CentreRight,
                        Anchor.BottomLeft,
                        Anchor.BottomCentre,
                        Anchor.BottomRight,
                    }
                },
                new NumberSettingsEntry<float>
                {
                    Name = CloudMusicStrings.PositionX,
                    Bindable = config.GetBindable<float>(LyricSettings.LyricPositionX),
                    DisplayAsPercentage = true
                },
                new NumberSettingsEntry<float>
                {
                    Name = CloudMusicStrings.PositionY,
                    Bindable = config.GetBindable<float>(LyricSettings.LyricPositionY),
                    DisplayAsPercentage = true
                },
                new NumberSettingsEntry<float>
                {
                    Name = "歌曲相似度阈值",
                    Bindable = config.GetBindable<float>(LyricSettings.TitleSimilarThreshold),
                    DisplayAsPercentage = true,
                    Description = "网易云搜出的歌词有时不一定能和当前谱面匹配。 阈值越高, 对网易云返回的搜索结果检查越严格，搜索成功率也就越低"
                },
                new BooleanSettingsEntry
                {
                    Name = "启用用户定义",
                    Bindable = config.GetBindable<bool>(LyricSettings.EnableUserDefinitions),
                    Description = "启用用户定义后，将通过设置的URL获取相关设置来优先匹配本地谱面ID以提供更准确的歌词查询"
                },
                new BooleanSettingsEntry
                {
                    Name = "输出定义到日志",
                    Bindable = config.GetBindable<bool>(LyricSettings.OutputDefinitionInLogs),
                    Description = "更新定义时输出内容到日志中，可以在某些情况下帮助查找相关信息"
                },
                new StringSettingsEntry
                {
                    Name = "用户定义文件URL",
                    Bindable = config.GetBindable<string>(LyricSettings.UserDefinitionURL),
                    Description = "将通过此URL拉取用户定义配置"
                },
            };

            return entries;
        }

        public override PluginSidebarPage CreateSidebarPage()
            => new LyricSidebarSectionContainer(this);

        public override int Version => 10;

        internal WorkingBeatmap CurrentWorkingBeatmap = null!;
        private readonly LyricLineHandler lrcLine = new LyricLineHandler();

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.CreateContent()"/>
        /// </summary>
        protected override Drawable CreateContent() => lrcLine;

        public readonly LyricProcessor LyricProcessor = new LyricProcessor();

        private List<Lyric>? cachedLyrics;

        public readonly List<Lyric> EmptyLyricList = new List<Lyric>();

        private APILyricResponseRoot? currentResponseRoot;

        [NotNull]
        public List<Lyric> Lyrics
        {
            get => cachedLyrics ?? EmptyLyricList;
            private set => cachedLyrics = value;
        }

        public void ReplaceLyricWith(List<Lyric> newList, bool saveToDisk)
        {
            CurrentStatus.Value = Status.Working;

            Lyrics = newList;

            if (saveToDisk)
                WriteLyricToDisk();

            CurrentStatus.Value = Status.Finish;
        }

        public void GetLyricFor(int id)
        {
            CurrentStatus.Value = Status.Working;
            LyricProcessor.SearchByNeteaseID(id, CurrentWorkingBeatmap, onLyricRequestFinished, onLyricRequestFail);
        }

        private Track track = null!;

        public readonly BindableDouble Offset = new BindableDouble
        {
            MaxValue = 3000,
            MinValue = -3000
        };

        private readonly Bindable<bool> autoSave = new Bindable<bool>();

        public readonly Bindable<Status> CurrentStatus = new Bindable<Status>();

        public readonly Bindable<float> TitleSimilarThreshold = new Bindable<float>();

        public LyricPlugin()
        {
            Name = "歌词";
            Description = "从网易云音乐获取歌词信息";
            Author = "MATRIX-夜翎";
            Depth = -1;

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload
            });

            RelativeSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.BottomCentre;
        }

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.OnContentLoaded(Drawable)"/>
        /// </summary>
        protected override bool OnContentLoaded(Drawable content) => true;

        private readonly SimpleEntry lyricEntry = new SimpleEntry
        {
            Enabled = false
        };

        [Cached]
        public UserDefinitionHelper UserDefinitionHelper { get; private set; } = new UserDefinitionHelper();

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (LyricConfigManager)Dependencies.Get<LLinPluginManager>().GetConfigManager(this);

            config.BindWith(LyricSettings.EnablePlugin, Enabled);
            config.BindWith(LyricSettings.SaveLrcWhenFetchFinish, autoSave);
            config.BindWith(LyricSettings.TitleSimilarThreshold, TitleSimilarThreshold);

            AddInternal(LyricProcessor);
            AddInternal(UserDefinitionHelper);

            PluginManager!.RegisterDBusObject(dbusObject);

            if (LLin != null)
                LLin.Exiting += onMvisExiting;

            Offset.BindValueChanged(v =>
            {
                if (currentResponseRoot != null)
                    currentResponseRoot.LocalOffset = v.NewValue;
            });
        }

        private void onMvisExiting()
        {
            resetDBusMessage();
            PluginManager!.UnRegisterDBusObject(dbusObject);

            if (!Disabled.Value)
                PluginManager.RemoveDBusMenuEntry(lyricEntry);
        }

        public void WriteLyricToDisk(WorkingBeatmap? currentBeatmap = null)
        {
            currentBeatmap ??= CurrentWorkingBeatmap;
            LyricProcessor.WriteLrcToFile(currentResponseRoot, currentBeatmap);
        }

        public void RefreshLyric(bool noLocalFile = false)
        {
            CurrentStatus.Value = Status.Working;

            if (lrcLine != null)
            {
                lrcLine.Text = string.Empty;
                lrcLine.TranslatedText = string.Empty;
            }

            Lyrics.Clear();
            currentResponseRoot = null;
            CurrentLine = null;

            if (UserDefinitionHelper.BeatmapMetaHaveDefinition(CurrentWorkingBeatmap.BeatmapInfo, out int neid))
                GetLyricFor(neid);
            else if (UserDefinitionHelper.OnlineIDHaveDefinition(CurrentWorkingBeatmap.BeatmapSetInfo.OnlineID, out neid))
                GetLyricFor(neid);
            else
                LyricProcessor.Search(SearchOption.From(CurrentWorkingBeatmap, noLocalFile, onLyricRequestFinished, onLyricRequestFail, TitleSimilarThreshold.Value));
        }

        private double targetTime => track.CurrentTime + Offset.Value;

        private void onBeatmapChanged(WorkingBeatmap working)
        {
            if (Disabled.Value) return;

            if (CurrentWorkingBeatmap != null) WriteLyricToDisk(CurrentWorkingBeatmap);

            CurrentWorkingBeatmap = working;
            track = working.Track;

            CurrentStatus.Value = Status.Working;

            RefreshLyric();
        }

        private void onLyricRequestFail(string msg)
        {
            //onLyricRequestFail会在非Update上执行，因此添加Schedule确保不会发生InvalidThreadForMutationException
            Schedule(() =>
            {
                Lyrics.Clear();
                CurrentStatus.Value = Status.Failed;
            });
        }

        private void onLyricRequestFinished(APILyricResponseRoot responseRoot)
        {
            Schedule(() =>
            {
                Offset.Value = responseRoot.LocalOffset;
                currentResponseRoot = responseRoot;

                Lyrics = responseRoot.ToLyricList();

                if (autoSave.Value)
                    WriteLyricToDisk();

                CurrentStatus.Value = Status.Finish;
            });
        }

        public override bool Disable()
        {
            this.MoveToX(-10, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint);

            resetDBusMessage();
            PluginManager!.RemoveDBusMenuEntry(lyricEntry);

            return base.Disable();
        }

        public override bool Enable()
        {
            bool result = base.Enable();

            this.MoveToX(0, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);

            LLin?.OnBeatmapChanged(onBeatmapChanged, this, true);

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
            {
                dbusObject.RawLyric = currentLine?.Content;
                dbusObject.TranslatedLyric = currentLine?.TranslatedString;

                PluginManager!.AddDBusMenuEntry(lyricEntry);
            }

            return result;
        }

        private void resetDBusMessage()
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
            {
                dbusObject.RawLyric = string.Empty;
                dbusObject.TranslatedLyric = string.Empty;
            }
        }

        protected override bool PostInit() => true;

        private Lyric? currentLine;
        private readonly Lyric emptyLine = new Lyric();

        public Lyric? CurrentLine
        {
            get => currentLine;
            set
            {
                value ??= emptyLine;

                currentLine = value;

                if (RuntimeInfo.OS != RuntimeInfo.Platform.Linux) return;

                dbusObject.RawLyric = value.Content;
                dbusObject.TranslatedLyric = value.TranslatedString;

                lyricEntry.Label = $"♩: {value.TranslatedString}\n♪: {value.Content}";
            }
        }

        private readonly Lyric defaultLrc = new Lyric();
        private readonly LyricDBusObject dbusObject = new LyricDBusObject();

        protected override void Update()
        {
            base.Update();

            Padding = new MarginPadding { Bottom = (LLin?.BottomBarHeight ?? 0) + 20 };

            if (ContentLoaded)
            {
                var lrc = Lyrics.FindLast(l => targetTime >= l.Time) ?? defaultLrc;

                if (!lrc.Equals(CurrentLine))
                {
                    lrcLine.Text = lrc.Content;
                    lrcLine.TranslatedText = lrc.TranslatedString;

                    CurrentLine = lrc.GetCopy();
                }
            }
        }

        public enum Status
        {
            Working,
            Failed,
            Finish
        }
    }
}
