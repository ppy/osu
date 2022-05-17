// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

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
    [LocalisableDescription(typeof(FirstRunSetupOverlayStrings), nameof(FirstRunSetupOverlayStrings.ImportTitle))]
    public class ScreenImportFromStable : FirstRunSetupScreen
    {
        private static readonly Vector2 button_size = new Vector2(400, 50);

        private ProgressRoundedButton importButton = null!;

        [Resolved]
        private LegacyImportManager legacyImportManager { get; set; } = null!;

        private CancellationTokenSource? stablePathUpdateCancellation;

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
                    Text =
                        "If you have an installation of a previous osu! version, you can choose to migrate your existing content. Note that this will create a copy, and not affect your existing installation.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                stableLocatorTextBox = new StableLocatorLabelledTextBox
                {
                    Label = "previous osu! install",
                    PlaceholderText = "Click to locate a previous osu! install"
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

            stableLocatorTextBox.Current.BindValueChanged(_ => updateStablePath(), true);
        }

        private void updateStablePath()
        {
            stablePathUpdateCancellation?.Cancel();

            var storage = legacyImportManager.GetCurrentStableStorage();

            if (storage == null)
            {
                foreach (var c in contentCheckboxes)
                    c.Current.Disabled = true;

                stableLocatorTextBox.Current.Value = string.Empty;
                importButton.Enabled.Value = false;
                return;
            }

            foreach (var c in contentCheckboxes)
            {
                c.Current.Disabled = false;
                c.UpdateCount();
            }

            stableLocatorTextBox.Current.Value = storage.GetFullPath(string.Empty);

            stablePathUpdateCancellation = new CancellationTokenSource();
            importButton.Enabled.Value = true;
        }

        private void runImport()
        {
            importButton.Enabled.Value = false;
            stableLocatorTextBox.Current.Disabled = true;

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
                    stableLocatorTextBox.Current.Disabled = false;
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

        internal class StableLocatorLabelledTextBox : LabelledTextBoxWithPopover, ICanAcceptFiles
        {
            [Resolved]
            private LegacyImportManager legacyImportManager { get; set; } = null!;

            // TODO: test
            public IEnumerable<string> HandledExtensions { get; } = new[] { string.Empty };

            private readonly Bindable<DirectoryInfo> currentDirectory = new Bindable<DirectoryInfo>();

            [Resolved]
            private OsuGameBase game { get; set; } = null!;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                game.RegisterImportHandler(this);

                currentDirectory.BindValueChanged(onDirectorySelected);

                string? fullPath = legacyImportManager.GetCurrentStableStorage()?.GetFullPath(string.Empty);
                if (fullPath != null)
                    currentDirectory.Value = new DirectoryInfo(fullPath);
            }

            private void onDirectorySelected(ValueChangedEvent<DirectoryInfo> directory)
            {
                if (directory.NewValue == null || directory.OldValue == null)
                {
                    Current.Value = string.Empty;
                    return;
                }

                // DirectorySelectors can trigger a noop value changed, but `DirectoryInfo` equality doesn't catch this.
                if (directory.OldValue.FullName == directory.NewValue.FullName)
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
                game.UnregisterImportHandler(this);
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
