using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.Helper;
using Mvis.Plugin.CloudMusicSupport.Misc;
using Mvis.Plugin.CloudMusicSupport.Sidebar;
using Mvis.Plugin.CloudMusicSupport.UI;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.Plugins.Types;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Audio;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace Mvis.Plugin.CloudMusicSupport
{
    public class LyricPlugin : BindableControlledPlugin, IProvideAudioControlPlugin
    {
        /// <summary>
        /// 请参阅 <see cref="MvisPlugin.TargetLayer"/>
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

        public override int Version => 4;

        private WorkingBeatmap currentWorkingBeatmap;
        private LyricLine lrcLine;

        /// <summary>
        /// 请参阅 <see cref="MvisPlugin.CreateContent()"/>
        /// </summary>
        protected override Drawable CreateContent() => lrcLine = new LyricLine();

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
            MvisScreen.RequestAudioControl(this,
                "编辑歌词需要禁用切歌功能",
                () => IsEditing = false,
                onAllow);
        }

        public bool IsEditing
        {
            set
            {
                if (!value)
                    MvisScreen.ReleaseAudioControlFrom(this);
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

            RelativeSizeAxes = Axes.X;
            Height = 300;
            Anchor = Origin = Anchor.BottomCentre;
        }

        /// <summary>
        /// 请参阅 <see cref="MvisPlugin.OnContentLoaded(Drawable)"/>
        /// </summary>
        protected override bool OnContentLoaded(Drawable content) => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (LyricConfigManager)Dependencies.Get<MvisPluginManager>().GetConfigManager(this);

            config.BindWith(LyricSettings.EnablePlugin, Value);
            config.BindWith(LyricSettings.LyricOffset, offset);
            autoSave = config.GetBindable<bool>(LyricSettings.SaveLrcWhenFetchFinish);

            AddInternal(processor);
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

            processor.StartFetchLrcFor(currentWorkingBeatmap, noLocalFile, onLyricRequestFinished, onLyricRequestFail);
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
            Schedule(() => CurrentStatus.Value = Status.Failed);
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

            return base.Disable();
        }

        public override bool Enable()
        {
            bool result = base.Enable();

            this.MoveToX(0, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);

            MvisScreen?.OnBeatmapChanged(onBeatmapChanged, this, true);

            return result;
        }

        protected override bool PostInit() => true;

        public Lyric CurrentLine;
        private readonly Lyric defaultLrc = new Lyric();

        protected override void Update()
        {
            base.Update();

            Margin = new MarginPadding { Bottom = (MvisScreen?.BottombarHeight ?? 0) + 20 };

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
