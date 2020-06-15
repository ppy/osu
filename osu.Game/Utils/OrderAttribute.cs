// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Utils
{
    public static class OrderAttributeUtils
    {
        /// <summary>
        /// Get values of an enum in order. Supports custom ordering via <see cref="OrderAttribute"/>.
        /// </summary>
        public static IEnumerable<T> GetValuesInOrder<T>()
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidOperationException("T must be an enum");

            IEnumerable<T> items = (T[])Enum.GetValues(type);

            if (Attribute.GetCustomAttribute(type, typeof(HasOrderedElementsAttribute)) == null)
                return items;

            return items.OrderBy(i =>
            {
                if (type.GetField(i.ToString()).GetCustomAttributes(typeof(OrderAttribute), false).FirstOrDefault() is OrderAttribute attr)
                    return attr.Order;

                throw new ArgumentException($"Not all values of {nameof(T)} have {nameof(OrderAttribute)} specified.");
            });
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class OrderAttribute : Attribute
    {
        public readonly int Order;

        public OrderAttribute(int order)
        {
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public class HasOrderedElementsAttribute : Attribute
    {
    }
}
