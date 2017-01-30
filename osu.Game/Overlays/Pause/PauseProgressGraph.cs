using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Configuration;

namespace osu.Game.Overlays.Pause
{
    public class PauseProgressGraph : FlowContainer
    {
        private WorkingBeatmap current;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            current = osuGame.Beatmap.Value;
        }

        public PauseProgressGraph()
        {
            // TODO: Implement the pause progress graph
        }
    }
}
