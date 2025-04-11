// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Screens.OnlinePlay.Tournaments.Components;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Results.Components
{
    public partial class DrawableTournamentRound : CompositeDrawable
    {
        [UsedImplicitly]
        private readonly Bindable<string> name;

        [UsedImplicitly]
        private readonly Bindable<string> description;

        public DrawableTournamentRound(TournamentRound round, bool losers = false)
        {
            TournamentSpriteText textName;
            TournamentSpriteText textDescription;

            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    textDescription = new TournamentSpriteText
                    {
                        Colour = TournamentsRoomSubScreen.TEXT_COLOUR,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre
                    },
                    textName = new TournamentSpriteText
                    {
                        Font = OsuFont.Torus.With(weight: FontWeight.Bold),
                        Colour = TournamentsRoomSubScreen.TEXT_COLOUR,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre
                    },
                }
            };

            name = round.Name.GetBoundCopy();
            name.BindValueChanged(_ => textName.Text = ((losers ? "Losers " : "") + round.Name).ToUpperInvariant(), true);

            description = round.Description.GetBoundCopy();
            description.BindValueChanged(_ => textDescription.Text = round.Description.Value?.ToUpperInvariant() ?? string.Empty, true);
        }
    }
}
