
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;

namespace osu.Game.Online.Chat
{
    public class MessagePreprocessor: Component
    {
        private readonly IBindable<WorkingBeatmap> currentBeatmap = new Bindable<WorkingBeatmap>();

        public string PreProcess(string text)
        {
            if (!text.Contains("$"))
                return text;

            return Regex.Replace(text, @"\$[a-zA-Z0-9]+", delegate (Match match)
            {
                string cmd = match.Value.Substring(1, match.Value.Length - 1);

                switch (cmd)
                {
                    case "np":
                        BeatmapInfo beatmapInfo = currentBeatmap.Value.Beatmap.BeatmapInfo;
                        return $"[https://osu.ppy.sh/b/{beatmapInfo.OnlineBeatmapID} {beatmapInfo.ToString()}]";

                    default:
                        return match.Value;
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(Bindable<WorkingBeatmap> beatmap)
        {
            this.currentBeatmap.BindTo(beatmap);
        }
    }
}
