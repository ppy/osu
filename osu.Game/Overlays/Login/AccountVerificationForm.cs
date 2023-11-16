// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Overlays.Login
{
    public partial class AccountVerificationForm : FillFlowContainer
    {
        private OsuTextBox code = null!;
        private LinkFlowContainer explainText = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0, SettingsSection.ITEM_SPACING);

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS },
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, SettingsSection.ITEM_SPACING),
                    Children = new Drawable[]
                    {
                        new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = "An email has been sent to you with a verification code. Enter the code.",
                        },
                        code = new OsuTextBox
                        {
                            PlaceholderText = "Enter code",
                            RelativeSizeAxes = Axes.X,
                            TabbableContentContainer = this
                        },
                        explainText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        new ErrorTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0,
                        },
                    },
                },
                new LinkFlowContainer
                {
                    Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };

            explainText.AddParagraph("Make sure to check your spam folder if you can't find the email.");
            explainText.AddParagraph("If you can't access your email or have forgotten what you used, please follow the ");
            explainText.AddLink("email recovery process here", () => { });
            explainText.AddText(". You can also ");
            explainText.AddLink("request another code", () => { });
            explainText.AddText(" or ");
            explainText.AddLink("sign out", () => { });
            explainText.AddText(".");
        }

        public override bool AcceptsFocus => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override void OnFocus(FocusEvent e)
        {
            Schedule(() => { GetContainingInputManager().ChangeFocus(code); });
        }
    }
}
