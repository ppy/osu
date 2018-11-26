// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class NoteSelectionBlueprint : ManiaSelectionBlueprint
    {
        public NoteSelectionBlueprint(DrawableNote note)
            : base(note)
        {
            AddInternal(new EditNotePiece { RelativeSizeAxes = Axes.X });
        }

        protected override void Update()
        {
            base.Update();

            Size = HitObject.DrawSize;
        }
    }
}
