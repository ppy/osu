// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    public class BindableBeatDivisor : BindableInt
    {
        public static readonly int[] PREDEFINED_DIVISORS = { 1, 2, 3, 4, 6, 8, 12, 16 };

        public Bindable<BeatDivisorPresetCollection> ValidDivisors { get; } = new Bindable<BeatDivisorPresetCollection>(BeatDivisorPresetCollection.COMMON);

        public BindableBeatDivisor(int value = 1)
            : base(value)
        {
            ValidDivisors.BindValueChanged(_ => updateBindableProperties(), true);
            BindValueChanged(_ => ensureValidDivisor());
        }

        /// <summary>
        /// Set a divisor, updating the valid divisor range appropriately.
        /// </summary>
        /// <param name="divisor">The intended divisor.</param>
        public void SetArbitraryDivisor(int divisor)
        {
            // If the current valid divisor range doesn't contain the proposed value, attempt to find one which does.
            if (!ValidDivisors.Value.Presets.Contains(divisor))
            {
                if (BeatDivisorPresetCollection.COMMON.Presets.Contains(divisor))
                    ValidDivisors.Value = BeatDivisorPresetCollection.COMMON;
                else if (BeatDivisorPresetCollection.TRIPLETS.Presets.Contains(divisor))
                    ValidDivisors.Value = BeatDivisorPresetCollection.TRIPLETS;
                else
                    ValidDivisors.Value = BeatDivisorPresetCollection.Custom(divisor);
            }

            Value = divisor;
        }

        private void updateBindableProperties()
        {
            ensureValidDivisor();

            MinValue = ValidDivisors.Value.Presets.Min();
            MaxValue = ValidDivisors.Value.Presets.Max();
        }

        private void ensureValidDivisor()
        {
            if (!ValidDivisors.Value.Presets.Contains(Value))
                Value = 1;
        }

        public void SelectNext()
        {
            var presets = ValidDivisors.Value.Presets;
            if (presets.Cast<int?>().SkipWhile(preset => preset != Value).ElementAtOrDefault(1) is int newValue)
                Value = newValue;
        }

        public void SelectPrevious()
        {
            var presets = ValidDivisors.Value.Presets;
            if (presets.Cast<int?>().TakeWhile(preset => preset != Value).LastOrDefault() is int newValue)
                Value = newValue;
        }

        protected override int DefaultPrecision => 1;

        public override void BindTo(Bindable<int> them)
        {
            // bind to valid divisors first (if applicable) to ensure correct transfer of the actual divisor.
            if (them is BindableBeatDivisor otherBeatDivisor)
                ValidDivisors.BindTo(otherBeatDivisor.ValidDivisors);

            base.BindTo(them);
        }

        protected override Bindable<int> CreateInstance() => new BindableBeatDivisor();

        /// <summary>
        /// Retrieves the appropriate colour for a beat divisor.
        /// </summary>
        /// <param name="beatDivisor">The beat divisor.</param>
        /// <param name="colours">The set of colours.</param>
        /// <returns>The applicable colour from <paramref name="colours"/> for <paramref name="beatDivisor"/>.</returns>
        public static Color4 GetColourFor(int beatDivisor, OsuColour colours)
        {
            switch (beatDivisor)
            {
                case 1:
                    return Color4.White;

                case 2:
                    return colours.Red;

                case 4:
                    return colours.Blue;

                case 8:
                    return colours.Yellow;

                case 16:
                    return colours.PurpleDark;

                case 3:
                    return colours.Purple;

                case 6:
                    return colours.YellowDark;

                case 12:
                    return colours.YellowDarker;

                default:
                    return Color4.Red;
            }
        }

        /// <summary>
        /// Get a relative display size for the specified divisor.
        /// </summary>
        /// <param name="beatDivisor">The beat divisor.</param>
        /// <returns>A relative size which can be used to display ticks.</returns>
        public static Vector2 GetSize(int beatDivisor)
        {
            switch (beatDivisor)
            {
                case 1:
                case 2:
                    return new Vector2(0.6f, 0.9f);

                case 3:
                case 4:
                    return new Vector2(0.5f, 0.8f);

                case 6:
                case 8:
                    return new Vector2(0.4f, 0.7f);

                default:
                    return new Vector2(0.3f, 0.6f);
            }
        }

        /// <summary>
        /// Retrieves the applicable divisor for a specific beat index.
        /// </summary>
        /// <param name="index">The 0-based beat index.</param>
        /// <param name="beatDivisor">The beat divisor.</param>
        /// <param name="validDivisors">The list of valid divisors which can be chosen from. Assumes ordered from low to high. Defaults to <see cref="PREDEFINED_DIVISORS"/> if omitted.</param>
        /// <returns>The applicable divisor.</returns>
        public static int GetDivisorForBeatIndex(int index, int beatDivisor, int[] validDivisors = null)
        {
            validDivisors ??= PREDEFINED_DIVISORS;

            int beat = index % beatDivisor;

            foreach (int divisor in validDivisors)
            {
                if ((beat * divisor) % beatDivisor == 0)
                    return divisor;
            }

            return 0;
        }
    }
}
