// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Comments;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneCommentReportButton : ThemeComparisonTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup API", () => ((DummyAPIAccess)API).HandleRequest += req =>
            {
                switch (req)
                {
                    case CommentReportRequest report:
                        Scheduler.AddDelayed(report.TriggerSuccess, 1000);
                        return true;
                }

                return false;
            });
        }

        protected override Drawable CreateContent() => new PopoverContainer
        {
            RelativeSizeAxes = Axes.Both,
            Child = new CommentReportButton(new Comment { User = new APIUser { Username = "Someone" } })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(2f),
            }.With(b => Schedule(b.ShowPopover)),
        };
    }
}
