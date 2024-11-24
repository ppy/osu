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
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Localisation;
using osu.Game.Models;
using osu.Game.Utils;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class ResourcesSection : SetupSection
    {
        private FormFileSelector audioTrackChooser = null!;
        private FormFileSelector backgroundChooser = null!;

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
        private RoundedButton updateAllDifficultiesButton = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            headerBackground = new SetupScreenHeaderBackground
            {
                RelativeSizeAxes = Axes.X,
                Height = 110,
            };

            Children = new Drawable[]
            {
                backgroundChooser = new FormFileSelector(".jpg", ".jpeg", ".png")
                {
                    Caption = GameplaySettingsStrings.BackgroundHeader,
                    PlaceholderText = EditorSetupStrings.ClickToSelectBackground,
                },
                audioTrackChooser = new FormFileSelector(".mp3", ".ogg")
                {
                    Caption = EditorSetupStrings.AudioTrack,
                    PlaceholderText = EditorSetupStrings.ClickToSelectTrack,
                },
                updateAllDifficultiesButton = new RoundedButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Update all difficulties",
                    Action = updateAllDifficulties,
                    Enabled = { Value = false },
                }
            };

            backgroundChooser.PreviewContainer.Add(headerBackground);

            if (!string.IsNullOrEmpty(working.Value.Metadata.BackgroundFile))
                backgroundChooser.Current.Value = new FileInfo(working.Value.Metadata.BackgroundFile);

            if (!string.IsNullOrEmpty(working.Value.Metadata.AudioFile))
                audioTrackChooser.Current.Value = new FileInfo(working.Value.Metadata.AudioFile);

            backgroundChooser.Current.BindValueChanged(backgroundChanged);
            audioTrackChooser.Current.BindValueChanged(audioTrackChanged);
        }

        public bool ChangeBackgroundImage(FileInfo source)
        {
            if (!source.Exists)
                return false;

            var beatmap = working.Value.BeatmapInfo;
            var set = working.Value.BeatmapSetInfo;

            string[] filenames = set.Files.Select(f => f.Filename).Where(f =>
                f.StartsWith(@"bg", StringComparison.OrdinalIgnoreCase) &&
                f.EndsWith(source.Extension, StringComparison.OrdinalIgnoreCase)).ToArray();

            string currentFilename = working.Value.Metadata.BackgroundFile;
            string? newFilename = null;

            var oldFile = set.GetFile(currentFilename);

            if (oldFile != null && set.Beatmaps.Where(b => !b.Equals(beatmap)).All(b => b.Metadata.BackgroundFile != currentFilename))
            {
                beatmaps.DeleteFile(set, oldFile);
                newFilename = currentFilename;
            }

            newFilename ??= NamingUtils.GetNextBestFilename(filenames, $@"bg{source.Extension}");

            using (var stream = source.OpenRead())
                beatmaps.AddFile(set, stream, newFilename);

            working.Value.Metadata.BackgroundFile = newFilename;
            updateAllDifficultiesButton.Enabled.Value = true;

            editorBeatmap.SaveState();

            headerBackground.UpdateBackground();
            editor?.ApplyToBackground(bg => bg.RefreshBackground());

            return true;
        }

        public bool ChangeAudioTrack(FileInfo source)
        {
            if (!source.Exists)
                return false;

            var beatmap = working.Value.BeatmapInfo;
            var set = working.Value.BeatmapSetInfo;

            string[] filenames = set.Files.Select(f => f.Filename).Where(f =>
                f.StartsWith(@"audio", StringComparison.OrdinalIgnoreCase) &&
                f.EndsWith(source.Extension, StringComparison.OrdinalIgnoreCase)).ToArray();

            string currentFilename = working.Value.Metadata.AudioFile;
            string? newFilename = null;

            var oldFile = set.GetFile(currentFilename);

            if (oldFile != null && set.Beatmaps.Where(b => !b.Equals(beatmap)).All(b => b.Metadata.AudioFile != currentFilename))
            {
                beatmaps.DeleteFile(set, oldFile);
                newFilename = currentFilename;
            }

            newFilename ??= NamingUtils.GetNextBestFilename(filenames, $@"audio{source.Extension}");

            using (var stream = source.OpenRead())
                beatmaps.AddFile(set, stream, newFilename);

            working.Value.Metadata.AudioFile = newFilename;
            updateAllDifficultiesButton.Enabled.Value = true;

            editorBeatmap.SaveState();
            music.ReloadCurrentTrack();

            return true;
        }

        private void updateAllDifficulties()
        {
            var beatmap = working.Value.BeatmapInfo;
            var set = working.Value.BeatmapSetInfo;

            string backgroundFile = working.Value.Metadata.BackgroundFile;
            string audioFile = working.Value.Metadata.AudioFile;

            foreach (var otherBeatmap in set.Beatmaps.Where(b => !b.Equals(beatmap)))
            {
                var otherWorking = beatmaps.GetWorkingBeatmap(otherBeatmap);

                if (!string.Equals(otherBeatmap.Metadata.BackgroundFile, backgroundFile, StringComparison.OrdinalIgnoreCase))
                {
                    if (set.GetFile(otherBeatmap.Metadata.BackgroundFile) is RealmNamedFileUsage file)
                        beatmaps.DeleteFile(set, file);

                    otherBeatmap.Metadata.BackgroundFile = backgroundFile;
                }

                if (!string.Equals(otherBeatmap.Metadata.AudioFile, audioFile, StringComparison.OrdinalIgnoreCase))
                {
                    if (set.GetFile(otherBeatmap.Metadata.AudioFile) is RealmNamedFileUsage file)
                        beatmaps.DeleteFile(set, file);

                    otherBeatmap.Metadata.AudioFile = audioFile;
                }

                beatmaps.Save(otherBeatmap, otherWorking.Beatmap);
            }

            editorBeatmap.SaveState();
            updateAllDifficultiesButton.Enabled.Value = false;
        }

        private void backgroundChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (file.NewValue == null || !ChangeBackgroundImage(file.NewValue))
                backgroundChooser.Current.Value = file.OldValue;
        }

        private void audioTrackChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (file.NewValue == null || !ChangeAudioTrack(file.NewValue))
                audioTrackChooser.Current.Value = file.OldValue;
        }
    }
}
