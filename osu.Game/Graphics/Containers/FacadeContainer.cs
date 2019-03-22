// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    public class FacadeContainer : Container
    {
        [Cached]
        private Facade facade;

        public FacadeContainer()
        {
            facade = new Facade();
        }

        public void SetLogo(OsuLogo logo)
        {
            facade.Size = new Vector2(logo.SizeForFlow);
        }
    }

    public class Facade : Container
    {
    }
}
