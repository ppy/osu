// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public abstract partial class DrawableTournamentTeam : CompositeDrawable
    {
        public readonly TournamentTeam? Team;

        protected readonly Container Flag;
        protected readonly TournamentSpriteText AcronymText;

        [UsedImplicitly]
        private Bindable<string>? acronym;

        protected DrawableTournamentTeam(TournamentTeam? team)
        {
            Team = team;

            Flag = new DrawableTeamFlag(team);
            AcronymText = new TournamentSpriteText
            {
                Font = OsuFont.Torus.With(weight: FontWeight.Regular),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (Team == null)
                return;

            (acronym = Team.Acronym.GetBoundCopy()).BindValueChanged(_ => AcronymText.Text = Team?.Acronym.Value?.ToUpperInvariant() ?? string.Empty, true);
        }
    }
}
