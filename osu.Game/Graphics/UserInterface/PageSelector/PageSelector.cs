// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public class PageSelector : CompositeDrawable
    {
        public readonly BindableInt CurrentPage = new BindableInt(1);
        public readonly BindableInt MaxPages = new BindableInt(1);

        private readonly FillFlowContainer<PageSelectorPageButton> itemsFlow;

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
                    itemsFlow = new FillFlowContainer<PageSelectorPageButton>
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

            MaxPages.BindValueChanged(_ => redraw());
            CurrentPage.BindValueChanged(page => onCurrentPageChanged(page.NewValue));
            redraw();
        }

        private void onCurrentPageChanged(int newPage)
        {
            if (newPage < 1)
            {
                CurrentPage.Value = 1;
                return;
            }

            if (newPage > MaxPages.Value)
            {
                CurrentPage.Value = MaxPages.Value;
                return;
            }

            itemsFlow.ForEach(page => page.Selected = page.Page == newPage);
            updateButtonsState();
        }

        private void redraw()
        {
            itemsFlow.Clear();

            if (MaxPages.Value < 1)
            {
                MaxPages.Value = 1;
                return;
            }

            for (int i = 1; i <= MaxPages.Value; i++)
                addDrawablePage(i);

            if (CurrentPage.Value == 1)
                CurrentPage.TriggerChange();
            else
                CurrentPage.Value = 1;
        }

        private void updateButtonsState()
        {
            int newPage = CurrentPage.Value;
            int maxPages = MaxPages.Value;

            previousPageButton.Enabled.Value = newPage != 1;
            nextPageButton.Enabled.Value = newPage != maxPages;
        }

        private void addDrawablePage(int page) => itemsFlow.Add(new PageSelectorPageButton(page)
        {
            Action = () => CurrentPage.Value = page,
        });
    }
}
