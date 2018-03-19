using Symcol.Rulesets.Core.Multiplayer.Networking;
using Symcol.Rulesets.Core.Multiplayer.Screens;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Shape.Multi
{
    public class ShapeLobbyScreen : RulesetLobbyScreen
    {
        public override string RulesetName => "shape";

        public override RulesetMatchScreen MatchScreen => new ShapeMatchScreen(RulesetNetworkingClientHandler);
    }
}
