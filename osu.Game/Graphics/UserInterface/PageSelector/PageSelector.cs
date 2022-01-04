// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public class PageSelector : CompositeDrawable
    {
        public readonly BindableInt CurrentPage = new BindableInt { MinValue = 0, };

        public readonly BindableInt AvailablePages = new BindableInt(1) { MinValue = 1, };

        private readonly FillFlowContainer itemsFlow;

        private readonly PageSelectorPrevNextButton previousPageButton;
        private readonly PageSelectorPrevNextButton nextPageButton;

        public PageSelector()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    previousPageButton = new PageSelectorPrevNextButton(false, "prev")
                    {
                        Action = () => CurrentPage.Value -= 1,
                    },
                    itemsFlow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                    },
                    nextPageButton = new PageSelectorPrevNextButton(true, "next")
                    {
                        Action = () => CurrentPage.Value += 1
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentPage.BindValueChanged(onCurrentPageChanged);
            AvailablePages.BindValueChanged(_ => redraw(), true);
        }

        private void onCurrentPageChanged(ValueChangedEvent<int> currentPage)
        {
            if (currentPage.NewValue >= AvailablePages.Value)
            {
                CurrentPage.Value = AvailablePages.Value - 1;
                return;
            }

            foreach (var page in itemsFlow.OfType<PageSelectorPageButton>())
                page.Selected = page.PageNumber == currentPage.NewValue + 1;

            previousPageButton.Enabled.Value = currentPage.NewValue != 0;
            nextPageButton.Enabled.Value = currentPage.NewValue < AvailablePages.Value - 1;
        }

        private void redraw()
        {
            itemsFlow.Clear();

            for (int i = 0; i < AvailablePages.Value; i++)
            {
                int pageIndex = i;

                itemsFlow.Add(new PageSelectorPageButton(pageIndex + 1)
                {
                    Action = () => CurrentPage.Value = pageIndex,
                });
            }

            if (CurrentPage.Value != 0)
                CurrentPage.Value = 0;
            else
                CurrentPage.TriggerChange();
        }
    }
}
