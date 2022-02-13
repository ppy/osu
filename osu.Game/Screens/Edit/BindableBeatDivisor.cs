// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Compose.Components;
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

        public void Next()
        {
            var presets = ValidDivisors.Value.Presets;
            Value = presets.Cast<int?>().SkipWhile(preset => preset != Value).ElementAtOrDefault(1) ?? presets[0];
        }

        public void Previous()
        {
            var presets = ValidDivisors.Value.Presets;
            Value = presets.Cast<int?>().TakeWhile(preset => preset != Value).LastOrDefault() ?? presets[^1];
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
        /// Retrieves the applicable divisor for a specific beat index.
        /// </summary>
        /// <param name="index">The 0-based beat index.</param>
        /// <param name="beatDivisor">The beat divisor.</param>
        /// <returns>The applicable divisor.</returns>
        public static int GetDivisorForBeatIndex(int index, int beatDivisor)
        {
            int beat = index % beatDivisor;

            foreach (int divisor in PREDEFINED_DIVISORS)
            {
                if ((beat * divisor) % beatDivisor == 0)
                    return divisor;
            }

            return 0;
        }
    }
}
