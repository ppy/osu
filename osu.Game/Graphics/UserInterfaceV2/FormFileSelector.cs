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
using osu.Framework.Graphics.Colour;
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

        private readonly Bindable<FileInfo?> popoverSelection = new Bindable<FileInfo?>();

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

        private Box background = null!;

        private FormFieldCaption caption = null!;
        private OsuSpriteText placeholderText = null!;
        private OsuSpriteText filenameText = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        private ISystemFileSelector? systemFileSelector;

        protected virtual bool IsPopoverVisible => popoverState.Value == Visibility.Visible;

        public FormFileSelector(params string[] handledExtensions)
        {
            this.handledExtensions = handledExtensions;
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
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
                    Height = 50,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Padding = new MarginPadding(9),
                    Children = new Drawable[]
                    {
                        caption = new FormFieldCaption
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Caption = Caption,
                            TooltipText = HintText,
                        },
                        placeholderText = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Width = 1,
                            Text = PlaceholderText,
                            Colour = colourProvider.Foreground1,
                        },
                        filenameText = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Width = 1,
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

            systemFileSelector = host.CreateSystemFileSelector(handledExtensions);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            popoverState.BindValueChanged(_ => UpdateState());
            current.BindDisabledChanged(_ => UpdateState());
            current.BindValueChanged(_ =>
            {
                UpdateState();
                onCurrentChanged();
            }, true);

            popoverSelection.BindValueChanged(f =>
            {
                this.HidePopover();
                OnFileSelected(f.NewValue!);
            });

            if (systemFileSelector != null)
                systemFileSelector.Selected += f => Schedule(OnFileSelected, f);

            FinishTransforms(true);
            game.RegisterImportHandler(this);
        }

        private void onCurrentChanged()
        {
            initialChooserPath = Current.Value?.DirectoryName;
            placeholderText.Alpha = Current.Value == null ? 1 : 0;
            filenameText.Text = Current.Value?.Name ?? string.Empty;
            background.FlashColour(ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark2), 800, Easing.OutQuint);
        }

        /// <summary>
        /// Triggered when a file is selected from a file selector. By default, this directly propagates the selection to <see cref="Current"/>,
        /// but extra dialogs can be included before propagating selection to <see cref="Current"/> by overriding this method.
        /// </summary>
        /// <param name="file">The file selected by the file selector.</param>
        protected virtual void OnFileSelected(FileInfo file) => Current.Value = file;

        protected override bool OnClick(ClickEvent e)
        {
            if (systemFileSelector != null)
                systemFileSelector.Present();
            else
                this.ShowPopover();

            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            UpdateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            UpdateState();
        }

        public void UpdateState()
        {
            caption.Colour = Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content2;
            filenameText.Colour = Current.Disabled || Current.Value == null ? colourProvider.Foreground1 : colourProvider.Content1;

            if (!Current.Disabled)
            {
                BorderThickness = IsHovered || IsPopoverVisible ? 2 : 0;
                BorderColour = IsPopoverVisible ? colourProvider.Highlight1 : colourProvider.Light4;

                if (IsPopoverVisible)
                    background.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark3);
                else if (IsHovered)
                    background.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark4);
                else
                    background.Colour = colourProvider.Background5;
            }
            else
            {
                background.Colour = colourProvider.Background4;
            }
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

        public Popover GetPopover()
        {
            var popover = new FileChooserPopover(handledExtensions, popoverSelection, initialChooserPath);
            popoverState.UnbindBindings();
            popoverState.BindTo(popover.State);
            return popover;
        }

        private partial class FileChooserPopover : OsuPopover
        {
            protected override string PopInSampleName => "UI/overlay-big-pop-in";
            protected override string PopOutSampleName => "UI/overlay-big-pop-out";

            public FileChooserPopover(string[] handledExtensions, Bindable<FileInfo?> current, string? chooserPath)
                : base(false)
            {
                Child = new Container
                {
                    Size = new Vector2(600, 400),
                    // simplest solution to avoid underlying text to bleed through the bottom border
                    // https://github.com/ppy/osu/pull/30005#issuecomment-2378884430
                    Padding = new MarginPadding { Bottom = 1 },
                    Child = new OsuFileSelector(chooserPath, handledExtensions)
                    {
                        RelativeSizeAxes = Axes.Both,
                        CurrentFile = { BindTarget = current },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Body.BorderColour = colourProvider.Highlight1;
                Body.BorderThickness = 2;
            }
        }
    }
}
