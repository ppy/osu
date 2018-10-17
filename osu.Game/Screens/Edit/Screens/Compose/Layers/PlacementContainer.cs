// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit.Tools;
using Container = System.ComponentModel.Container;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class PlacementContainer : CompositeDrawable
    {
        private readonly Container maskContainer;

        private readonly IBindable<HitObjectCompositionTool> compositionTool = new Bindable<HitObjectCompositionTool>();

        [Resolved]
        private IPlacementHandler placementHandler { get; set; }

        public PlacementContainer(IBindable<HitObjectCompositionTool> compositionTool)
        {
            this.compositionTool.BindTo(compositionTool);

            RelativeSizeAxes = Axes.Both;

            this.compositionTool.BindValueChanged(onToolChanged);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Refresh the mask after each placement
            placementHandler.PlacementFinished += _ => onToolChanged(compositionTool.Value);
        }

        private void onToolChanged(HitObjectCompositionTool tool)
        {
            ClearInternal();

            var mask = tool?.CreatePlacementMask();
            if (mask != null)
                InternalChild = mask;
        }
    }
}
