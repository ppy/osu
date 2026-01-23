// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class FormSampleSetChooser : FormDropdown<EditorBeatmapSkin.SampleSet?>, IHasPopover
    {
        private EditorBeatmapSkin? beatmapSkin;

        public FormSampleSetChooser()
        {
            Caption = "Custom sample sets";
        }

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap editorBeatmap)
        {
            beatmapSkin = editorBeatmap.BeatmapSkin;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            populateItems();
            if (beatmapSkin != null)
                beatmapSkin.BeatmapSkinChanged += scheduleItemPopulation;

            Current.Value = Items.First(i => i?.SampleSetIndex > 0);
            Current.BindValueChanged(val =>
            {
                if (val.NewValue?.SampleSetIndex == -1)
                    this.ShowPopover();
            });
        }

        private void populateItems()
        {
            var items = beatmapSkin?.GetAvailableSampleSets().ToList() ?? [new EditorBeatmapSkin.SampleSet(1)];
            items.Add(new EditorBeatmapSkin.SampleSet(-1, "Add new..."));
            Items = items;
        }

        private void scheduleItemPopulation() => Schedule(populateItems);

        protected override LocalisableString GenerateItemText(EditorBeatmapSkin.SampleSet? item)
        {
            if (item == null)
                return string.Empty;

            return base.GenerateItemText(item);
        }

        public Popover GetPopover() => new NewSampleSetPopover(
            Items.Any(i => i?.SampleSetIndex > 0) ? Items.Max(i => i!.SampleSetIndex) : 0,
            idx =>
            {
                if (idx == null)
                {
                    Current.Value = Items.FirstOrDefault(i => i?.SampleSetIndex > 0);
                    return;
                }

                if (Items.SingleOrDefault(i => i?.SampleSetIndex == idx) is EditorBeatmapSkin.SampleSet existing)
                {
                    Current.Value = existing;
                    return;
                }

                var sampleSet = new EditorBeatmapSkin.SampleSet(idx.Value, $@"Custom #{idx}");
                var newItems = Items.ToList();
                newItems.Insert(newItems.Count - 1, sampleSet);
                Items = newItems;
                Current.Value = sampleSet;
            });

        protected override void Dispose(bool isDisposing)
        {
            if (beatmapSkin != null)
                beatmapSkin.BeatmapSkinChanged -= scheduleItemPopulation;

            base.Dispose(isDisposing);
        }

        private partial class NewSampleSetPopover : OsuPopover
        {
            private readonly int currentLargestIndex;
            private readonly Action<int?> onCommit;

            private int? committedIndex;

            private LabelledNumberBox numberBox = null!;

            public NewSampleSetPopover(int currentLargestIndex, Action<int?> onCommit)
            {
                this.currentLargestIndex = currentLargestIndex;
                this.onCommit = onCommit;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Child = numberBox = new LabelledNumberBox
                {
                    RelativeSizeAxes = Axes.None,
                    Width = 250,
                    Label = "Sample set index",
                    Current = { Value = (currentLargestIndex + 1).ToString(CultureInfo.InvariantCulture) }
                };
                numberBox.OnCommit += (_, _) =>
                {
                    if (int.TryParse(numberBox.Current.Value, out int parsed))
                        committedIndex = parsed;
                    Hide();
                };
            }

            protected override void OnFocus(FocusEvent e)
            {
                base.OnFocus(e);
                // avoids infinite refocus loop
                if (committedIndex == null)
                    GetContainingFocusManager()?.ChangeFocus(numberBox);
            }

            public override void Hide()
            {
                if (State.Value == Visibility.Visible)
                    onCommit.Invoke(committedIndex > 0 ? committedIndex : null);
                base.Hide();
            }
        }
    }
}
