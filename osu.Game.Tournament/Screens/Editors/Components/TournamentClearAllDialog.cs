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

    // // Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
    // // See the LICENCE file in the repository root for full licence text.
    //
    // using osu.Framework.Graphics.Containers;
    // using osu.Framework.Graphics.Sprites;
    // using osu.Game.Overlays.Dialog;
    // using osu.Game.Tournament.Screens.Ladder.Components;
    //
    // namespace osu.Game.Tournament.Screens.Editors.Components
    // {
    //     public partial class LadderResetTeamsDialog : DangerousActionDialog
    //     {
    //         public LadderResetTeamsDialog(Container<DrawableTournamentMatch> matchesContainer)
    //         {
    //             HeaderText = @"Confirm reset teams?";
    //             Icon = FontAwesome.Solid.Undo;
    //             DangerousAction = () =>
    //             {
    //                 foreach (var p in matchesContainer)
    //                     p.Match.Reset();
    //             };
    //         }
    //     }
    // }

    }
}
