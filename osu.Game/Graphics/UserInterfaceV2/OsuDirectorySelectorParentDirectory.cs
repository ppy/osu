// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterfaceV2
{
    internal partial class OsuDirectorySelectorParentDirectory : OsuDirectorySelectorDirectory
    {
        protected override IconUsage? Icon => FontAwesome.Solid.Folder;

        public OsuDirectorySelectorParentDirectory(DirectoryInfo directory)
            : base(directory, "..")
        {
        }
    }
}
