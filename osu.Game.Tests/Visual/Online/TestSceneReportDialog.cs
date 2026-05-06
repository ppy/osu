// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneReportDialog : OsuTestScene
    {
        private DialogOverlay dialogOverlay = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create dialog overlay", () =>
            {
                Child = dialogOverlay = new DialogOverlay();
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
            AddStep("push dialog", () => dialogOverlay.Push(new TestReportDialog("test")));

            AddStep("try to report", () => dialogOverlay.CurrentDialog.ChildrenOfType<Button>().First().TriggerClick());
            AddWaitStep("wait", 3);
            AddAssert("nothing happened", () => this.ChildrenOfType<TestReportDialog>().Any(), () => Is.True);

            AddStep("input reason", () => this.ChildrenOfType<OsuTextBox>().First().Text = "reason");
            AddStep("send report", () => this.ChildrenOfType<Button>().First().TriggerClick());

            AddUntilStep("wait for loading layer to show", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.True);
            AddWaitStep("wait some", 3);
            AddStep("complete request", () => pendingRequest.TriggerSuccess());
            AddUntilStep("wait for loading layer to hide", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.False);

            AddAssert("ensure form is not present", () => this.ChildrenOfType<ReverseChildIDFillFlowContainer<Drawable>>().First().IsPresent, () => Is.False);
            AddAssert("ensure confirmation is present", () => this.ChildrenOfType<ReportDialog<ChatReportReason>.ReportConfirmation>().First().IsPresent, () => Is.True);
            AddUntilStep("wait for dialog to hide", () => this.ChildrenOfType<TestReportDialog>().Any(), () => Is.False);
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
            AddStep("push dialog", () => dialogOverlay.Push(new TestReportDialog("test")));

            AddStep("input reason", () => this.ChildrenOfType<OsuTextBox>().First().Text = "reason");
            AddStep("send report", () => this.ChildrenOfType<Button>().First().TriggerClick());

            AddUntilStep("wait for loading layer to show", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.True);
            AddWaitStep("wait some", 3);
            AddStep("fail request", () => pendingRequest.TriggerFailure(new APIException("test error", new HttpRequestException("test error"))));
            AddUntilStep("wait for loading layer to hide", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.False);

            AddAssert("ensure form is present", () => this.ChildrenOfType<ReverseChildIDFillFlowContainer<Drawable>>().First().IsPresent, () => Is.True);
            AddAssert("ensure error is present", () => this.ChildrenOfType<ErrorTextFlowContainer>().First().IsPresent, () => Is.True);
            AddAssert("ensure confirmation is not present", () => this.ChildrenOfType<ReportDialog<ChatReportReason>.ReportConfirmation>().First().IsPresent, () => Is.False);
        }

        public partial class TestReportDialog : ReportDialog<ChatReportReason>
        {
            public TestReportDialog(string name)
                : base($"Report {name}?")
            {
            }

            protected override APIRequest GetRequest(ChatReportReason reason, string comment) => new ChatReportRequest(1, reason, comment);
        }
    }
}
