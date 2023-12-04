// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Testing;
using osu.Game.Graphics.Cursor;
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
                Children = new[]
                {
                    generateUser(@"peppy", 2, CountryCode.AU, @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg", false, "99EB47"),
                    generateUser(@"flyte", 3103765, CountryCode.JP, @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg", true),
                    generateUser(@"joshika39", 17032217, CountryCode.RS, @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg", false),
                    new UpdateableAvatar(),
                    new UpdateableAvatar()
                },
            };
        });

        [Test]
        public void TestClickableAvatarHover()
        {
            AddStep("hover avatar with user panel", () => InputManager.MoveMouseTo(this.ChildrenOfType<ClickableAvatar>().ElementAt(1)));
            AddUntilStep("wait for tooltip to show", () => this.ChildrenOfType<ClickableAvatar.UserCardTooltip>().FirstOrDefault()?.State.Value == Visibility.Visible);
            AddStep("hover out", () => InputManager.MoveMouseTo(new Vector2(0)));
            AddUntilStep("wait for tooltip to hide", () => this.ChildrenOfType<ClickableAvatar.UserCardTooltip>().FirstOrDefault()?.State.Value == Visibility.Hidden);

            AddStep("hover avatar without user panel", () => InputManager.MoveMouseTo(this.ChildrenOfType<ClickableAvatar>().ElementAt(0)));
            AddUntilStep("wait for tooltip to show", () => this.ChildrenOfType<OsuTooltipContainer.OsuTooltip>().FirstOrDefault()?.State.Value == Visibility.Visible);
            AddStep("hover out", () => InputManager.MoveMouseTo(new Vector2(0)));
            AddUntilStep("wait for tooltip to hide", () => this.ChildrenOfType<OsuTooltipContainer.OsuTooltip>().FirstOrDefault()?.State.Value == Visibility.Hidden);
        }

        private Drawable generateUser(string username, int id, CountryCode countryCode, string cover, bool showPanel, string? color = null)
        {
            var user = new APIUser
            {
                Username = username,
                Id = id,
                CountryCode = countryCode,
                CoverUrl = cover,
                Colour = color ?? "000000",
                Status =
                {
                    Value = new UserStatusOnline()
                },
            };

            return new ClickableAvatar(user, showPanel)
            {
                Width = 50,
                Height = 50,
                CornerRadius = 10,
                Masking = true,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 1,
                    Colour = Color4.Black.Opacity(0.2f),
                },
            };
        }
    }
}
