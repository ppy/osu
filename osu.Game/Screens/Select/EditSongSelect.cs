// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;

namespace osu.Game.Screens.Select
{
    public class EditSongSelect : SongSelect
    {
        protected override bool ShowFooter => false;

        protected override bool OnStart()
        {
            this.Exit();
            return true;
        }
    }
}
