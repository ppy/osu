// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class TournamentClearAllDialog : DangerousActionDialog
    {
        public TournamentClearAllDialog(IList storage)
        {
            HeaderText = @"Confirm clear all?";
            Icon = FontAwesome.Solid.Trash;
            DangerousAction = storage.Clear;
        }
    }
}
