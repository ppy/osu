// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Screens
{
    public partial class BracketResetTeamsDialog : DangerousActionDialog
    {
        private readonly Container<DrawableTournamentMatch> matchesContainer;

        public BracketResetTeamsDialog(Container<DrawableTournamentMatch> matchesContainer)
        {
            this.matchesContainer = matchesContainer;
            BodyText = @"";
            HeaderText = @"Confirm reset teams?";
            Icon = FontAwesome.Solid.Undo;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            DangerousAction = () =>
            {
                foreach (var p in matchesContainer)
                    p.Match.Reset();
            };
        }
    }
}
