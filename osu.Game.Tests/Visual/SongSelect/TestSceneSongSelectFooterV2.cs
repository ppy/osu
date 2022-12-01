// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Select.FooterV2;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneSongSelectFooterV2 : OsuManualInputManagerTestScene
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            FooterV2 footer;

            Child = footer = new FooterV2
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            footer.AddButton(new FooterButtonModsV2());
            footer.AddButton(new FooterButtonRandomV2());
            footer.AddButton(new FooterButtonOptionsV2());
        });

        [Test]
        public void TestBasic()
        {
        }
    }
}
