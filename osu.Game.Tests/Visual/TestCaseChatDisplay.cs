// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    [Description("Testing chat api and overlay")]
    internal class TestCaseChatDisplay : OsuTestCase
    {
        private readonly BeatmapSetOverlay beatmapSetOverlay;
        private readonly ChatOverlay chat;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) => dependencies = new DependencyContainer(parent);

        public TestCaseChatDisplay()
        {
            Add(chat = new ChatOverlay
            {
                State = Visibility.Visible
            });

            Add(beatmapSetOverlay = new BeatmapSetOverlay());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(chat);
            dependencies.Cache(beatmapSetOverlay);
        }
    }
}
