// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Tournament.IPC;

namespace osu.Game.Tournament.Screens
{
    public class SetupScreen : TournamentScreen
    {
        [Resolved]
        private MatchIPCInfo ipc { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new SpriteText
            {
                Text = (ipc as FileBasedIPC)?.Storage.GetFullPath(string.Empty)
            });
        }
    }
}
