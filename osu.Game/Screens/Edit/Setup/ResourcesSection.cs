// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Setup
{
    internal class ResourcesSection : SetupSection
    {
        private LabelledTextBox audioTrackTextBox;
        private LabelledTextBox backgroundTextBox;

        public override LocalisableString Title => "Resources";

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; }

        [Resolved(canBeNull: true)]
        private Editor editor { get; set; }

        [Resolved]
        private SetupScreenHeader header { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Container audioTrackFileChooserContainer = createFileChooserContainer();
            Container backgroundFileChooserContainer = createFileChooserContainer();

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        backgroundTextBox = new FileChooserLabelledTextBox(".jpg", ".jpeg", ".png")
                        {
                            Label = "Background",
                            FixedLabelWidth = LABEL_WIDTH,
                            PlaceholderText = "Click to select a background image",
                            Current = { Value = working.Value.Metadata.BackgroundFile },
                            Target = backgroundFileChooserContainer,
                            TabbableContentContainer = this
                        },
                        backgroundFileChooserContainer,
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        audioTrackTextBox = new FileChooserLabelledTextBox(".mp3", ".ogg")
                        {
                            Label = "Audio Track",
                            FixedLabelWidth = LABEL_WIDTH,
                            PlaceholderText = "Click to select a track",
                            Current = { Value = working.Value.Metadata.AudioFile },
                            Target = audioTrackFileChooserContainer,
                            TabbableContentContainer = this
                        },
                        audioTrackFileChooserContainer,
                    }
                }
            };

            backgroundTextBox.Current.BindValueChanged(backgroundChanged);
            audioTrackTextBox.Current.BindValueChanged(audioTrackChanged);
        }

        private static Container createFileChooserContainer() =>
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

        public bool ChangeBackgroundImage(string path)
        {
            var info = new FileInfo(path);

            if (!info.Exists)
                return false;

            var set = working.Value.BeatmapSetInfo;

            // remove the previous background for now.
            // in the future we probably want to check if this is being used elsewhere (other difficulties?)
            var oldFile = set.Files.FirstOrDefault(f => f.Filename == working.Value.Metadata.BackgroundFile);

            using (var stream = info.OpenRead())
            {
                if (oldFile != null)
                    beatmaps.ReplaceFile(set, oldFile, stream, info.Name);
                else
                    beatmaps.AddFile(set, stream, info.Name);
            }

            working.Value.Metadata.BackgroundFile = info.Name;
            header.Background.UpdateBackground();

            return true;
        }

        public bool ChangeAudioTrack(string path)
        {
            var info = new FileInfo(path);

            if (!info.Exists)
                return false;

            var set = working.Value.BeatmapSetInfo;

            // remove the previous audio track for now.
            // in the future we probably want to check if this is being used elsewhere (other difficulties?)
            var oldFile = set.Files.FirstOrDefault(f => f.Filename == working.Value.Metadata.AudioFile);

            using (var stream = info.OpenRead())
            {
                if (oldFile != null)
                    beatmaps.ReplaceFile(set, oldFile, stream, info.Name);
                else
                    beatmaps.AddFile(set, stream, info.Name);
            }

            working.Value.Metadata.AudioFile = info.Name;

            music.ReloadCurrentTrack();

            editor?.UpdateClockSource();
            return true;
        }

        private void backgroundChanged(ValueChangedEvent<string> filePath)
        {
            if (!ChangeBackgroundImage(filePath.NewValue))
                backgroundTextBox.Current.Value = filePath.OldValue;
        }

        private void audioTrackChanged(ValueChangedEvent<string> filePath)
        {
            if (!ChangeAudioTrack(filePath.NewValue))
                audioTrackTextBox.Current.Value = filePath.OldValue;
        }
    }
}
