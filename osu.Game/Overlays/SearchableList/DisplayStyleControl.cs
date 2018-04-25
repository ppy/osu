// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.SearchableList
{
    public class DisplayStyleControl<T> : Container
    {
        public readonly SlimEnumDropdown<T> Dropdown;
        public readonly Bindable<PanelDisplayStyle> DisplayStyle = new Bindable<PanelDisplayStyle>();

        public DisplayStyleControl()
        {
            AutoSizeAxes = Axes.Both;

            Children = new[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Spacing = new Vector2(10f, 0f),
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(5f, 0f),
                            Direction = FillDirection.Horizontal,
                            Children = new[]
                            {
                                new DisplayStyleToggleButton(FontAwesome.fa_th_large, PanelDisplayStyle.Grid, DisplayStyle),
                                new DisplayStyleToggleButton(FontAwesome.fa_list_ul, PanelDisplayStyle.List, DisplayStyle),
                            },
                        },
                        Dropdown = new SlimEnumDropdown<T>
                        {
                            RelativeSizeAxes = Axes.None,
                            Width = 160f,
                        },
                    },
                },
            };

            DisplayStyle.Value = PanelDisplayStyle.Grid;
        }

        private class DisplayStyleToggleButton : OsuClickableContainer
        {
            private readonly SpriteIcon icon;
            private readonly PanelDisplayStyle style;
            private readonly Bindable<PanelDisplayStyle> bindable;

            public DisplayStyleToggleButton(FontAwesome icon, PanelDisplayStyle style, Bindable<PanelDisplayStyle> bindable)
            {
                this.bindable = bindable;
                this.style = style;
                Size = new Vector2(25f);

                Children = new Drawable[]
                {
                    this.icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = icon,
                        Size = new Vector2(18),
                        Alpha = 0.5f,
                    },
                };

                bindable.ValueChanged += Bindable_ValueChanged;
                Bindable_ValueChanged(bindable.Value);
                Action = () => bindable.Value = this.style;
            }

            private void Bindable_ValueChanged(PanelDisplayStyle style)
            {
                icon.FadeTo(style == this.style ? 1.0f : 0.5f, 100);
            }

            protected override void Dispose(bool isDisposing)
            {
                bindable.ValueChanged -= Bindable_ValueChanged;
            }
        }
    }

    public enum PanelDisplayStyle
    {
        Grid,
        List,
    }
}
