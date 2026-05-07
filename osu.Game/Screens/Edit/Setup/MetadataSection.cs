// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;

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

        private bool reloading;
        private bool dirty;

        public override LocalisableString Title => EditorSetupStrings.MetadataHeader;

        [Resolved]
        private Editor? editor { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load(SetupScreen? setupScreen)
        {
            Children = new Drawable[]
            {
                ArtistTextBox = createTextBox<FormTextBox>(EditorSetupStrings.Artist),
                RomanisedArtistTextBox = createTextBox<FormRomanisedTextBox>(EditorSetupStrings.RomanisedArtist),
                TitleTextBox = createTextBox<FormTextBox>(EditorSetupStrings.Title),
                RomanisedTitleTextBox = createTextBox<FormRomanisedTextBox>(EditorSetupStrings.RomanisedTitle),
                creatorTextBox = createTextBox<FormTextBox>(EditorSetupStrings.Creator),
                difficultyTextBox = createTextBox<FormTextBox>(EditorSetupStrings.DifficultyName),
                sourceTextBox = createTextBox<FormTextBox>(BeatmapsetsStrings.ShowInfoSource),
                tagsTextBox = createTextBox<FormTextBox>(BeatmapsetsStrings.ShowInfoMapperTags),
                new RoundedButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = EditorSetupStrings.SyncMetadataWithAllDifficulties,
                    TooltipText = EditorSetupStrings.SyncMetadataWithAllDifficultiesTooltip,
                    Margin = new MarginPadding { Top = 10 },
                    Action = () =>
                    {
                        if (working.Value.BeatmapSetInfo.Beatmaps.Count <= 1)
                            return;

                        dialogOverlay?.Push(new SyncMetadataConfirmationDialog(syncMetadataToAllOtherDifficulties));
                    },
                    Enabled = { Value = working.Value.BeatmapSetInfo.Beatmaps.Count > 1 }
                }
            };

            if (setupScreen != null)
                setupScreen.MetadataChanged += reloadMetadata;

            reloadMetadata();
        }

        private void syncMetadataToAllOtherDifficulties()
        {
            if (working.Value.BeatmapSetInfo.Beatmaps.Count <= 1)
                return;

            applyMetadata();

            var set = working.Value.BeatmapSetInfo;
            var current = Beatmap.BeatmapInfo;
            var source = Beatmap.Metadata;

            foreach (var b in set.Beatmaps)
            {
                if (b.Equals(current))
                    continue;

                b.Metadata.ArtistUnicode = source.ArtistUnicode;
                b.Metadata.Artist = source.Artist;
                b.Metadata.TitleUnicode = source.TitleUnicode;
                b.Metadata.Title = source.Title;
                b.Metadata.Source = source.Source;
                b.Metadata.Tags = source.Tags;

                try
                {
                    var beatmapWorking = beatmaps.GetWorkingBeatmap(b);
                    beatmaps.Save(b, beatmapWorking.GetPlayableBeatmap(b.Ruleset), beatmapWorking.GetSkin());
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Failed to sync metadata to {b.GetDisplayTitle()}");
                    return;
                }
            }

            // Persist the current difficulty and align with how resource changes re-save the current beatmap.
            // The reload is a crude measure to ensure places like the verify tab do not continue showing problems with mismatching metadata.
            editor?.SaveAndReload(withDialog: false);
        }

        private TTextBox createTextBox<TTextBox>(LocalisableString label)
            where TTextBox : FormTextBox, new()
            => new TTextBox
            {
                Caption = label,
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
            {
                // Apply immediately on any change to ensure that if the user hits Ctrl+S after making a change (without committing)
                // it will still apply to the beatmap.
                item.Current.BindValueChanged(_ => applyMetadata());
                item.OnCommit += (_, newText) =>
                {
                    if (newText && dirty)
                        Beatmap.SaveState();
                };
            }

            if (editor != null)
                editor.Saved += () => dirty = false;

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

        private void reloadMetadata()
        {
            reloading = true;

            var metadata = Beatmap.Metadata;

            RomanisedArtistTextBox.ReadOnly = false;
            RomanisedTitleTextBox.ReadOnly = false;

            ArtistTextBox.Current.Value = !string.IsNullOrEmpty(metadata.ArtistUnicode) ? metadata.ArtistUnicode : metadata.Artist;
            RomanisedArtistTextBox.Current.Value = !string.IsNullOrEmpty(metadata.Artist) ? metadata.Artist : MetadataUtils.StripNonRomanisedCharacters(metadata.ArtistUnicode);
            TitleTextBox.Current.Value = !string.IsNullOrEmpty(metadata.TitleUnicode) ? metadata.TitleUnicode : metadata.Title;
            RomanisedTitleTextBox.Current.Value = !string.IsNullOrEmpty(metadata.Title) ? metadata.Title : MetadataUtils.StripNonRomanisedCharacters(metadata.TitleUnicode);
            creatorTextBox.Current.Value = metadata.Author.Username;
            difficultyTextBox.Current.Value = Beatmap.BeatmapInfo.DifficultyName;
            sourceTextBox.Current.Value = metadata.Source;
            tagsTextBox.Current.Value = metadata.Tags;

            updateReadOnlyState();

            reloading = false;
        }

        private void applyMetadata()
        {
            if (reloading)
                return;

            Beatmap.Metadata.ArtistUnicode = ArtistTextBox.Current.Value;
            Beatmap.Metadata.Artist = RomanisedArtistTextBox.Current.Value;
            Beatmap.Metadata.TitleUnicode = TitleTextBox.Current.Value;
            Beatmap.Metadata.Title = RomanisedTitleTextBox.Current.Value;
            Beatmap.Metadata.Author.Username = creatorTextBox.Current.Value;
            Beatmap.BeatmapInfo.DifficultyName = difficultyTextBox.Current.Value;
            Beatmap.Metadata.Source = sourceTextBox.Current.Value;
            Beatmap.Metadata.Tags = tagsTextBox.Current.Value;

            dirty = true;
        }

        private partial class FormRomanisedTextBox : FormTextBox
        {
            internal override InnerTextBox CreateTextBox() => new RomanisedTextBox();

            private partial class RomanisedTextBox : InnerTextBox
            {
                public RomanisedTextBox()
                {
                    InputProperties = new TextInputProperties(TextInputType.Text, false);
                }

                protected override bool CanAddCharacter(char character)
                    => MetadataUtils.IsRomanised(character);
            }
        }
    }
}
