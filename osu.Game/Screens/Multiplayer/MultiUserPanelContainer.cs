// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using System.Linq;

namespace osu.Game.Screens.Multiplayer
{
    public class MultiUserPanelContainer : FillFlowContainer<MultiUserPanel>
    {
        public MultiUserPanel Host
        {
            get { return Children.FirstOrDefault(c => c.Host); }
            set { Children.ForEach(c => c.Host = c == value); }
        }
    }
}
