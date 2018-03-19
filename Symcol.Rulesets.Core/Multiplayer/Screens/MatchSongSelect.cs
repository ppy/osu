using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using System;
using osu.Game.Screens;
using osu.Framework.Screens;
using Symcol.Rulesets.Core.Multiplayer.Networking;

namespace Symcol.Rulesets.Core.Multiplayer.Screens
{
    public class MatchSongSelect : SongSelect
    {
        public WorkingBeatmap SelectedMap;

        private bool exiting;

        protected override BackgroundScreen CreateBackground() => null;

        public Action Action;

        public readonly RulesetNetworkingClientHandler RulesetNetworkingClientHandler;

        public MatchSongSelect(RulesetNetworkingClientHandler rulesetNetworkingClientHandler)
        {
            RulesetNetworkingClientHandler = rulesetNetworkingClientHandler;
        }

        protected override void OnEntering(Screen last)
        {
            Add(RulesetNetworkingClientHandler);
            base.OnEntering(last);
        }

        protected override bool OnSelectionFinalised()
        {
            if (!exiting)
            {
                RulesetNetworkingClientHandler.OnMapChange?.Invoke(null);
                SelectedMap = Beatmap.Value;
                Action();
                exiting = true;
                Remove(RulesetNetworkingClientHandler);
                Exit();
            }
            return true;
        }
    }
}
