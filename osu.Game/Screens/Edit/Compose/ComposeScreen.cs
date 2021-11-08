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
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Compose
{
    public class ComposeScreen : EditorScreenWithTimeline, IKeyBindingHandler<PlatformAction>
    {
        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private EditorClock clock { get; set; }

        [Resolved(Name = nameof(Editor.Clipboard))]
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

            return new EditorSkinProvidingContainer(EditorBeatmap).WithChild(content);
        }

        #region Input Handling

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Action == PlatformAction.Copy)
                host.GetClipboard()?.SetText(formatSelectionAsString());

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
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

        #region Clipboard operations

        public override void Cut()
        {
            base.Cut();

            Copy();
            EditorBeatmap.RemoveRange(EditorBeatmap.SelectedHitObjects.ToArray());
        }

        public override void Copy()
        {
            base.Copy();

            if (EditorBeatmap.SelectedHitObjects.Count == 0)
                return;

            clipboard.Value = new ClipboardContent(EditorBeatmap).Serialize();
        }

        public override void Paste()
        {
            base.Paste();

            if (string.IsNullOrEmpty(clipboard.Value))
                return;

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

        #endregion
    }
}
