// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components
{
    public partial class FormSampleSet : CompositeDrawable, IHasCurrentValue<EditorBeatmapSkin.SampleSet?>
    {
        public Bindable<EditorBeatmapSkin.SampleSet?> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public Func<FileInfo, string, string>? SampleAddRequested { get; init; }
        public Action<string>? SampleRemoveRequested { get; init; }

        private readonly BindableWithCurrent<EditorBeatmapSkin.SampleSet?> current = new BindableWithCurrent<EditorBeatmapSkin.SampleSet?>();
        private readonly Dictionary<(string name, string bank), SampleButton> buttons = new Dictionary<(string, string), SampleButton>();

        private FormControlBackground background = null!;
        private FormFieldCaption caption = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(9),
                    Spacing = new Vector2(7),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        caption = new FormFieldCaption(),
                        new GridContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            RowDimensions = Enumerable.Repeat(new Dimension(GridSizeMode.AutoSize), 4).ToArray(),
                            ColumnDimensions = Enumerable.Repeat(new Dimension(GridSizeMode.AutoSize), 5).ToArray(),
                            Content = createTableContent().ToArray(),
                        }
                    },
                },
            };
        }

        private IEnumerable<Drawable[]> createTableContent()
        {
            string[] columns = HitSampleInfo.ALL_ADDITIONS.Prepend(HitSampleInfo.HIT_NORMAL).ToArray();
            string[] rows = HitSampleInfo.ALL_BANKS;

            yield return columns.Select(makeTableHeading).Prepend(Empty()).ToArray();

            foreach (string row in rows)
            {
                List<Drawable> drawables = [makeTableHeading(row)];

                foreach (string col in columns)
                    drawables.Add(buttons[(col, row)] = makeButton());

                yield return drawables.ToArray();
            }
        }

        private OsuSpriteText makeTableHeading(string text) => new OsuSpriteText
        {
            Text = text,
            Font = OsuFont.Style.Caption1,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        private SampleButton makeButton() => new SampleButton
        {
            Width = 60,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Margin = new MarginPadding(5),
            SampleAddRequested = SampleAddRequested,
            SampleRemoveRequested = SampleRemoveRequested,
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
            Current.BindValueChanged(setChanged, true);
        }

        private void setChanged(ValueChangedEvent<EditorBeatmapSkin.SampleSet?> valueChangedEvent)
        {
            var set = valueChangedEvent.NewValue;

            caption.Caption = set?.Name ?? default(LocalisableString);
            Alpha = set != null && set.SampleSetIndex > 0 ? 1 : 0;

            if (set != null)
            {
                foreach (var (sample, button) in buttons)
                {
                    button.ExpectedFilename.Value = $@"{sample.bank}-{sample.name}{(set.SampleSetIndex > 1 ? set.SampleSetIndex : null)}";
                    button.ActualFilename.Value = set.FindSampleIfExists(sample.name, sample.bank);
                }
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            caption.Colour = colourProvider.Content2;

            background.VisualStyle = IsHovered ? VisualStyle.Hovered : VisualStyle.Normal;
        }

        public partial class SampleButton : OsuButton, IHasPopover, IHasContextMenu
        {
            /// <summary>
            /// The expected filename for the sample that this button represents.
            /// Does not contain extension.
            /// </summary>
            public Bindable<string> ExpectedFilename { get; } = new Bindable<string>();

            /// <summary>
            /// The actual chosen filename for the sample that this button represent.
            /// Can be <see langword="null"/> if the sample is omitted / missing.
            /// Does contain extension.
            /// </summary>
            public Bindable<string?> ActualFilename { get; } = new Bindable<string?>();

            /// <summary>
            /// Invoked when a new sample is selected via this button.
            /// </summary>
            public Func<FileInfo, string, string>? SampleAddRequested { get; init; }

            /// <summary>
            /// Invoked when a sample removal is selected via this button.
            /// </summary>
            public Action<string>? SampleRemoveRequested { get; init; }

            private Bindable<FileInfo?> selectedFile { get; } = new Bindable<FileInfo?>();

            private TrianglesV2? triangles { get; set; }

            protected override float HoverLayerFinalAlpha => 0;

            private Color4? triangleGradientSecondColour;
            private SpriteIcon icon = null!;

            [Resolved]
            private OverlayColourProvider overlayColourProvider { get; set; } = null!;

            [Resolved]
            private EditorBeatmap? editorBeatmap { get; set; }

            private HoverSounds? hoverSounds;

            private ISample? sample;

            public SampleButton()
                : base(null)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Add(icon = new SpriteIcon
                {
                    Icon = FontAwesome.Solid.Plus,
                    Size = new Vector2(16),
                    Shadow = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

                Action = () =>
                {
                    if (ActualFilename.Value == null)
                    {
                        selectedFile.Value = null;
                        this.ShowPopover();
                    }
                    else
                        sample?.Play();
                };

                if (editorBeatmap?.BeatmapSkin != null)
                    editorBeatmap.BeatmapSkin.BeatmapSkinChanged += recycleSamples;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Content.CornerRadius = 4;

                Add(triangles = new TrianglesV2
                {
                    Thickness = 0.02f,
                    SpawnRatio = 0.6f,
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                });

                ActualFilename.BindValueChanged(_ => updateState(), true);
                selectedFile.BindValueChanged(_ => addSample());
            }

            private void updateState()
            {
                BackgroundColour = ActualFilename.Value == null ? overlayColourProvider.Background3 : overlayColourProvider.Colour3;
                triangleGradientSecondColour = BackgroundColour.Lighten(0.2f);
                icon.Icon = ActualFilename.Value == null ? FontAwesome.Solid.Plus : FontAwesome.Solid.Play;

                recycleSamples();

                if (triangles == null)
                    return;

                triangles.Colour = ColourInfo.GradientVertical(triangleGradientSecondColour.Value, BackgroundColour);
            }

            private void recycleSamples() => Schedule(() =>
            {
                if (hoverSounds?.Parent == this)
                {
                    RemoveInternal(hoverSounds, true);
                    hoverSounds = null;
                }

                AddInternal(hoverSounds = (ActualFilename.Value == null ? new HoverClickSounds(HoverSampleSet.Button) : new HoverSounds(HoverSampleSet.Button)));

                if (ActualFilename.Value != null)
                {
                    // to cover all bases, invalidate the extensionless filename (which gameplay is most likely to use)
                    // as well as the filename with extension (which we are using here).
                    editorBeatmap?.BeatmapSkin?.Skin.Samples?.Invalidate(ExpectedFilename.Value);
                    editorBeatmap?.BeatmapSkin?.Skin.Samples?.Invalidate(ActualFilename.Value);
                    sample = editorBeatmap?.BeatmapSkin?.Skin.Samples?.Get(ActualFilename.Value);
                }
                else
                {
                    sample = null;
                }
            });

            protected override bool OnHover(HoverEvent e)
            {
                Debug.Assert(triangleGradientSecondColour != null);

                Background.FadeColour(triangleGradientSecondColour.Value, 300, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Background.FadeColour(BackgroundColour, 300, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            private void addSample()
            {
                if (selectedFile.Value == null)
                    return;

                this.HidePopover();
                ActualFilename.Value = SampleAddRequested?.Invoke(selectedFile.Value, ExpectedFilename.Value) ?? selectedFile.Value.ToString();
            }

            private void deleteSample()
            {
                if (ActualFilename.Value == null)
                    return;

                SampleRemoveRequested?.Invoke(ActualFilename.Value);
                ActualFilename.Value = null;
            }

            public Popover? GetPopover() => ActualFilename.Value == null ? new FormFileSelector.FileChooserPopover(SupportedExtensions.AUDIO_EXTENSIONS, selectedFile, null) : null;

            public MenuItem[]? ContextMenuItems =>
                ActualFilename.Value != null
                    ? [new OsuMenuItem(CommonStrings.ButtonsDelete, MenuItemType.Destructive, deleteSample)]
                    : null;

            protected override void Dispose(bool isDisposing)
            {
                if (editorBeatmap?.BeatmapSkin != null)
                    editorBeatmap.BeatmapSkin.BeatmapSkinChanged -= recycleSamples;
                base.Dispose(isDisposing);
            }
        }
    }
}
