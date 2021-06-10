// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class MetadataSection : SetupSection
    {
        private LabelledTextBox artistTextBox;
        private LabelledTextBox titleTextBox;
        private LabelledTextBox creatorTextBox;
        private LabelledTextBox difficultyTextBox;

        public override LocalisableString Title => "Metadata";

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                artistTextBox = createTextBox("Artist", Beatmap.Metadata.Artist),
                titleTextBox = createTextBox("Title", Beatmap.Metadata.Title),
                creatorTextBox = createTextBox("Creator", Beatmap.Metadata.AuthorString),
                difficultyTextBox = createTextBox("Difficulty Name", Beatmap.BeatmapInfo.Version)
            };

            foreach (var item in Children.OfType<LabelledTextBox>())
                item.OnCommit += onCommit;
        }

        private LabelledTextBox createTextBox(string label, string initialValue) => new LabelledTextBox
        {
            Label = label,
            FixedLabelWidth = LABEL_WIDTH,
            Current = { Value = initialValue },
            TabbableContentContainer = this
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (string.IsNullOrEmpty(artistTextBox.Current.Value))
                GetContainingInputManager().ChangeFocus(artistTextBox);
        }

        private void onCommit(TextBox sender, bool newText)
        {
            if (!newText) return;

            // for now, update these on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            Beatmap.Metadata.Artist = artistTextBox.Current.Value;
            Beatmap.Metadata.Title = titleTextBox.Current.Value;
            Beatmap.Metadata.AuthorString = creatorTextBox.Current.Value;
            Beatmap.BeatmapInfo.Version = difficultyTextBox.Current.Value;
        }
    }
}
