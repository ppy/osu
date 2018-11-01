// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit.Tools;
using Container = System.ComponentModel.Container;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class PlacementContainer : CompositeDrawable
    {
        private readonly Container maskContainer;

        public PlacementContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private HitObjectCompositionTool currentTool;

        /// <summary>
        /// The current placement tool.
        /// </summary>
        public HitObjectCompositionTool CurrentTool
        {
            get => currentTool;
            set
            {
                if (currentTool == value)
                    return;
                currentTool = value;

                Refresh();
            }
        }

        /// <summary>
        /// Refreshes the current placement tool.
        /// </summary>
        public void Refresh()
        {
            ClearInternal();

            var mask = CurrentTool?.CreatePlacementMask();
            if (mask != null)
                InternalChild = mask;
        }
    }
}
