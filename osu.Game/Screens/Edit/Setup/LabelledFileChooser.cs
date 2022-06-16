// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

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
using osu.Game.Database;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    /// <summary>
    /// A labelled drawable displaying file chooser on click, with placeholder text support.
    /// todo: this should probably not use PopoverTextBox just to display placeholder text, but is the best way for now.
    /// </summary>
    internal class LabelledFileChooser : LabelledDrawable<LabelledTextBoxWithPopover.PopoverTextBox>, IHasCurrentValue<FileInfo?>, ICanAcceptFiles, IHasPopover
    {
        private readonly string[] handledExtensions;

        public IEnumerable<string> HandledExtensions => handledExtensions;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        private string? chooserPath;

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

            chooserPath = file.NewValue?.DirectoryName;
        }

        Task ICanAcceptFiles.Import(params string[] paths)
        {
            Schedule(() => Current.Value = new FileInfo(paths.First()));
            return Task.CompletedTask;
        }

        Task ICanAcceptFiles.Import(params ImportTask[] tasks) => throw new NotImplementedException();

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

        public Popover GetPopover() => new FileChooserPopover(handledExtensions, Current, chooserPath);

        private class FileChooserPopover : OsuPopover
        {
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
