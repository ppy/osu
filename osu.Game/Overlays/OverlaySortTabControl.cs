// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osu.Game.Overlays.Comments;
using JetBrains.Annotations;
using System;
using osu.Framework.Extensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays
{
    public partial class OverlaySortTabControl<T> : CompositeDrawable, IHasCurrentValue<T>
    {
        public TabControl<T> TabControl { get; }

        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public LocalisableString Title
        {
            get => text.Text;
            set => text.Text = value;
        }

        private readonly OsuSpriteText text;

        public OverlaySortTabControl()
        {
            AutoSizeAxes = Axes.Both;
            AddInternal(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                        Text = SortStrings.Default
                    },
                    TabControl = CreateControl().With(c =>
                    {
                        c.Anchor = Anchor.CentreLeft;
                        c.Origin = Anchor.CentreLeft;
                        c.Current = current;
                    })
                }
            });
        }

        [NotNull]
        protected virtual SortTabControl CreateControl() => new SortTabControl();

        protected partial class SortTabControl : OsuTabControl<T>
        {
            protected override Dropdown<T> CreateDropdown() => null;

            protected override TabItem<T> CreateTabItem(T value) => new SortTabItem(value);

            protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
            };

            public SortTabControl()
            {
                AutoSizeAxes = Axes.Both;
            }
        }

        protected partial class SortTabItem : TabItem<T>
        {
            public SortTabItem(T value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;
                Child = CreateTabButton(value);
            }

            [NotNull]
            protected virtual TabButton CreateTabButton(T value) => new TabButton(value)
            {
                Active = { BindTarget = Active }
            };

            protected override void OnActivated()
            {
            }

            protected override void OnDeactivated()
            {
            }
        }

        public partial class TabButton : HeaderButton
        {
            public readonly BindableBool Active = new BindableBool();

            protected override Container<Drawable> Content => content;

            protected virtual Color4 ContentColour
            {
                set => text.Colour = value;
            }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            private readonly SpriteText text;
            private readonly FillFlowContainer content;

            public TabButton(T value)
            {
                base.Content.Add(content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(3, 0),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                            Text = (value as Enum)?.GetLocalisableDescription() ?? value.ToString()
                        }
                    }
                });

                AddInternal(new HoverSounds(HoverSampleSet.TabSelect));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Active.BindValueChanged(_ => UpdateState(), true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                UpdateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e) => UpdateState();

            protected virtual void UpdateState()
            {
                if (Active.Value || IsHovered)
                    ShowBackground();
                else
                    HideBackground();

                ContentColour = Active.Value && !IsHovered ? colourProvider.Light1 : Color4.White;

                text.Font = text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.SemiBold);
            }
        }
    }
}
