// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableMatchPairing : CompositeDrawable
    {
        private readonly MatchPairing pairing;
        private readonly FillFlowContainer<DrawableMatchTeam> flow;

        public DrawableMatchPairing(MatchPairing pairing)
        {
            this.pairing = pairing;

            AutoSizeAxes = Axes.Both;

            Margin = new MarginPadding(5);

            InternalChild = flow = new FillFlowContainer<DrawableMatchTeam>
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
            };

            pairing.Team1.BindValueChanged(_ => updateTeams());
            pairing.Team2.BindValueChanged(_ => updateTeams());

            updateTeams();
        }

        private void updateTeams()
        {
            // todo: teams may need to be bindable for transitions at a later point.

            flow.Children = new[]
            {
                new DrawableMatchTeam(pairing.Team1, pairing),
                new DrawableMatchTeam(pairing.Team2, pairing)
            };
        }
    }
}
