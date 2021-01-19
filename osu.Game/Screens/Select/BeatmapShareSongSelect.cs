using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Share;

namespace osu.Game.Screens.Select
{
    public class BeatmapShareSongSelect : SongSelect
    {
        private readonly BindableList<BeatmapSetInfo> selectedBeatmapSets = new BindableList<BeatmapSetInfo>();

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new ShareBeatmapDetailArea(selectedBeatmapSets, createNewPiece);

        private void createNewPiece()
        {
            if (selectedBeatmapSets.All(b =>
                b.OnlineBeatmapSetID != Beatmap.Value.BeatmapSetInfo.OnlineBeatmapSetID))
                selectedBeatmapSets.Add(Beatmap.Value.BeatmapSetInfo);
        }

        protected override bool OnStart()
        {
            this.Push(new WriteToFileScreen
            {
                SelectedBeatmapSets = { BindTarget = selectedBeatmapSets }
            });
            return true;
        }
    }
}
