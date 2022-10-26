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
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinEditorMenuBackgroundChooser : Container, ICanAcceptFiles, IHasPopover
    {
        private readonly string[] handledExtensions = { ".jpg", ".jpeg", ".png" };

        private readonly SkinEditor skinEditor;

        private Bindable<FileInfo?> menuBackground;

        private MenuBackgroundChooserPopover? popover;

        public bool PopoverVisible => popover?.State.Value == Visibility.Visible;

        [Resolved(canBeNull: true)]
        private OsuGame? game { get; set; }

        public SkinEditorMenuBackgroundChooser(SkinEditor skinEditor)
        {
            this.skinEditor = skinEditor;

            menuBackground = new Bindable<FileInfo?>();
            menuBackground.BindValueChanged(onFileSelected);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        private void onFileSelected(ValueChangedEvent<FileInfo?> file)
        {
            if (file.NewValue == null)
                return;

            this.HidePopover();
            skinEditor.SetMenuBackground(file.NewValue);
        }

        public Task Import(params string[] paths)
        {
            Schedule(() =>
            {
                HoverClickSounds sounds = new HoverClickSounds();
                AddInternal(sounds);
                sounds.TriggerClick();
                sounds.Expire();
                menuBackground.Value = new FileInfo(paths.First());
            });

            return Task.CompletedTask;
        }

        public Task Import(params ImportTask[] tasks) => throw new NotImplementedException();

        public IEnumerable<string> HandledExtensions => handledExtensions;

        public Popover GetPopover()
        {
            popover = new MenuBackgroundChooserPopover(handledExtensions, menuBackground);
            popover.State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case Visibility.Visible:
                        game?.RegisterImportHandler(this);
                        break;

                    case Visibility.Hidden:
                        game?.UnregisterImportHandler(this);
                        break;
                }
            });

            return popover;
        }

        private class MenuBackgroundChooserPopover : OsuPopover
        {
            public MenuBackgroundChooserPopover(string[] handledExtensions, Bindable<FileInfo?> currentFile)
            {
                Child = new Container
                {
                    Size = new Vector2(600, 400),
                    Child = new OsuFileSelector(currentFile.Value?.DirectoryName, handledExtensions)
                    {
                        RelativeSizeAxes = Axes.Both,
                        CurrentFile = { BindTarget = currentFile },
                    },
                };
            }
        }
    }
}
