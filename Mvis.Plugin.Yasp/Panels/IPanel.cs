using osu.Game.Beatmaps;

namespace Mvis.Plugin.Yasp.Panels
{
    public interface IPanel
    {
        public void Refresh(WorkingBeatmap beatmap);
    }
}
