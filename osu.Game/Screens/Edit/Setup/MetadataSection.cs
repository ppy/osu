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
        protected FormTextBox ArtistTextBox = null!;
        protected FormTextBox RomanisedArtistTextBox = null!;

        protected FormTextBox TitleTextBox = null!;
        protected FormTextBox RomanisedTitleTextBox = null!;

        private FormTextBox creatorTextBox = null!;
        private FormTextBox difficultyTextBox = null!;
        private FormTextBox sourceTextBox = null!;
        private FormTextBox tagsTextBox = null!;

        public override LocalisableString Title => EditorSetupStrings.MetadataHeader;

        [BackgroundDependencyLoader]
        private void load()
        {
            var metadata = Beatmap.Metadata;

            Children = new[]
            {
                ArtistTextBox = createTextBox<FormTextBox>(EditorSetupStrings.Artist,
                    !string.IsNullOrEmpty(metadata.ArtistUnicode) ? metadata.ArtistUnicode : metadata.Artist),
                RomanisedArtistTextBox = createTextBox<FormRomanisedTextBox>(EditorSetupStrings.RomanisedArtist,
                    !string.IsNullOrEmpty(metadata.Artist) ? metadata.Artist : MetadataUtils.StripNonRomanisedCharacters(metadata.ArtistUnicode)),
                TitleTextBox = createTextBox<FormTextBox>(EditorSetupStrings.Title,
                    !string.IsNullOrEmpty(metadata.TitleUnicode) ? metadata.TitleUnicode : metadata.Title),
                RomanisedTitleTextBox = createTextBox<FormRomanisedTextBox>(EditorSetupStrings.RomanisedTitle,
                    !string.IsNullOrEmpty(metadata.Title) ? metadata.Title : MetadataUtils.StripNonRomanisedCharacters(metadata.ArtistUnicode)),
                creatorTextBox = createTextBox<FormTextBox>(EditorSetupStrings.Creator, metadata.Author.Username),
                difficultyTextBox = createTextBox<FormTextBox>(EditorSetupStrings.DifficultyName, Beatmap.BeatmapInfo.DifficultyName),
                sourceTextBox = createTextBox<FormTextBox>(BeatmapsetsStrings.ShowInfoSource, metadata.Source),
                tagsTextBox = createTextBox<FormTextBox>(BeatmapsetsStrings.ShowInfoTags, metadata.Tags)
            };
        }

        private TTextBox createTextBox<TTextBox>(LocalisableString label, string initialValue)
            where TTextBox : FormTextBox, new()
            => new TTextBox
            {
                Caption = label,
                Current = { Value = initialValue },
                TabbableContentContainer = this
            };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (string.IsNullOrEmpty(ArtistTextBox.Current.Value))
                ScheduleAfterChildren(() => GetContainingFocusManager()!.ChangeFocus(ArtistTextBox));

            ArtistTextBox.Current.BindValueChanged(artist => transferIfRomanised(artist.NewValue, RomanisedArtistTextBox));
            TitleTextBox.Current.BindValueChanged(title => transferIfRomanised(title.NewValue, RomanisedTitleTextBox));

            foreach (var item in Children.OfType<FormTextBox>())
                item.OnCommit += onCommit;

            updateReadOnlyState();
        }

        private void transferIfRomanised(string value, FormTextBox target)
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

        private partial class FormRomanisedTextBox : FormTextBox
        {
            internal override InnerTextBox CreateTextBox() => new RomanisedTextBox();

            private partial class RomanisedTextBox : InnerTextBox
            {
                protected override bool AllowIme => false;

                protected override bool CanAddCharacter(char character)
                    => MetadataUtils.IsRomanised(character);
            }
        }
    }
}
