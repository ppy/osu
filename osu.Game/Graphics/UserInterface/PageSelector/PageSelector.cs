// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public partial class PageSelector : CompositeDrawable
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
                    previousPageButton = new PageSelectorPrevNextButton(false, CommonStrings.PaginationPrevious)
                    {
                        Action = () => CurrentPage.Value -= 1,
                    },
                    itemsFlow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                    },
                    nextPageButton = new PageSelectorPrevNextButton(true, CommonStrings.PaginationNext)
                    {
                        Action = () => CurrentPage.Value += 1
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentPage.BindValueChanged(_ => Scheduler.AddOnce(redraw));
            AvailablePages.BindValueChanged(_ =>
            {
                CurrentPage.Value = 0;

                // AddOnce as the reset of CurrentPage may also trigger a redraw.
                Scheduler.AddOnce(redraw);
            }, true);
        }

        private void redraw()
        {
            if (CurrentPage.Value >= AvailablePages.Value)
            {
                CurrentPage.Value = AvailablePages.Value - 1;
                return;
            }

            previousPageButton.Enabled.Value = CurrentPage.Value != 0;
            nextPageButton.Enabled.Value = CurrentPage.Value < AvailablePages.Value - 1;

            itemsFlow.Clear();

            int totalPages = AvailablePages.Value;
            bool lastWasEllipsis = false;

            for (int i = 0; i < totalPages; i++)
            {
                int pageIndex = i;

                bool shouldShowPage = pageIndex == 0 || pageIndex == totalPages - 1 || Math.Abs(pageIndex - CurrentPage.Value) <= 2;

                if (shouldShowPage)
                {
                    lastWasEllipsis = false;
                    itemsFlow.Add(new PageSelectorPageButton(pageIndex + 1)
                    {
                        Action = () => CurrentPage.Value = pageIndex,
                        Selected = CurrentPage.Value == pageIndex,
                    });
                }
                else if (!lastWasEllipsis)
                {
                    lastWasEllipsis = true;
                    itemsFlow.Add(new PageEllipsis());
                }
            }
        }
    }
}
