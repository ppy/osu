// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Setup
{
    internal partial class ResourcesSection : SetupSection
    {
        private LabelledFileChooser audioTrackChooser = null!;
        private LabelledFileChooser backgroundChooser = null!;

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

        [Resolved]
        private SetupScreenHeader header { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                backgroundChooser = new LabelledFileChooser(".jpg", ".jpeg", ".png")
                {
                    Label = GameplaySettingsStrings.BackgroundHeader,
                    FixedLabelWidth = LABEL_WIDTH,
                    TabbableContentContainer = this
                },
                audioTrackChooser = new LabelledFileChooser(".mp3", ".ogg")
                {
                    Label = EditorSetupStrings.AudioTrack,
                    FixedLabelWidth = LABEL_WIDTH,
                    TabbableContentContainer = this
                },
            };

            if (!string.IsNullOrEmpty(working.Value.Metadata.BackgroundFile))
                backgroundChooser.Current.Value = new FileInfo(working.Value.Metadata.BackgroundFile);

            if (!string.IsNullOrEmpty(working.Value.Metadata.AudioFile))
                audioTrackChooser.Current.Value = new FileInfo(working.Value.Metadata.AudioFile);

            backgroundChooser.Current.BindValueChanged(backgroundChanged);
            audioTrackChooser.Current.BindValueChanged(audioTrackChanged);

            updatePlaceholderText();
        }

        public bool ChangeBackgroundImage(FileInfo source)
        {
            if (!source.Exists)
                return false;

            var set = working.Value.BeatmapSetInfo;

            var destination = new FileInfo($@"bg{source.Extension}");

            // remove the previous background for now.
            // in the future we probably want to check if this is being used elsewhere (other difficulties?)
            var oldFile = set.GetFile(working.Value.Metadata.BackgroundFile);

            using (var stream = source.OpenRead())
            {
                if (oldFile != null)
                    beatmaps.DeleteFile(set, oldFile);

                beatmaps.AddFile(set, stream, destination.Name);
            }

            editorBeatmap.SaveState();

            working.Value.Metadata.BackgroundFile = destination.Name;
            header.Background.UpdateBackground();

            editor?.ApplyToBackground(bg => bg.RefreshBackground());

            return true;
        }

        public bool ChangeAudioTrack(FileInfo source)
        {
            if (!source.Exists)
                return false;

            var set = working.Value.BeatmapSetInfo;

            var destination = new FileInfo($@"audio{source.Extension}");

            // remove the previous audio track for now.
            // in the future we probably want to check if this is being used elsewhere (other difficulties?)
            var oldFile = set.GetFile(working.Value.Metadata.AudioFile);

            using (var stream = source.OpenRead())
            {
                if (oldFile != null)
                    beatmaps.DeleteFile(set, oldFile);

                beatmaps.AddFile(set, stream, destination.Name);
            }

            working.Value.Metadata.AudioFile = destination.Name;

            editorBeatmap.SaveState();
            music.ReloadCurrentTrack();

            return true;
        }

        private void backgroundChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (file.NewValue == null || !ChangeBackgroundImage(file.NewValue))
                backgroundChooser.Current.Value = file.OldValue;

            updatePlaceholderText();
        }

        private void audioTrackChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (file.NewValue == null || !ChangeAudioTrack(file.NewValue))
                audioTrackChooser.Current.Value = file.OldValue;

            updatePlaceholderText();
        }

        private void updatePlaceholderText()
        {
            audioTrackChooser.Text = audioTrackChooser.Current.Value?.Name ?? EditorSetupStrings.ClickToSelectTrack;
            backgroundChooser.Text = backgroundChooser.Current.Value?.Name ?? EditorSetupStrings.ClickToSelectBackground;
        }
    }
}
