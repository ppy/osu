// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class DeleteRoundDialog : DeletionDialog
    {
        public DeleteRoundDialog(TournamentRound round, Action action)
        {
            HeaderText = round.Name.Value.Length > 0
                ? DialogStrings.DeleteRoundPrompt(round.Name.Value)
                : DialogStrings.DeleteUnnamedRoundPrompt;
            DangerousAction = action;
        }
    }
}
