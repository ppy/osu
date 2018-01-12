// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// A <see cref="CompositeDrawable"/> that has <see cref="Handle"/>s around its border.
    /// </summary>
    public class HandleContainer : CompositeDrawable
    {
        /// <summary>
        /// Invoked when a <see cref="Handle"/> requires the current drag rectangle.
        /// </summary>
        public Func<RectangleF> GetDragRectangle;

        /// <summary>
        /// Invoked when a <see cref="Handle"/> wants to update the drag rectangle.
        /// </summary>
        public Action<RectangleF> UpdateDragRectangle;

        /// <summary>
        /// Invoked when a <see cref="Handle"/> has finished updates to the drag rectangle.
        /// </summary>
        public Action FinishDrag;

        public HandleContainer()
        {
            InternalChildren = new Drawable[]
            {
                new Handle
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre
                },
                new Handle
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre
                },
                new Handle
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.Centre
                },
                new Handle
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre
                },
                new Handle
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.Centre
                },
                new Handle
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.Centre
                },
                new Handle
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.Centre
                },
                new Handle
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre
                },
                new OriginHandle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };

            InternalChildren.OfType<Handle>().ForEach(m =>
            {
                m.GetDragRectangle = () => GetDragRectangle();
                m.UpdateDragRectangle = r => UpdateDragRectangle(r);
                m.FinishDrag = () => FinishDrag();
            });
        }
    }
}
