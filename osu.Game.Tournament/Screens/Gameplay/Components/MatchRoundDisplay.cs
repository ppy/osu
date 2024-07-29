// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class MatchRoundDisplay : TournamentSpriteTextWithBackground
    {
        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        public MatchRoundDisplay()
        {
            Background.Alpha = 0;
            Text.Colour = Color4.White;
            Text.Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 72);
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match) =>
            Text.Text = $":: {match.NewValue?.Round.Value?.Name.Value} ::" ?? "Unknown Round";
    }
}
