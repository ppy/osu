// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneSongSelectFooter : OsuManualInputManagerTestScene
    {
        public TestSceneSongSelectFooter()
        {
            AddStep("Create footer", () =>
            {
                Footer footer;
                AddRange(new Drawable[]
                {
                    footer = new Footer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                });

                footer.AddButton(new FooterButtonMods(), null);
                footer.AddButton(new FooterButtonRandom
                {
                    NextRandom = () => { },
                    PreviousRandom = () => { },
                }, null);
                footer.AddButton(new FooterButtonOptions(), null);
            });
        }
    }
}
