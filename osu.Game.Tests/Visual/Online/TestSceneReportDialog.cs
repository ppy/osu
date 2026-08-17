// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;
using osu.Game.Overlays.Settings;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneReportDialog : OsuTestScene
    {
        private DialogOverlay dialogOverlay = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private ChatReportRequest? pendingRequest;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create dialog overlay", () =>
            {
                Child = dialogOverlay = new DialogOverlay();
            });

            AddStep("setup request handling", () =>
            {
                pendingRequest = null;
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
        }

        [Test]
        public void TestSuccess()
        {
            AddStep("try to report", () => dialogOverlay.CurrentDialog!.PerformAction<TestReportDialog.SubmitButton>());
            AddWaitStep("wait", 3);
            AddAssert("nothing happened", () => dialogOverlay.CurrentDialog!.ChildrenOfType<LoadingLayer>().First().IsReadOnly, () => Is.False);

            AddStep("input reason", () => this.ChildrenOfType<OsuTextBox>().First().Text = "reason");
            AddStep("send report", () => dialogOverlay.CurrentDialog!.PerformAction<TestReportDialog.SubmitButton>());

            confirmSuccess();
        }

        [Test]
        public void TestFailure()
        {
            AddStep("input reason", () => this.ChildrenOfType<OsuTextBox>().First().Text = "reason");
            AddStep("send report", () => dialogOverlay.CurrentDialog!.PerformAction<TestReportDialog.SubmitButton>());

            AddUntilStep("wait for loading layer to show", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.True);
            AddUntilStep("wait for request triggered", () => pendingRequest != null);
            AddStep("fail request", () => pendingRequest!.TriggerFailure(new APIException("test error", new HttpRequestException("test error"))));
            AddUntilStep("wait for loading layer to hide", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.False);

            AddAssert("ensure form is present", () => this.ChildrenOfType<ReverseChildIDFillFlowContainer<Drawable>>().First().IsPresent, () => Is.True);
            AddAssert("ensure error is present", () => this.ChildrenOfType<SettingsNote>().First().Current.Value?.Text.ToString(), () => Is.EqualTo("test error"));
            AddAssert("ensure header text is not updated", () => dialogOverlay.CurrentDialog!.HeaderText.ToString(), () => Is.EqualTo("Report test?"));

            // now test a success after the failure
            AddStep("send report", () => dialogOverlay.CurrentDialog!.PerformAction<TestReportDialog.SubmitButton>());

            AddUntilStep("wait for loading layer to show", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.True);
            AddUntilStep("wait for request triggered", () => pendingRequest != null);
            AddStep("complete request", () => pendingRequest!.TriggerSuccess());

            confirmSuccess();
        }

        private void confirmSuccess()
        {
            AddUntilStep("wait for loading layer to show", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.True);
            AddUntilStep("wait for request triggered", () => pendingRequest != null);
            AddStep("complete request", () => pendingRequest!.TriggerSuccess());
            AddUntilStep("wait for loading layer to hide", () => this.ChildrenOfType<LoadingLayer>().First().IsPresent, () => Is.False);

            AddAssert("ensure form is not present", () => this.ChildrenOfType<ReverseChildIDFillFlowContainer<Drawable>>().First().IsPresent, () => Is.False);
            AddAssert("ensure header text is updated", () => dialogOverlay.CurrentDialog!.HeaderText, () => Is.EqualTo(UsersStrings.ReportThanks));
            AddUntilStep("wait for dialog to hide", () => this.ChildrenOfType<TestReportDialog>().Any(), () => Is.False);
        }

        public partial class TestReportDialog : ReportDialog<ChatReportReason>
        {
            public TestReportDialog(string name)
                : base($"Report {name}?")
            {
            }

            protected override APIRequest CreateRequest(ChatReportReason reason, string comment) => new ChatReportRequest(1, reason, comment);
        }
    }
}
