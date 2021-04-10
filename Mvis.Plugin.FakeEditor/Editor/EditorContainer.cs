using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;

namespace Mvis.Plugin.FakeEditor.Editor
{
    public class EditorContainer : Container
    {
        private readonly WorkingBeatmap beatmap;

        private Ruleset ruleset;
        private BeatmapSkinProvidingContainer beatmapSkinProvider;
        private SkinProvidingContainer rulesetSkinProvider;
        private EditorBeatmap editorBeatmap;
        private DependencyContainer dependencies;

        public EditorContainer(WorkingBeatmap b)
        {
            beatmap = b;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset = beatmap.BeatmapInfo.Ruleset?.CreateInstance();

            var playableBeatmap = beatmap.GetPlayableBeatmap(beatmap.BeatmapInfo.Ruleset);

            if (editorBeatmap == null)
            {
                AddInternal(editorBeatmap = new EditorBeatmap(playableBeatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });

                dependencies.CacheAs(editorBeatmap);
            }

            if (ruleset == null)
                return;

            beatmapSkinProvider = new BeatmapSkinProvidingContainer(beatmap.Skin);
            rulesetSkinProvider = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider, editorBeatmap.PlayableBeatmap));

            editorBeatmap.BeatmapInfo = beatmap.BeatmapInfo;

            InternalChild = beatmapSkinProvider.WithChild(rulesetSkinProvider.WithChild(ruleset.CreateHitObjectComposer()));
        }
    }
}
