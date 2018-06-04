// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Overlays.Direct
{
    public class DownloadButton : OsuClickableContainer
    {
        private readonly SpriteIcon icon;

        public DownloadButton(BeatmapSetInfo set, bool noVideo = false)
        {
            BeatmapSetDownloader downloader;
            Children = new Drawable[]
            {
                downloader = new BeatmapSetDownloader(set, noVideo),
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(30),
                    Icon = FontAwesome.fa_osu_chevron_down_o,
                },
            };

            Action = downloader.Download;

            downloader.Downloaded.ValueChanged += e =>
            {
                if (e)
                    this.FadeOut(200);
                else
                    this.FadeIn(200);
            };
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            icon.ScaleTo(0.9f, 1000, Easing.Out);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            icon.ScaleTo(1f, 500, Easing.OutElastic);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnHover(InputState state)
        {
            icon.ScaleTo(1.1f, 500, Easing.OutElastic);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            icon.ScaleTo(1f, 500, Easing.OutElastic);
        }
    }
}
