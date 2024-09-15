// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Components
{
    public partial class RevertBoardDialog : DangerousActionDialog
    {
        public RevertBoardDialog(Action action)
        {
            HeaderText = @"Warning: Revert";
            BodyText = @"This would fully reset the board to the initial state, including swaps and chats. Are you sure?";
            Icon = FontAwesome.Solid.Undo;
            DangerousAction = action;
        }
    }
}
