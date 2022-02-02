// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Compose
{
    public class ComposeScreen : EditorScreenWithTimeline
    {
        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private EditorClock clock { get; set; }

        private Bindable<string> clipboard { get; set; }

        private HitObjectComposer composer;

        public ComposeScreen()
            : base(EditorScreenMode.Compose)
        {
        }

        private Ruleset ruleset;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            ruleset = parent.Get<IBindable<WorkingBeatmap>>().Value.BeatmapInfo.Ruleset.CreateInstance();
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

            return new EditorSkinProvidingContainer(EditorBeatmap).WithChild(content);
        }

        [BackgroundDependencyLoader]
        private void load(EditorClipboard clipboard)
        {
            this.clipboard = clipboard.Content.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // May be null in the case of a ruleset that doesn't have editor support, see CreateMainContent().
            if (composer == null)
                return;

            EditorBeatmap.SelectedHitObjects.BindCollectionChanged((_, __) => updateClipboardActionAvailability());
            clipboard.BindValueChanged(_ => updateClipboardActionAvailability());
            composer.OnLoadComplete += _ => updateClipboardActionAvailability();
            updateClipboardActionAvailability();
        }

        #region Clipboard operations

        protected override void PerformCut()
        {
            base.PerformCut();

            Copy();
            EditorBeatmap.RemoveRange(EditorBeatmap.SelectedHitObjects.ToArray());
        }

        protected override void PerformCopy()
        {
            base.PerformCopy();

            clipboard.Value = new ClipboardContent(EditorBeatmap).Serialize();

            host.GetClipboard()?.SetText(formatSelectionAsString());
        }

        protected override void PerformPaste()
        {
            base.PerformPaste();

            var objects = clipboard.Value.Deserialize<ClipboardContent>().HitObjects;

            Debug.Assert(objects.Any());

            double timeOffset = clock.CurrentTime - objects.Min(o => o.StartTime);

            foreach (var h in objects)
                h.StartTime += timeOffset;

            EditorBeatmap.BeginChange();

            EditorBeatmap.SelectedHitObjects.Clear();

            EditorBeatmap.AddRange(objects);
            EditorBeatmap.SelectedHitObjects.AddRange(objects);

            EditorBeatmap.EndChange();
        }

        private void updateClipboardActionAvailability()
        {
            CanCut.Value = CanCopy.Value = EditorBeatmap.SelectedHitObjects.Any();
            CanPaste.Value = composer.IsLoaded && !string.IsNullOrEmpty(clipboard.Value);
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
