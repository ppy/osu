// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Objects.Drawables
{
    public partial class DrawableStreamHitCircle : DrawableHitCircle
    {
        public new StreamHitCircle HitObject => (StreamHitCircle)base.HitObject;

        public Stream? Stream => DrawableStream?.HitObject;

        public DrawableStream? DrawableStream => ParentHitObject as DrawableStream;

        public DrawableStreamHitCircle()
            : base(null)
        {
        }

        public DrawableStreamHitCircle(StreamHitCircle h)
            : base(h)
        {
        }

        protected override void UpdatePosition()
        {
            Position = HitObject.StackedPosition - Stream?.Position ?? Vector2.Zero;
        }
    }
}
