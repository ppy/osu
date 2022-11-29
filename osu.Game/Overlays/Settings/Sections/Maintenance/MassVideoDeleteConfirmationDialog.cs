// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class MassVideoDeleteConfirmationDialog : MassDeleteConfirmationDialog
    {
        public MassVideoDeleteConfirmationDialog(Action deleteAction)
            : base(deleteAction)
        {
            BodyText = "All beatmap videos? This cannot be undone!";
        }
    }
}
