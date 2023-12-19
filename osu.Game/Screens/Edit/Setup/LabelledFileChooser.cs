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
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    /// <summary>
    /// A labelled drawable displaying file chooser on click, with placeholder text support.
    /// todo: this should probably not use PopoverTextBox just to display placeholder text, but is the best way for now.
    /// </summary>
    internal partial class LabelledFileChooser : LabelledDrawable<LabelledTextBoxWithPopover.PopoverTextBox>, IHasCurrentValue<FileInfo?>, ICanAcceptFiles, IHasPopover
    {
        private readonly string[] handledExtensions;

        public IEnumerable<string> HandledExtensions => handledExtensions;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        /// <summary>
        /// The initial path to use when displaying the <see cref="FileChooserPopover"/>.
        /// </summary>
        /// <remarks>
        /// Uses a <see langword="null"/> value before the first selection is made
        /// to ensure that the first selection starts at <see cref="GameHost.InitialFileSelectorPath"/>.
        /// </remarks>
        private string? initialChooserPath;

        private readonly BindableWithCurrent<FileInfo?> current = new BindableWithCurrent<FileInfo?>();

        public Bindable<FileInfo?> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public LocalisableString Text
        {
            get => Component.PlaceholderText;
            set => Component.PlaceholderText = value;
        }

        public CompositeDrawable TabbableContentContainer
        {
            set => Component.TabbableContentContainer = value;
        }

        public LabelledFileChooser(params string[] handledExtensions)
            : base(false)
        {
            this.handledExtensions = handledExtensions;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            game.RegisterImportHandler(this);
            Current.BindValueChanged(onFileSelected);
        }

        private void onFileSelected(ValueChangedEvent<FileInfo?> file)
        {
            if (file.NewValue != null)
                this.HidePopover();

            initialChooserPath = file.NewValue?.DirectoryName;
        }

        Task ICanAcceptFiles.Import(params string[] paths)
        {
            Schedule(() => Current.Value = new FileInfo(paths.First()));
            return Task.CompletedTask;
        }

        Task ICanAcceptFiles.Import(ImportTask[] tasks, ImportParameters parameters) => throw new NotImplementedException();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (game.IsNotNull())
                game.UnregisterImportHandler(this);
        }

        protected override LabelledTextBoxWithPopover.PopoverTextBox CreateComponent() => new LabelledTextBoxWithPopover.PopoverTextBox
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.X,
            CornerRadius = CORNER_RADIUS,
            OnFocused = this.ShowPopover,
        };

        public Popover GetPopover() => new FileChooserPopover(handledExtensions, Current, initialChooserPath);

        private partial class FileChooserPopover : OsuPopover
        {
            protected override string PopInSampleName => "UI/overlay-big-pop-in";
            protected override string PopOutSampleName => "UI/overlay-big-pop-out";

            public FileChooserPopover(string[] handledExtensions, Bindable<FileInfo?> currentFile, string? chooserPath)
            {
                Child = new Container
                {
                    Size = new Vector2(600, 400),
                    Child = new OsuFileSelector(chooserPath, handledExtensions)
                    {
                        RelativeSizeAxes = Axes.Both,
                        CurrentFile = { BindTarget = currentFile }
                    },
                };
            }
        }
    }
}
