// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Setup
{
    public partial class BackgroundVideoSelectScreen : TournamentScreen
    {
        private VideoTypeDropdown videoDropdown = null!;
        private TournamentSpriteText videoInfo = null!;

        private TourneyVideo videoPreview = null!;

        private Container videoContainer = null!;

        private SpriteIcon currentFileIcon = null!;
        private OsuSpriteText currentFileText = null!;

        private RoundedButton saveButton = null!;
        private RoundedButton resetButton = null!;

        private string initialPath = null!;
        private string videoPath = null!;
        private bool pathValid = false;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [Resolved]
        private MatchIPCInfo ipc { get; set; } = null!;

        private OsuFileSelector fileSelector = null!;
        private DialogOverlay? overlay;

        [BackgroundDependencyLoader(true)]
        private void load(TournamentStorage storage, OsuColour colours)
        {
            initialPath = new DirectoryInfo(storage.GetFullPath(string.Empty)).FullName;
            videoPath = new DirectoryInfo(storage.GetFullPath("./Videos")).FullName;

            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f, 0.8f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeaFoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.4f, 1f),
                            RowDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Relative, 0.8f),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "Video Settings",
                                        Font = OsuFont.Default.With(size: 32, weight: FontWeight.SemiBold)
                                    },
                                },
                                new Drawable[]
                                {
                                    fileSelector = new OsuFileSelector(initialPath)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                },
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(10),
                                        Children = new Drawable[]
                                        {
                                            currentFileIcon = new SpriteIcon
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Icon = FontAwesome.Solid.Edit,
                                                Size = new Vector2(16),
                                            },
                                            currentFileText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Text = "Select a file!",
                                                Font = OsuFont.Default.With(size: 16),
                                            },
                                        }
                                    }
                                },
                            }
                        },
                        new GridContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.6f, 1f),
                            RowDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Relative, 0.2f),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(10),
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(10),
                                                AutoSizeAxes = Axes.Both,
                                                Margin = new MarginPadding { Top = 10 },
                                                Children = new Drawable[]
                                                {
                                                    new SpriteIcon
                                                    {
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        Icon = FontAwesome.Solid.InfoCircle,
                                                        Size = new Vector2(24),
                                                        Colour = Color4.White,
                                                    },
                                                    videoInfo = new TournamentSpriteText
                                                    {
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        Text = "Unknown",
                                                        Font = OsuFont.Default.With(size: 24),
                                                    },
                                                },
                                            },
                                            videoDropdown = new VideoTypeDropdown
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                LabelText = "Select background video for",
                                                Margin = new MarginPadding { Top = 10 },
                                            },

                                            videoContainer = new Container
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                RelativeSizeAxes = Axes.Both,
                                                Child = videoPreview = new TourneyVideo(LadderInfo.BackgroundVideoFiles.First(v => v.Key == BackgroundVideoProps.GetVideoFromName(videoDropdown.Current.Value)).Value)
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    Loop = true,
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                            },

                                        }
                                    }
                                },
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(20),
                                        Children = new Drawable[]
                                        {
                                            saveButton = new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 200,
                                                Text = "Set and save",
                                                Action = saveSetting
                                            },
                                            resetButton = new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 200,
                                                Text = "Reset...",
                                                // Action = ?
                                            },
                                        }
                                    }
                                }
                            }
                        }
                    },
                },
                new BackButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    State = { Value = Visibility.Visible },
                    Action = () => sceneManager?.SetScreen(typeof(SetupScreen))
                }
            });

            saveButton.Enabled.Value = false;

            videoInfo.Text = LadderInfo.BackgroundVideoFiles.First(v => v.Key == BackgroundVideoProps.GetVideoFromName(videoDropdown.Current.Value)).Value;
            videoInfo.Colour = videoPreview.VideoAvailable ? Color4.SkyBlue : Color4.Orange;

            videoDropdown.Current.BindValueChanged(e =>
            {
                videoContainer.Child = videoPreview = new TourneyVideo(LadderInfo.BackgroundVideoFiles.First(v => v.Key == BackgroundVideoProps.GetVideoFromName(e.NewValue)).Value)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                };

                videoInfo.Text = $"Use video: {LadderInfo.BackgroundVideoFiles.First(v => v.Key == BackgroundVideoProps.GetVideoFromName(e.NewValue)).Value}";
                videoInfo.Colour = videoPreview.VideoAvailable ? Color4.SkyBlue : Color4.Orange;
            }, true);

            fileSelector.CurrentPath.BindValueChanged(pathChanged, true);
            fileSelector.CurrentFile.BindValueChanged(fileChanged, true);
        }

        private void pathChanged(ValueChangedEvent<DirectoryInfo> e)
        {
            if (e.NewValue == null)
            {
                pathValid = false;
                return;
            }

            pathValid = e.NewValue.FullName == videoPath;
        }

        private void fileChanged(ValueChangedEvent<FileInfo> selectedFile)
        {
            if (selectedFile.NewValue == null)
            {
                currentFileText.Text = "Select a file!";
                currentFileIcon.Icon = FontAwesome.Solid.Edit;
                return;
            }

            string lowerFileName = selectedFile.NewValue.Name.ToLowerInvariant();

            bool valid = lowerFileName.EndsWith(".mp4") || lowerFileName.EndsWith(".avi") || lowerFileName.EndsWith(".m4v");

            if (!valid)
            {
                currentFileText.Text = $"{selectedFile.NewValue.Name}: Invalid file type.";
                currentFileText.Colour = Color4.Orange;
                currentFileIcon.Icon = FontAwesome.Solid.ExclamationCircle;
                currentFileIcon.Colour = Color4.Orange;
                saveButton.Enabled.Value = false;
            }
            else
            {
                if (pathValid)
                {
                    currentFileText.Text = $"{selectedFile.NewValue.Name}: Preview on the right!";
                    currentFileText.Colour = Color4.SkyBlue;
                    currentFileIcon.Icon = FontAwesome.Solid.CheckCircle;
                    currentFileIcon.Colour = Color4.SkyBlue;
                    videoContainer.Child = new TourneyVideo(selectedFile.NewValue.Name.Split('.')[0])
                    {
                        Loop = true,
                        RelativeSizeAxes = Axes.Both,
                    };

                    saveButton.Enabled.Value = true;
                }
                else
                {
                    currentFileText.Text = $"Must select a file from current tournament's Video path.";
                    currentFileText.Colour = Color4.Orange;
                    currentFileIcon.Icon = FontAwesome.Solid.ExclamationCircle;
                    currentFileIcon.Colour = Color4.Orange;
                    saveButton.Enabled.Value = false;
                }
            }
        }

        private void saveSetting()
        {
            BackgroundVideo currentType = BackgroundVideoProps.GetVideoFromName(videoDropdown.Current.Value);
            var target = LadderInfo.BackgroundVideoFiles.First(v => v.Key == currentType);

            LadderInfo.BackgroundVideoFiles.Remove(target);
            LadderInfo.BackgroundVideoFiles.Add(
                new KeyValuePair<BackgroundVideo, string>(currentType, fileSelector.CurrentFile.Value.Name.Split('.')[0]));

            saveButton.FlashColour(Color4.White, 500);
            saveButton.Enabled.Value = false;
        }
    }
}
