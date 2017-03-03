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
using osu.Framework.Lists;
using osu.Game.Beatmaps.Drawables;
using osu.Framework.Timing;
using osu.Framework.Input;
using OpenTK.Input;
using System.Collections;
using osu.Framework.MathUtils;

namespace osu.Game.Screens.Select
{
    class CarouselContainer : ScrollContainer, IEnumerable<BeatmapGroup>
    {
        private Container<Panel> scrollableContent;
        private List<BeatmapGroup> groups = new List<BeatmapGroup>();

        public BeatmapGroup SelectedGroup { get; private set; }
        public BeatmapPanel SelectedPanel { get; private set; }

        private List<float> yPositions = new List<float>();
        private CarouselLifetimeList<Panel> lifetime;

        public CarouselContainer()
        {
            DistanceDecayJump = 0.01;

            Add(scrollableContent = new Container<Panel>(lifetime = new CarouselLifetimeList<Panel>(DepthComparer))
            {
                RelativeSizeAxes = Axes.X,
            });
        }

        internal class CarouselLifetimeList<T> : LifetimeList<Panel>
        {
            public CarouselLifetimeList(IComparer<Panel> comparer)
                : base(comparer)
            {
            }

            public int StartIndex;
            public int EndIndex;

            public override bool Update(FrameTimeInfo time)
            {
                bool anyAliveChanged = false;

                //check existing items to make sure they haven't died.
                foreach (var item in AliveItems.ToArray())
                {
                    item.UpdateTime(time);
                    if (!item.IsAlive)
                    {
                        //todo: make this more efficient
                        int i = IndexOf(item);
                        anyAliveChanged |= CheckItem(item, ref i);
                    }
                }

                //handle custom range
                for (int i = StartIndex; i < EndIndex; i++)
                {
                    var item = this[i];
                    item.UpdateTime(time);
                    anyAliveChanged |= CheckItem(item, ref i);
                }

                return anyAliveChanged;
            }
        }

        public void AddGroup(BeatmapGroup group)
        {
            group.State = BeatmapGroupState.Collapsed;
            groups.Add(group);

            group.Header.Depth = -scrollableContent.Children.Count();
            scrollableContent.Add(group.Header);

            foreach (BeatmapPanel panel in group.BeatmapPanels)
            {
                panel.Depth = -scrollableContent.Children.Count();
                scrollableContent.Add(panel);
            }

            computeYPositions();
        }

        public void RemoveGroup(BeatmapGroup group)
        {
            groups.Remove(group);
            scrollableContent.Remove(group.Header);
            scrollableContent.Remove(group.BeatmapPanels);

            computeYPositions();
        }

        private void movePanel(Panel panel, bool advance, bool animated, ref float currentY)
        {
            yPositions.Add(currentY);
            panel.MoveToY(currentY, animated && (panel.IsOnScreen || panel.State != PanelSelectedState.Hidden) ? 750 : 0, EasingTypes.OutExpo);

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
                    SelectGroup(group, panel, animated);
                    return;
                }
            }
        }

        public void SelectGroup(BeatmapGroup group, BeatmapPanel panel, bool animated = true)
        {
            if (SelectedGroup != null && SelectedGroup != group && SelectedGroup.State != BeatmapGroupState.Hidden)
                SelectedGroup.State = BeatmapGroupState.Collapsed;

            group.State = BeatmapGroupState.Expanded;
            SelectedGroup = group;
            panel.State = PanelSelectedState.Selected;
            SelectedPanel = panel;

            float selectedY = computeYPositions(animated);
            ScrollTo(selectedY, animated);
        }

        public void Sort(FilterControl.SortMode mode)
        {
            switch (mode)
            {
                case FilterControl.SortMode.Artist:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Artist, y.BeatmapSet.Metadata.Artist));
                    break;
                case FilterControl.SortMode.Title:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Title, y.BeatmapSet.Metadata.Title));
                    break;
                case FilterControl.SortMode.Author:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Author, y.BeatmapSet.Metadata.Author));
                    break;
                case FilterControl.SortMode.Difficulty:
                    groups.Sort((x, y) =>
                    {
                        float xAverage = 0, yAverage = 0;
                        int counter = 0;
                        foreach (BeatmapInfo set in x.BeatmapSet.Beatmaps)
                        {
                            xAverage += set.StarDifficulty;
                            counter++;
                        }
                        xAverage /= counter;
                        counter = 0;
                        foreach (BeatmapInfo set in y.BeatmapSet.Beatmaps)
                        {
                            yAverage += set.StarDifficulty;
                            counter++;
                        }
                        yAverage /= counter;
                        if (xAverage > yAverage)
                            return 1;
                        else
                            return -1;
                    });
                    break;
                default:
                    throw new NotImplementedException();
            }
            scrollableContent.Clear(false);
            lifetime.Clear();
            foreach (BeatmapGroup group in groups)
            {
                group.Header.Depth = -scrollableContent.Children.Count();
                scrollableContent.Add(group.Header);

                foreach (BeatmapPanel panel in group.BeatmapPanels)
                {
                    panel.Depth = -scrollableContent.Children.Count();
                    scrollableContent.Add(panel);
                }
            }

        }

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

            // Determine which items stopped being on screen for future removal from the lifetimelist.
            float drawHeight = DrawHeight;
            float halfHeight = drawHeight / 2;

            foreach (Panel p in lifetime.AliveItems)
            {
                float panelPosY = p.Position.Y;
                p.IsOnScreen = panelPosY >= Current - p.DrawHeight && panelPosY <= Current + drawHeight;
                updatePanel(p, halfHeight);
            }

            // Determine range of indices for items that are now definitely on screen to be added
            // to the lifetimelist in the future.
            int firstIndex = yPositions.BinarySearch(Current - Panel.MAX_HEIGHT);
            if (firstIndex < 0) firstIndex = ~firstIndex;
            int lastIndex = yPositions.BinarySearch(Current + drawHeight);
            if (lastIndex < 0) lastIndex = ~lastIndex;

            lifetime.StartIndex = firstIndex;
            lifetime.EndIndex = lastIndex;

            for (int i = firstIndex; i < lastIndex; ++i)
            {
                Panel p = lifetime[i];
                if (p.State != PanelSelectedState.Hidden)
                    p.IsOnScreen = true; //we don't want to update the on-screen state of hidden pannels as they have incorrect (stacked) y values.
                updatePanel(p, halfHeight);
            }
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
                    SelectGroup(SelectedGroup, SelectedGroup.BeatmapPanels[i]);
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
            if (groups.Count < 1)
                return;
            BeatmapGroup group = groups[RNG.Next(groups.Count)];
            BeatmapPanel panel = group?.BeatmapPanels.First();
            if (panel == null)
                return;
            SelectGroup(group, panel);
        }

        public IEnumerator<BeatmapGroup> GetEnumerator() => groups.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
