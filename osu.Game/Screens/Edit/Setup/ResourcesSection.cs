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
            editor?.ApplyToBackground(bg => bg.RefreshBackground());
            return true;
        }

        public bool ChangeAudioTrack(FileInfo source, bool applyToAllDifficulties)
        {
            if (!source.Exists)
                return false;

            changeResource(source, applyToAllDifficulties, @"audio",
                metadata => metadata.AudioFile,
                (metadata, name) => metadata.AudioFile = name);

            music.ReloadCurrentTrack();
            return true;
        }

        private void changeResource(FileInfo source, bool applyToAllDifficulties, string baseFilename, Func<BeatmapMetadata, string> readFilename, Action<BeatmapMetadata, string> writeFilename)
        {
            var set = working.Value.BeatmapSetInfo;

            string newFilename = string.Empty;

            if (applyToAllDifficulties)
            {
                newFilename = $"{baseFilename}{source.Extension}";

                foreach (var beatmap in set.Beatmaps)
                {
                    if (set.GetFile(readFilename(beatmap.Metadata)) is RealmNamedFileUsage otherExistingFile)
                        beatmaps.DeleteFile(set, otherExistingFile);

                    writeFilename(beatmap.Metadata, newFilename);
                }
            }
            else
            {
                var thisBeatmap = working.Value.BeatmapInfo;

                string[] filenames = set.Files.Select(f => f.Filename).Where(f =>
                    f.StartsWith(baseFilename, StringComparison.OrdinalIgnoreCase) &&
                    f.EndsWith(source.Extension, StringComparison.OrdinalIgnoreCase)).ToArray();

                string currentFilename = readFilename(working.Value.Metadata);

                var oldFile = set.GetFile(currentFilename);

                if (oldFile != null && set.Beatmaps.Where(b => !b.Equals(thisBeatmap)).All(b => readFilename(b.Metadata) != currentFilename))
                {
                    beatmaps.DeleteFile(set, oldFile);
                    newFilename = currentFilename;
                }

                if (string.IsNullOrEmpty(newFilename))
                    newFilename = NamingUtils.GetNextBestFilename(filenames, $@"{baseFilename}{source.Extension}");

                writeFilename(working.Value.Metadata, newFilename);
            }

            using (var stream = source.OpenRead())
                beatmaps.AddFile(set, stream, newFilename);

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
