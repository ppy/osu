using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Collections.Interface;

namespace osu.Game.Screens.Share
{
    public class ShareBeatmapPiece : OsuRearrangeableListItem<BeatmapSetInfo>
    {
        private readonly WorkingBeatmap working;

        public Action<BeatmapSetInfo> RemoveItem;

        public ShareBeatmapPiece(BeatmapSetInfo item, WorkingBeatmap working)
            : base(item)
        {
            this.working = working;
        }

        protected override Drawable CreateContent()
        {
            return new BeatmapPiece(working);
        }

        protected override bool OnClick(ClickEvent e)
        {
            this.ScaleTo(0, 300, Easing.In).Expire();
            RemoveItem?.Invoke(Model);
            return base.OnClick(e);
        }
    }
}
