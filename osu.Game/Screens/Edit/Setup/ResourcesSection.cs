// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Localisation;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit.Components;
using osu.Game.Storyboards;
using osu.Game.Utils;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class ResourcesSection : SetupSection
    {
        private FormBeatmapFileSelector audioTrackChooser = null!;
        private FormBeatmapFileSelector backgroundChooser = null!;
        private FormBeatmapFileSelector videoChooser = null!;

        private readonly Bindable<EditorBeatmapSkin.SampleSet?> currentSampleSet = new Bindable<EditorBeatmapSkin.SampleSet?>();

        public override LocalisableString Title => EditorSetupStrings.ResourcesHeader;

        [Resolved]
        private MusicController music { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> currentWorkingBeatmap { get; set; } = null!;

        [Resolved]
        private Editor? editor { get; set; }

        [Resolved]
        private SetupScreen setupScreen { get; set; } = null!;

        private SetupScreenBackgroundPreview backgroundPreview = null!;
        private SetupScreenVideoPreview videoPreview = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            backgroundPreview = new SetupScreenBackgroundPreview
            {
                RelativeSizeAxes = Axes.X,
                Height = 110,
            };
            videoPreview = new SetupScreenVideoPreview
            {
                RelativeSizeAxes = Axes.X,
                Height = 110,
            };

            bool beatmapHasMultipleDifficulties = currentWorkingBeatmap.Value.BeatmapSetInfo.Beatmaps.Count > 1;

            Children = new Drawable[]
            {
                backgroundChooser = new FormBeatmapFileSelector(beatmapHasMultipleDifficulties, SupportedExtensions.IMAGE_EXTENSIONS)
                {
                    Caption = GameplaySettingsStrings.BackgroundHeader,
                    PlaceholderText = EditorSetupStrings.ClickToSelectBackground,
                },
                videoChooser = new FormBeatmapFileSelector(beatmapHasMultipleDifficulties, SupportedExtensions.VIDEO_EXTENSIONS)
                {
                    Caption = EditorSetupStrings.Video,
                    PlaceholderText = EditorSetupStrings.ClickToSelectVideo,
                    HintText = EditorSetupStrings.VideoHint,
                    AllowClear = true,
                },
                audioTrackChooser = new FormBeatmapFileSelector(beatmapHasMultipleDifficulties, SupportedExtensions.AUDIO_EXTENSIONS)
                {
                    Caption = EditorSetupStrings.AudioTrack,
                    PlaceholderText = EditorSetupStrings.ClickToSelectTrack,
                },
                new FormSampleSetChooser
                {
                    Current = { BindTarget = currentSampleSet },
                },
                new FormSampleSet
                {
                    Current = { BindTarget = currentSampleSet },
                    SampleAddRequested = (file, targetName) =>
                    {
                        string actualFilename = string.Concat(targetName, file.Extension);
                        using var stream = file.OpenRead();
                        beatmaps.AddFile(currentWorkingBeatmap.Value.BeatmapSetInfo, stream, actualFilename);
                        return actualFilename;
                    },
                    SampleRemoveRequested = filename =>
                    {
                        var file = currentWorkingBeatmap.Value.BeatmapSetInfo.GetFile(filename);
                        if (file != null)
                            beatmaps.DeleteFile(currentWorkingBeatmap.Value.BeatmapSetInfo, file);
                    }
                },
            };

            backgroundChooser.PreviewContainer.Add(backgroundPreview);
            videoChooser.PreviewContainer.Add(videoPreview);

            if (!string.IsNullOrEmpty(currentWorkingBeatmap.Value.Metadata.BackgroundFile))
                backgroundChooser.Current.Value = new FileInfo(currentWorkingBeatmap.Value.Metadata.BackgroundFile);

            if (currentWorkingBeatmap.Value.Storyboard.PrimaryVideo is StoryboardVideo video)
                videoChooser.Current.Value = new FileInfo(video.Path);

            if (!string.IsNullOrEmpty(currentWorkingBeatmap.Value.Metadata.AudioFile))
                audioTrackChooser.Current.Value = new FileInfo(currentWorkingBeatmap.Value.Metadata.AudioFile);

            backgroundChooser.Current.BindValueChanged(backgroundChanged);
            videoChooser.Current.BindValueChanged(videoChanged);
            audioTrackChooser.Current.BindValueChanged(audioTrackChanged);
        }

        public bool ChangeBackgroundImage(FileInfo source, bool applyToAllDifficulties)
        {
            if (!source.Exists)
                return false;

            changeResource(source, applyToAllDifficulties, @"bg",
                working => working.BeatmapInfo.Metadata.BackgroundFile,
                (working, name) => working.BeatmapInfo.Metadata.BackgroundFile = name.AsNonNull());

            backgroundPreview.UpdateBackground();
            editor?.ApplyToBackground(bg => ((EditorBackgroundScreen)bg).RefreshBackgroundAsync());
            return true;
        }

        public bool ChangeVideo(FileInfo? source, bool applyToAllDifficulties)
        {
            if (source != null && !source.Exists)
                return false;

            changeResource(source, applyToAllDifficulties, @"video",
                working => working.Storyboard.PrimaryVideo?.Path ?? string.Empty,
                (working, name) =>
                {
                    var videoLayer = working.Storyboard.GetLayer(@"Video");
                    videoLayer.Elements.RemoveAll(elem => elem is StoryboardVideo);
                    if (name != null)
                        videoLayer.Elements.Insert(0, new StoryboardVideo(StoryboardElementSource.Beatmap, name, 0));
                });

            videoPreview.UpdateVideo();
            editor?.ApplyToBackground(bg => ((EditorBackgroundScreen)bg).RefreshBackgroundAsync());
            return true;
        }

        public bool ChangeAudioTrack(FileInfo source, bool applyToAllDifficulties)
        {
            if (!source.Exists)
                return false;

            string artist;
            string title;

            try
            {
                using (var tagSource = TagLibUtils.GetTagLibFile(source.FullName))
                {
                    artist = tagSource.Tag.JoinedAlbumArtists ?? tagSource.Tag.JoinedPerformers;
                    title = tagSource.Tag.Title;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "The selected audio track appears to be corrupted. Please select another one.");
                return false;
            }

            changeResource(source, applyToAllDifficulties, @"audio",
                working => working.BeatmapInfo.Metadata.AudioFile,
                (working, name) =>
                {
                    working.BeatmapInfo.Metadata.AudioFile = name.AsNonNull();

                    if (!string.IsNullOrWhiteSpace(artist))
                    {
                        working.BeatmapInfo.Metadata.ArtistUnicode = artist;
                        working.BeatmapInfo.Metadata.Artist = MetadataUtils.StripNonRomanisedCharacters(working.BeatmapInfo.Metadata.ArtistUnicode);
                    }

                    if (!string.IsNullOrEmpty(title))
                    {
                        working.BeatmapInfo.Metadata.TitleUnicode = title;
                        working.BeatmapInfo.Metadata.Title = MetadataUtils.StripNonRomanisedCharacters(working.BeatmapInfo.Metadata.TitleUnicode);
                    }
                });

            music.ReloadCurrentTrack();
            setupScreen.MetadataChanged?.Invoke();
            return true;
        }

        private void changeResource(
            FileInfo? source,
            bool applyToAllDifficulties,
            string baseFilename,
            Func<WorkingBeatmap, string> readOldFilenameFrom,
            Action<WorkingBeatmap, string?> writeNewFilenameTo)
        {
            var set = currentWorkingBeatmap.Value.BeatmapSetInfo;
            var currentBeatmapInfo = currentWorkingBeatmap.Value.BeatmapInfo;

            var otherBeatmaps = set.Beatmaps.Where(b => !b.Equals(currentBeatmapInfo));

            // First, clean up files which will no longer be used.
            if (applyToAllDifficulties)
            {
                foreach (var b in set.Beatmaps)
                {
                    var working = beatmaps.GetWorkingBeatmap(b);
                    if (set.GetFile(readOldFilenameFrom(working)) is RealmNamedFileUsage otherExistingFile)
                        beatmaps.DeleteFile(set, otherExistingFile);
                }
            }
            else
            {
                RealmNamedFileUsage? oldFile = set.GetFile(readOldFilenameFrom(currentWorkingBeatmap.Value));

                if (oldFile != null)
                {
                    bool oldFileUsedInOtherDiff = false;

                    foreach (var b in otherBeatmaps)
                    {
                        var working = beatmaps.GetWorkingBeatmap(b);

                        if (readOldFilenameFrom(working) == oldFile.Filename)
                        {
                            oldFileUsedInOtherDiff = true;
                            break;
                        }
                    }

                    if (!oldFileUsedInOtherDiff)
                        beatmaps.DeleteFile(set, oldFile);
                }
            }

            string? newFilename = null;

            if (source != null)
            {
                // Choose a new filename that doesn't clash with any other existing files.
                newFilename = $"{baseFilename}{source.Extension}";

                if (set.GetFile(newFilename) != null)
                {
                    string[] existingFilenames = set.Files.Select(f => f.Filename).Where(f =>
                        f.StartsWith(baseFilename, StringComparison.OrdinalIgnoreCase) &&
                        f.EndsWith(source.Extension, StringComparison.OrdinalIgnoreCase)).ToArray();
                    newFilename = NamingUtils.GetNextBestFilename(existingFilenames, $@"{baseFilename}{source.Extension}");
                }

                using (var stream = source.OpenRead())
                    beatmaps.AddFile(set, stream, newFilename);
            }

            if (applyToAllDifficulties)
            {
                foreach (var b in otherBeatmaps)
                {
                    // save the difficulty to re-encode the .osu file, updating any reference of the old filename.
                    //
                    // note that this triggers a full save flow, including triggering a difficulty calculation.
                    // this is not a cheap operation and should be reconsidered in the future.
                    var beatmapWorking = beatmaps.GetWorkingBeatmap(b);
                    writeNewFilenameTo(beatmapWorking, newFilename);
                    beatmaps.Save(b, beatmapWorking.GetPlayableBeatmap(b.Ruleset), beatmapWorking.GetSkin(), beatmapWorking.Storyboard);
                }
            }

            writeNewFilenameTo(currentWorkingBeatmap.Value, newFilename);

            // editor change handler cannot be aware of any file changes or other difficulties having their metadata modified.
            // for simplicity's sake, trigger a save when changing any resource to ensure the change is correctly saved.
            editor?.Save();
        }

        // to avoid scaring users, both background & audio choosers use fake `FileInfo`s with user-friendly filenames
        // when displaying an imported beatmap rather than the actual SHA-named file in storage.
        // however, that means that when a background or audio file is chosen that is broken or doesn't exist on disk when switching away from the fake files,
        // the rollback could enter an infinite loop, because the fake `FileInfo`s *also* don't exist on disk - at least not in the fake location they indicate.
        // to circumvent this issue, just allow rollback to proceed always without actually running any of the change logic to ensure visual consistency.
        // note that this means that `Change{BackgroundImage,AudioTrack}()` are required to not have made any modifications to the beatmap files
        // (or at least cleaned them up properly themselves) if they return `false`.
        private bool rollingBackBackgroundChange;
        private bool rollingBackVideoChange;
        private bool rollingBackAudioChange;

        private void backgroundChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (rollingBackBackgroundChange)
                return;

            if (file.NewValue == null || !ChangeBackgroundImage(file.NewValue, backgroundChooser.ApplyToAllDifficulties.Value))
            {
                rollingBackBackgroundChange = true;
                backgroundChooser.Current.Value = file.OldValue;
                rollingBackBackgroundChange = false;
            }
        }

        private void videoChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (rollingBackVideoChange)
                return;

            if (!ChangeVideo(file.NewValue, videoChooser.ApplyToAllDifficulties.Value))
            {
                rollingBackVideoChange = true;
                videoChooser.Current.Value = file.OldValue;
                rollingBackVideoChange = false;
            }
        }

        private void audioTrackChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (rollingBackAudioChange)
                return;

            if (file.NewValue == null || !ChangeAudioTrack(file.NewValue, audioTrackChooser.ApplyToAllDifficulties.Value))
            {
                rollingBackAudioChange = true;
                audioTrackChooser.Current.Value = file.OldValue;
                rollingBackAudioChange = false;
            }
        }
    }
}
