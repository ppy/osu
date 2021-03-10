using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Screens.Mvis.Plugins
{
    [Cached(typeof(IBeatSnapProvider))]
    [Cached(typeof(ISamplePlaybackDisabler))]
    public class FakeEditor : MvisPlugin, IBeatSnapProvider, ISamplePlaybackDisabler
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

        private readonly WorkingBeatmap beatmap;
        private EditorBeatmap editorBeatmap;

        public FakeEditor(WorkingBeatmap beatmap)
        {
            Name = "假Editor";
            Description = "用于提供打击音效; Mfosu自带插件";

            this.beatmap = beatmap;
            Masking = true;
        }

        private readonly BindableBool enableFakeEditor = new BindableBool();

        [Resolved]
        private MusicController musicController { get; set; }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisEnableFakeEditor, enableFakeEditor);
            enableFakeEditor.BindValueChanged(onEnableFakeEditorChanged);
        }

        private void onEnableFakeEditorChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                if (ContentLoaded)
                {
                    this.FadeIn();
                }
                else
                    Load();
            }
            else
            {
                if (ContentLoaded)
                    this.FadeOut();
                else
                    Cancel();
            }
        }

        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        public EditorClock EditorClock;

        private Ruleset ruleset;
        private BeatmapSkinProvidingContainer beatmapSkinProvider;
        private SkinProvidingContainer rulesetSkinProvider;

        public void Seek(double location) => EditorClock?.Seek(location);

        protected override void Update()
        {
            EditorClock?.ProcessFrame();
            base.Update();
        }

        protected override Drawable CreateContent()
            => beatmapSkinProvider.WithChild(rulesetSkinProvider.WithChild(ruleset.CreateHitObjectComposer()));

        protected override bool PostInit()
        {
            if (!enableFakeEditor.Value)
                return false;

            beatDivisor.Value = beatmap.BeatmapInfo.BeatDivisor;

            EditorClock = new EditorClock(beatmap, beatDivisor)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                IsCoupled = true,
                DisableSourceAdjustment = true
            };

            AddInternal(EditorClock);
            dependencies.CacheAs(EditorClock);
            dependencies.CacheAs(beatDivisor);

            var playableBeatmap = beatmap.GetPlayableBeatmap(beatmap.BeatmapInfo.Ruleset);

            AddInternal(editorBeatmap = new EditorBeatmap(playableBeatmap)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            AddInternal(new BlockMouseBox
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
                Alpha = 0.001f
            });

            dependencies.CacheAs(editorBeatmap);

            ruleset = beatmap.BeatmapInfo.Ruleset?.CreateInstance();

            if (ruleset == null)
            {
                Logger.Log($"未能从 {beatmap.BeatmapInfo} 获取到可用的ruleset, 将中止加载");
                return false;
            }

            beatmapSkinProvider = new BeatmapSkinProvidingContainer(beatmap.Skin);
            rulesetSkinProvider = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider, editorBeatmap.PlayableBeatmap));

            return true;
        }

        protected override bool OnContentLoaded(Drawable content)
        {
            EditorClock.ChangeSource(musicController.CurrentTrack);

            //todo: 移除下面这一行的同时确保samplePlaybackDisabled的值可以正常随音乐变动
            samplePlaybackDisabled.Value = !musicController.CurrentTrack.IsRunning;

            if (MvisScreen != null)
                MvisScreen.OnTrackRunningToggle += running => samplePlaybackDisabled.Value = !running;

            //Logger.Log($"Clock源: {EditorClock.Source}");
            //Logger.Log($"是否不能单独操作: {EditorClock.IsCoupled}");
            //Logger.Log($"是否在运行: {EditorClock.IsRunning}");
            //Logger.Log($"当前Track是否在运行: {music.CurrentTrack.IsRunning}");
            //Logger.Log($"在Seek或已经停止: {EditorClock.SeekingOrStopped}");

            return true;
        }

        public double SnapTime(double time, double? referenceTime = null) => editorBeatmap.SnapTime(time, referenceTime);

        public double GetBeatLengthAtTime(double referenceTime) => editorBeatmap.GetBeatLengthAtTime(referenceTime);

        public int BeatDivisor => beatDivisor.Value;

        public class BlockMouseBox : Box
        {
            protected override bool OnClick(ClickEvent e) => true;
            protected override bool OnMouseMove(MouseMoveEvent e) => true;
            protected override bool OnMouseDown(MouseDownEvent e) => true;
        }
    }
}
