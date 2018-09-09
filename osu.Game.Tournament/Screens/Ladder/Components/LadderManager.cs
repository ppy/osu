using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.States;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Components;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderManager : CompositeDrawable
    {
        public readonly LadderInfo Info;
        public readonly List<TournamentTeam> Teams;
        private readonly OsuContextMenuContainer content;

        public LadderManager(LadderInfo info, List<TournamentTeam> teams)
        {
            Info = info;
            Teams = teams;

            RelativeSizeAxes = Axes.Both;

            InternalChild = content = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both
            };

            foreach (var pairing in info.Pairings)
                addPairing(pairing);
        }

        protected void AddPairing(MatchPairing pairing)
        {
            Info.Pairings.Add(pairing);
            addPairing(pairing);
        }

        private void addPairing(MatchPairing pairing) => content.Add(new DrawableMatchPairing(pairing));

        protected override bool OnClick(InputState state)
        {
            AddPairing(new MatchPairing
            {
                Position = new Point((int)state.Mouse.Position.X, (int)state.Mouse.Position.Y)
            });

            return true;
        }
    }
}
