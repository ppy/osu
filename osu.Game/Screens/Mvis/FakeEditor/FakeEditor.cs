using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;

namespace osu.Game.Screens.Mvis.FakeEditor
{
    [Cached(typeof(IBeatSnapProvider))]
    public class FakeEditor : Container, IBeatSnapProvider
    {
        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        private readonly WorkingBeatmap beatmap;
        private EditorBeatmap editorBeatmap;

        public FakeEditor(WorkingBeatmap beatmap)
        {
            this.beatmap = beatmap;
        }

        [Resolved]
        private MusicController music { get; set; }

        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        public EditorClock EditorClock;

        public void Seek(double location) => EditorClock?.Seek(location);

        [BackgroundDependencyLoader]
        private void load()
        {
            beatDivisor.Value = beatmap.BeatmapInfo.BeatDivisor;

            EditorClock = new EditorClock(beatmap, beatDivisor)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                IsCoupled = false
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

            AddInternal(new BlockMouseContainer
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
                Alpha = 0.001f
            });

            dependencies.CacheAs(editorBeatmap);
        }

        protected override void Update()
        {
            EditorClock.ProcessFrame();
            base.Update();
        }

        protected override void LoadComplete()
        {
            var ruleset = beatmap.BeatmapInfo.Ruleset?.CreateInstance();

            if (ruleset == null)
            {
                Logger.Log($"未能从 {beatmap.BeatmapInfo} 获取到可用的ruleset, 将中止加载");
                return;
            }

            var beatmapSkinProvider = new BeatmapSkinProvidingContainer(beatmap.Skin);

            // the beatmapSkinProvider is used as the fallback source here to allow the ruleset-specific skin implementation
            // full access to all skin sources.
            var rulesetSkinProvider = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider, editorBeatmap.PlayableBeatmap));

            AddInternal(beatmapSkinProvider.WithChild(rulesetSkinProvider.WithChild(ruleset.CreateHitObjectComposer())));
            EditorClock.ChangeSource(music.CurrentTrack, false);
            EditorClock.Start();

            //Logger.Log($"Clock源: {EditorClock.Source}");
            //Logger.Log($"是否在运行: {EditorClock.IsRunning}");
            //Logger.Log($"当前Track是否在运行: {music.CurrentTrack.IsRunning}");
            //Logger.Log($"在Seek或已经停止: {EditorClock.SeekingOrStopped}");

            base.LoadComplete();
        }

        public double SnapTime(double time, double? referenceTime = null) => editorBeatmap.SnapTime(time, referenceTime);

        public double GetBeatLengthAtTime(double referenceTime) => editorBeatmap.GetBeatLengthAtTime(referenceTime);

        public int BeatDivisor => beatDivisor.Value;

        private class BlockMouseContainer : Box
        {
            protected override bool OnClick(ClickEvent e) => true;
            protected override bool OnMouseMove(MouseMoveEvent e) => true;
            protected override bool OnMouseDown(MouseDownEvent e) => true;
        }
    }
}
