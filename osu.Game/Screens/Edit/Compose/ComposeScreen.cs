// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Skinning;

namespace osu.Game.Screens.Edit.Compose
{
    public class ComposeScreen : EditorScreenWithTimeline
    {
        private HitObjectComposer composer;

        public ComposeScreen()
            : base(EditorScreenMode.Compose)
        {
        }

        private Ruleset ruleset;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            ruleset = parent.Get<IBindable<WorkingBeatmap>>().Value.BeatmapInfo.Ruleset?.CreateInstance();
            composer = ruleset?.CreateHitObjectComposer();

            // make the composer available to the timeline and other components in this screen.
            if (composer != null)
                dependencies.CacheAs(composer);

            return dependencies;
        }

        protected override Drawable CreateMainContent()
        {
            if (ruleset == null || composer == null)
                return new ScreenWhiteBox.UnderConstructionMessage(ruleset == null ? "This beatmap" : $"{ruleset.Description}'s composer");

            var beatmapSkinProvider = new BeatmapSkinProvidingContainer(Beatmap.Value.Skin);

            // the beatmapSkinProvider is used as the fallback source here to allow the ruleset-specific skin implementation
            // full access to all skin sources.
            var rulesetSkinProvider = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider, EditorBeatmap.PlayableBeatmap));

            // load the skinning hierarchy first.
            // this is intentionally done in two stages to ensure things are in a loaded state before exposing the ruleset to skin sources.
            return beatmapSkinProvider.WithChild(rulesetSkinProvider.WithChild(composer));
        }

        protected override Drawable CreateTimelineContent() => composer == null ? base.CreateTimelineContent() : new TimelineBlueprintContainer(composer);
    }
}
