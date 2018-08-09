// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaScrollingInfo : IScrollingInfo
    {
        private readonly Bindable<ManiaScrollingDirection> configDirection = new Bindable<ManiaScrollingDirection>();

        public readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();
        IBindable<ScrollingDirection> IScrollingInfo.Direction => Direction;

        public ManiaScrollingInfo(ManiaConfigManager config)
        {
            config.BindWith(ManiaSetting.ScrollDirection, configDirection);
            configDirection.BindValueChanged(v => Direction.Value = (ScrollingDirection)v, true);
        }
    }
}
