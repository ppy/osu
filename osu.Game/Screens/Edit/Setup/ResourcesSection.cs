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
        private EditorBeatmap editorBeatmap { get; set; } = null!;

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

            bool multipleDifficulties = working.Value.BeatmapSetInfo.Beatmaps.Count > 1;

            Children = new Drawable[]
            {
                backgroundChooser = new FormBeatmapFileSelector(multipleDifficulties, ".jpg", ".jpeg", ".png")
                {
                    Caption = GameplaySettingsStrings.BackgroundHeader,
                    PlaceholderText = EditorSetupStrings.ClickToSelectBackground,
                },
                audioTrackChooser = new FormBeatmapFileSelector(multipleDifficulties, ".mp3", ".ogg")
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

            var set = working.Value.BeatmapSetInfo;

            if (applyToAllDifficulties)
            {
                string newFilename = $@"bg{source.Extension}";

                foreach (var beatmapInSet in set.Beatmaps)
                {
                    if (set.GetFile(beatmapInSet.Metadata.BackgroundFile) is RealmNamedFileUsage existingFile)
                        beatmaps.DeleteFile(set, existingFile);

                    if (beatmapInSet.Metadata.BackgroundFile != newFilename)
                    {
                        beatmapInSet.Metadata.BackgroundFile = newFilename;

                        if (!beatmapInSet.Equals(working.Value.BeatmapInfo))
                            beatmaps.Save(beatmapInSet, beatmaps.GetWorkingBeatmap(beatmapInSet).Beatmap);
                    }
                }
            }
            else
            {
                var beatmap = working.Value.BeatmapInfo;

                string[] filenames = set.Files.Select(f => f.Filename).Where(f =>
                    f.StartsWith(@"bg", StringComparison.OrdinalIgnoreCase) &&
                    f.EndsWith(source.Extension, StringComparison.OrdinalIgnoreCase)).ToArray();

                string currentFilename = working.Value.Metadata.BackgroundFile;

                var oldFile = set.GetFile(currentFilename);
                string? newFilename = null;

                if (oldFile != null && set.Beatmaps.Where(b => !b.Equals(beatmap)).All(b => b.Metadata.BackgroundFile != currentFilename))
                {
                    beatmaps.DeleteFile(set, oldFile);
                    newFilename = currentFilename;
                }

                newFilename ??= NamingUtils.GetNextBestFilename(filenames, $@"bg{source.Extension}");
                working.Value.Metadata.BackgroundFile = newFilename;
            }

            using (var stream = source.OpenRead())
                beatmaps.AddFile(set, stream, working.Value.Metadata.BackgroundFile);

            editorBeatmap.SaveState();

            headerBackground.UpdateBackground();
            editor?.ApplyToBackground(bg => bg.RefreshBackground());

            return true;
        }

        public bool ChangeAudioTrack(FileInfo source, bool applyToAllDifficulties)
        {
            if (!source.Exists)
                return false;

            var set = working.Value.BeatmapSetInfo;

            if (applyToAllDifficulties)
            {
                string newFilename = $@"audio{source.Extension}";

                foreach (var beatmapInSet in set.Beatmaps)
                {
                    if (set.GetFile(beatmapInSet.Metadata.AudioFile) is RealmNamedFileUsage existingFile)
                        beatmaps.DeleteFile(set, existingFile);

                    if (beatmapInSet.Metadata.AudioFile != newFilename)
                    {
                        beatmapInSet.Metadata.AudioFile = newFilename;

                        if (!beatmapInSet.Equals(working.Value.BeatmapInfo))
                            beatmaps.Save(beatmapInSet, beatmaps.GetWorkingBeatmap(beatmapInSet).Beatmap);
                    }
                }
            }
            else
            {
                var beatmap = working.Value.BeatmapInfo;

                string[] filenames = set.Files.Select(f => f.Filename).Where(f =>
                    f.StartsWith(@"audio", StringComparison.OrdinalIgnoreCase) &&
                    f.EndsWith(source.Extension, StringComparison.OrdinalIgnoreCase)).ToArray();

                string currentFilename = working.Value.Metadata.AudioFile;

                var oldFile = set.GetFile(currentFilename);
                string? newFilename = null;

                if (oldFile != null && set.Beatmaps.Where(b => !b.Equals(beatmap)).All(b => b.Metadata.AudioFile != currentFilename))
                {
                    beatmaps.DeleteFile(set, oldFile);
                    newFilename = currentFilename;
                }

                newFilename ??= NamingUtils.GetNextBestFilename(filenames, $@"audio{source.Extension}");
                working.Value.Metadata.AudioFile = newFilename;
            }

            using (var stream = source.OpenRead())
                beatmaps.AddFile(set, stream, working.Value.Metadata.AudioFile);

            editorBeatmap.SaveState();
            music.ReloadCurrentTrack();

            return true;
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
