// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web.ModelValidation.Store
{
    public static class ProductStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.ModelValidation.Store.Product";

        /// <summary>
        /// "There is not enough of this item left!"
        /// </summary>
        public static LocalisableString InsufficientStock => new TranslatableString(getKey(@"insufficient_stock"), @"There is not enough of this item left!");

        /// <summary>
        /// "This item has to be checked out separately from other items"
        /// </summary>
        public static LocalisableString MustSeparate => new TranslatableString(getKey(@"must_separate"), @"This item has to be checked out separately from other items");

        /// <summary>
        /// "This item is not available."
        /// </summary>
        public static LocalisableString NotAvailable => new TranslatableString(getKey(@"not_available"), @"This item is not available.");

        /// <summary>
        /// "You can only order {0} of this item per order."
        /// </summary>
        public static LocalisableString TooMany(string count) => new TranslatableString(getKey(@"too_many"), @"You can only order {0} of this item per order.", count);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}