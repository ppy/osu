// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Skinning;
using osuTK;
using Realms;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class ScreenImportFromLazer : OsuScreen
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        [Resolved]
        private Storage? storage { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private DirectorySelector directorySelector = null!;
        private RoundedButton importButton = null!;
        private TextFlowContainer statusText = null!;

        // Checkboxes
        private OsuCheckbox checkboxBeatmaps = null!;
        private OsuCheckbox checkboxScores = null!;
        private OsuCheckbox checkboxSkins = null!;
        private OsuCheckbox checkboxCollections = null!;

        private string? validLazerPath;

        [BackgroundDependencyLoader]
        private void load()
        {
            checkboxBeatmaps = new OsuCheckbox
            {
                LabelText = "Beatmaps",
                Current = { Value = false, Disabled = true }
            };
            checkboxScores = new OsuCheckbox
            {
                LabelText = "Scores",
                Current = { Value = false, Disabled = true }
            };
            checkboxSkins = new OsuCheckbox
            {
                LabelText = "Skins",
                Current = { Value = false, Disabled = true }
            };
            checkboxCollections = new OsuCheckbox
            {
                LabelText = "Collections",
                Current = { Value = false, Disabled = true }
            };

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 80),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 20 },
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = "Import from osu!lazer",
                                        Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                                        Colour = colourProvider.Content1,
                                    },
                                    new IconButton
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Icon = FontAwesome.Solid.Times,
                                        Action = this.Exit,
                                        TooltipText = "Back"
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 30, Bottom = 30 },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Distributed, 0.6f),
                                    new Dimension(GridSizeMode.Absolute, 20),
                                    new Dimension(GridSizeMode.Distributed, 0.4f),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Masking = true,
                                            CornerRadius = 10,
                                            Children = new Drawable[]
                                            {
                                                new Box { RelativeSizeAxes = Axes.Both, Colour = colourProvider.Background3 },
                                                directorySelector = new OsuDirectorySelector { RelativeSizeAxes = Axes.Both }
                                            }
                                        },
                                        null!,
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Masking = true,
                                            CornerRadius = 10,
                                            Children = new Drawable[]
                                            {
                                                new Box { RelativeSizeAxes = Axes.Both, Colour = colourProvider.Background5 },

                                                new OsuScrollContainer
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    ScrollbarVisible = true,
                                                    Padding = new MarginPadding { Top = 20, Left = 20, Right = 20, Bottom = 80 },
                                                    Child = new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        Direction = FillDirection.Vertical,
                                                        Spacing = new Vector2(0, 10),
                                                        Children = new Drawable[]
                                                        {
                                                            new OsuSpriteText
                                                            {
                                                                Text = "Import Options",
                                                                Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                                                                Colour = colourProvider.Content1
                                                            },

                                                            new Box { RelativeSizeAxes = Axes.X, Height = 1, Colour = colourProvider.Light4, Margin = new MarginPadding { Vertical = 5 } },

                                                            checkboxBeatmaps,
                                                            checkboxScores,
                                                            checkboxSkins,
                                                            checkboxCollections,

                                                            new Box { RelativeSizeAxes = Axes.X, Height = 1, Colour = colourProvider.Light4, Margin = new MarginPadding { Vertical = 10 } },

                                                            statusText = new TextFlowContainer
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Colour = colours.Red1
                                                            }
                                                        }
                                                    }
                                                },

                                                importButton = new RoundedButton
                                                {
                                                    Anchor = Anchor.BottomCentre,
                                                    Origin = Anchor.BottomCentre,
                                                    RelativeSizeAxes = Axes.X,
                                                    Width = 0.9f,
                                                    Height = 50,
                                                    Margin = new MarginPadding { Bottom = 20 },
                                                    Text = "Start Import",
                                                    Action = startImport,
                                                    Enabled = { Value = false }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            directorySelector.CurrentPath.BindValueChanged(e => validatePath(e.NewValue), true);
        }

        private void validatePath(DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists) return;

            importButton.Enabled.Value = false;
            statusText.Text = "Checking...";

            resetLabels();
            validLazerPath = null;

            Task.Run(() =>
            {
                string proposedPath = directory.FullName;
                string realmPath = Path.Combine(proposedPath, "client.realm");

                if (!File.Exists(realmPath))
                {
                    Schedule(() => statusText.Text = "No client.realm found.");
                    return;
                }

                try
                {
                    var config = new RealmConfiguration(realmPath)
                    {
                        IsReadOnly = true,
                        SchemaVersion = RealmAccess.schema_version
                    };

                    using (var realm = Realm.GetInstance(config))
                    {
                        int countMaps = realm.All<BeatmapSetInfo>().Count();
                        int countScores = realm.All<ScoreInfo>().Count();
                        int countSkins = realm.All<SkinInfo>().Count();
                        int countCollections = realm.All<BeatmapCollection>().Count();

                        Schedule(() =>
                        {
                            statusText.Text = "";

                            updateCheckbox(checkboxBeatmaps, "Beatmaps", countMaps);
                            updateCheckbox(checkboxScores, "Scores", countScores);
                            updateCheckbox(checkboxSkins, "Skins", countSkins);
                            updateCheckbox(checkboxCollections, "Collections", countCollections);

                            validLazerPath = proposedPath;
                            importButton.Enabled.Value = true;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Schedule(() => statusText.Text = "Error reading database.");
                    Logger.Error(ex, "Error validating lazer path");
                }
            });
        }

        private void resetLabels()
        {
            checkboxBeatmaps.Current.Disabled = false;
            checkboxScores.Current.Disabled = false;
            checkboxSkins.Current.Disabled = false;
            checkboxCollections.Current.Disabled = false;

            checkboxBeatmaps.LabelText = "Beatmaps";
            checkboxScores.LabelText = "Scores";
            checkboxSkins.LabelText = "Skins";
            checkboxCollections.LabelText = "Collections";

            checkboxBeatmaps.Current.Value = false;
            checkboxScores.Current.Value = false;
            checkboxSkins.Current.Value = false;
            checkboxCollections.Current.Value = false;

            checkboxBeatmaps.Current.Disabled = true;
            checkboxScores.Current.Disabled = true;
            checkboxSkins.Current.Disabled = true;
            checkboxCollections.Current.Disabled = true;
        }

        private void updateCheckbox(OsuCheckbox checkbox, string name, int count)
        {
            checkbox.Current.Disabled = false;

            if (count > 0)
            {
                checkbox.LabelText = $"{name} ({count:N0})";
                checkbox.Current.Value = true;
                checkbox.Alpha = 1;
            }
            else
            {
                checkbox.LabelText = $"{name} (None found)";
                checkbox.Current.Value = false;

                checkbox.Current.Disabled = true;
                checkbox.Alpha = 0.5f;
            }
        }

        private void startImport()
        {
            if (validLazerPath == null || storage == null) return;

            bool importBeatmaps = checkboxBeatmaps.Current.Value;
            bool importScores = checkboxScores.Current.Value;
            bool importSkins = checkboxSkins.Current.Value;
            bool importCollections = checkboxCollections.Current.Value;

            if (!importBeatmaps && !importScores && !importSkins && !importCollections)
            {
                statusText.Text = "Please select at least one item to import.";
                statusText.FadeIn(100).Then().Delay(2000).FadeOut(500);
                return;
            }

            var lazerImportManager = new LazerImportManager(realmAccess, notifications, storage);

            string sourcePath = validLazerPath;
            this.Exit();

            Task.Run(async () =>
            {
                try
                {
                    await lazerImportManager.ImportFrom(sourcePath, importBeatmaps, importScores, importSkins, importCollections).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Import failed");
                }
            });
        }
    }
}
