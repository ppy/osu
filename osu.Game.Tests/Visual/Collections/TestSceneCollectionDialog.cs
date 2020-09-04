// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Collections;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Collections
{
    public class TestSceneCollectionDialog : OsuTestScene
    {
        [Cached]
        private DialogOverlay dialogOverlay;

        public TestSceneCollectionDialog()
        {
            Children = new Drawable[]
            {
                new CollectionDialog { State = { Value = Visibility.Visible } },
                dialogOverlay = new DialogOverlay()
            };
        }
    }
}
