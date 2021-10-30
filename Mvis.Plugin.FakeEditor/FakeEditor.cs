using Mvis.Plugin.FakeEditor.Config;
using Mvis.Plugin.FakeEditor.Editor;
using Mvis.Plugin.FakeEditor.UI;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.LLin.Misc;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Game.Screens.Play;

namespace Mvis.Plugin.FakeEditor
{
    [Cached(typeof(IBeatSnapProvider))]
    [Cached(typeof(ISamplePlaybackDisabler))]
    public class FakeEditor : BindableControlledPlugin, IBeatSnapProvider, ISamplePlaybackDisabler
    {
        public IBindable<bool> SamplePlaybackDisabled => samplePlaybackDisabled;

        private readonly BindableBool samplePlaybackDisabled = new BindableBool
        {
            Value = true,
            Default = true
        };

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        private WorkingBeatmap beatmap;

        public override int Version => 8;

        public FakeEditor()
        {
            Name = "谱面编辑器";
            Description = "用于提供Note音效; 高内存占用, 不要用来尝试那些会崩掉你游戏/电脑的图";
            Author = "mf-osu";

            Masking = true;

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload
            });
        }

        [Resolved]
        private MusicController musicController { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (FakeEditorConfigManager)dependencies.Get<LLinPluginManager>().GetConfigManager(this);
            config.BindWith(FakeEditorSetting.EnableFakeEditor, Value);

            dependencies.CacheAs(beatDivisor);

            AddInternal(new BlockMouseBox
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
                Alpha = 0.001f
            });

            if (LLin != null)
            {
                LLin.OnSeek += Seek;
                LLin.OnBeatmapChanged(initDependencies, this);
            }
        }

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new FakeEditorConfigManager(storage);

        public override PluginSettingsSubSection CreateSettingsSubSection()
            => new FakeEditorSettings(this);

        public override PluginSidebarSettingsSection CreateSidebarSettingsSection()
            => new FakeEditorSidebarSection(this);

        public override void UnLoad()
        {
            if (LLin != null)
                LLin.OnSeek -= Seek;

            base.UnLoad();
        }

        private void initDependencies(WorkingBeatmap beatmap)
        {
            this.beatmap = beatmap;

            EditorClock?.ChangeSource(beatmap.Track);
            Seek(beatmap.Track.CurrentTime);

            beatDivisor.Value = beatmap.BeatmapInfo.BeatDivisor;

            if (EditorClock == null)
            {
                AddInternal(EditorClock = new EditorClock(beatmap.GetPlayableBeatmap(beatmap.BeatmapInfo.Ruleset ?? new DummyRulesetInfo()), beatDivisor)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    IsCoupled = true,
                    DisableSourceAdjustment = true
                });

                dependencies.CacheAs(EditorClock);
            }

            reload();
        }

        private void reload()
        {
            Clear();

            if (!Disabled.Value)
                Load();
        }

        protected override void OnValueChanged(ValueChangedEvent<bool> v)
        {
            base.OnValueChanged(v);

            updateSamplePlaybackDisabled();

            if (v.NewValue)
            {
                if (ContentLoaded)
                    this.FadeTo(0.01f);
            }
            else
            {
                if (ContentLoaded)
                    this.FadeOut();
            }
        }

        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        public EditorClock EditorClock;

        public void Seek(double location) => EditorClock?.Seek(location);

        protected override void Update()
        {
            EditorClock?.ProcessFrame();
            base.Update();
        }

        protected override Drawable CreateContent()
            => new EditorContainer(beatmap);

        protected override bool PostInit() => true;

        protected override bool OnContentLoaded(Drawable content)
        {
            EditorClock.ChangeSource(musicController.CurrentTrack);

            //todo: 移除下面这一行的同时确保samplePlaybackDisabled的值可以正常随音乐变动
            updateSamplePlaybackDisabled();

            if (LLin != null)
                LLin.OnTrackRunningToggle += _ => updateSamplePlaybackDisabled();

            //Logger.Log($"Clock源: {EditorClock.Source}");
            //Logger.Log($"是否不能单独操作: {EditorClock.IsCoupled}");
            //Logger.Log($"是否在运行: {EditorClock.IsRunning}");
            //Logger.Log($"当前Track是否在运行: {music.CurrentTrack.IsRunning}");
            //Logger.Log($"在Seek或已经停止: {EditorClock.SeekingOrStopped}");

            return true;
        }

        private void updateSamplePlaybackDisabled() =>
            samplePlaybackDisabled.Value = !Value.Value || !musicController.CurrentTrack.IsRunning;

        public double SnapTime(double time, double? referenceTime = null) => 0;

        public double GetBeatLengthAtTime(double referenceTime) => 0;

        public int BeatDivisor => beatDivisor.Value;
    }
}
