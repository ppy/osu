// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.SelectV2;
using osu.Game.Screens.SelectV2.Footer;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneSongSelectV2 : ScreenTestScene
    {
        [Cached]
        private readonly FooterV2 screenFooter;

        [Cached]
        private readonly OsuLogo logo;

        public TestSceneSongSelectV2()
        {
            Children = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = screenFooter = new FooterV2
                    {
                        OnBack = () => Stack.CurrentScreen.Exit(),
                    },
                },
                logo = new OsuLogo
                {
                    Alpha = 0f,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Stack.ScreenPushed += updateFooter;
            Stack.ScreenExited += updateFooter;
        }

        private void updateFooter(IScreen? _, IScreen? newScreen)
        {
            if (newScreen is IOsuScreen osuScreen && osuScreen.AllowNewFooter)
            {
                screenFooter.Show();
                screenFooter.SetButtons(osuScreen.CreateFooterButtons());
            }
            else
            {
                screenFooter.Hide();
                screenFooter.SetButtons(Array.Empty<FooterButtonV2>());
            }
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load screen", () => Stack.Push(new SongSelectV2()));
            AddWaitStep("wait for transition", 3);
        }

        protected override void Update()
        {
            base.Update();
            Stack.Padding = new MarginPadding { Bottom = screenFooter.DrawHeight - screenFooter.Y };
        }
    }
}
