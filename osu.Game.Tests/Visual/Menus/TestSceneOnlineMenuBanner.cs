// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Menu;
using osuTK;

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
                    FetchOnlineContent = false,
                    DelayBetweenRotation = 500,
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

            AddUntilStep("wait for one image shown", () =>
            {
                var images = onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>();

                if (images.Count() != 1)
                    return false;

                var image = images.Single();

                return image.IsPresent && image.Image.Url == "https://osu.ppy.sh/home/news/2023-12-21-project-loved-december-2023";
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

            AddUntilStep("wait for new image shown", () =>
            {
                var images = onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>();

                if (images.Count() != 1)
                    return false;

                var image = images.Single();

                return image.IsPresent && image.Image.Url == "https://osu.ppy.sh/community/contests/189";
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

            AddUntilStep("wait for no image shown", () => onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>().Single().Size, () => Is.EqualTo(Vector2.Zero));

            AddStep("unset system title", () => onlineMenuBanner.Current.Value = new APIMenuContent());

            AddUntilStep("wait for no image shown", () => !onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>().Any());
        }

        [Test]
        public void TestMultipleImages()
        {
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

            AddUntilStep("wait for first image shown", () =>
            {
                var images = onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>();

                if (images.Count() != 2)
                    return false;

                return images.First().IsPresent && !images.Last().IsPresent;
            });

            AddUntilStep("wait for second image shown", () =>
            {
                var images = onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>();

                if (images.Count() != 2)
                    return false;

                return !images.First().IsPresent && images.Last().IsPresent;
            });
        }

        [Test]
        public void TestFutureSingle()
        {
            AddStep("set image with time constraints", () => onlineMenuBanner.Current.Value = new APIMenuContent
            {
                Images = new[]
                {
                    new APIMenuImage
                    {
                        Image = @"https://assets.ppy.sh/main-menu/project-loved-2@2x.png",
                        Url = @"https://osu.ppy.sh/home/news/2023-12-21-project-loved-december-2023",
                        Begins = DateTimeOffset.Now.AddSeconds(2),
                        Expires = DateTimeOffset.Now.AddSeconds(5),
                    },
                },
            });

            AddUntilStep("wait for no image shown", () => !onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>().Any(i => i.IsPresent));

            AddUntilStep("wait for one image shown", () =>
            {
                var images = onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>();

                if (images.Count() != 1)
                    return false;

                var image = images.Single();

                return image.IsPresent && image.Image.Url == "https://osu.ppy.sh/home/news/2023-12-21-project-loved-december-2023";
            });

            AddUntilStep("wait for no image shown", () => !onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>().Any(i => i.IsPresent));
        }

        [Test]
        public void TestExpiryMultiple()
        {
            AddStep("set multiple images, second expiring soon", () => onlineMenuBanner.Current.Value = new APIMenuContent
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
                        Expires = DateTimeOffset.Now.AddSeconds(2),
                    }
                },
            });

            AddUntilStep("wait for first image shown", () =>
            {
                var images = onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>();

                if (images.Count() != 2)
                    return false;

                return images.First().IsPresent && !images.Last().IsPresent;
            });

            AddUntilStep("wait for second image shown", () =>
            {
                var images = onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>();

                if (images.Count() != 2)
                    return false;

                return !images.First().IsPresent && images.Last().IsPresent;
            });

            AddUntilStep("wait for expiry", () =>
            {
                return onlineMenuBanner
                       .ChildrenOfType<OnlineMenuBanner.MenuImage>()
                       .Any(i => !i.Image.IsCurrent);
            });

            AddUntilStep("wait for first image shown", () =>
            {
                var images = onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>();

                if (images.Count() != 2)
                    return false;

                return images.First().IsPresent && !images.Last().IsPresent;
            });
        }
    }
}
