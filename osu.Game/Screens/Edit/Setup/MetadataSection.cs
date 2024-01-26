// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class MetadataSection : SetupSection
    {
        protected LabelledTextBox ArtistTextBox = null!;
        protected LabelledTextBox RomanisedArtistTextBox = null!;

        protected LabelledTextBox TitleTextBox = null!;
        protected LabelledTextBox RomanisedTitleTextBox = null!;

        private LabelledTextBox creatorTextBox = null!;
        private LabelledTextBox difficultyTextBox = null!;
        private LabelledTextBox sourceTextBox = null!;
        private LabelledTextBox tagsTextBox = null!;

        public override LocalisableString Title => EditorSetupStrings.MetadataHeader;

        [BackgroundDependencyLoader]
        private void load()
        {
            var metadata = Beatmap.Metadata;

            Children = new[]
            {
                ArtistTextBox = createTextBox<LabelledTextBox>(EditorSetupStrings.Artist,
                    !string.IsNullOrEmpty(metadata.ArtistUnicode) ? metadata.ArtistUnicode : metadata.Artist),
                RomanisedArtistTextBox = createTextBox<LabelledRomanisedTextBox>(EditorSetupStrings.RomanisedArtist,
                    !string.IsNullOrEmpty(metadata.Artist) ? metadata.Artist : MetadataUtils.StripNonRomanisedCharacters(metadata.ArtistUnicode)),

                Empty(),

                TitleTextBox = createTextBox<LabelledTextBox>(EditorSetupStrings.Title,
                    !string.IsNullOrEmpty(metadata.TitleUnicode) ? metadata.TitleUnicode : metadata.Title),
                RomanisedTitleTextBox = createTextBox<LabelledRomanisedTextBox>(EditorSetupStrings.RomanisedTitle,
                    !string.IsNullOrEmpty(metadata.Title) ? metadata.Title : MetadataUtils.StripNonRomanisedCharacters(metadata.ArtistUnicode)),

                Empty(),

                creatorTextBox = createTextBox<LabelledTextBox>(EditorSetupStrings.Creator, metadata.Author.Username),
                difficultyTextBox = createTextBox<LabelledTextBox>(EditorSetupStrings.DifficultyName, Beatmap.BeatmapInfo.DifficultyName),
                sourceTextBox = createTextBox<LabelledTextBox>(BeatmapsetsStrings.ShowInfoSource, metadata.Source),
                tagsTextBox = createTextBox<LabelledTextBox>(BeatmapsetsStrings.ShowInfoTags, metadata.Tags)
            };
        }

        private TTextBox createTextBox<TTextBox>(LocalisableString label, string initialValue)
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

            if (string.IsNullOrEmpty(ArtistTextBox.Current.Value))
                ScheduleAfterChildren(() => GetContainingInputManager().ChangeFocus(ArtistTextBox));

            ArtistTextBox.Current.BindValueChanged(artist => transferIfRomanised(artist.NewValue, RomanisedArtistTextBox));
            TitleTextBox.Current.BindValueChanged(title => transferIfRomanised(title.NewValue, RomanisedTitleTextBox));

            foreach (var item in Children.OfType<LabelledTextBox>())
                item.OnCommit += onCommit;

            updateReadOnlyState();
        }

        private void transferIfRomanised(string value, LabelledTextBox target)
        {
            if (MetadataUtils.IsRomanised(value))
                target.Current.Value = value;

            updateReadOnlyState();
        }

        private void updateReadOnlyState()
        {
            RomanisedArtistTextBox.ReadOnly = MetadataUtils.IsRomanised(ArtistTextBox.Current.Value);
            RomanisedTitleTextBox.ReadOnly = MetadataUtils.IsRomanised(TitleTextBox.Current.Value);
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
            Beatmap.Metadata.ArtistUnicode = ArtistTextBox.Current.Value;
            Beatmap.Metadata.Artist = RomanisedArtistTextBox.Current.Value;

            Beatmap.Metadata.TitleUnicode = TitleTextBox.Current.Value;
            Beatmap.Metadata.Title = RomanisedTitleTextBox.Current.Value;

            Beatmap.Metadata.Author.Username = creatorTextBox.Current.Value;
            Beatmap.BeatmapInfo.DifficultyName = difficultyTextBox.Current.Value;
            Beatmap.Metadata.Source = sourceTextBox.Current.Value;
            Beatmap.Metadata.Tags = tagsTextBox.Current.Value;

            Beatmap.SaveState();
        }
    }
}
