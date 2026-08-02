// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public partial class NoteSelectionBlueprint : ManiaSelectionBlueprint<Note>
    {
        private readonly EditNotePiece notePiece;

        public NoteSelectionBlueprint(Note note)
            : base(note)
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            AddInternal(notePiece = new EditNotePiece
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            });
        }

        protected override void Update()
        {
            base.Update();

            notePiece.Height = DrawableObject.DrawHeight;
        }

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            notePiece.Scale = new Vector2(1, direction.NewValue == ScrollingDirection.Down ? 1 : -1);
        }
    }
}
