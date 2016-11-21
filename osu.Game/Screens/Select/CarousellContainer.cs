//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transformations;
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
        public BeatmapPanel SelectedPanel { get; private set; }

        private Cached yPositions = new Cached();

        public CarousellContainer()
        {
            Add(scrollableContent = new Container<Panel>
            {
                RelativeSizeAxes = Axes.X,
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
            float currentY = DrawHeight / 2;
            float selectedY = currentY;

            foreach (BeatmapGroup group in groups)
            {
                group.Header.MoveToY(currentY, 500, EasingTypes.OutExpo);
                currentY += group.Header.DrawSize.Y + 5;

                if (group.State == BeatmapGroupState.Expanded)
                {
                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                    {
                        if (panel == SelectedPanel)
                            selectedY = currentY + panel.DrawHeight - DrawHeight / 2;

                        panel.MoveToY(currentY, 500, EasingTypes.OutExpo);
                        currentY += panel.DrawHeight + 5;
                    }
                }
            }

            currentY += DrawHeight / 2;
            scrollableContent.Height = currentY;

            ScrollTo(selectedY);
        }

        private void scrollToSelected()
        {
            if (SelectedPanel == null)
                return;
            ScrollTo(getScrollPos(SelectedPanel).Y - DrawHeight / 2);
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
            SelectedPanel = panel;

            yPositions.Invalidate();
        }

        protected override void UpdateLayout()
        {
            base.UpdateLayout();

            if (!yPositions.EnsureValid())
                yPositions.Refresh(computeYPositions);
        }

        private static float offsetX(Vector2 pos, Panel panel, float dist)
        {
            float result = 25 + dist * 0.1f;
            if (!(panel is BeatmapSetHeader) || panel.State != PanelSelectedState.Selected)
                result += 100;

            return result;
        }

        private Vector2 getScrollPos(Panel panel)
        {
            return panel.Position + panel.DrawSize;;
        }

        private Vector2 getDrawPos(Panel panel)
        {
            return panel.ToSpaceOfOtherDrawable(panel.DrawSize / 2.0f, this);
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
                Vector2 panelPos = getDrawPos(panel);

                float halfHeight = DrawSize.Y / 2;
                float dist = Math.Abs(panelPos.Y - halfHeight);
                panel.Position = new Vector2(offsetX(panelPos, panel, dist), panelPosLocal.Y);

                panel.Alpha = MathHelper.Clamp(1.75f - 1.5f * dist / halfHeight, 0, 1);
            }
        }
    }
}
