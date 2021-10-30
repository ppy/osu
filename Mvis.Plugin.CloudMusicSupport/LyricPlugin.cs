using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using M.DBus.Tray;
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
using osu.Framework.Graphics.Audio;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace Mvis.Plugin.CloudMusicSupport
{
    public class LyricPlugin : BindableControlledPlugin, IProvideAudioControlPlugin
    {
        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.TargetLayer"/>
        /// </summary>
        public override TargetLayer Target => TargetLayer.Foreground;

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new LyricConfigManager(storage);

        public override PluginSettingsSubSection CreateSettingsSubSection()
            => new LyricSettingsSubSection(this);

        public override PluginSidebarPage CreateSidebarPage()
            => new LyricSidebarSectionContainer(this);

        public override PluginSidebarSettingsSection CreateSidebarSettingsSection()
            => new LyricSidebarSection(this);

        public override int Version => 8;

        private WorkingBeatmap currentWorkingBeatmap;
        private LyricLineHandler lrcLine;

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.CreateContent()"/>
        /// </summary>
        protected override Drawable CreateContent() => lrcLine = new LyricLineHandler();

        private readonly LyricProcessor processor = new LyricProcessor();

        [NotNull]
        public List<Lyric> Lyrics { get; private set; } = new List<Lyric>();

        public void ReplaceLyricWith(List<Lyric> newList, bool saveToDisk)
        {
            CurrentStatus.Value = Status.Working;

            Lyrics = newList;

            if (saveToDisk)
                WriteLyricToDisk();

            CurrentStatus.Value = Status.Finish;
        }

        [Resolved]
        private MusicController controller { get; set; }

        [Resolved]
        private MConfigManager mConfig { get; set; }

        public void RequestControl(Action onAllow)
        {
            LLin.RequestAudioControl(this,
                CloudMusicStrings.AudioControlRequest,
                () => IsEditing = false,
                onAllow);
        }

        public void GetLyricFor(int id)
        {
            CurrentStatus.Value = Status.Working;
            processor.StartFetchById(id, onLyricRequestFinished, onLyricRequestFail);
        }

        public bool IsEditing
        {
            set
            {
                if (!value)
                    LLin.ReleaseAudioControlFrom(this);
            }
        }

        private Track track;
        private readonly BindableDouble offset = new BindableDouble();
        private Bindable<bool> autoSave;

        public readonly Bindable<Status> CurrentStatus = new Bindable<Status>();

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

        [Resolved]
        private OsuGame game { get; set; }

        private readonly SimpleEntry lyricEntry = new SimpleEntry
        {
            Enabled = false
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (LyricConfigManager)Dependencies.Get<LLinPluginManager>().GetConfigManager(this);

            config.BindWith(LyricSettings.EnablePlugin, Value);
            config.BindWith(LyricSettings.LyricOffset, offset);
            autoSave = config.GetBindable<bool>(LyricSettings.SaveLrcWhenFetchFinish);

            AddInternal(processor);

            PluginManager.RegisterDBusObject(dbusObject = new LyricDBusObject());

            if (LLin != null)
            {
                LLin.Exiting += onMvisExiting;
            }
        }

        private void onMvisExiting()
        {
            resetDBusMessage();
            PluginManager.UnRegisterDBusObject(new LyricDBusObject());

            if (!Disabled.Value)
                PluginManager.RemoveDBusMenuEntry(lyricEntry);
        }

        public void WriteLyricToDisk()
        {
            processor.WriteLrcToFile(Lyrics, currentWorkingBeatmap);
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
            CurrentLine = null;

            processor.StartFetchByBeatmap(currentWorkingBeatmap, noLocalFile, onLyricRequestFinished, onLyricRequestFail);
        }

        private double targetTime => track.CurrentTime + offset.Value;

        private void onBeatmapChanged(WorkingBeatmap working)
        {
            if (Disabled.Value) return;

            currentWorkingBeatmap = working;
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

        private void onLyricRequestFinished(List<Lyric> lyrics)
        {
            Schedule(() =>
            {
                Lyrics = lyrics;

                if (autoSave.Value)
                    WriteLyricToDisk();

                CurrentStatus.Value = Status.Finish;
            });
        }

        public override bool Disable()
        {
            this.MoveToX(-10, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint);

            resetDBusMessage();
            PluginManager.RemoveDBusMenuEntry(lyricEntry);

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

                PluginManager.AddDBusMenuEntry(lyricEntry);
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

        private Lyric currentLine;
        private readonly Lyric emptyLine = new Lyric();

        public Lyric CurrentLine
        {
            get => currentLine;
            set
            {
                value ??= emptyLine;

                currentLine = value;

                if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
                {
                    dbusObject.RawLyric = value.Content;
                    dbusObject.TranslatedLyric = value.TranslatedString;

                    lyricEntry.Label = value.Content + "\n" + value.TranslatedString;
                }
            }
        }

        private readonly Lyric defaultLrc = new Lyric();
        private LyricDBusObject dbusObject;

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

        public void NextTrack()
        {
        }

        public void PrevTrack()
        {
        }

        public void TogglePause() => controller.TogglePause();

        public void Seek(double position) => currentWorkingBeatmap?.Track.Seek(position);

        public DrawableTrack GetCurrentTrack() => controller.CurrentTrack;

        public bool IsCurrent { get; set; }
    }
}
