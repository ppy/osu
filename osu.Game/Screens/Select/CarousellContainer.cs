//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
            DistanceDecayJump = 0.01;

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

        private static void addPanel(Panel panel, ref float currentY)
        {
            panel.Depth = -currentY;
            panel.MoveToY(currentY, 750, EasingTypes.OutExpo);
            currentY += panel.DrawHeight + 5;
        }

        private void computeYPositions()
        {
            float currentY = DrawHeight / 2;
            float selectedY = currentY;

            foreach (BeatmapGroup group in groups)
            {
                addPanel(group.Header, ref currentY);

                if (group.State == BeatmapGroupState.Expanded)
                {
                    group.Header.MoveToX(-100, 500, EasingTypes.OutExpo);

                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                    {
                        if (panel == SelectedPanel)
                            selectedY = currentY + 1.5f * panel.DrawHeight - DrawHeight / 2;

                        addPanel(panel, ref currentY);
                    }
                }
                else
                {
                    group.Header.MoveToX(0, 500, EasingTypes.OutExpo);
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
                {
                    SelectGroup(group, panel);
                    return;
                }
            }
        }

        public void SelectGroup(BeatmapGroup group, BeatmapPanel panel)
        {
            if (SelectedGroup != null && SelectedGroup != group)
            {
                SelectedGroup.State = BeatmapGroupState.Collapsed;
                foreach (BeatmapPanel p in group.BeatmapPanels)
                    p.MoveToY(group.Header.Position.Y);
            }

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

        private static float offsetX(Vector2 pos, Panel panel, float dist, float halfHeight)
        {
            // The radius of the circle the carousel moves on.
            const float CIRCLE_RADIUS = 4;
            double discriminant = Math.Max(0, CIRCLE_RADIUS * CIRCLE_RADIUS - dist * dist);
            float x = (CIRCLE_RADIUS - (float)Math.Sqrt(discriminant)) * halfHeight;

            return 125 + x;
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
                float dist = Math.Abs(1f - panelPos.Y / halfHeight);
                panel.OriginPosition = new Vector2(-offsetX(panelPos, panel, dist, halfHeight), 0);

                panel.Alpha = MathHelper.Clamp(1.75f - 1.5f * dist, 0, 1);
            }
        }
    }
}
