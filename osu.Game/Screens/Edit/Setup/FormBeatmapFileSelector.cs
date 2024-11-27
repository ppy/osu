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
        private readonly bool multipleDifficulties;

        public readonly Bindable<bool> ApplyToAllDifficulties = new Bindable<bool>(true);

        public FormBeatmapFileSelector(bool multipleDifficulties, params string[] handledExtensions)
            : base(handledExtensions)
        {
            this.multipleDifficulties = multipleDifficulties;
        }

        protected override FileChooserPopover CreatePopover(string[] handledExtensions, Bindable<FileInfo?> current, string? chooserPath)
        {
            var popover = new BeatmapFileChooserPopover(handledExtensions, current, chooserPath, multipleDifficulties);

            popover.ApplyToAllDifficulties.ValueChanged += v =>
            {
                Debug.Assert(v.NewValue != null);
                ApplyToAllDifficulties.Value = v.NewValue.Value;
            };

            return popover;
        }

        private partial class BeatmapFileChooserPopover : FileChooserPopover
        {
            private readonly bool multipleDifficulties;

            public readonly Bindable<bool?> ApplyToAllDifficulties = new Bindable<bool?>();

            private Container changeScopeContainer = null!;

            public BeatmapFileChooserPopover(string[] handledExtensions, Bindable<FileInfo?> current, string? chooserPath, bool multipleDifficulties)
                : base(handledExtensions, current, chooserPath)
            {
                this.multipleDifficulties = multipleDifficulties;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colours)
            {
                Add(changeScopeContainer = new InputBlockingContainer
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
                                            Text = "Apply this change to all difficulties?",
                                            Margin = new MarginPadding { Bottom = 20f },
                                        },
                                        new RoundedButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Width = 300f,
                                            Text = "Apply to all difficulties",
                                            Action = () => ApplyToAllDifficulties.Value = true,
                                            BackgroundColour = colours.Red2,
                                        },
                                        new RoundedButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Width = 300f,
                                            Text = "Only apply to this difficulty",
                                            Action = () => ApplyToAllDifficulties.Value = false,
                                        },
                                    }
                                }
                            }
                        },
                    }
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                ApplyToAllDifficulties.ValueChanged += onChangeScopeSelected;
            }

            protected override void OnFileSelected(FileInfo file)
            {
                if (multipleDifficulties)
                    changeScopeContainer.FadeIn(200, Easing.InQuint);
                else
                    base.OnFileSelected(file);
            }

            private void onChangeScopeSelected(ValueChangedEvent<bool?> c)
            {
                if (c.NewValue == null)
                    return;

                Debug.Assert(FileSelector.CurrentFile.Value != null);
                base.OnFileSelected(FileSelector.CurrentFile.Value);
            }
        }
    }
}
