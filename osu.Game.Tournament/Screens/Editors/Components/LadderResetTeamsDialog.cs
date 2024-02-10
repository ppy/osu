// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class LadderResetTeamsDialog : DangerousActionDialog
    {
        public LadderResetTeamsDialog(Action action)
        {
            HeaderText = @"Reset teams?";
            Icon = FontAwesome.Solid.Undo;
            DangerousAction = action;
        }
    }
}
