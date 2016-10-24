using System;
using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.UserInterface
{
    public class FilteringSearchList<T> : Container
        where T : Drawable
    {
        public List<T> Items { get; }

        private FlowContainer contentFlowContainer;

        public FilteringSearchList(List<T> items)
        {
            Items = items;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            Children = new Drawable[]
            {
                new ScrollContainer
                {
                    Children = new Drawable[]
                    {
                        contentFlowContainer = new FlowContainer
                        {
                            Direction = FlowDirection.VerticalOnly,
                            Children = Items
                        }
                    }
                }
            };
        }

        public void Filter(Func<T, bool> predicate)
        {
            foreach (var item in Items)
            {
                if (predicate(item))
                    item.Show();
                else
                    item.Hide();
            }
            contentFlowContainer.Invalidate();
        }
    }
}