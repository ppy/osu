// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : Container
    {
        private readonly FillFlowContainer<PlaylistItem> items;

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                List<PlaylistItem> newItems = new List<PlaylistItem>();

                int i = 0;
                foreach (var item in value)
                {
                    newItems.Add(new PlaylistItem(item, i++)
                    {
                        OnSelect = (b, idx) => OnSelect?.Invoke(b, idx)
                    });
                }

                items.Children = newItems;
            }
        }

        public Action<BeatmapSetInfo, int> OnSelect;

        private BeatmapSetInfo current;
        public BeatmapSetInfo Current
        {
            get { return current; }
            set
            {
                if (value == current) return;
                current = value;

                foreach (PlaylistItem s in items.Children)
                    s.Current = s.RepresentedSet.ID == value.ID;
            }
        }

        public PlaylistList()
        {
            Children = new Drawable[]
            {
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        items = new FillFlowContainer<PlaylistItem>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                    },
                },
            };
        }
    }
}