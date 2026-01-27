// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormFileSelector : CompositeDrawable, IHasCurrentValue<FileInfo?>, ICanAcceptFiles, IHasPopover
    {
        public Bindable<FileInfo?> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<FileInfo?> current = new BindableWithCurrent<FileInfo?>();

        public IEnumerable<string> HandledExtensions => handledExtensions;

        private readonly string[] handledExtensions;

        /// <summary>
        /// The initial path to use when displaying the <see cref="FileChooserPopover"/>.
        /// </summary>
        /// <remarks>
        /// Uses a <see langword="null"/> value before the first selection is made
        /// to ensure that the first selection starts at <see cref="GameHost.InitialFileSelectorPath"/>.
        /// </remarks>
        private string? initialChooserPath;

        private readonly Bindable<Visibility> popoverState = new Bindable<Visibility>();

        /// <summary>
        /// Caption describing this file selector, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this file selector, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        /// <summary>
        /// Text displayed in the selector when no file is selected.
        /// </summary>
        public LocalisableString PlaceholderText { get; init; }

        public Container PreviewContainer { get; private set; } = null!;

        private FormControlBackground background = null!;

        private FormFieldCaption caption = null!;
        private OsuSpriteText placeholderText = null!;
        private OsuSpriteText filenameText = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        public FormFileSelector(params string[] handledExtensions)
        {
            this.handledExtensions = handledExtensions;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                PreviewContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Horizontal = 1.5f,
                        Top = 1.5f,
                        Bottom = 50
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Padding = new MarginPadding(9),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 4f),
                            Children = new Drawable[]
                            {
                                caption = new FormFieldCaption
                                {
                                    Caption = Caption,
                                    TooltipText = HintText,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new[]
                                    {
                                        placeholderText = new OsuSpriteText
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Width = 1,
                                            Text = PlaceholderText,
                                            Colour = colourProvider.Foreground1,
                                        },
                                        filenameText = new OsuSpriteText
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Width = 1,
                                        },
                                    }
                                }
                            },
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Icon = FontAwesome.Solid.FolderOpen,
                            Size = new Vector2(16),
                            Colour = colourProvider.Light1,
                        }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            popoverState.BindValueChanged(_ => updateState());
            current.BindDisabledChanged(_ => updateState());
            current.BindValueChanged(_ =>
            {
                updateState();
                onFileSelected();
            }, true);
            FinishTransforms(true);
            game.RegisterImportHandler(this);
        }

        private void onFileSelected()
        {
            if (Current.Value != null)
                this.HidePopover();

            initialChooserPath = Current.Value?.DirectoryName;
            placeholderText.Alpha = Current.Value == null ? 1 : 0;
            filenameText.Text = Current.Value?.Name ?? string.Empty;
            background.Flash();
        }

        protected override bool OnClick(ClickEvent e)
        {
            this.ShowPopover();
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        private void updateState()
        {
            caption.Colour = Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content2;
            filenameText.Colour = Current.Disabled || Current.Value == null ? colourProvider.Foreground1 : colourProvider.Content1;

            if (Current.Disabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (popoverState.Value == Visibility.Visible)
                background.VisualStyle = VisualStyle.Focused;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (game.IsNotNull())
                game.UnregisterImportHandler(this);
        }

        Task ICanAcceptFiles.Import(params string[] paths)
        {
            Schedule(() => Current.Value = new FileInfo(paths.First()));
            return Task.CompletedTask;
        }

        Task ICanAcceptFiles.Import(ImportTask[] tasks, ImportParameters parameters) => throw new NotImplementedException();

        protected virtual FileChooserPopover CreatePopover(string[] handledExtensions, Bindable<FileInfo?> current, string? chooserPath) =>
            new FileChooserPopover(handledExtensions, current, chooserPath);

        public Popover GetPopover()
        {
            var popover = CreatePopover(handledExtensions, Current, initialChooserPath);
            popoverState.UnbindBindings();
            popoverState.BindTo(popover.State);
            return popover;
        }

        public partial class FileChooserPopover : OsuPopover
        {
            protected override string PopInSampleName => "UI/overlay-big-pop-in";
            protected override string PopOutSampleName => "UI/overlay-big-pop-out";

            private readonly Bindable<FileInfo?> current = new Bindable<FileInfo?>();

            protected OsuFileSelector FileSelector;

            public FileChooserPopover(string[] handledExtensions, Bindable<FileInfo?> current, string? chooserPath)
                : base(false)
            {
                Child = new Container
                {
                    Size = new Vector2(600, 400),
                    // simplest solution to avoid underlying text to bleed through the bottom border
                    // https://github.com/ppy/osu/pull/30005#issuecomment-2378884430
                    Padding = new MarginPadding { Bottom = 1 },
                    Child = FileSelector = new OsuFileSelector(chooserPath, handledExtensions)
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                };

                this.current.BindTo(current);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Add(new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 2,
                    CornerRadius = 10,
                    BorderColour = colourProvider.Highlight1,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Transparent,
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                FileSelector.CurrentFile.ValueChanged += f =>
                {
                    if (f.NewValue != null)
                        OnFileSelected(f.NewValue);
                };
            }

            protected virtual void OnFileSelected(FileInfo file) => current.Value = file;
        }
    }
}
