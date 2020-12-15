// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Setup
{
    internal class ResourcesSection : SetupSection, ICanAcceptFiles
    {
        private LabelledTextBox audioTrackTextBox;
        private Container backgroundSpriteContainer;

        public IEnumerable<string> HandledExtensions => ImageExtensions.Concat(AudioExtensions);

        public static string[] ImageExtensions { get; } = { ".jpg", ".jpeg", ".png" };

        public static string[] AudioExtensions { get; } = { ".mp3", ".ogg" };

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved(canBeNull: true)]
        private Editor editor { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Container audioTrackFileChooserContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

            Children = new Drawable[]
            {
                backgroundSpriteContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 250,
                    Masking = true,
                    CornerRadius = 10,
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
            };

            updateBackgroundSprite();

            audioTrackTextBox.Current.BindValueChanged(audioTrackChanged);
        }

        Task ICanAcceptFiles.Import(params string[] paths)
        {
            Schedule(() =>
            {
                var firstFile = new FileInfo(paths.First());

                if (ImageExtensions.Contains(firstFile.Extension))
                {
                    ChangeBackgroundImage(firstFile.FullName);
                }
                else if (AudioExtensions.Contains(firstFile.Extension))
                {
                    audioTrackTextBox.Text = firstFile.FullName;
                }
            });
            return Task.CompletedTask;
        }

        Task ICanAcceptFiles.Import(Stream stream, string filename) => throw new NotImplementedException();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            game.RegisterImportHandler(this);
        }

        public bool ChangeBackgroundImage(string path)
        {
            var info = new FileInfo(path);

            if (!info.Exists)
                return false;

            var set = Beatmap.Value.BeatmapSetInfo;

            // remove the previous background for now.
            // in the future we probably want to check if this is being used elsewhere (other difficulties?)
            var oldFile = set.Files.FirstOrDefault(f => f.Filename == Beatmap.Value.Metadata.BackgroundFile);

            using (var stream = info.OpenRead())
            {
                if (oldFile != null)
                    beatmaps.ReplaceFile(set, oldFile, stream, info.Name);
                else
                    beatmaps.AddFile(set, stream, info.Name);
            }

            Beatmap.Value.Metadata.BackgroundFile = info.Name;
            updateBackgroundSprite();

            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            game?.UnregisterImportHandler(this);
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

        private void updateBackgroundSprite()
        {
            LoadComponentAsync(new BeatmapBackgroundSprite(Beatmap.Value)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            }, background =>
            {
                if (background.Texture != null)
                    backgroundSpriteContainer.Child = background;
                else
                {
                    backgroundSpriteContainer.Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Colours.GreySeafoamDarker,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 24))
                        {
                            Text = "Drag image here to set beatmap background!",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.X,
                        }
                    };
                }

                background.FadeInFromZero(500);
            });
        }
    }
}
