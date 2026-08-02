// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Chat;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneReportPopover : OsuTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private ReportPopoverContainer popover = null!;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create popover", () =>
            {
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = popover = new ReportPopoverContainer(),
                };
            });
        }

        [Test]
        public void TestSuccess()
        {
            ChatReportRequest pendingRequest = null!;

            AddStep("setup request handling", () =>
            {
                dummyAPI.HandleRequest += request =>
                {
                    if (request is ChatReportRequest chatReportRequest)
                    {
                        pendingRequest = chatReportRequest;
                        return true;
                    }

                    return false;
                };
            });
            AddStep("show popover", () => popover.ShowPopover());
            AddStep("input reason", () => this.ChildrenOfType<OsuTextBox>().First().Text = "reason");
            AddStep("send report", () => this.ChildrenOfType<Button>().First().TriggerClick());
            AddUntilStep("wait for loading layer to hide", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.True);
            AddWaitStep("wait some", 3);
            AddStep("complete request", () => pendingRequest.TriggerSuccess());
            AddUntilStep("wait for loading layer to hide", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.False);
            AddAssert("ensure form is not present", () => this.ChildrenOfType<ReverseChildIDFillFlowContainer<Drawable>>().First().IsPresent, () => Is.False);
            AddAssert("ensure confirmation is present", () => this.ChildrenOfType<ReportPopover<ChatReportReason>.ReportConfirmation>().First().IsPresent, () => Is.True);
            AddUntilStep("wait for popover to hide", () => this.ChildrenOfType<ReportPopoverContainer.TestReportPopover>().First().IsPresent, () => Is.False);
        }

        [Test]
        public void TestFailure()
        {
            ChatReportRequest pendingRequest = null!;

            AddStep("setup request handling", () =>
            {
                dummyAPI.HandleRequest += request =>
                {
                    if (request is ChatReportRequest chatReportRequest)
                    {
                        pendingRequest = chatReportRequest;
                        return true;
                    }

                    return false;
                };
            });
            AddStep("show popover", () => popover.ShowPopover());
            AddStep("input reason", () => this.ChildrenOfType<OsuTextBox>().First().Text = "reason");
            AddStep("send report", () => this.ChildrenOfType<Button>().First().TriggerClick());
            AddUntilStep("wait for loading layer to hide", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.True);
            AddWaitStep("wait some", 3);
            AddStep("fail request", () => pendingRequest.TriggerFailure(new APIException("test error", new HttpRequestException("test error"))));
            AddUntilStep("wait for loading layer to hide", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.False);
            AddAssert("ensure form is present", () => this.ChildrenOfType<ReverseChildIDFillFlowContainer<Drawable>>().First().IsPresent, () => Is.True);
            AddAssert("ensure error is present", () => this.ChildrenOfType<ErrorTextFlowContainer>().First().IsPresent, () => Is.True);
            AddAssert("ensure confirmation is not present", () => this.ChildrenOfType<ReportPopover<ChatReportReason>.ReportConfirmation>().First().IsPresent, () => Is.False);
        }

        protected partial class ReportPopoverContainer : Drawable, IHasPopover
        {
            public Popover GetPopover() => new TestReportPopover("test");

            public partial class TestReportPopover : ReportPopover<ChatReportReason>
            {
                private IAPIProvider api { get; set; } = null!;

                public TestReportPopover(string name)
                    : base($"Report {name}?")
                {
                }

                protected override APIRequest GetRequest(ChatReportReason reason, string comment) => new ChatReportRequest(1, reason, comment);
            }
        }
    }
}
