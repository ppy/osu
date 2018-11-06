// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class BlueprintContainer : CompositeDrawable
    {
        private MaskContainer maskContainer;

        [Resolved]
        private HitObjectComposer composer { get; set; }

        public BlueprintContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            maskContainer = new MaskContainer();

            var maskSelection = composer.CreateMaskSelection();

            maskContainer.MaskSelected += maskSelection.HandleSelected;
            maskContainer.MaskDeselected += maskSelection.HandleDeselected;
            maskContainer.MaskSelectionRequested += maskSelection.HandleSelectionRequested;
            maskContainer.MaskDragRequested += maskSelection.HandleDrag;

            maskSelection.DeselectAll = maskContainer.DeselectAll;

            var dragBox = new DragBox(maskContainer.Select);
            dragBox.DragEnd += () => maskSelection.UpdateVisibility();

            InternalChildren = new[]
            {
                dragBox,
                maskSelection,
                maskContainer,
                dragBox.CreateProxy()
            };

            foreach (var obj in composer.HitObjects)
                AddMaskFor(obj);
        }

        protected override bool OnClick(ClickEvent e)
        {
            maskContainer.DeselectAll();
            return true;
        }

        /// <summary>
        /// Adds a mask for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create a mask for.</param>
        public void AddMaskFor(DrawableHitObject hitObject)
        {
            var mask = composer.CreateMaskFor(hitObject);
            if (mask == null)
                return;

            maskContainer.Add(mask);
        }

        /// <summary>
        /// Removes a mask for a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> for which to remove the mask.</param>
        public void RemoveMaskFor(DrawableHitObject hitObject)
        {
            var maskToRemove = maskContainer.Single(m => m.HitObject == hitObject);
            if (maskToRemove == null)
                return;

            maskToRemove.Deselect();
            maskContainer.Remove(maskToRemove);
        }
    }
}
