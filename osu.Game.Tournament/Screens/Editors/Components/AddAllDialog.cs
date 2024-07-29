// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class AddAllDialog : DangerousActionDialog
    {
        public AddAllDialog(Action action)
        {
            HeaderText = "You are going to add all countries as individual teams, which is not recommanded in most cases.\nAre you sure to do that?";
            Icon = FontAwesome.Solid.Question;
            DangerousAction = action;
        }
    }
}
