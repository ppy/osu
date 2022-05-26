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
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Database;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    /// <summary>
    /// A labelled textbox which reveals an inline file chooser when clicked.
    /// </summary>
    internal class FileChooserLabelledTextBox : LabelledTextBoxWithPopover, ICanAcceptFiles
    {
        private readonly string[] handledExtensions;

        public IEnumerable<string> HandledExtensions => handledExtensions;

        private readonly Bindable<FileInfo> currentFile = new Bindable<FileInfo>();

        [Resolved]
        private OsuGameBase game { get; set; }

        public FileChooserLabelledTextBox(params string[] handledExtensions)
        {
            this.handledExtensions = handledExtensions;
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

            this.HidePopover();
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

        public override Popover GetPopover() => new FileChooserPopover(handledExtensions, currentFile);

        private class FileChooserPopover : OsuPopover
        {
            public FileChooserPopover(string[] handledExtensions, Bindable<FileInfo> currentFile)
            {
                Child = new Container
                {
                    Size = new Vector2(600, 400),
                    Child = new OsuFileSelector(currentFile.Value?.DirectoryName, handledExtensions)
                    {
                        RelativeSizeAxes = Axes.Both,
                        CurrentFile = { BindTarget = currentFile }
                    },
                };
            }
        }
    }
}
