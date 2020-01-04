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
        public const int HEIGHT = 20;

        public readonly BindableInt CurrentPage = new BindableInt(1);
        public readonly BindableInt MaxPages = new BindableInt(1);

        private readonly FillFlowContainer<DrawablePage> itemsFlow;

        private readonly PageSelectorButton previousPageButton;
        private readonly PageSelectorButton nextPageButton;

        public PageSelector()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    previousPageButton = new PageSelectorButton(false, "prev")
                    {
                        Action = () => CurrentPage.Value -= 1,
                    },
                    itemsFlow = new FillFlowContainer<DrawablePage>
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                    },
                    nextPageButton = new PageSelectorButton(true, "next")
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

            itemsFlow.ForEach(page => page.Selected = page.Page == newPage ? true : false);
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

            if (CurrentPage.Value > MaxPages.Value)
            {
                CurrentPage.Value = MaxPages.Value;
                return;
            }

            if (CurrentPage.Value < 1)
            {
                CurrentPage.Value = 1;
                return;
            }

            CurrentPage.TriggerChange();
        }

        private void updateButtonsState()
        {
            int newPage = CurrentPage.Value;
            int maxPages = MaxPages.Value;

            previousPageButton.Enabled.Value = newPage != 1;
            nextPageButton.Enabled.Value = newPage != maxPages;
        }

        private void addDrawablePage(int page) => itemsFlow.Add(new DrawablePage(page)
        {
            Action = () => CurrentPage.Value = page,
        });
    }
}
