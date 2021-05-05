// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class MigrationSelectScreen : DirectorySelectScreen
    {
        [Resolved]
        private Storage storage { get; set; }

        protected override DirectoryInfo InitialPath => new DirectoryInfo(storage.GetFullPath(string.Empty)).Parent;

        protected override OsuSpriteText CreateHeader() => new OsuSpriteText
        {
            Text = "Please select a new location",
            Font = OsuFont.Default.With(size: 40)
        };

        protected override void OnSelection(DirectoryInfo directory)
        {
            var target = directory;

            try
            {
                if (target.GetDirectories().Length > 0 || target.GetFiles().Length > 0)
                    target = target.CreateSubdirectory("osu-lazer");
            }
            catch (Exception e)
            {
                Logger.Log($"Error during migration: {e.Message}", level: LogLevel.Error);
                return;
            }

            ValidForResume = false;
            BeginMigration(target);
        }

        protected virtual void BeginMigration(DirectoryInfo target) => this.Push(new MigrationRunScreen(target));
    }
}
