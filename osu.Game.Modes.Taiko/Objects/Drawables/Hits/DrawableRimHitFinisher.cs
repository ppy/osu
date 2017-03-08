// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Circle;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Hits
{
    public class DrawableRimHitFinisher : DrawableHitFinisher
    {
        public override Color4 ExplodeColour { get; protected set; }

        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.K });

        public DrawableRimHitFinisher(TaikoHitObject hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ExplodeColour = colours.Blue;
        }

        protected override CirclePiece CreateBody() => new RimHitCirclePiece
        {
            Scale = new Vector2(1.5f)
        };
    }
}
