// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Compose
{
    public partial class ComposeScreen : EditorScreenWithTimeline, IGameplaySettings
    {
        [Resolved]
        private Clipboard hostClipboard { get; set; } = null!;

        [Resolved]
        private EditorClock clock { get; set; }

        [Resolved]
        private IGameplaySettings globalGameplaySettings { get; set; }

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // May be null in the case of a ruleset that doesn't have editor support, see CreateMainContent().
            if (composer == null)
                return;

            EditorBeatmap.SelectedHitObjects.BindCollectionChanged((_, _) => updateClipboardActionAvailability());
            composer.OnLoadComplete += _ => updateClipboardActionAvailability();
            updateClipboardActionAvailability();
        }

        #region Clipboard operations

        public override void Cut()
        {
            if (!CanCut.Value)
                return;

            Copy();
            EditorBeatmap.RemoveRange(EditorBeatmap.SelectedHitObjects.ToArray());
        }

        public override void Copy()
        {
            // on stable, pressing Ctrl-C would copy the current timestamp to system clipboard
            // regardless of whether anything was even selected at all.
            // UX-wise this is generally strange and unexpected, but make it work anyways to preserve muscle memory.
            // note that this means that `getTimestamp()` must handle no-selection case, too.
            var clipboardData = new ClipboardData
            {
                Text = getTimestamp()
            };

            if (CanCopy.Value)
            {
                clipboardData.CustomFormatValues[ClipboardContent.CLIPBOARD_FORMAT] = new ClipboardContent(EditorBeatmap).Serialize();
            }

            hostClipboard.SetData(clipboardData);

            updateClipboardActionAvailability();
        }

        public override void Paste()
        {
            string clipboardContent = hostClipboard.GetCustom(ClipboardContent.CLIPBOARD_FORMAT);

            var objects = clipboardContent?.Deserialize<ClipboardContent>().HitObjects;

            if (objects == null || objects.Count == 0)
                return;

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
            CanPaste.Value = composer.IsLoaded && !string.IsNullOrEmpty(hostClipboard.GetCustom("osu/hitobjects"));
        }

        private string getTimestamp()
        {
            if (composer == null)
                return string.Empty;

            double displayTime = EditorBeatmap.SelectedHitObjects.MinBy(h => h.StartTime)?.StartTime ?? clock.CurrentTime;
            string selectionAsString = composer.ConvertSelectionToString();

            return !string.IsNullOrEmpty(selectionAsString)
                ? $"{displayTime.ToEditorFormattedString()} ({selectionAsString}) - "
                : $"{displayTime.ToEditorFormattedString()} - ";
        }

        #endregion

        // Combo colour normalisation should not be applied in the editor.
        // Note this doesn't affect editor test mode.
        IBindable<float> IGameplaySettings.ComboColourNormalisationAmount => new Bindable<float>();

        // Arguable.
        IBindable<float> IGameplaySettings.PositionalHitsoundsLevel => globalGameplaySettings.PositionalHitsoundsLevel;
    }
}
