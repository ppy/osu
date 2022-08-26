// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Edit.Setup;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunOverlayImportFromStableScreenStrings), nameof(FirstRunOverlayImportFromStableScreenStrings.Header))]
    public class ScreenImportFromStable : FirstRunSetupScreen
    {
        private static readonly Vector2 button_size = new Vector2(400, 50);

        private ProgressRoundedButton importButton = null!;

        private OsuTextFlowContainer progressText = null!;

        [Resolved]
        private LegacyImportManager legacyImportManager { get; set; } = null!;

        private StableLocatorLabelledTextBox stableLocatorTextBox = null!;

        private IEnumerable<ImportCheckbox> contentCheckboxes => Content.Children.OfType<ImportCheckbox>();

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load()
        {
            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Colour = OverlayColourProvider.Content1,
                    Text = FirstRunOverlayImportFromStableScreenStrings.Description,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                stableLocatorTextBox = new StableLocatorLabelledTextBox
                {
                    Label = FirstRunOverlayImportFromStableScreenStrings.LocateDirectoryLabel,
                    PlaceholderText = FirstRunOverlayImportFromStableScreenStrings.LocateDirectoryPlaceholder
                },
                new ImportCheckbox(CommonStrings.Beatmaps, StableContent.Beatmaps),
                new ImportCheckbox(CommonStrings.Scores, StableContent.Scores),
                new ImportCheckbox(CommonStrings.Skins, StableContent.Skins),
                new ImportCheckbox(CommonStrings.Collections, StableContent.Collections),
                importButton = new ProgressRoundedButton
                {
                    Size = button_size,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = FirstRunOverlayImportFromStableScreenStrings.ImportButton,
                    Action = runImport
                },
                progressText = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Colour = OverlayColourProvider.Content1,
                    Text = FirstRunOverlayImportFromStableScreenStrings.ImportInProgress,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Alpha = 0,
                },
            };

            stableLocatorTextBox.Current.BindValueChanged(_ => updateStablePath(), true);
        }

        private void updateStablePath()
        {
            var storage = legacyImportManager.GetCurrentStableStorage();

            if (storage == null)
            {
                toggleInteraction(false);

                stableLocatorTextBox.Current.Disabled = false;
                stableLocatorTextBox.Current.Value = string.Empty;
                return;
            }

            foreach (var c in contentCheckboxes)
            {
                c.Current.Disabled = false;
                c.UpdateCount();
            }

            toggleInteraction(true);
            stableLocatorTextBox.Current.Value = storage.GetFullPath(string.Empty);
            importButton.Enabled.Value = true;
        }

        private void runImport()
        {
            toggleInteraction(false);
            progressText.FadeIn(1000, Easing.OutQuint);

            StableContent importableContent = 0;

            foreach (var c in contentCheckboxes.Where(c => c.Current.Value))
                importableContent |= c.StableContent;

            legacyImportManager.ImportFromStableAsync(importableContent, false).ContinueWith(t => Schedule(() =>
            {
                progressText.FadeOut(500, Easing.OutQuint);

                if (t.IsCompletedSuccessfully)
                    importButton.Complete();
                else
                {
                    toggleInteraction(true);
                    importButton.Abort();
                }
            }));
        }

        private void toggleInteraction(bool allow)
        {
            importButton.Enabled.Value = allow;
            stableLocatorTextBox.Current.Disabled = !allow;
            foreach (var c in contentCheckboxes)
                c.Current.Disabled = !allow;
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

                Current.Default = true;
                Current.Value = true;

                LabelText = title;
            }

            public void UpdateCount()
            {
                LabelText = LocalisableString.Interpolate($"{title} ({FirstRunOverlayImportFromStableScreenStrings.Calculating})");

                countUpdateCancellation?.Cancel();
                countUpdateCancellation = new CancellationTokenSource();

                legacyImportManager.GetImportCount(StableContent, countUpdateCancellation.Token).ContinueWith(task => Schedule(() =>
                {
                    if (task.IsCanceled)
                        return;

                    int count = task.GetResultSafely();

                    LabelText = LocalisableString.Interpolate($"{title} ({FirstRunOverlayImportFromStableScreenStrings.Items(count)})");
                }));
            }
        }

        internal class StableLocatorLabelledTextBox : LabelledTextBoxWithPopover, ICanAcceptFiles
        {
            [Resolved]
            private LegacyImportManager legacyImportManager { get; set; } = null!;

            public IEnumerable<string> HandledExtensions { get; } = new[] { string.Empty };

            private readonly Bindable<DirectoryInfo> currentDirectory = new Bindable<DirectoryInfo>();

            [Resolved(canBeNull: true)] // Can't really be null but required to handle potential of disposal before DI completes.
            private OsuGameBase? game { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                game?.RegisterImportHandler(this);

                currentDirectory.BindValueChanged(onDirectorySelected);

                string? fullPath = legacyImportManager.GetCurrentStableStorage()?.GetFullPath(string.Empty);
                if (fullPath != null)
                    currentDirectory.Value = new DirectoryInfo(fullPath);
            }

            private void onDirectorySelected(ValueChangedEvent<DirectoryInfo> directory)
            {
                if (directory.NewValue == null)
                {
                    Current.Value = string.Empty;
                    return;
                }

                // DirectorySelectors can trigger a noop value changed, but `DirectoryInfo` equality doesn't catch this.
                if (directory.OldValue?.FullName == directory.NewValue.FullName)
                    return;

                if (directory.NewValue?.GetFiles(@"osu!.*.cfg").Any() ?? false)
                {
                    this.HidePopover();

                    string path = directory.NewValue.FullName;

                    legacyImportManager.UpdateStorage(path);
                    Current.Value = path;
                }
            }

            Task ICanAcceptFiles.Import(params string[] paths)
            {
                Schedule(() => currentDirectory.Value = new DirectoryInfo(paths.First()));
                return Task.CompletedTask;
            }

            Task ICanAcceptFiles.Import(params ImportTask[] tasks) => throw new NotImplementedException();

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                game?.UnregisterImportHandler(this);
            }

            public override Popover GetPopover() => new DirectoryChooserPopover(currentDirectory);

            private class DirectoryChooserPopover : OsuPopover
            {
                public DirectoryChooserPopover(Bindable<DirectoryInfo> currentDirectory)
                {
                    Child = new Container
                    {
                        Size = new Vector2(600, 400),
                        Child = new OsuDirectorySelector(currentDirectory.Value?.FullName)
                        {
                            RelativeSizeAxes = Axes.Both,
                            CurrentPath = { BindTarget = currentDirectory }
                        },
                    };
                }
            }
        }
    }
}
