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
        public override bool HideOverlaysOnEnter => true;

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

        private Container contentContainer = null!;
        private DirectorySelector directorySelector = null!;
        private FormButton importButton = null!;
        private TextFlowContainer statusText = null!;

        private FillFlowContainer checkboxesContainer = null!;
        private FormCheckBox checkboxBeatmaps = null!;
        private FormCheckBox checkboxScores = null!;
        private FormCheckBox checkboxSkins = null!;
        private FormCheckBox checkboxCollections = null!;

        private string? fullLazerPath;

        private const float duration = 300;
        private const float button_height = 50;
        private const float button_vertical_margin = 15;

        [BackgroundDependencyLoader]
        private void load()
        {
            checkboxesContainer = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
            };

            resetLabels();

            InternalChild = contentContainer = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.9f, 0.8f),
                Children = new Drawable[]
                {
                    directorySelector = new OsuDirectorySelector
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.65f,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.35f,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colourProvider.Background4,
                                RelativeSizeAxes = Axes.Both
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Bottom = button_height + button_vertical_margin * 2 },
                                Child = new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Top = 20, Left = 20, Right = 20 },
                                    Child = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 5),
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = "Import Options",
                                                Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                                                Colour = colourProvider.Content1,
                                                Margin = new MarginPadding { Bottom = 10 }
                                            },
                                            checkboxesContainer,
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 1,
                                                Colour = colourProvider.Light4,
                                                Margin = new MarginPadding { Vertical = 10 }
                                            },
                                            statusText = new TextFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Colour = colours.Red1
                                            }
                                        }
                                    }
                                }
                            },
                            importButton = new FormButton
                            {
                                Caption = "Import selected data from the chosen installation",
                                ButtonText = "Start Import",
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.X,
                                Width = 0.9f,
                                Margin = new MarginPadding { Bottom = button_vertical_margin },
                                Action = startImport,
                                Enabled = { Value = false }
                            }
                        }
                    }
                }
            };

            directorySelector.CurrentPath.BindValueChanged(e => validatePath(e.NewValue), true);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            contentContainer.ScaleTo(0.95f).ScaleTo(1, duration, Easing.OutQuint);
            this.FadeInFromZero(duration);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            contentContainer.ScaleTo(0.95f, duration, Easing.OutQuint);
            this.FadeOut(duration, Easing.OutQuint);
            return base.OnExiting(e);
        }

        private void validatePath(DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists) return;

            importButton.Enabled.Value = false;
            statusText.Text = "Checking...";

            resetLabels();
            fullLazerPath = null;

            Task.Run(() =>
            {
                string proposedPath = Path.GetFullPath(directory.FullName);
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

                            checkboxBeatmaps = createCheckbox("Beatmaps", countMaps);
                            checkboxScores = createCheckbox("Scores", countScores);
                            checkboxSkins = createCheckbox("Skins", countSkins);
                            checkboxCollections = createCheckbox("Collections", countCollections);

                            checkboxesContainer.Children = new Drawable[]
                            {
                                checkboxBeatmaps,
                                checkboxScores,
                                checkboxSkins,
                                checkboxCollections
                            };

                            fullLazerPath = proposedPath;
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
            checkboxBeatmaps = new FormCheckBox { Caption = "Beatmaps", Current = { Value = false, Disabled = true } };
            checkboxScores = new FormCheckBox { Caption = "Scores", Current = { Value = false, Disabled = true } };
            checkboxSkins = new FormCheckBox { Caption = "Skins", Current = { Value = false, Disabled = true } };
            checkboxCollections = new FormCheckBox { Caption = "Collections", Current = { Value = false, Disabled = true } };

            checkboxesContainer.Children = new Drawable[]
            {
                checkboxBeatmaps,
                checkboxScores,
                checkboxSkins,
                checkboxCollections
            };
        }

        private FormCheckBox createCheckbox(string name, int count)
        {
            if (count > 0)
            {
                return new FormCheckBox
                {
                    Caption = $"{name} ({count:N0})",
                    Current = { Value = true, Disabled = false },
                    Alpha = 1
                };
            }

            return new FormCheckBox
            {
                Caption = $"{name} (None found)",
                Current = { Value = false, Disabled = true },
                Alpha = 0.5f
            };
        }

        private void startImport()
        {
            if (fullLazerPath == null || storage == null) return;

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

            this.Exit();

            Task.Run(async () =>
            {
                try
                {
                    await lazerImportManager.ImportFrom(fullLazerPath, importBeatmaps, importScores, importSkins, importCollections).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Import failed");
                }
            });
        }
    }
}
