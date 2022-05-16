// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.IO;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osuTK;
using CommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunSetupOverlayStrings), nameof(FirstRunSetupOverlayStrings.ImportTitle))]
    public class ScreenImportFromStable : FirstRunSetupScreen
    {
        private static readonly Vector2 button_size = new Vector2(400, 50);

        private ProgressRoundedButton importButton = null!;
        private RoundedButton locateStableButton = null!;

        private OsuTextFlowContainer currentStablePath = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private LegacyImportManager legacyImportManager { get; set; } = null!;

        private CancellationTokenSource? stablePathUpdateCancellation;

        private IEnumerable<ImportCheckbox> contentCheckboxes => Content.Children.OfType<ImportCheckbox>();

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load()
        {
            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Colour = OverlayColourProvider.Content1,
                    Text =
                        "If you have an installation of a previous osu! version, you can choose to migrate your existing content. Note that this will create a copy, and not affect your existing installation.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                currentStablePath = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: HEADER_FONT_SIZE, weight: FontWeight.SemiBold))
                {
                    Colour = OverlayColourProvider.Content2,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.Centre,
                },
                locateStableButton = new RoundedButton
                {
                    Size = button_size,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = "Change location",
                    Action = locateStable,
                },
                new ImportCheckbox("Beatmaps", StableContent.Beatmaps),
                new ImportCheckbox("Scores", StableContent.Scores),
                new ImportCheckbox("Skins", StableContent.Skins),
                new ImportCheckbox("Collections", StableContent.Collections),
                importButton = new ProgressRoundedButton
                {
                    Size = button_size,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = FirstRunSetupOverlayStrings.ImportContentFromStable,
                    Action = runImport
                },
            };

            updateStablePath();
        }

        private void locateStable() => this.Push(new LocateStableScreen());

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            if (e.Last is LocateStableScreen)
                // stable storage may have changed.
                Schedule(updateStablePath);
        }

        private void updateStablePath()
        {
            stablePathUpdateCancellation?.Cancel();

            var storage = legacyImportManager.GetCurrentStableStorage();

            if (storage == null)
            {
                foreach (var c in contentCheckboxes)
                    c.Current.Disabled = true;
                currentStablePath.FadeColour(colours.Red1, 500, Easing.OutQuint);
                currentStablePath.Text = "No installation found";

                importButton.Enabled.Value = false;
                return;
            }

            foreach (var c in contentCheckboxes)
            {
                c.Current.Disabled = false;
                c.UpdateCount();
            }

            currentStablePath.FadeColour(OverlayColourProvider.Content2);
            currentStablePath.Text = $"Found installation: {storage.GetFullPath(string.Empty)}";
            stablePathUpdateCancellation = new CancellationTokenSource();
            importButton.Enabled.Value = true;
        }

        private void runImport()
        {
            importButton.Enabled.Value = false;
            locateStableButton.Enabled.Value = false;

            StableContent importableContent = 0;

            foreach (var c in contentCheckboxes.Where(c => c.Current.Value))
                importableContent |= c.StableContent;

            legacyImportManager.ImportFromStableAsync(importableContent, false).ContinueWith(t => Schedule(() =>
            {
                if (t.IsCompletedSuccessfully)
                    importButton.Complete();
                else
                {
                    importButton.Enabled.Value = true;
                    locateStableButton.Enabled.Value = true;
                    importButton.Abort();
                }
            }));
        }

        private class ImportCheckbox : SettingsCheckbox
        {
            public readonly StableContent StableContent;

            private readonly LocalisableString title;

            [Resolved]
            private LegacyImportManager legacyImportManager { get; set; } = null!;

            private CancellationTokenSource? countUpdateCancellation;

            public ImportCheckbox(LocalisableString title, StableContent stableContent)
            {
                this.title = title;

                StableContent = stableContent;

                Current.Value = true;

                LabelText = title;
            }

            public void UpdateCount()
            {
                LabelText = LocalisableString.Interpolate($"{title} (calculating...)");

                countUpdateCancellation?.Cancel();
                countUpdateCancellation = new CancellationTokenSource();

                legacyImportManager.GetImportCount(StableContent, countUpdateCancellation.Token).ContinueWith(task => Schedule(() =>
                {
                    if (task.IsCanceled)
                        return;

                    LabelText = LocalisableString.Interpolate($"{title} ({task.GetResultSafely()} items)");
                }));
            }
        }

        private class LocateStableScreen : FirstRunSetupScreen
        {
            private RoundedButton selectionButton = null!;

            private OsuDirectorySelector directorySelector = null!;

            protected bool IsValidDirectory(DirectoryInfo? info) => info?.GetFiles("osu!.*.cfg").Any() ?? false;

            public LocalisableString HeaderText => "Please select your osu!stable install location";

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                // Don't want the scroll content provided by `FirstRunSetupScreen` so we don't use `Content`.
                InternalChild = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = CONTENT_PADDING },
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new OsuTextFlowContainer(cp =>
                                    {
                                        cp.Font = OsuFont.Default.With(size: 24);
                                    })
                                    {
                                        Text = HeaderText,
                                        TextAnchor = Anchor.TopCentre,
                                        Margin = new MarginPadding(10),
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                    }
                                },
                                new Drawable[]
                                {
                                    directorySelector = new OsuDirectorySelector
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                },
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                RelativeSizeAxes = Axes.X,
                                                Width = 0.45f,
                                                Height = button_size.Y,
                                                Margin = new MarginPadding(10),
                                                BackgroundColour = colours.Pink2,
                                                Text = CommonStrings.ButtonsCancel,
                                                Action = this.Exit
                                            },
                                            selectionButton = new RoundedButton
                                            {
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                RelativeSizeAxes = Axes.X,
                                                Width = 0.45f,
                                                Height = button_size.Y,
                                                Margin = new MarginPadding(10),
                                                Text = MaintenanceSettingsStrings.SelectDirectory,
                                                Action = () =>
                                                {
                                                    legacyImportManager.UpdateStorage(directorySelector.CurrentPath.Value.FullName);
                                                    this.Exit();
                                                }
                                            },
                                        }
                                    },
                                }
                            }
                        }
                    }
                };
            }

            [Resolved]
            private LegacyImportManager legacyImportManager { get; set; } = null!;

            protected override void LoadComplete()
            {
                if (legacyImportManager.GetCurrentStableStorage() is StableStorage storage)
                    directorySelector.CurrentPath.Value = new DirectoryInfo(storage.GetFullPath(string.Empty));

                directorySelector.CurrentPath.BindValueChanged(e => selectionButton.Enabled.Value = e.NewValue != null && IsValidDirectory(e.NewValue), true);
                base.LoadComplete();
            }

            public override void OnSuspending(ScreenTransitionEvent e)
            {
                base.OnSuspending(e);

                this.FadeOut(250);
            }
        }
    }
}
