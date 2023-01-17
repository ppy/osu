// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Taiko;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneUserRequest : OsuTestScene
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Bindable<APIUser> user = new Bindable<APIUser>();
        private GetUserRequest request;
        private readonly LoadingLayer loading;

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
                    loading = new LoadingLayer()
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

        private partial class UserTestContainer : FillFlowContainer
        {
            public readonly Bindable<APIUser> User = new Bindable<APIUser>();

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

            private void onUserUpdate(ValueChangedEvent<APIUser> user)
            {
                Clear();

                AddRange(new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = $@"Username: {user.NewValue?.Username}"
                    },
                    new OsuSpriteText
                    {
                        Text = $@"RankedScore: {user.NewValue?.Statistics.RankedScore}"
                    },
                });
            }
        }
    }
}
