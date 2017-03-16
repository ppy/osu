// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Game.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Drawables;
using osu.Framework.Input;
using OpenTK.Input;
using System.Collections;
using osu.Framework.MathUtils;
using System.Diagnostics;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    internal class CarouselContainer : ScrollContainer, IEnumerable<BeatmapGroup>
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
            groups.Add(group);

            panels.Add(group.Header);
            group.Header.UpdateClock(Clock);
            foreach (BeatmapPanel panel in group.BeatmapPanels)
            {
                panels.Add(panel);
                panel.UpdateClock(Clock);
            }

            computeYPositions();
        }

        public void RemoveGroup(BeatmapGroup group)
        {
            groups.Remove(group);
            panels.Remove(group.Header);
            foreach (var p in group.BeatmapPanels)
                panels.Remove(p);

            scrollableContent.Remove(group.Header);
            scrollableContent.Remove(group.BeatmapPanels);

            computeYPositions();
        }

        private void movePanel(Panel panel, bool advance, bool animated, ref float currentY)
        {
            yPositions.Add(currentY);
            panel.MoveToY(currentY, animated ? 750 : 0, EasingTypes.OutExpo);

            if (advance)
                currentY += panel.DrawHeight + 5;
        }

        /// <summary>
        /// Computes the target Y positions for every panel in the carousel.
        /// </summary>
        /// <returns>The Y position of the currently selected panel.</returns>
        private float computeYPositions(bool animated = true)
        {
            yPositions.Clear();

            float currentY = DrawHeight / 2;
            float selectedY = currentY;

            foreach (BeatmapGroup group in groups)
            {
                movePanel(group.Header, group.State != BeatmapGroupState.Hidden, animated, ref currentY);

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

                        movePanel(panel, true, animated, ref currentY);
                    }
                }
                else
                {
                    group.Header.MoveToX(0, 500, EasingTypes.OutExpo);

                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                    {
                        panel.MoveToX(0, 500, EasingTypes.OutExpo);
                        movePanel(panel, false, animated, ref currentY);
                    }
                }
            }

            currentY += DrawHeight / 2;
            scrollableContent.Height = currentY;

            return selectedY;
        }

        public void SelectBeatmap(BeatmapInfo beatmap, bool animated = true)
        {
            foreach (BeatmapGroup group in groups)
            {
                var panel = group.BeatmapPanels.FirstOrDefault(p => p.Beatmap.Equals(beatmap));
                if (panel != null)
                {
                    selectGroup(group, panel, animated);
                    return;
                }
            }
        }

        private void selectGroup(BeatmapGroup group, BeatmapPanel panel, bool animated = true)
        {
            Trace.Assert(group.BeatmapPanels.Contains(panel), @"Selected panel must be in provided group");

            if (SelectedGroup != null && SelectedGroup != group && SelectedGroup.State != BeatmapGroupState.Hidden)
                SelectedGroup.State = BeatmapGroupState.Collapsed;

            group.State = BeatmapGroupState.Expanded;
            SelectedGroup = group;
            panel.State = PanelSelectedState.Selected;
            SelectedPanel = panel;

            float selectedY = computeYPositions(animated);
            ScrollTo(selectedY, animated);
        }

        public void Sort(SortMode mode)
        {
            List<BeatmapGroup> sortedGroups = new List<BeatmapGroup>(groups);
            switch (mode)
            {
                case SortMode.Artist:
                    sortedGroups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Artist, y.BeatmapSet.Metadata.Artist, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Title:
                    sortedGroups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Title, y.BeatmapSet.Metadata.Title, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Author:
                    sortedGroups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Author, y.BeatmapSet.Metadata.Author, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Difficulty:
                    sortedGroups.Sort((x, y) => x.BeatmapSet.MaxStarDifficulty.CompareTo(y.BeatmapSet.MaxStarDifficulty));
                    break;
                default:
                    Sort(SortMode.Artist); // Temporary
                    break;
            }

            scrollableContent.Clear(false);
            panels.Clear();
            groups.Clear();

            foreach (BeatmapGroup group in sortedGroups)
                AddGroup(group);
        }

        /// <summary>
        /// Computes the x-offset of currently visible panels. Makes the carousel appear round.
        /// </summary>
        /// <param name="dist">
        /// Vertical distance from the center of the carousel container
        /// ranging from -1 to 1.
        /// </param>
        /// <param name="halfHeight">Half the height of the carousel container.</param>
        private static float offsetX(float dist, float halfHeight)
        {
            // The radius of the circle the carousel moves on.
            const float circle_radius = 3;
            double discriminant = Math.Max(0, circle_radius * circle_radius - dist * dist);
            float x = (circle_radius - (float)Math.Sqrt(discriminant)) * halfHeight;

            return 125 + x;
        }

        /// <summary>
        /// Update a panel's x position and multiplicative alpha based on its y position and
        /// the current scroll position.
        /// </summary>
        /// <param name="p">The panel to be updated.</param>
        /// <param name="halfHeight">Half the draw height of the carousel container.</param>
        private void updatePanel(Panel p, float halfHeight)
        {
            var height = p.IsPresent ? p.DrawHeight : 0;

            float panelDrawY = p.Position.Y - Current + height / 2;
            float dist = Math.Abs(1f - panelDrawY / halfHeight);

            // Setting the origin position serves as an additive position on top of potential
            // local transformation we may want to apply (e.g. when a panel gets selected, we
            // may want to smoothly transform it leftwards.)
            p.OriginPosition = new Vector2(-offsetX(dist, halfHeight), 0);

            // We are applying a multiplicative alpha (which is internally done by nesting an
            // additional container and setting that container's alpha) such that we can
            // layer transformations on top, with a similar reasoning to the previous comment.
            p.SetMultiplicativeAlpha(MathHelper.Clamp(1.75f - 1.5f * dist, 0, 1));
        }

        protected override void Update()
        {
            base.Update();

            float drawHeight = DrawHeight;

            // Remove all panels that should no longer be on-screen
            scrollableContent.RemoveAll(delegate (Panel p)
            {
                float panelPosY = p.Position.Y;
                bool remove = panelPosY < Current - p.DrawHeight || panelPosY > Current + drawHeight || !p.IsPresent;
                return remove;
            });

            // Find index range of all panels that should be on-screen
            Trace.Assert(panels.Count == yPositions.Count);

            int firstIndex = yPositions.BinarySearch(Current - Panel.MAX_HEIGHT);
            if (firstIndex < 0) firstIndex = ~firstIndex;
            int lastIndex = yPositions.BinarySearch(Current + drawHeight);
            if (lastIndex < 0) lastIndex = ~lastIndex;

            // Add those panels within the previously found index range that should be displayed.
            for (int i = firstIndex; i < lastIndex; ++i)
            {
                Panel panel = panels[i];
                if (panel.State == PanelSelectedState.Hidden)
                    continue;

                // Only add if we're not already part of the content.
                if (!scrollableContent.Contains(panel))
                {
                    // Makes sure headers are always _below_ panels,
                    // and depth flows downward.
                    panel.Depth = i + (panel is BeatmapSetHeader ? panels.Count : 0);
                    scrollableContent.Add(panel);
                }
            }

            // Update externally controlled state of currently visible panels
            // (e.g. x-offset and opacity).
            float halfHeight = drawHeight / 2;
            foreach (Panel p in scrollableContent.Children)
                updatePanel(p, halfHeight);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            int direction = 0;
            bool skipDifficulties = false;

            switch (args.Key)
            {
                case Key.Up:
                    direction = -1;
                    break;
                case Key.Down:
                    direction = 1;
                    break;
                case Key.Left:
                    direction = -1;
                    skipDifficulties = true;
                    break;
                case Key.Right:
                    direction = 1;
                    skipDifficulties = true;
                    break;
            }

            if (direction == 0)
                return base.OnKeyDown(state, args);

            SelectNext(direction, skipDifficulties);
            return true;
        }

        public void SelectNext(int direction = 1, bool skipDifficulties = true)
        {
            if (!skipDifficulties && SelectedGroup != null)
            {
                int i = SelectedGroup.BeatmapPanels.IndexOf(SelectedPanel) + direction;

                if (i >= 0 && i < SelectedGroup.BeatmapPanels.Count)
                {
                    //changing difficulty panel, not set.
                    selectGroup(SelectedGroup, SelectedGroup.BeatmapPanels[i]);
                    return;
                }
            }

            int startIndex = groups.IndexOf(SelectedGroup);
            int index = startIndex;

            do
            {
                index = (index + direction + groups.Count) % groups.Count;
                if (groups[index].State != BeatmapGroupState.Hidden)
                {
                    SelectBeatmap(groups[index].BeatmapPanels.First().Beatmap);
                    return;
                }
            } while (index != startIndex);
        }

        public void SelectRandom()
        {
            List<BeatmapGroup> visibleGroups = groups.Where(selectGroup => selectGroup.State != BeatmapGroupState.Hidden).ToList();
            if (visibleGroups.Count < 1)
                return;
            BeatmapGroup group = visibleGroups[RNG.Next(visibleGroups.Count)];
            BeatmapPanel panel = group?.BeatmapPanels.First();

            if (panel == null)
                return;

            selectGroup(group, panel);
        }

        public IEnumerator<BeatmapGroup> GetEnumerator() => groups.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
