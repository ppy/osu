// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Settings
{
    public abstract class SettingsItem<T> : FillFlowContainer, IFilterable
    {
        protected abstract Drawable CreateControl();

        protected Drawable Control { get; }

        private IHasCurrentValue<T> controlWithCurrent => Control as IHasCurrentValue<T>;

        private SpriteText text;

        public virtual string LabelText
        {
            get { return text?.Text ?? string.Empty; }
            set
            {
                if (text == null)
                {
                    // construct lazily for cases where the label is not needed (may be provided by the Control).
                    Add(text = new OsuSpriteText { Depth = 1 });
                }

                text.Text = value;
            }
        }

        // hold a reference to the provided bindable so we don't have to in every settings section.
        private Bindable<T> bindable;

        public virtual Bindable<T> Bindable
        {
            get
            {
                return bindable;
            }

            set
            {
                bindable = value;
                controlWithCurrent?.Current.BindTo(bindable);
            }
        }

        public IEnumerable<string> FilterTerms => new[] { LabelText };

        public bool MatchingFilter
        {
            set
            {
                // probably needs a better transition.
                this.FadeTo(value ? 1 : 0);
            }
        }

        protected SettingsItem()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = 5 };

            if ((Control = CreateControl()) != null)
            {
                if (controlWithCurrent != null)
                    controlWithCurrent.Current.DisabledChanged += disabled => { Colour = disabled ? Color4.Gray : Color4.White; };
                Add(Control);
            }
        }
    }
}
