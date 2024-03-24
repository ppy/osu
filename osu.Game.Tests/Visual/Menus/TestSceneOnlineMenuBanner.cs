// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneOnlineMenuBanner : OsuTestScene
    {
        private OnlineMenuBanner onlineMenuBanner = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create banner", () =>
            {
                Child = onlineMenuBanner = new OnlineMenuBanner
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    State = { Value = Visibility.Visible }
                };
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("set online content", () => onlineMenuBanner.Current.Value = new APIMenuContent
            {
                Images = new[]
                {
                    new APIMenuImage
                    {
                        Image = @"https://assets.ppy.sh/main-menu/project-loved-2@2x.png",
                        Url = @"https://osu.ppy.sh/home/news/2023-12-21-project-loved-december-2023",
                    }
                },
            });

            AddStep("set another title", () => onlineMenuBanner.Current.Value = new APIMenuContent
            {
                Images = new[]
                {
                    new APIMenuImage
                    {
                        Image = @"https://assets.ppy.sh/main-menu/wf2023-vote@2x.png",
                        Url = @"https://osu.ppy.sh/community/contests/189",
                    }
                }
            });

            AddStep("set multiple images", () => onlineMenuBanner.Current.Value = new APIMenuContent
            {
                Images = new[]
                {
                    new APIMenuImage
                    {
                        Image = @"https://assets.ppy.sh/main-menu/project-loved-2@2x.png",
                        Url = @"https://osu.ppy.sh/home/news/2023-12-21-project-loved-december-2023",
                    },
                    new APIMenuImage
                    {
                        Image = @"https://assets.ppy.sh/main-menu/wf2023-vote@2x.png",
                        Url = @"https://osu.ppy.sh/community/contests/189",
                    }
                },
            });

            AddStep("set title with nonexistent image", () => onlineMenuBanner.Current.Value = new APIMenuContent
            {
                Images = new[]
                {
                    new APIMenuImage
                    {
                        Image = @"https://test.invalid/@2x", // .invalid TLD reserved by https://datatracker.ietf.org/doc/html/rfc2606#section-2
                        Url = @"https://osu.ppy.sh/community/contests/189",
                    }
                }
            });

            AddStep("unset system title", () => onlineMenuBanner.Current.Value = new APIMenuContent());
        }
    }
}
