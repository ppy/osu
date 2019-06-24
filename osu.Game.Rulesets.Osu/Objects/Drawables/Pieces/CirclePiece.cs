// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class CirclePiece : Container, IKeyBindingHandler<OsuAction>
    {
        // IsHovered is used
        public override bool HandlePositionalInput => true;

        public Func<bool> Hit;

        public OsuAction? HitAction;

        public CirclePiece()
        {
            Size = new Vector2((float)OsuHitObject.OBJECT_RADIUS * 2);
            Masking = true;
            CornerRadius = Size.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChild = new SkinnableDrawable("Play/osu/hitcircle", _ => new DefaultCirclePiece());
        }

        public bool OnPressed(OsuAction action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    if (IsHovered && (Hit?.Invoke() ?? false))
                    {
                        HitAction = action;
                        return true;
                    }

                    break;
            }

            return false;
        }

        public bool OnReleased(OsuAction action) => false;
    }
}
