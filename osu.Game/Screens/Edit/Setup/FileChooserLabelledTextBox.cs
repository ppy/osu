// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    /// <summary>
    /// A labelled textbox which reveals an inline file chooser when clicked.
    /// </summary>
    internal class FileChooserLabelledTextBox : LabelledTextBox, ICanAcceptFiles
    {
        private readonly string[] handledExtensions;
        public IEnumerable<string> HandledExtensions => handledExtensions;

        /// <summary>
        /// The target container to display the file chooser in.
        /// </summary>
        public Container Target;

        private readonly Bindable<FileInfo> currentFile = new Bindable<FileInfo>();

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private SectionsContainer<SetupSection> sectionsContainer { get; set; }

        public FileChooserLabelledTextBox(params string[] handledExtensions)
        {
            this.handledExtensions = handledExtensions;
        }

        protected override OsuTextBox CreateTextBox() =>
            new FileChooserOsuTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                CornerRadius = CORNER_RADIUS,
                OnFocused = DisplayFileChooser
            };

        public void DisplayFileChooser()
        {
            FileSelector fileSelector;

            Target.Child = fileSelector = new FileSelector(currentFile.Value?.DirectoryName, handledExtensions)
            {
                RelativeSizeAxes = Axes.X,
                Height = 400,
                CurrentFile = { BindTarget = currentFile }
            };

            sectionsContainer.ScrollTo(fileSelector);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            game.RegisterImportHandler(this);
            currentFile.BindValueChanged(onFileSelected);
        }

        private void onFileSelected(ValueChangedEvent<FileInfo> file)
        {
            if (file.NewValue == null)
                return;

            Target.Clear();
            Current.Value = file.NewValue.FullName;
        }

        Task ICanAcceptFiles.Import(params string[] paths)
        {
            Schedule(() => currentFile.Value = new FileInfo(paths.First()));
            return Task.CompletedTask;
        }

        Task ICanAcceptFiles.Import(params ImportTask[] tasks) => throw new NotImplementedException();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            game.UnregisterImportHandler(this);
        }

        internal class FileChooserOsuTextBox : OsuTextBox
        {
            public Action OnFocused;

            protected override void OnFocus(FocusEvent e)
            {
                OnFocused?.Invoke();
                base.OnFocus(e);

                GetContainingInputManager().TriggerFocusContention(this);
            }
        }
    }
}
