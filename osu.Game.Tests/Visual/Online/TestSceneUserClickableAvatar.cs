// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneUserClickableAvatar : OsuManualInputManagerTestScene
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = new Vector2(10f),
                Children = new Drawable[]
                {
                    new ClickableAvatar(new APIUser
                    {
                        Username = @"flyte", Id = 3103765, CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
                    })
                    {
                        Width = 50,
                        Height = 50,
                        CornerRadius = 10,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow, Radius = 1, Colour = Color4.Black.Opacity(0.2f),
                        },
                    },
                    new ClickableAvatar(new APIUser
                    {
                        Username = @"peppy", Id = 2, Colour = "99EB47", CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    })
                    {
                        Width = 50,
                        Height = 50,
                        CornerRadius = 10,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow, Radius = 1, Colour = Color4.Black.Opacity(0.2f),
                        },
                    },
                    new ClickableAvatar(new APIUser
                    {
                        Username = @"flyte",
                        Id = 3103765,
                        CountryCode = CountryCode.JP,
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                        Status =
                        {
                            Value = new UserStatusOnline()
                        }
                    })
                    {
                        Width = 50,
                        Height = 50,
                        CornerRadius = 10,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow, Radius = 1, Colour = Color4.Black.Opacity(0.2f),
                        },
                    },
                },
            };
        });

        [Test]
        public void TestClickableAvatarHover()
        {
            AddStep($"click {1}. {nameof(ClickableAvatar)}", () =>
            {
                var targets = this.ChildrenOfType<ClickableAvatar>().ToList();
                if (targets.Count < 1)
                    return;

                InputManager.MoveMouseTo(targets[0]);
            });
            AddWaitStep("wait for tooltip to show", 5);
            AddStep("Hover out", () => InputManager.MoveMouseTo(new Vector2(0)));
            AddWaitStep("wait for tooltip to hide", 3);

            AddStep($"click {2}. {nameof(ClickableAvatar)}", () =>
            {
                var targets = this.ChildrenOfType<ClickableAvatar>().ToList();
                if (targets.Count < 2)
                    return;

                InputManager.MoveMouseTo(targets[1]);
            });
            AddWaitStep("wait for tooltip to show", 5);
            AddStep("Hover out", () => InputManager.MoveMouseTo(new Vector2(0)));
            AddWaitStep("wait for tooltip to hide", 3);

            AddStep($"click {3}. {nameof(ClickableAvatar)}", () =>
            {
                var targets = this.ChildrenOfType<ClickableAvatar>().ToList();
                if (targets.Count < 3)
                    return;

                InputManager.MoveMouseTo(targets[2]);
            });
            AddWaitStep("wait for tooltip to show", 5);
            AddStep("Hover out", () => InputManager.MoveMouseTo(new Vector2(0)));
            AddWaitStep("wait for tooltip to hide", 3);
        }
    }
}
