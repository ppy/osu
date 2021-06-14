// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web.Paypal
{
    public static class ErrorsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Paypal.Errors";

        /// <summary>
        /// "The selected payment method was rejected by Paypal."
        /// </summary>
        public static LocalisableString InstrumentDeclined => new TranslatableString(getKey(@"instrument_declined"), @"The selected payment method was rejected by Paypal.");

        /// <summary>
        /// "No payment information was found."
        /// </summary>
        public static LocalisableString InvalidResourceId => new TranslatableString(getKey(@"invalid_resource_id"), @"No payment information was found.");

        /// <summary>
        /// "The payment was rejected, but we&#39;re not sure why."
        /// </summary>
        public static LocalisableString Unknown => new TranslatableString(getKey(@"unknown"), @"The payment was rejected, but we're not sure why.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}