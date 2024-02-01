// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager which only shows notifications after a game update completes.
    /// </summary>
    public partial class GameUpdateManager : UpdateManager
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new GameVersionUpdater());
        }
    }
}
