using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// A <see cref="CompositeDrawable"/> that has <see cref="Marker"/>s around its border.
    /// </summary>
    public class MarkerContainer : CompositeDrawable
    {
        /// <summary>
        /// Invoked when a <see cref="Marker"/> requires the current drag rectangle.
        /// </summary>
        public Func<RectangleF> GetDragRectangle;

        /// <summary>
        /// Invoked when a <see cref="Marker"/> wants to update the drag rectangle.
        /// </summary>
        public Action<RectangleF> UpdateDragRectangle;

        /// <summary>
        /// Invoked when a <see cref="Marker"/> has finished updates to the drag rectangle.
        /// </summary>
        public Action FinishCapture;

        public MarkerContainer()
        {
            InternalChildren = new Drawable[]
            {
                new Marker
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre
                },
                new CentreMarker
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };

            InternalChildren.OfType<Marker>().ForEach(m =>
            {
                m.GetDragRectangle = () => GetDragRectangle();
                m.UpdateDragRectangle = r => UpdateDragRectangle(r);
                m.FinishCapture = () => FinishCapture();
            });
        }
    }
}
