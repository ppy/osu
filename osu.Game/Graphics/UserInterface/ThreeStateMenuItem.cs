// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class ThreeStateMenuItem : StatefulMenuItem<ThreeStates>
    {
        public ThreeStateMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : base(text, type, getNextState)
        {
        }

        public override IconUsage? GetIconForState(ThreeStates state)
        {
            switch (state)
            {
                case ThreeStates.Indeterminate:
                    return FontAwesome.Regular.Circle;

                case ThreeStates.Enabled:
                    return FontAwesome.Solid.Check;
            }

            return null;
        }

        private static ThreeStates getNextState(ThreeStates state)
        {
            switch (state)
            {
                case ThreeStates.Disabled:
                    return ThreeStates.Enabled;

                case ThreeStates.Indeterminate:
                    return ThreeStates.Enabled;

                case ThreeStates.Enabled:
                    return ThreeStates.Disabled;
            }

            return ThreeStates.Disabled;
        }
    }

    public enum ThreeStates
    {
        Disabled,
        Indeterminate,
        Enabled
    }
}
