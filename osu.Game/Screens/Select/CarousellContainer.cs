//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Beatmaps.Drawable;
using osu.Game.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Select
{
    class CarousellContainer : ScrollContainer
    {
        private Container<Panel> scrollableContent;
        private List<BeatmapGroup> groups = new List<BeatmapGroup>();
        public BeatmapGroup SelectedGroup { get; private set; }

        private Cached yPositions = new Cached();

        public CarousellContainer()
        {
            Add(scrollableContent = new Container<Panel>
            {
                Padding = new MarginPadding { Left = 25, Top = 25, Bottom = 25 },
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            });
        }

        public void AddGroup(BeatmapGroup group)
        {
            group.State = BeatmapGroupState.Collapsed;

            groups.Add(group);

            yPositions.Invalidate();
        }

        private void computeYPositions()
        {
            float currentY = 0;
            foreach (BeatmapGroup group in groups)
            {
                group.Header.Position = new Vector2(group.Header.Position.X, currentY);
                currentY += group.Header.DrawSize.Y + 5;

                if (group.State == BeatmapGroupState.Expanded)
                {
                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                    {
                        panel.Position = new Vector2(panel.Position.X, currentY);
                        currentY += panel.DrawSize.Y + 5;
                    }
                }
            }
        }

        public void SelectBeatmap(BeatmapInfo beatmap)
        {
            foreach (BeatmapGroup group in groups)
            {
                var panel = group.BeatmapPanels.FirstOrDefault(p => p.Beatmap.Equals(beatmap));
                if (panel != null)
                    SelectGroup(group, panel);
            }
        }

        public void SelectGroup(BeatmapGroup group, BeatmapPanel panel)
        {
            if (SelectedGroup != null && SelectedGroup != group)
                SelectedGroup.State = BeatmapGroupState.Collapsed;

            SelectedGroup = group;
            panel.State = PanelSelectedState.Selected;

            yPositions.Invalidate();
        }

        protected override void UpdateLayout()
        {
            base.UpdateLayout();

            if (!yPositions.EnsureValid())
                yPositions.Refresh(computeYPositions);
        }

        protected override void Update()
        {
            base.Update();

            scrollableContent.Clear(false);

            foreach (BeatmapGroup group in groups)
            {
                scrollableContent.Add(group.Header);

                if (group.State == BeatmapGroupState.Expanded)
                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                        scrollableContent.Add(panel);
            }

            foreach (Panel panel in scrollableContent.Children)
            {
                if (panel.Position.Y < 0)
                    continue;

                Vector2 panelPosLocal = panel.Position;
                Vector2 panelPos = panel.ToSpaceOfOtherDrawable(panel.DrawSize / 2.0f, this);

                float halfHeight = DrawSize.Y / 2;
                float dist = Math.Abs(panelPos.Y - halfHeight);
                panel.Position = new Vector2(
                    (panel is BeatmapSetHeader && panel.State == PanelSelectedState.Selected ? 0 : 100) + dist * 0.1f, panelPosLocal.Y);

                panel.Alpha = MathHelper.Clamp(1.75f - 1.5f * dist / halfHeight, 0, 1);
            }
        }
    }
}
