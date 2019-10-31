// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public abstract class LabelledComponent<T, U> : LabelledDrawable<T>, IHasCurrentValue<U>
        where T : Drawable, IHasCurrentValue<U>
    {
        protected LabelledComponent(bool padded)
            : base(padded)
        {
        }

        public Bindable<U> Current
        {
            get => Component.Current;
            set => Component.Current = value;
        }
    }
}
