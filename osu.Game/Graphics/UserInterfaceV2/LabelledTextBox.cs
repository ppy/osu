// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class LabelledTextBox : LabelledComponent<OsuTextBox, string>
    {
        public event TextBox.OnCommitHandler OnCommit;

        public LabelledTextBox()
            : base(false)
        {
        }

        public bool ReadOnly
        {
            get => Component.ReadOnly;
            set => Component.ReadOnly = value;
        }

        public LocalisableString PlaceholderText
        {
            set => Component.PlaceholderText = value;
        }

        public string Text
        {
            get => Component.Text;
            set => Component.Text = value;
        }

        public CompositeDrawable TabbableContentContainer
        {
            set => Component.TabbableContentContainer = value;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Component.BorderColour = colours.Blue;
        }

        protected virtual OsuTextBox CreateTextBox() => new OsuTextBox();

        public override bool AcceptsFocus => true;

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);
            GetContainingInputManager().ChangeFocus(Component);
        }

        protected override OsuTextBox CreateComponent() => CreateTextBox().With(t =>
        {
            t.CommitOnFocusLost = true;
            t.Anchor = Anchor.Centre;
            t.Origin = Anchor.Centre;
            t.RelativeSizeAxes = Axes.X;
            t.CornerRadius = CORNER_RADIUS;

            t.OnCommit += (sender, newText) => OnCommit?.Invoke(sender, newText);
        });
    }
}
