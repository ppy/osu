// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.Sprites
{
    public class OsuLinkSpriteText : OsuSpriteText
    {
        private readonly OsuHoverContainer content;

        private BeatmapSetOverlay beatmapSetOverlay;

        public override bool HandleInput => content.Action != null;

        protected override Container<Drawable> Content => content ?? (Container<Drawable>)this;

        protected override IEnumerable<Drawable> FlowingChildren => Children;

        private string url;

        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                if (value != null)
                {
                    url = value;
                    loadAction();
                }
            }
        }

        public OsuLinkSpriteText()
        {
            AddInternal(content = new OsuHoverContainer
            {
                AutoSizeAxes = Axes.Both,
            });
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapSetOverlay beatmapSetOverlay)
        {
            this.beatmapSetOverlay = beatmapSetOverlay;
        }

        private void loadAction()
        {
            if (Url == null || String.IsNullOrEmpty(Url))
                return;

            var url = Url;

            if (url.StartsWith("https://")) url = url.Substring(8);
            else if (url.StartsWith("http://")) url = url.Substring(7);
            else content.Action = () => Process.Start(Url);

            if (url.StartsWith("osu.ppy.sh/"))
            {
                url = url.Substring(11);
                if (url.StartsWith("s") || url.StartsWith("beatmapsets"))
                    content.Action = () => beatmapSetOverlay.ShowBeatmapSet(getIdFromUrl(url));
                else if (url.StartsWith("b") || url.StartsWith("beatmaps"))
                    content.Action = () => beatmapSetOverlay.ShowBeatmap(getIdFromUrl(url));
                // else if (url.StartsWith("d"))     Maybe later
            }
        }

        private int getIdFromUrl(string url)
        {
            var lastSlashIndex = url.LastIndexOf('/');
            if (lastSlashIndex == url.Length)
            {
                url = url.Substring(url.Length - 1);
                lastSlashIndex = url.LastIndexOf('/');
            }

            return int.Parse(url.Substring(lastSlashIndex + 1));
        }
    }
}
