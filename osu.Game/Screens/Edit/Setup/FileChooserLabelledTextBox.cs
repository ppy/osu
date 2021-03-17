// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    /// <summary>
    /// A labelled textbox which reveals an inline file chooser when clicked.
    /// </summary>
    internal class FileChooserLabelledTextBox : LabelledTextBox
    {
        public Container Target;

        private readonly IBindable<FileInfo> currentFile = new Bindable<FileInfo>();

        [Resolved]
        private SectionsContainer<SetupSection> sectionsContainer { get; set; }

        public FileChooserLabelledTextBox()
        {
            currentFile.BindValueChanged(onFileSelected);
        }

        private void onFileSelected(ValueChangedEvent<FileInfo> file)
        {
            if (file.NewValue == null)
                return;

            Target.Clear();
            Current.Value = file.NewValue.FullName;
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

            Target.Child = fileSelector = new FileSelector(validFileExtensions: ResourcesSection.AudioExtensions)
            {
                RelativeSizeAxes = Axes.X,
                Height = 400,
                CurrentFile = { BindTarget = currentFile }
            };

            sectionsContainer.ScrollTo(fileSelector);
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
