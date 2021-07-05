// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class FileSelector : DirectorySelector
    {
        private readonly string[] validFileExtensions;

        [Cached]
        public readonly Bindable<FileInfo> CurrentFile = new Bindable<FileInfo>();

        public FileSelector(string initialPath = null, string[] validFileExtensions = null)
            : base(initialPath)
        {
            this.validFileExtensions = validFileExtensions ?? Array.Empty<string>();
        }

        protected override IEnumerable<DisplayPiece> GetEntriesForPath(DirectoryInfo path)
        {
            foreach (var dir in base.GetEntriesForPath(path))
                yield return dir;

            IEnumerable<FileInfo> files = path.GetFiles();

            if (validFileExtensions.Length > 0)
                files = files.Where(f => validFileExtensions.Contains(f.Extension));

            foreach (var file in files.OrderBy(d => d.Name))
            {
                if ((file.Attributes & FileAttributes.Hidden) == 0)
                    yield return new FilePiece(file);
            }
        }

        protected class FilePiece : DisplayPiece
        {
            private readonly FileInfo file;

            [Resolved]
            private Bindable<FileInfo> currentFile { get; set; }

            public FilePiece(FileInfo file)
            {
                this.file = file;
            }

            protected override bool OnClick(ClickEvent e)
            {
                currentFile.Value = file;
                return true;
            }

            protected override string FallbackName => file.Name;

            protected override IconUsage? Icon
            {
                get
                {
                    switch (file.Extension)
                    {
                        case ".ogg":
                        case ".mp3":
                        case ".wav":
                            return FontAwesome.Regular.FileAudio;

                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                            return FontAwesome.Regular.FileImage;

                        case ".mp4":
                        case ".avi":
                        case ".mov":
                        case ".flv":
                            return FontAwesome.Regular.FileVideo;

                        default:
                            return FontAwesome.Regular.File;
                    }
                }
            }
        }
    }
}
