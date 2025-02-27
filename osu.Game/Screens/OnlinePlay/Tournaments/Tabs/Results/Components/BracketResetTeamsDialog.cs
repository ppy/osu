// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Results.Components
{
    public partial class BracketResetTeamsDialog : DangerousActionDialog
    {
        public BracketResetTeamsDialog(Action action)
        {
            HeaderText = @"Reset teams?";
            Icon = FontAwesome.Solid.Undo;
            DangerousAction = action;
        }
    }
}
