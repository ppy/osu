using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.UserInterface
{
    public class FilteringSearchList<T> : Container where T : Drawable
    {
        public List<T> Items { get; }

        public PropertyInfo FilterProperty
        {
            get { return filterProperty; }
            private set
            {
                if (typeof(T).GetProperty(value.Name) != null)
                    filterProperty = value;
                else
                    throw new ArgumentException($"\"{value.Name}\" property was not found in \"{typeof(T).Name}\" class");
            }
        }

        private FlowContainer contentFlowContainer;
        private PropertyInfo filterProperty;

        public FilteringSearchList(List<T> items, PropertyInfo filterProperty)
        {
            Items = items;
            FilterProperty = filterProperty;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        contentFlowContainer = new FlowContainer
                        {
                            Direction = FlowDirection.VerticalOnly,
                            RelativeSizeAxes = Axes.Both,
                            Children = Items
                        }
                    }
                }
            };
        }

        public void Filter(string input = null)
        {
            var filteringResult =
                FilterProperty == null || string.IsNullOrEmpty(input)
                    ? Items
                    : Items.Where(i => FilterProperty.GetValue(i).ToString().IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0);
            foreach (var listItem in contentFlowContainer.Children)
            {
                if (filteringResult.Contains(listItem)) listItem.Show();
                else listItem.Hide();
            }
            contentFlowContainer.Invalidate();
        }
    }
}