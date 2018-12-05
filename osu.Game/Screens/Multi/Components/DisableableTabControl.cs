// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Multi.Components
{
    public abstract class DisableableTabControl<T> : TabControl<T>
    {
        public IEnumerable<T> DisabledItems
        {
            set
            {
                foreach (var item in value)
                    (TabMap[item] as DisableableTabItem<T>)?.Disable();
            }
        }

        protected abstract class DisableableTabItem<T> : TabItem<T>
        {
            protected DisableableTabItem(T value)
                : base(value)
            {
            }

            private bool isDisabled;

            public void Disable()
            {
                Alpha = 0.2f;
                isDisabled = true;
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (isDisabled)
                    return true;
                return base.OnClick(e);
            }
        }
    }
}
