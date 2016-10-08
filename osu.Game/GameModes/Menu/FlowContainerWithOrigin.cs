using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.GameModes.Menu
{
    /// <summary>
    /// A flow container with an origin based on one of its contained drawables.
    /// </summary>
    public class FlowContainerWithOrigin : FlowContainer
    {
        /// <summary>
        /// A target drawable which this flowcontainer should be centered around.
        /// This target MUST be in this FlowContainer's *direct* children.
        /// </summary>
        internal Drawable CentreTarget;

        public override Anchor Origin => Anchor.Custom;

        public override Vector2 OriginPosition
        {
            get
            {
                if (CentreTarget == null)
                    return base.OriginPosition;

                return CentreTarget.Position + CentreTarget.Size / 2;
            }
        }

        public FlowContainerWithOrigin()
        {
            Direction = FlowDirection.HorizontalOnly;
        }
    }
}
