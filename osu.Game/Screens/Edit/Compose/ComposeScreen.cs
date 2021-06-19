// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Skinning;

namespace osu.Game.Screens.Edit.Compose
{
    public class ComposeScreen : EditorScreenWithTimeline, IKeyBindingHandler<PlatformAction>
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private EditorClock clock { get; set; }

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

            return wrapSkinnableContent(composer);
        }

        protected override Drawable CreateTimelineContent()
        {
            if (ruleset == null || composer == null)
                return base.CreateTimelineContent();

            return wrapSkinnableContent(new TimelineBlueprintContainer(composer));
        }

        private Drawable wrapSkinnableContent(Drawable content)
        {
            Debug.Assert(ruleset != null);

            var beatmapSkinProvider = new BeatmapSkinProvidingContainer(beatmap.Value.Skin);

            // the beatmapSkinProvider is used as the fallback source here to allow the ruleset-specific skin implementation
            // full access to all skin sources.
            var rulesetSkinProvider = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider, EditorBeatmap.PlayableBeatmap));

            // load the skinning hierarchy first.
            // this is intentionally done in two stages to ensure things are in a loaded state before exposing the ruleset to skin sources.
            return beatmapSkinProvider.WithChild(rulesetSkinProvider.WithChild(content));
        }

        #region Input Handling

        public bool OnPressed(PlatformAction action)
        {
            if (action.ActionType == PlatformActionType.Copy)
                host.GetClipboard().SetText(formatSelectionAsString());

            return false;
        }

        public void OnReleased(PlatformAction action)
        {
        }

        private string formatSelectionAsString()
        {
            if (composer == null)
                return string.Empty;

            double displayTime = EditorBeatmap.SelectedHitObjects.OrderBy(h => h.StartTime).FirstOrDefault()?.StartTime ?? clock.CurrentTime;
            string selectionAsString = composer.ConvertSelectionToString();

            return !string.IsNullOrEmpty(selectionAsString)
                ? $"{displayTime.ToEditorFormattedString()} ({selectionAsString}) - "
                : $"{displayTime.ToEditorFormattedString()} - ";
        }

        #endregion
    }
}
