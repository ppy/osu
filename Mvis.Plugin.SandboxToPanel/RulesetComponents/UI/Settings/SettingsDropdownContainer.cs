using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI.Settings
{
    public abstract partial class SettingsDropdownContainer : CompositeDrawable
    {
        private readonly BindableBool expanded = new BindableBool();

        protected SettingsDropdownContainer(string label)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Horizontal = 20 };
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Header(label)
                    {
                        Action = expanded.Toggle
                    },
                    new DropdownContent
                    {
                        Expanded = { BindTarget = expanded },
                        Child = CreateContent().With(c =>
                        {
                            c.Anchor = Anchor.TopCentre;
                            c.Origin = Anchor.TopCentre;
                        })
                    }
                }
            };
        }

        protected abstract Drawable CreateContent();

        private partial class Header : OsuDropdown<string>.OsuDropdownHeader
        {
            public Header(string label)
            {
                Label = label;
                Enabled.Value = true;
            }
        }

        private partial class DropdownContent : Container
        {
            private const int animation_duration = 250;

            public readonly BindableBool Expanded = new BindableBool();

            protected override Container<Drawable> Content { get; }

            public DropdownContent()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                AutoSizeDuration = animation_duration;
                AutoSizeEasing = Easing.Out;
                InternalChild = Content = new Container
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Alpha = 0
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Expanded.BindValueChanged(updateState, true);
            }

            private void updateState(ValueChangedEvent<bool> expanded)
            {
                ClearTransforms(true);

                if (expanded.NewValue)
                {
                    AutoSizeAxes = Axes.Y;
                    Content.FadeIn(animation_duration, Easing.OutQuint);
                }
                else
                {
                    AutoSizeAxes = Axes.None;
                    this.ResizeHeightTo(0, animation_duration, Easing.OutQuint);

                    Content.FadeOut(animation_duration, Easing.OutQuint);
                }
            }
        }
    }
}
