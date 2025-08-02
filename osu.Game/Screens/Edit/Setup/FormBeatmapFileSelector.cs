// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Setup
{
    /// <summary>
    /// A type of <see cref="FormFileSelector"/> dedicated to beatmap resources.
    /// </summary>
    /// <remarks>
    /// This expands on <see cref="FormFileSelector"/> by adding an intermediate step before finalisation
    /// to choose whether the selected file should be applied to the current difficulty or all difficulties in the set,
    /// the user's choice is saved in <see cref="ApplyToAllDifficulties"/> before the file selection is finalised and propagated to <see cref="FormFileSelector.Current"/>.
    /// </remarks>
    public partial class FormBeatmapFileSelector : FormFileSelector
    {
        private readonly bool beatmapHasMultipleDifficulties;

        public readonly Bindable<bool> ApplyToAllDifficulties = new Bindable<bool>(true);

        public FormBeatmapFileSelector(bool beatmapHasMultipleDifficulties, params string[] handledExtensions)
            : base(handledExtensions)
        {
            this.beatmapHasMultipleDifficulties = beatmapHasMultipleDifficulties;
        }

        protected override FileChooserPopover CreatePopover(string[] handledExtensions, Bindable<FileInfo?> current, string? chooserPath)
        {
            var popover = new BeatmapFileChooserPopover(handledExtensions, current, chooserPath, beatmapHasMultipleDifficulties);
            popover.ApplyToAllDifficulties.BindTo(ApplyToAllDifficulties);
            return popover;
        }

        private partial class BeatmapFileChooserPopover : FileChooserPopover
        {
            private readonly bool beatmapHasMultipleDifficulties;

            public readonly Bindable<bool> ApplyToAllDifficulties = new Bindable<bool>(true);

            private Container selectApplicationScopeContainer = null!;

            public BeatmapFileChooserPopover(string[] handledExtensions, Bindable<FileInfo?> current, string? chooserPath, bool beatmapHasMultipleDifficulties)
                : base(handledExtensions, current, chooserPath)
            {
                this.beatmapHasMultipleDifficulties = beatmapHasMultipleDifficulties;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colours)
            {
                Add(selectApplicationScopeContainer = new InputBlockingContainer
                {
                    Alpha = 0f,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background6.Opacity(0.9f),
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            CornerRadius = 10f,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Background5,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0f, 10f),
                                    Margin = new MarginPadding(30),
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = EditorSetupStrings.ApplicationScopeSelectionTitle,
                                            Margin = new MarginPadding { Bottom = 20f },
                                        },
                                        new RoundedButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Width = 300f,
                                            Text = EditorSetupStrings.ApplyToAllDifficulties,
                                            Action = () =>
                                            {
                                                ApplyToAllDifficulties.Value = true;
                                                updateFileSelection();
                                            },
                                            BackgroundColour = colours.Red2,
                                        },
                                        new RoundedButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Width = 300f,
                                            Text = EditorSetupStrings.ApplyToThisDifficulty,
                                            Action = () =>
                                            {
                                                ApplyToAllDifficulties.Value = false;
                                                updateFileSelection();
                                            },
                                        },
                                    }
                                }
                            }
                        },
                    }
                });
            }

            protected override void OnFileSelected(FileInfo file)
            {
                if (beatmapHasMultipleDifficulties)
                    selectApplicationScopeContainer.FadeIn(200, Easing.InQuint);
                else
                    base.OnFileSelected(file);
            }

            private void updateFileSelection()
            {
                Debug.Assert(FileSelector.CurrentFile.Value != null);
                base.OnFileSelected(FileSelector.CurrentFile.Value);
            }
        }
    }
}
