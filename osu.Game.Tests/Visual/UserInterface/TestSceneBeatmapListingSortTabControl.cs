// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneBeatmapListingSortTabControl : OsuManualInputManagerTestScene
    {
        private readonly BeatmapListingSortTabControl control;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneBeatmapListingSortTabControl()
        {
            OsuSpriteText current;
            OsuSpriteText direction;

            Add(control = new BeatmapListingSortTabControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    current = new OsuSpriteText(),
                    direction = new OsuSpriteText()
                }
            });

            control.SortDirection.BindValueChanged(sortDirection => direction.Text = $"Sort direction: {sortDirection.NewValue}", true);
            control.Current.BindValueChanged(criteria => current.Text = $"Criteria: {criteria.NewValue}", true);
        }

        [Test]
        public void TestRankedSort()
        {
            criteriaShowsOnCategory(true, SortCriteria.Ranked, SearchCategory.Any);
            criteriaShowsOnCategory(true, SortCriteria.Ranked, SearchCategory.Leaderboard);
            criteriaShowsOnCategory(true, SortCriteria.Ranked, SearchCategory.Ranked);
            criteriaShowsOnCategory(true, SortCriteria.Ranked, SearchCategory.Qualified);
            criteriaShowsOnCategory(true, SortCriteria.Ranked, SearchCategory.Loved);
            criteriaShowsOnCategory(true, SortCriteria.Ranked, SearchCategory.Favourites);
            criteriaShowsOnCategory(false, SortCriteria.Ranked, SearchCategory.Pending);
            criteriaShowsOnCategory(false, SortCriteria.Ranked, SearchCategory.Wip);
            criteriaShowsOnCategory(false, SortCriteria.Ranked, SearchCategory.Graveyard);
            criteriaShowsOnCategory(true, SortCriteria.Ranked, SearchCategory.Mine);
        }

        [Test]
        public void TestUpdatedSort()
        {
            criteriaShowsOnCategory(true, SortCriteria.Updated, SearchCategory.Any);
            criteriaShowsOnCategory(false, SortCriteria.Updated, SearchCategory.Leaderboard);
            criteriaShowsOnCategory(false, SortCriteria.Updated, SearchCategory.Ranked);
            criteriaShowsOnCategory(false, SortCriteria.Updated, SearchCategory.Qualified);
            criteriaShowsOnCategory(false, SortCriteria.Updated, SearchCategory.Loved);
            criteriaShowsOnCategory(true, SortCriteria.Updated, SearchCategory.Favourites);
            criteriaShowsOnCategory(true, SortCriteria.Updated, SearchCategory.Pending);
            criteriaShowsOnCategory(true, SortCriteria.Updated, SearchCategory.Wip);
            criteriaShowsOnCategory(true, SortCriteria.Updated, SearchCategory.Graveyard);
            criteriaShowsOnCategory(true, SortCriteria.Updated, SearchCategory.Mine);
        }

        [Test]
        public void TestNominationsSort()
        {
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Any);
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Leaderboard);
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Ranked);
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Qualified);
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Loved);
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Favourites);
            criteriaShowsOnCategory(true, SortCriteria.Nominations, SearchCategory.Pending);
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Wip);
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Graveyard);
            criteriaShowsOnCategory(false, SortCriteria.Nominations, SearchCategory.Mine);
        }

        [Test]
        public void TestResetNoQuery()
        {
            resetUsesCriteriaOnCategory(SortCriteria.Ranked, SearchCategory.Any);
            resetUsesCriteriaOnCategory(SortCriteria.Ranked, SearchCategory.Leaderboard);
            resetUsesCriteriaOnCategory(SortCriteria.Ranked, SearchCategory.Ranked);
            resetUsesCriteriaOnCategory(SortCriteria.Ranked, SearchCategory.Qualified);
            resetUsesCriteriaOnCategory(SortCriteria.Ranked, SearchCategory.Loved);
            resetUsesCriteriaOnCategory(SortCriteria.Ranked, SearchCategory.Favourites);
            resetUsesCriteriaOnCategory(SortCriteria.Updated, SearchCategory.Pending);
            resetUsesCriteriaOnCategory(SortCriteria.Updated, SearchCategory.Wip);
            resetUsesCriteriaOnCategory(SortCriteria.Updated, SearchCategory.Graveyard);
            resetUsesCriteriaOnCategory(SortCriteria.Updated, SearchCategory.Mine);
        }

        [Test]
        public void TestSortDirectionOnCriteriaChange()
        {
            AddStep("set category to leaderboard", () => control.Reset(SearchCategory.Leaderboard, false));
            AddAssert("sort direction is descending", () => control.SortDirection.Value == SortDirection.Descending);

            AddStep("click ranked sort button", () =>
            {
                InputManager.MoveMouseTo(control.TabControl.ChildrenOfType<BeatmapListingSortTabControl.BeatmapTabButton>().Single(s => s.Active.Value));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("sort direction is ascending", () => control.SortDirection.Value == SortDirection.Ascending);

            AddStep("click first inactive sort button", () =>
            {
                InputManager.MoveMouseTo(control.TabControl.ChildrenOfType<BeatmapListingSortTabControl.BeatmapTabButton>().First(s => !s.Active.Value));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("sort direction is descending", () => control.SortDirection.Value == SortDirection.Descending);
        }

        private void criteriaShowsOnCategory(bool expected, SortCriteria criteria, SearchCategory category)
        {
            AddAssert($"{criteria.ToString().ToLowerInvariant()} {(expected ? "shown" : "not shown")} on {category.ToString().ToLowerInvariant()}", () =>
            {
                control.Reset(category, false);
                return control.ChildrenOfType<TabControl<SortCriteria>>().Single().Items.Contains(criteria) == expected;
            });
        }

        private void resetUsesCriteriaOnCategory(SortCriteria criteria, SearchCategory category)
        {
            AddAssert($"reset uses {criteria.ToString().ToLowerInvariant()} on {category.ToString().ToLowerInvariant()}", () =>
            {
                control.Reset(category, false);
                return control.Current.Value == criteria;
            });
        }
    }
}
