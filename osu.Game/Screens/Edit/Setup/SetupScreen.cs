// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    public class SetupScreen : EditorScreen
    {
        private FillFlowContainer flow;
        private LabelledTextBox artistTextBox;
        private LabelledTextBox titleTextBox;
        private LabelledTextBox creatorTextBox;
        private LabelledTextBox difficultyTextBox;
        private LabelledTextBox audioTrackTextBox;

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved(canBeNull: true)]
        private Editor editor { get; set; }

        public SetupScreen()
            : base(EditorScreenMode.SongSetup)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Container audioTrackFileChooserContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(50),
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeafoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(10),
                            Child = flow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(20),
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 250,
                                        Masking = true,
                                        CornerRadius = 10,
                                        Child = new BeatmapBackgroundSprite(Beatmap.Value)
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            FillMode = FillMode.Fill,
                                        },
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = "Resources"
                                    },
                                    audioTrackTextBox = new FileChooserLabelledTextBox
                                    {
                                        Label = "Audio Track",
                                        Current = { Value = Beatmap.Value.Metadata.AudioFile ?? "Click to select a track" },
                                        Target = audioTrackFileChooserContainer,
                                        TabbableContentContainer = this
                                    },
                                    audioTrackFileChooserContainer,
                                    new OsuSpriteText
                                    {
                                        Text = "Beatmap metadata"
                                    },
                                    artistTextBox = new LabelledTextBox
                                    {
                                        Label = "Artist",
                                        Current = { Value = Beatmap.Value.Metadata.Artist },
                                        TabbableContentContainer = this
                                    },
                                    titleTextBox = new LabelledTextBox
                                    {
                                        Label = "Title",
                                        Current = { Value = Beatmap.Value.Metadata.Title },
                                        TabbableContentContainer = this
                                    },
                                    creatorTextBox = new LabelledTextBox
                                    {
                                        Label = "Creator",
                                        Current = { Value = Beatmap.Value.Metadata.AuthorString },
                                        TabbableContentContainer = this
                                    },
                                    difficultyTextBox = new LabelledTextBox
                                    {
                                        Label = "Difficulty Name",
                                        Current = { Value = Beatmap.Value.BeatmapInfo.Version },
                                        TabbableContentContainer = this
                                    },
                                }
                            },
                        },
                    }
                }
            };

            audioTrackTextBox.Current.BindValueChanged(audioTrackChanged);

            foreach (var item in flow.OfType<LabelledTextBox>())
                item.OnCommit += onCommit;
        }

        public bool ChangeAudioTrack(string path)
        {
            var info = new FileInfo(path);

            if (!info.Exists)
                return false;

            var set = Beatmap.Value.BeatmapSetInfo;

            // remove the previous audio track for now.
            // in the future we probably want to check if this is being used elsewhere (other difficulties?)
            var oldFile = set.Files.FirstOrDefault(f => f.Filename == Beatmap.Value.Metadata.AudioFile);

            using (var stream = info.OpenRead())
            {
                if (oldFile != null)
                    beatmaps.ReplaceFile(set, oldFile, stream, info.Name);
                else
                    beatmaps.AddFile(set, stream, info.Name);
            }

            Beatmap.Value.Metadata.AudioFile = info.Name;

            music.ReloadCurrentTrack();

            editor?.UpdateClockSource();
            return true;
        }

        private void audioTrackChanged(ValueChangedEvent<string> filePath)
        {
            if (!ChangeAudioTrack(filePath.NewValue))
                audioTrackTextBox.Current.Value = filePath.OldValue;
        }

        private void onCommit(TextBox sender, bool newText)
        {
            if (!newText) return;

            // for now, update these on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            Beatmap.Value.Metadata.Artist = artistTextBox.Current.Value;
            Beatmap.Value.Metadata.Title = titleTextBox.Current.Value;
            Beatmap.Value.Metadata.AuthorString = creatorTextBox.Current.Value;
            Beatmap.Value.BeatmapInfo.Version = difficultyTextBox.Current.Value;
        }
    }

    internal class FileChooserLabelledTextBox : LabelledTextBox
    {
        public Container Target;

        private readonly IBindable<FileInfo> currentFile = new Bindable<FileInfo>();

        public FileChooserLabelledTextBox()
        {
            currentFile.BindValueChanged(onFileSelected);
        }

        private void onFileSelected(ValueChangedEvent<FileInfo> file)
        {
            if (file.NewValue == null)
                return;

            Target.Clear();
            Current.Value = file.NewValue.FullName;
        }

        protected override OsuTextBox CreateTextBox() =>
            new FileChooserOsuTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                CornerRadius = CORNER_RADIUS,
                OnFocused = DisplayFileChooser
            };

        public void DisplayFileChooser()
        {
            Target.Child = new FileSelector(validFileExtensions: new[] { ".mp3", ".ogg" })
            {
                RelativeSizeAxes = Axes.X,
                Height = 400,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                CurrentFile = { BindTarget = currentFile }
            };
        }

        internal class FileChooserOsuTextBox : OsuTextBox
        {
            public Action OnFocused;

            protected override void OnFocus(FocusEvent e)
            {
                OnFocused?.Invoke();
                base.OnFocus(e);

                GetContainingInputManager().TriggerFocusContention(this);
            }
        }
    }
}
