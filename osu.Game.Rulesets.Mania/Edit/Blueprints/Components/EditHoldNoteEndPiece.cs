﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints.Components
{
    public partial class EditHoldNoteEndPiece : CompositeDrawable
    {
        public Action? DragStarted { get; init; }
        public Action<Vector2>? Dragging { get; init; }
        public Action? DragEnded { get; init; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = DefaultNotePiece.NOTE_HEIGHT;

            CornerRadius = 5;
            Masking = true;

            InternalChild = new DefaultNotePiece();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            DragStarted?.Invoke();
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);
            Dragging?.Invoke(e.ScreenSpaceMousePosition);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);
            DragEnded?.Invoke();
        }

        private void updateState()
        {
            var colour = colours.Yellow;

            if (IsHovered)
                colour = colour.Lighten(1);

            Colour = colour;
        }
    }
}
