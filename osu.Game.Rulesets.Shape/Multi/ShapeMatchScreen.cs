using Symcol.Rulesets.Core.Multiplayer.Networking;
using Symcol.Rulesets.Core.Multiplayer.Screens;

namespace osu.Game.Rulesets.Shape.Multi
{
    public class ShapeMatchScreen : RulesetMatchScreen
    {
        public readonly RulesetNetworkingClientHandler ShapeNetworkingClientHandler;

        public ShapeMatchScreen(RulesetNetworkingClientHandler shapeNetworkingClientHandler) : base(shapeNetworkingClientHandler)
        {
            ShapeNetworkingClientHandler = shapeNetworkingClientHandler;
        }
    }
}
