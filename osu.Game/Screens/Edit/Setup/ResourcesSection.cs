// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Localisation;
using osu.Game.Models;
using osu.Game.Screens.Backgrounds;
using osu.Game.Utils;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class ResourcesSection : SetupSection
    {
        private FormBeatmapFileSelector audioTrackChooser = null!;
        private FormBeatmapFileSelector backgroundChooser = null!;

        public override LocalisableString Title => EditorSetupStrings.ResourcesHeader;

        [Resolved]
        private MusicController music { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        [Resolved]
        private Editor? editor { get; set; }

        [Resolved]
        private SetupScreen setupScreen { get; set; } = null!;

        private SetupScreenHeaderBackground headerBackground = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            headerBackground = new SetupScreenHeaderBackground
            {
                RelativeSizeAxes = Axes.X,
                Height = 110,
            };

            bool beatmapHasMultipleDifficulties = working.Value.BeatmapSetInfo.Beatmaps.Count > 1;

            Children = new Drawable[]
            {
                backgroundChooser = new FormBeatmapFileSelector(beatmapHasMultipleDifficulties, SupportedExtensions.IMAGE_EXTENSIONS)
                {
                    Caption = GameplaySettingsStrings.BackgroundHeader,
                    PlaceholderText = EditorSetupStrings.ClickToSelectBackground,
                },
                audioTrackChooser = new FormBeatmapFileSelector(beatmapHasMultipleDifficulties, SupportedExtensions.AUDIO_EXTENSIONS)
                {
                    Caption = EditorSetupStrings.AudioTrack,
                    PlaceholderText = EditorSetupStrings.ClickToSelectTrack,
                },
            };

            backgroundChooser.PreviewContainer.Add(headerBackground);

            if (!string.IsNullOrEmpty(working.Value.Metadata.BackgroundFile))
                backgroundChooser.Current.Value = new FileInfo(working.Value.Metadata.BackgroundFile);

            if (!string.IsNullOrEmpty(working.Value.Metadata.AudioFile))
                audioTrackChooser.Current.Value = new FileInfo(working.Value.Metadata.AudioFile);

            backgroundChooser.Current.BindValueChanged(backgroundChanged);
            audioTrackChooser.Current.BindValueChanged(audioTrackChanged);
        }

        public bool ChangeBackgroundImage(FileInfo source, bool applyToAllDifficulties)
        {
            if (!source.Exists)
                return false;

            changeResource(source, applyToAllDifficulties, @"bg",
                metadata => metadata.BackgroundFile,
                (metadata, name) => metadata.BackgroundFile = name);

            headerBackground.UpdateBackground();
            editor?.ApplyToBackground(bg => ((EditorBackgroundScreen)bg).RefreshBackground());
            return true;
        }

        public bool ChangeAudioTrack(FileInfo source, bool applyToAllDifficulties)
        {
            if (!source.Exists)
                return false;

            var tagSource = TagLib.File.Create(source.FullName);

            changeResource(source, applyToAllDifficulties, @"audio",
                metadata => metadata.AudioFile,
                (metadata, name) =>
                {
                    metadata.AudioFile = name;

                    string artist = tagSource.Tag.JoinedAlbumArtists;

                    if (!string.IsNullOrWhiteSpace(artist))
                    {
                        metadata.ArtistUnicode = artist;
                        metadata.Artist = MetadataUtils.StripNonRomanisedCharacters(metadata.ArtistUnicode);
                    }

                    string title = tagSource.Tag.Title;

                    if (!string.IsNullOrEmpty(title))
                    {
                        metadata.TitleUnicode = title;
                        metadata.Title = MetadataUtils.StripNonRomanisedCharacters(metadata.TitleUnicode);
                    }
                });

            music.ReloadCurrentTrack();
            setupScreen.MetadataChanged?.Invoke();
            return true;
        }

        private void changeResource(FileInfo source, bool applyToAllDifficulties, string baseFilename, Func<BeatmapMetadata, string> readFilename, Action<BeatmapMetadata, string> writeMetadata)
        {
            var set = working.Value.BeatmapSetInfo;
            var beatmap = working.Value.BeatmapInfo;

            var otherBeatmaps = set.Beatmaps.Where(b => !b.Equals(beatmap));

            // First, clean up files which will no longer be used.
            if (applyToAllDifficulties)
            {
                foreach (var b in set.Beatmaps)
                {
                    if (set.GetFile(readFilename(b.Metadata)) is RealmNamedFileUsage otherExistingFile)
                        beatmaps.DeleteFile(set, otherExistingFile);
                }
            }
            else
            {
                RealmNamedFileUsage? oldFile = set.GetFile(readFilename(working.Value.Metadata));

                if (oldFile != null)
                {
                    bool oldFileUsedInOtherDiff = otherBeatmaps
                        .Any(b => readFilename(b.Metadata) == oldFile.Filename);
                    if (!oldFileUsedInOtherDiff)
                        beatmaps.DeleteFile(set, oldFile);
                }
            }

            // Choose a new filename that doesn't clash with any other existing files.
            string newFilename = $"{baseFilename}{source.Extension}";

            if (set.GetFile(newFilename) != null)
            {
                string[] existingFilenames = set.Files.Select(f => f.Filename).Where(f =>
                    f.StartsWith(baseFilename, StringComparison.OrdinalIgnoreCase) &&
                    f.EndsWith(source.Extension, StringComparison.OrdinalIgnoreCase)).ToArray();
                newFilename = NamingUtils.GetNextBestFilename(existingFilenames, $@"{baseFilename}{source.Extension}");
            }

            using (var stream = source.OpenRead())
                beatmaps.AddFile(set, stream, newFilename);

            if (applyToAllDifficulties)
            {
                foreach (var b in otherBeatmaps)
                {
                    writeMetadata(b.Metadata, newFilename);

                    // save the difficulty to re-encode the .osu file, updating any reference of the old filename.
                    //
                    // note that this triggers a full save flow, including triggering a difficulty calculation.
                    // this is not a cheap operation and should be reconsidered in the future.
                    var beatmapWorking = beatmaps.GetWorkingBeatmap(b);
                    beatmaps.Save(b, beatmapWorking.Beatmap, beatmapWorking.GetSkin());
                }
            }

            writeMetadata(beatmap.Metadata, newFilename);

            // editor change handler cannot be aware of any file changes or other difficulties having their metadata modified.
            // for simplicity's sake, trigger a save when changing any resource to ensure the change is correctly saved.
            editor?.Save();
        }

        private void backgroundChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (file.NewValue == null || !ChangeBackgroundImage(file.NewValue, backgroundChooser.ApplyToAllDifficulties.Value))
                backgroundChooser.Current.Value = file.OldValue;
        }

        private void audioTrackChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (file.NewValue == null || !ChangeAudioTrack(file.NewValue, audioTrackChooser.ApplyToAllDifficulties.Value))
                audioTrackChooser.Current.Value = file.OldValue;
        }
    }
}
