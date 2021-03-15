// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Platform;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneTabletSettings : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var tabletHandler = host.AvailableInputHandlers.OfType<ITabletHandler>().FirstOrDefault();

            if (tabletHandler == null)
                return;

            tabletHandler.AreaOffset.MinValue = new Size(0, 0);
            tabletHandler.AreaOffset.MaxValue = new Size(160, 100);
            tabletHandler.AreaOffset.Value = new Size(10, 10);

            tabletHandler.AreaSize.MinValue = new Size(0, 0);
            tabletHandler.AreaSize.MaxValue = new Size(160, 100);
            tabletHandler.AreaSize.Value = new Size(100, 80);

            AddRange(new Drawable[]
            {
                new TabletSettings(tabletHandler),
            });
        }
    }
}
