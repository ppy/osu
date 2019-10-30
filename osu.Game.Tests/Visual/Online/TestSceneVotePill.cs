// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Overlays.Comments;
using osu.Framework.Allocation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneVotePill : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(VotePill)
        };

        private VotePill votePill;

        [BackgroundDependencyLoader]
        private void load()
        {
            var userComment = new Comment
            {
                IsVoted = false,
                UserId = API.LocalUser.Value?.Id,
                VotesCount = 10,
            };

            var randomComment = new Comment
            {
                IsVoted = false,
                UserId = 455454,
                VotesCount = 2,
            };

            AddStep("Random comment", () => addVotePill(randomComment));
            AddStep("Click", () => votePill.Click());
            AddAssert("Loading", () => votePill.IsLoading == true);
            AddStep("User comment", () => addVotePill(userComment));
            AddStep("Click", () => votePill.Click());
            AddAssert("Not loading", () => votePill.IsLoading == false);
            AddStep("Log out", API.Logout);
            AddStep("Click", () => votePill.Click());
            AddAssert("Not loading", () => votePill.IsLoading == false);
        }

        private void addVotePill(Comment comment)
        {
            Clear();
            Add(votePill = new VotePill(comment)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }
    }
}
