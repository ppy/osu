//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Game.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.Select
{
    class CarouselContainer : ScrollContainer
    {
        private Container<Panel> scrollableContent;
        private List<BeatmapGroup> groups = new List<BeatmapGroup>();
        private List<Panel> panels = new List<Panel>();

        public BeatmapGroup SelectedGroup { get; private set; }
        public BeatmapPanel SelectedPanel { get; private set; }
        
        private List<float> yPositions = new List<float>();
        
        public CarouselContainer()
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
            panels.Add(group.Header);
            foreach (BeatmapPanel panel in group.BeatmapPanels)
                panels.Add(panel);

            computeYPositions();
        }

        private void movePanel(Panel panel, bool advance, ref float currentY)
        {
            yPositions.Add(currentY);
            panel.MoveToY(currentY, 750, EasingTypes.OutExpo);

            if (advance)
                currentY += panel.DrawHeight + 5;
        }

        /// <summary>
        /// Computes the target Y positions for every panel in the carousel.
        /// </summary>
        /// <returns>The Y position of the currently selected panel.</returns>
        private float computeYPositions()
        {
            yPositions.Clear();

            float currentY = DrawHeight / 2;
            float selectedY = currentY;

            foreach (BeatmapGroup group in groups)
            {
                movePanel(group.Header, true, ref currentY);

                if (group.State == BeatmapGroupState.Expanded)
                {
                    group.Header.MoveToX(-100, 500, EasingTypes.OutExpo);
                    var headerY = group.Header.Position.Y;

                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                    {
                        if (panel == SelectedPanel)
                            selectedY = currentY + panel.DrawHeight / 2 - DrawHeight / 2;

                        panel.MoveToX(-50, 500, EasingTypes.OutExpo);

                        //on first display we want to begin hidden under our group's header.
                        if (panel.Alpha == 0)
                            panel.MoveToY(headerY);

                        movePanel(panel, true, ref currentY);
                    }
                }
                else
                {
                    group.Header.MoveToX(0, 500, EasingTypes.OutExpo);

                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                    {
                        panel.MoveToX(0, 500, EasingTypes.OutExpo);
                        movePanel(panel, false, ref currentY);
                    }
                }
            }

            currentY += DrawHeight / 2;
            scrollableContent.Height = currentY;

            return selectedY;
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
                SelectedGroup.State = BeatmapGroupState.Collapsed;

            SelectedGroup = group;
            panel.State = PanelSelectedState.Selected;
            SelectedPanel = panel;

            float selectedY = computeYPositions();
            ScrollTo(selectedY);
        }

        private static float offsetX(Panel panel, float dist, float halfHeight)
        {
            // The radius of the circle the carousel moves on.
            const float CIRCLE_RADIUS = 4;
            double discriminant = Math.Max(0, CIRCLE_RADIUS * CIRCLE_RADIUS - dist * dist);
            float x = (CIRCLE_RADIUS - (float)Math.Sqrt(discriminant)) * halfHeight;

            return 125 + x;
        }

        private void addPanel(int index)
        {
            Panel panel = panels[index];
            if (panel.Hidden)
                return;

            if (!scrollableContent.Contains(panel))
            {
                panel.Depth = index + (panel is BeatmapSetHeader ? panels.Count : 0);
                scrollableContent.Add(panel);
            }
        }

        protected override void Update()
        {
            base.Update();

            float drawHeight = DrawHeight;
            scrollableContent.RemoveAll(delegate (Panel p)
            {
                float panelPosY = p.Position.Y;
                return panelPosY < Current - p.DrawHeight || panelPosY > Current + drawHeight || !IsVisible;
            });

            int firstIndex = yPositions.BinarySearch(Current - Panel.MAX_HEIGHT);
            if (firstIndex < 0) firstIndex = ~firstIndex;
            int lastIndex = yPositions.BinarySearch(Current + drawHeight);
            if (lastIndex < 0) lastIndex = ~lastIndex;

            for (int i = firstIndex; i < lastIndex; ++i)
                addPanel(i);

            float halfHeight = drawHeight / 2;
            foreach (Panel panel in scrollableContent.Children)
            {
                float panelDrawY = panel.Position.Y - Current + panel.DrawHeight / 2;
                float dist = Math.Abs(1f - panelDrawY / halfHeight);

                // Setting the origin position serves as an additive position on top of potential
                // local transformation we may want to apply (e.g. when a panel gets selected, we
                // may want to smoothly transform it leftwards.)
                panel.OriginPosition = new Vector2(-offsetX(panel, dist, halfHeight), 0);

                // We are applying a multiplicative alpha (which is internally done by nesting an
                // additional container and setting that container's alpha) such that we can
                // layer transformations on top, with a similar reasoning to the previous comment.
                panel.SetMultiplicativeAlpha(MathHelper.Clamp(1.75f - 1.5f * dist, 0, 1));
            }
        }
    }
}
