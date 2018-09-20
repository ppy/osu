﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class HitObjectMaskLayer : CompositeDrawable
    {
        private MaskContainer maskContainer;
        private HitObjectComposer composer;

        public HitObjectMaskLayer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(HitObjectComposer composer)
        {
            this.composer = composer;

            maskContainer = new MaskContainer();

            var maskSelection = composer.CreateMaskSelection();

            maskContainer.MaskSelected += maskSelection.HandleSelected;
            maskContainer.MaskDeselected += maskSelection.HandleDeselected;
            maskContainer.MaskSelectionRequested += maskSelection.HandleSelectionRequested;
            maskContainer.MaskDragRequested += maskSelection.HandleDrag;

            maskSelection.DeselectAll = maskContainer.DeselectAll;

            var dragLayer = new DragLayer(maskContainer.Select);
            dragLayer.DragEnd += () => maskSelection.UpdateVisibility();

            InternalChildren = new[]
            {
                dragLayer,
                maskSelection,
                maskContainer,
                dragLayer.CreateProxy()
            };

            foreach (var obj in composer.HitObjects)
                AddMask(obj);
        }

        /// <summary>
        /// Adds a mask for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create a mask for.</param>
        public void AddMask(DrawableHitObject hitObject)
        {
            var mask = composer.CreateMaskFor(hitObject);
            if (mask == null)
                return;

            maskContainer.Add(mask);
        }

        /// <summary>
        /// Adds a mask for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create a mask for.</param>
        public void RemoveMask(DrawableHitObject hitObject)
        {
            var mask = composer.CreateMaskFor(hitObject);
            if (mask == null)
                return;

            maskContainer.Add(mask);
        }

        /// <summary>
        /// Deselects all selected <see cref="DrawableHitObject"/>s.
        /// </summary>
        public void DeselectAll() => maskContainer.DeselectAll();

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            maskContainer.DeselectAll();
            return true;
        }
    }
}
