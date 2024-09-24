// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using MessagePack.Formatters;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Setup
{
    public partial class BackgroundVideoSelectScreen : TournamentScreen
    {
        private VideoTypeDropdown videoDropdown = null!;

        private TourneyVideo videoPreview = null!;

        private Container videoContainer = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [Resolved]
        private MatchIPCInfo ipc { get; set; } = null!;

        private OsuFileSelector fileSelector = null!;
        private DialogOverlay? overlay;

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage, OsuColour colours)
        {
            var initialStorage = (ipc as FileBasedIPC)?.IPCStorage ?? storage;
            string? initialPath = new DirectoryInfo(initialStorage.GetFullPath(string.Empty)).Parent?.FullName;

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
                                        Spacing = new Vector2(20),
                                        Children = new Drawable[]
                                        {
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 300,
                                                Text = "Refresh...",
                                                // Action = ?
                                            },
                                        }
                                    }
                                }
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
                                        Children = new Drawable[]
                                        {
                                            videoDropdown = new VideoTypeDropdown
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                LabelText = "Select background video for",
                                                Margin = new MarginPadding(10),
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
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 200,
                                                Text = "Set and save",
                                                // Action = ?
                                            },
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 200,
                                                Text = "Reset",
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

            videoDropdown.Current.BindValueChanged(e =>
            {
                videoContainer.Child = videoPreview = new TourneyVideo(LadderInfo.BackgroundVideoFiles.First(v => v.Key == BackgroundVideoProps.GetVideoFromName(e.NewValue)).Value)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                };
            }, true);
        }
    }
}
