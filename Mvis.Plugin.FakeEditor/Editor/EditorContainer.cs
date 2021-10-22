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
            //不知道为什么，谱面的Ruleset会是null??????
            var rulesetInfo = beatmap.BeatmapInfo.Ruleset ?? new DummyRulesetInfo();
            ruleset = rulesetInfo.CreateInstance();

            var playableBeatmap = beatmap.GetPlayableBeatmap(rulesetInfo);

            if (editorBeatmap == null)
            {
                AddInternal(editorBeatmap = new EditorBeatmap(playableBeatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });

                dependencies.CacheAs(editorBeatmap);
            }

            if (rulesetInfo is DummyRulesetInfo)
                return;

            beatmapSkinProvider = new BeatmapSkinProvidingContainer(beatmap.Skin);
            rulesetSkinProvider = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider, editorBeatmap.PlayableBeatmap));

            InternalChild = beatmapSkinProvider.WithChild(rulesetSkinProvider.WithChild(ruleset.CreateHitObjectComposer()));
        }
    }
}
