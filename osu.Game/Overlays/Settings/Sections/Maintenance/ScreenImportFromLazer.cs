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

        private OsuCheckbox checkboxBeatmaps = null!;
        private OsuCheckbox checkboxScores = null!;
        private OsuCheckbox checkboxSkins = null!;
        private OsuCheckbox checkboxCollections = null!;

        private OsuSpriteText beatmapStat = null!;
        private OsuSpriteText scoreStat = null!;
        private OsuSpriteText skinStat = null!;
        private OsuSpriteText collectionStat = null!;

        private string? validLazerPath;

        [BackgroundDependencyLoader]
        private void load()
        {
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

                                                new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Padding = new MarginPadding { Top = 20, Left = 20, Right = 20, Bottom = 80 },
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

                                                        checkboxBeatmaps = new OsuCheckbox
                                                        {
                                                            LabelText = "Beatmaps",
                                                            Current = { Value = true }
                                                        },
                                                        checkboxScores = new OsuCheckbox
                                                        {
                                                            LabelText = "Scores",
                                                            Current = { Value = true }
                                                        },
                                                        checkboxSkins = new OsuCheckbox
                                                        {
                                                            LabelText = "Skins",
                                                            Current = { Value = true }
                                                        },
                                                        checkboxCollections = new OsuCheckbox
                                                        {
                                                            LabelText = "Collections",
                                                            Current = { Value = true }
                                                        },

                                                        new Box { RelativeSizeAxes = Axes.X, Height = 1, Colour = colourProvider.Light4, Margin = new MarginPadding { Vertical = 10 } },

                                                        new OsuSpriteText
                                                        {
                                                            Text = "Found Data",
                                                            Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                                                            Colour = colourProvider.Content1
                                                        },

                                                        beatmapStat = new OsuSpriteText { Text = "Beatmaps: -", Font = OsuFont.GetFont(size: 14), Colour = colourProvider.Content2 },
                                                        scoreStat = new OsuSpriteText { Text = "Scores: -", Font = OsuFont.GetFont(size: 14), Colour = colourProvider.Content2 },
                                                        skinStat = new OsuSpriteText { Text = "Skins: -", Font = OsuFont.GetFont(size: 14), Colour = colourProvider.Content2 },
                                                        collectionStat = new OsuSpriteText { Text = "Collections: -", Font = OsuFont.GetFont(size: 14), Colour = colourProvider.Content2 },

                                                        statusText = new TextFlowContainer
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Colour = colours.Red1
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
            resetStats();
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
                            beatmapStat.Text = $"Beatmaps: {countMaps:N0}";
                            scoreStat.Text = $"Scores: {countScores:N0}";
                            skinStat.Text = $"Skins: {countSkins:N0}";
                            collectionStat.Text = $"Collections: {countCollections:N0}";

                            beatmapStat.Colour = countMaps > 0 ? colourProvider.Content1 : colourProvider.Content2;
                            scoreStat.Colour = countScores > 0 ? colourProvider.Content1 : colourProvider.Content2;

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

        private void resetStats()
        {
            beatmapStat.Text = "Beatmaps: -";
            scoreStat.Text = "Scores: -";
            skinStat.Text = "Skins: -";
            collectionStat.Text = "Collections: -";
        }

        private void startImport()
        {
            if (validLazerPath == null || storage == null) return;

            bool importBeatmaps = checkboxBeatmaps.Current.Value;
            bool importScores = checkboxScores.Current.Value;
            bool importSkins = checkboxSkins.Current.Value;
            bool importCollections = checkboxCollections.Current.Value;

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
