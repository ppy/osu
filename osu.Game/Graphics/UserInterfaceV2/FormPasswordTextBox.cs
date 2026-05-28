// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormPasswordTextBox : FormTextBox<InnerPasswordTextBox>;

    public partial class InnerPasswordTextBox : OsuPasswordTextBox, IInnerTextBox
    {
        public BindableBool Focused { get; } = new BindableBool();

        public Action? OnInputError { get; set; }

        protected override float LeftRightPadding => 0;

        public InnerPasswordTextBox()
        {
            DrawBorder = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = 16;
            TextContainer.Height = 1;
            BackgroundUnfocused = BackgroundFocused = BackgroundCommit = Colour4.Transparent;
        }

        protected override SpriteText CreatePlaceholder() => base.CreatePlaceholder().With(t => t.Margin = default);

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            Focused.Value = true;
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            base.OnFocusLost(e);

            Focused.Value = false;
        }

        protected override void NotifyInputError()
        {
            PlayFeedbackSample(FeedbackSampleType.TextInvalid);
            // base call intentionally suppressed
            OnInputError?.Invoke();
        }
    }
}

