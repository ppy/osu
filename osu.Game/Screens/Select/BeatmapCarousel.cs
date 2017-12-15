// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Framework.MathUtils;
using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Threading;
using osu.Framework.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;

namespace osu.Game.Screens.Select
{
    public class BeatmapCarousel : OsuScrollContainer
    {
        public BeatmapInfo SelectedBeatmap => selectedPanel?.Beatmap;

        public override bool HandleInput => AllowSelection;

        public Action BeatmapsChanged;

        public IEnumerable<BeatmapSetInfo> Beatmaps
        {
            get { return groups.Select(g => g.BeatmapSet); }
            set
            {
                scrollableContent.Clear(false);
                panels.Clear();
                groups.Clear();

                List<BeatmapGroup> newGroups = null;

                Task.Run(() =>
                {
                    newGroups = value.Select(createGroup).Where(g => g != null).ToList();
                    criteria.Filter(newGroups);
                }).ContinueWith(t =>
                {
                    Schedule(() =>
                    {
                        foreach (var g in newGroups)
                            addGroup(g);

                        computeYPositions();
                        BeatmapsChanged?.Invoke();
                    });
                });
            }
        }

        private readonly List<float> yPositions = new List<float>();

        /// <summary>
        /// Required for now unfortunately.
        /// </summary>
        private BeatmapManager manager;

        private readonly Container<Panel> scrollableContent;

        private readonly List<BeatmapGroup> groups = new List<BeatmapGroup>();

        private Bindable<SelectionRandomType> randomType;
        private readonly List<BeatmapGroup> seenGroups = new List<BeatmapGroup>();

        private readonly List<Panel> panels = new List<Panel>();

        private readonly Stack<KeyValuePair<BeatmapGroup, BeatmapPanel>> randomSelectedBeatmaps = new Stack<KeyValuePair<BeatmapGroup, BeatmapPanel>>();

        private BeatmapGroup selectedGroup;
        private BeatmapPanel selectedPanel;

        public BeatmapCarousel()
        {
            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = scrollableContent = new Container<Panel>
                {
                    RelativeSizeAxes = Axes.X,
                }
            });
        }

        public void RemoveBeatmap(BeatmapSetInfo beatmapSet)
        {
            Schedule(() => removeGroup(groups.Find(b => b.BeatmapSet.ID == beatmapSet.ID)));
        }

        public void UpdateBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            // todo: this method should be smarter as to not recreate panels that haven't changed, etc.
            var oldGroup = groups.Find(b => b.BeatmapSet.ID == beatmapSet.ID);

            var newGroup = createGroup(beatmapSet);

            int index = groups.IndexOf(oldGroup);
            if (index >= 0)
                groups.RemoveAt(index);

            if (newGroup != null)
            {
                if (index >= 0)
                    groups.Insert(index, newGroup);
                else
                    addGroup(newGroup);
            }

            bool hadSelection = selectedGroup == oldGroup;

            if (hadSelection && newGroup == null)
                selectedGroup = null;

            Filter(null, false);

            //check if we can/need to maintain our current selection.
            if (hadSelection && newGroup != null)
            {
                var newSelection =
                    newGroup.BeatmapPanels.Find(p => p.Beatmap.ID == selectedPanel?.Beatmap.ID);

                if (newSelection == null && oldGroup != null && selectedPanel != null)
                    newSelection = newGroup.BeatmapPanels[Math.Min(newGroup.BeatmapPanels.Count - 1, oldGroup.BeatmapPanels.IndexOf(selectedPanel))];

                selectGroup(newGroup, newSelection);
            }
        }

        public void SelectBeatmap(BeatmapInfo beatmap, bool animated = true)
        {
            if (beatmap == null || beatmap.Hidden)
            {
                SelectNext();
                return;
            }

            if (beatmap == SelectedBeatmap) return;

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

        public Action<BeatmapInfo> SelectionChanged;

        public Action StartRequested;

        public Action<BeatmapSetInfo> DeleteRequested;

        public Action<BeatmapSetInfo> RestoreRequested;

        public Action<BeatmapInfo> EditRequested;

        public Action<BeatmapInfo> HideDifficultyRequested;

        private void selectNullBeatmap()
        {
            selectedGroup = null;
            selectedPanel = null;
            SelectionChanged?.Invoke(null);
        }

        /// <summary>
        /// Increment selection in the carousel in a chosen direction.
        /// </summary>
        /// <param name="direction">The direction to increment. Negative is backwards.</param>
        /// <param name="skipDifficulties">Whether to skip individual difficulties and only increment over full groups.</param>
        public void SelectNext(int direction = 1, bool skipDifficulties = true)
        {
            // todo: we may want to refactor and remove this as an optimisation in the future.
            if (groups.All(g => g.State == BeatmapGroupState.Hidden))
            {
                selectNullBeatmap();
                return;
            }

            int originalIndex = Math.Max(0, groups.IndexOf(selectedGroup));
            int currentIndex = originalIndex;

            // local function to increment the index in the required direction, wrapping over extremities.
            int incrementIndex() => currentIndex = (currentIndex + direction + groups.Count) % groups.Count;

            // in the case we are skipping difficulties, we want to increment the index once before starting to find out new target
            // (we don't care about the currently selected group).
            if (skipDifficulties)
                incrementIndex();

            do
            {
                var group = groups[currentIndex];

                if (group.State == BeatmapGroupState.Hidden) continue;

                // we are only interested in non-filtered panels.
                IEnumerable<BeatmapPanel> validPanels = group.BeatmapPanels.Where(p => !p.Filtered);

                // if we are considering difficulties, we need to do a few extrea steps.
                if (!skipDifficulties)
                {
                    // we want to reverse the panel order if we are searching backwards.
                    if (direction < 0)
                        validPanels = validPanels.Reverse();

                    // if we are currently on the selected panel, let's try to find a valid difficulty before leaving to the next group.
                    // the first valid difficulty is found by skipping to the selected panel and then one further.
                    if (currentIndex == originalIndex)
                        validPanels = validPanels.SkipWhile(p => p != selectedPanel).Skip(1);
                }

                var next = validPanels.FirstOrDefault();

                // at this point, we can perform the selection change if we have a valid new target, else continue to increment in the specified direction.
                if (next != null)
                {
                    selectGroup(group, next);
                    return;
                }
            } while (incrementIndex() != originalIndex);
        }

        private IEnumerable<BeatmapGroup> getVisibleGroups() => groups.Where(selectGroup => selectGroup.State != BeatmapGroupState.Hidden);

        public void SelectNextRandom()
        {
            if (groups.Count == 0)
                return;

            var visibleGroups = getVisibleGroups();
            if (!visibleGroups.Any())
                return;

            if (selectedGroup != null)
                randomSelectedBeatmaps.Push(new KeyValuePair<BeatmapGroup, BeatmapPanel>(selectedGroup, selectedGroup.SelectedPanel));

            BeatmapGroup group;

            if (randomType == SelectionRandomType.RandomPermutation)
            {
                var notSeenGroups = visibleGroups.Except(seenGroups);
                if (!notSeenGroups.Any())
                {
                    seenGroups.Clear();
                    notSeenGroups = visibleGroups;
                }

                group = notSeenGroups.ElementAt(RNG.Next(notSeenGroups.Count()));
                seenGroups.Add(group);
            }
            else
                group = visibleGroups.ElementAt(RNG.Next(visibleGroups.Count()));

            BeatmapPanel panel = group.BeatmapPanels[RNG.Next(group.BeatmapPanels.Count)];

            selectGroup(group, panel);
        }

        public void SelectPreviousRandom()
        {
            if (!randomSelectedBeatmaps.Any())
                return;

            var visibleGroups = getVisibleGroups();
            if (!visibleGroups.Any())
                return;

            while (randomSelectedBeatmaps.Any())
            {
                var beatmapCoordinates = randomSelectedBeatmaps.Pop();
                var group = beatmapCoordinates.Key;
                if (visibleGroups.Contains(group))
                {
                    selectGroup(group, beatmapCoordinates.Value);
                    break;
                }
            }
        }

        private FilterCriteria criteria = new FilterCriteria();

        private ScheduledDelegate filterTask;

        public bool AllowSelection = true;

        public void FlushPendingFilters()
        {
            if (filterTask?.Completed == false)
                Filter(null, false);
        }

        public void Filter(FilterCriteria newCriteria = null, bool debounce = true)
        {
            if (newCriteria != null)
                criteria = newCriteria;

            Action perform = delegate
            {
                filterTask = null;

                criteria.Filter(groups);

                var filtered = new List<BeatmapGroup>(groups);

                scrollableContent.Clear(false);
                panels.Clear();
                groups.Clear();

                foreach (var g in filtered)
                    addGroup(g);

                computeYPositions();

                selectedGroup?.UpdateState();

                if (selectedGroup == null || selectedGroup.State == BeatmapGroupState.Hidden)
                    SelectNext();
                else
                    selectGroup(selectedGroup, selectedPanel);
            };

            filterTask?.Cancel();
            filterTask = null;

            if (debounce)
                filterTask = Scheduler.AddDelayed(perform, 250);
            else
                perform();
        }

        public void ScrollToSelected(bool animated = true)
        {
            float selectedY = computeYPositions(animated);
            ScrollTo(selectedY, animated);
        }

        private BeatmapGroup createGroup(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet.Beatmaps.All(b => b.Hidden))
                return null;

            foreach (var b in beatmapSet.Beatmaps)
            {
                if (b.Metadata == null)
                    b.Metadata = beatmapSet.Metadata;
            }

            return new BeatmapGroup(beatmapSet, manager)
            {
                SelectionChanged = (g, p) => selectGroup(g, p),
                StartRequested = b => StartRequested?.Invoke(),
                DeleteRequested = b => DeleteRequested?.Invoke(b),
                RestoreHiddenRequested = s => RestoreRequested?.Invoke(s),
                EditRequested = b => EditRequested?.Invoke(b),
                HideDifficultyRequested = b => HideDifficultyRequested?.Invoke(b),
                State = BeatmapGroupState.Collapsed
            };
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapManager manager, OsuConfigManager config)
        {
            this.manager = manager;

            randomType = config.GetBindable<SelectionRandomType>(OsuSetting.SelectionRandomType);
        }

        private void addGroup(BeatmapGroup group)
        {
            // prevent duplicates by concurrent independent actions trying to add a group
            if (groups.Any(g => g.BeatmapSet.ID == group.BeatmapSet.ID))
                return;

            groups.Add(group);
            panels.Add(group.Header);
            panels.AddRange(group.BeatmapPanels);
        }

        private void removeGroup(BeatmapGroup group)
        {
            if (group == null)
                return;

            if (selectedGroup == group)
            {
                if (getVisibleGroups().Count() == 1)
                    selectNullBeatmap();
                else
                    SelectNext();
            }

            groups.Remove(group);
            panels.Remove(group.Header);
            foreach (var p in group.BeatmapPanels)
                panels.Remove(p);

            scrollableContent.Remove(group.Header);
            scrollableContent.RemoveRange(group.BeatmapPanels);

            computeYPositions();
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
                    group.Header.MoveToX(-100, 500, Easing.OutExpo);
                    var headerY = group.Header.Position.Y;

                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                    {
                        if (panel == selectedPanel)
                            selectedY = currentY + panel.DrawHeight / 2 - DrawHeight / 2;

                        panel.MoveToX(-50, 500, Easing.OutExpo);

                        bool isHidden = panel.State == PanelSelectedState.Hidden;

                        //on first display we want to begin hidden under our group's header.
                        if (isHidden || panel.Alpha == 0)
                            panel.MoveToY(headerY);

                        movePanel(panel, !isHidden, animated, ref currentY);
                    }
                }
                else
                {
                    group.Header.MoveToX(0, 500, Easing.OutExpo);

                    foreach (BeatmapPanel panel in group.BeatmapPanels)
                    {
                        panel.MoveToX(0, 500, Easing.OutExpo);
                        movePanel(panel, false, animated, ref currentY);
                    }
                }
            }

            currentY += DrawHeight / 2;
            scrollableContent.Height = currentY;

            return selectedY;
        }

        private void movePanel(Panel panel, bool advance, bool animated, ref float currentY)
        {
            yPositions.Add(currentY);
            panel.MoveToY(currentY, animated ? 750 : 0, Easing.OutExpo);

            if (advance)
                currentY += panel.DrawHeight + 5;
        }

        private void selectGroup(BeatmapGroup group, BeatmapPanel panel = null, bool animated = true)
        {
            try
            {
                if (panel == null || panel.Filtered == true)
                    panel = group.BeatmapPanels.First(p => !p.Filtered);

                if (selectedPanel == panel) return;

                Trace.Assert(group.BeatmapPanels.Contains(panel), @"Selected panel must be in provided group");

                if (selectedGroup != null && selectedGroup != group && selectedGroup.State != BeatmapGroupState.Hidden)
                    selectedGroup.State = BeatmapGroupState.Collapsed;

                group.State = BeatmapGroupState.Expanded;
                group.SelectedPanel = panel;

                panel.State = PanelSelectedState.Selected;

                if (selectedPanel == panel) return;

                selectedPanel = panel;
                selectedGroup = group;

                SelectionChanged?.Invoke(panel.Beatmap);
            }
            finally
            {
                ScrollToSelected(animated);
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

        protected override void Update()
        {
            base.Update();

            float drawHeight = DrawHeight;

            // Remove all panels that should no longer be on-screen
            scrollableContent.RemoveAll(delegate(Panel p)
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
            if (lastIndex < 0)
            {
                lastIndex = ~lastIndex;

                // Add the first panel of the last visible beatmap group to preload its data.
                if (lastIndex != 0 && panels[lastIndex - 1] is BeatmapSetHeader)
                    lastIndex++;
            }

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

                    switch (panel.LoadState)
                    {
                        case LoadState.NotLoaded:
                            LoadComponentAsync(panel);
                            break;
                        case LoadState.Loading:
                            break;
                        default:
                            scrollableContent.Add(panel);
                            break;
                    }
                }
            }

            // Update externally controlled state of currently visible panels
            // (e.g. x-offset and opacity).
            float halfHeight = drawHeight / 2;
            foreach (Panel p in scrollableContent.Children)
                updatePanel(p, halfHeight);
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
    }
}
