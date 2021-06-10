// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class MetadataSection : SetupSection
    {
        private LabelledTextBox artistTextBox;
        private LabelledTextBox romanisedArtistTextBox;

        private LabelledTextBox titleTextBox;
        private LabelledTextBox romanisedTitleTextBox;

        private LabelledTextBox creatorTextBox;
        private LabelledTextBox difficultyTextBox;
        private LabelledTextBox sourceTextBox;
        private LabelledTextBox tagsTextBox;

        public override LocalisableString Title => "Metadata";

        [BackgroundDependencyLoader]
        private void load()
        {
            var metadata = Beatmap.Metadata;

            Children = new[]
            {
                artistTextBox = createTextBox<LabelledTextBox>("Artist",
                    !string.IsNullOrEmpty(metadata.ArtistUnicode) ? metadata.ArtistUnicode : metadata.Artist),
                romanisedArtistTextBox = createTextBox<LabelledRomanisedTextBox>("Romanised Artist",
                    !string.IsNullOrEmpty(metadata.Artist) ? metadata.Artist : MetadataUtils.StripNonRomanisedCharacters(metadata.ArtistUnicode)),

                Empty(),

                titleTextBox = createTextBox<LabelledTextBox>("Title",
                    !string.IsNullOrEmpty(metadata.TitleUnicode) ? metadata.TitleUnicode : metadata.Title),
                romanisedTitleTextBox = createTextBox<LabelledRomanisedTextBox>("Romanised Title",
                    !string.IsNullOrEmpty(metadata.Title) ? metadata.Title : MetadataUtils.StripNonRomanisedCharacters(metadata.ArtistUnicode)),

                Empty(),

                creatorTextBox = createTextBox<LabelledTextBox>("Creator", metadata.AuthorString),
                difficultyTextBox = createTextBox<LabelledTextBox>("Difficulty Name", Beatmap.BeatmapInfo.Version),
                sourceTextBox = createTextBox<LabelledTextBox>("Source", metadata.Source),
                tagsTextBox = createTextBox<LabelledTextBox>("Tags", metadata.Tags)
            };

            foreach (var item in Children.OfType<LabelledTextBox>())
                item.OnCommit += onCommit;
        }

        private TTextBox createTextBox<TTextBox>(string label, string initialValue)
            where TTextBox : LabelledTextBox, new()
            => new TTextBox
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

            artistTextBox.Current.BindValueChanged(artist => transferIfRomanised(artist.NewValue, romanisedArtistTextBox));
            titleTextBox.Current.BindValueChanged(title => transferIfRomanised(title.NewValue, romanisedTitleTextBox));
            updateReadOnlyState();
        }

        private void transferIfRomanised(string value, LabelledTextBox target)
        {
            if (MetadataUtils.IsRomanised(value))
                target.Current.Value = value;

            updateReadOnlyState();
            updateMetadata();
        }

        private void updateReadOnlyState()
        {
            romanisedArtistTextBox.ReadOnly = MetadataUtils.IsRomanised(artistTextBox.Current.Value);
            romanisedTitleTextBox.ReadOnly = MetadataUtils.IsRomanised(titleTextBox.Current.Value);
        }

        private void onCommit(TextBox sender, bool newText)
        {
            if (!newText) return;

            // for now, update on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            updateMetadata();
        }

        private void updateMetadata()
        {
            Beatmap.Metadata.ArtistUnicode = artistTextBox.Current.Value;
            Beatmap.Metadata.Artist = romanisedArtistTextBox.Current.Value;

            Beatmap.Metadata.TitleUnicode = titleTextBox.Current.Value;
            Beatmap.Metadata.Title = romanisedTitleTextBox.Current.Value;

            Beatmap.Metadata.AuthorString = creatorTextBox.Current.Value;
            Beatmap.BeatmapInfo.Version = difficultyTextBox.Current.Value;
            Beatmap.Metadata.Source = sourceTextBox.Current.Value;
            Beatmap.Metadata.Tags = tagsTextBox.Current.Value;
        }
    }
}
