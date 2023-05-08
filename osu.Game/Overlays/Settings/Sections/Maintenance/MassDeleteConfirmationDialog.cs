﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class MassDeleteConfirmationDialog : DangerousActionDialog
    {
        public MassDeleteConfirmationDialog(Action deleteAction)
        {
            BodyText = "Everything?";
            DangerousAction = deleteAction;
        }
    }
}
