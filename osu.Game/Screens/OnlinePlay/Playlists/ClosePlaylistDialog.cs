// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Localisation;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class ClosePlaylistDialog : DeletionDialog
    {
        public ClosePlaylistDialog(Room room, Action closeAction)
        {
            HeaderText = DialogStrings.ClosePlaylistHeaderText;
            BodyText = room.Name;
            DangerousAction = closeAction;
        }
    }
}
