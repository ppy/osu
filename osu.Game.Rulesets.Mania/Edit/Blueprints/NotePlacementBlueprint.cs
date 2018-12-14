// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class NotePlacementBlueprint : ManiaPlacementBlueprint<Note>
    {
        public NotePlacementBlueprint()
            : base(new Note())
        {
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Y;

            InternalChild = new EditNotePiece { RelativeSizeAxes = Axes.X };
        }

        protected override void Update()
        {
            base.Update();

            Width = SnappedWidth;
            Position = SnappedMousePosition;
        }
    }
}
