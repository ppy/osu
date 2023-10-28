// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamSeed : TournamentSpriteTextWithBackground
    {
        private readonly TournamentTeam? team;

        private IBindable<string> seed = null!;
        private Bindable<bool> displaySeed = null!;

        public DrawableTeamSeed(TournamentTeam? team)
        {
            this.team = team;
        }

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (team == null)
                return;

            seed = team.Seed.GetBoundCopy();
            seed.BindValueChanged(s => Text.Text = s.NewValue, true);

            displaySeed = ladder.DisplayTeamSeeds.GetBoundCopy();
            displaySeed.BindValueChanged(v => Alpha = v.NewValue ? 1 : 0, true);
        }
    }
}
