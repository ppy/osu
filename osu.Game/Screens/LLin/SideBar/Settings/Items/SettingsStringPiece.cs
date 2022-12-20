using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.LLin.Plugins.Config;

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public partial class SettingsStringPiece : SettingsPieceBasePanel, ISettingsItem<string>
    {
        private OsuTextBox textBox;
        public LocalisableString TooltipText { get; set; }
        public Bindable<string> Bindable { get; set; }

        private void updateWidth(int rows)
        {
            Schedule(() =>
            {
                this.ResizeWidthTo(150 * rows + 5 * (rows - 1), 300, Easing.OutQuint);
            });
        }

        private OsuSpriteText text = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 20),
            Margin = new MarginPadding { Horizontal = 10, Top = 5 },
            Alpha = 0
        };

        public override LocalisableString Description
        {
            get => base.Description;
            set
            {
                base.Description = value;
                text.Text = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider ccp, NewPluginSettingsSection npss)
        {
            npss.OnNewMaxRows += r =>
            {
                Schedule(() => updateWidth(r));
            };

            updateWidth(npss.MaxRows);

            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = 10, Bottom = 7.5f },
                Child = textBox = new SettingsTextBox
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Height = 0.6f,
                    CommitOnFocusLost = true,
                    Alpha = 0,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    BorderColour = ccp.Highlight1
                }
            });

            AddInternal(text);

            textBox.OnCommit += onTextBoxCommit;
        }

        protected override void LoadComplete()
        {
            textBox.Current.BindTo(Bindable);
            base.LoadComplete();
        }

        private void onTextBoxCommit(TextBox sender, bool newtext)
        {
            hideTextBox();
        }

        protected override bool OnHover(HoverEvent e)
        {
            showTextBox();
            return base.OnHover(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            showTextBox();
            return base.OnClick(e);
        }

        private void hideTextBox()
        {
            textBox.FadeOut(200, Easing.OutQuint);
            text.FadeOut(200, Easing.OutQuint);
            FillFlow.Delay(200).FadeIn(200, Easing.OutQuint);
        }

        private void showTextBox()
        {
            FillFlow.FadeTo(0.01f, 200, Easing.OutQuint);
            textBox.Delay(200).FadeIn(200, Easing.OutQuint);
            text.Delay(200).FadeIn(200, Easing.OutQuint);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!HasFocus && !textBox.HasFocus) hideTextBox();

            base.OnHoverLost(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            hideTextBox();
            base.OnFocusLost(e);
        }

        protected override void OnMiddleClick()
        {
            Bindable.Value = Bindable.Default;
            base.OnMiddleClick();
        }

        private partial class SettingsTextBox : OutlinedTextBox
        {
            [BackgroundDependencyLoader]
            private void load(CustomColourProvider ccp)
            {
                BackgroundCommit = ccp.Highlight1;
            }

            protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
            {
                //workaround: 字号不正确
                AutoSizeAxes = Axes.Both,
                Child = new OsuSpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: 35 * 0.6f) },
            };
        }
    }
}
