// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Users;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Taiko;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneUserRequest : OsuTestScene
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Bindable<User> user = new Bindable<User>();
        private GetUserRequest request;
        private readonly DimmedLoadingLayer loading;

        public TestSceneUserRequest()
        {
            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new UserTestContainer
                    {
                        User = { BindTarget = user }
                    },
                    loading = new DimmedLoadingLayer
                    {
                        Alpha = 0
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep(@"local user", () => getUser());
            AddStep(@"local user with taiko ruleset", () => getUser(ruleset: new TaikoRuleset().RulesetInfo));
            AddStep(@"cookiezi", () => getUser(124493));
            AddStep(@"cookiezi with mania ruleset", () => getUser(124493, new ManiaRuleset().RulesetInfo));
        }

        private void getUser(long? userId = null, RulesetInfo ruleset = null)
        {
            loading.Show();

            request?.Cancel();
            request = new GetUserRequest(userId, ruleset);
            request.Success += user =>
            {
                this.user.Value = user;
                loading.Hide();
            };
            api.Queue(request);
        }

        private class UserTestContainer : FillFlowContainer
        {
            public readonly Bindable<User> User = new Bindable<User>();

            public UserTestContainer()
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Vertical;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                User.BindValueChanged(onUserUpdate, true);
            }

            private void onUserUpdate(ValueChangedEvent<User> user)
            {
                Clear();

                AddRange(new Drawable[]
                {
                    new SpriteText
                    {
                        Text = $@"Username: {user.NewValue?.Username}"
                    },
                    new SpriteText
                    {
                        Text = $@"RankedScore: {user.NewValue?.Statistics.RankedScore}"
                    },
                });
            }
        }
    }
}
