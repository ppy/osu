// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableTournamentRound : CompositeDrawable
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
                        Colour = TournamentGame.TEXT_COLOUR,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre
                    },
                    textName = new TournamentSpriteText
                    {
                        Font = OsuFont.Torus.With(weight: FontWeight.Bold),
                        Colour = TournamentGame.TEXT_COLOUR,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre
                    },
                }
            };

            name = round.Name.GetBoundCopy();
            name.BindValueChanged(n => textName.Text = ((losers ? "Losers " : "") + round.Name).ToUpper(), true);

            description = round.Description.GetBoundCopy();
            description.BindValueChanged(n => textDescription.Text = round.Description.Value?.ToUpper(), true);
        }
    }
}
