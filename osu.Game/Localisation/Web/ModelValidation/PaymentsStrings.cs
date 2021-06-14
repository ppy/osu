// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web.ModelValidation
{
    public static class PaymentsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.ModelValidation.Payments";

        /// <summary>
        /// "Signatures do not match"
        /// </summary>
        public static LocalisableString SignatureNotMatch => new TranslatableString(getKey(@"signature.not_match"), @"Signatures do not match");

        /// <summary>
        /// "notification_type is not valid {0}"
        /// </summary>
        public static LocalisableString NotificationType(string type) => new TranslatableString(getKey(@"notification_type"), @"notification_type is not valid {0}", type);

        /// <summary>
        /// "Order is not valid"
        /// </summary>
        public static LocalisableString OrderInvalid => new TranslatableString(getKey(@"order.invalid"), @"Order is not valid");

        /// <summary>
        /// "`{0}` payment is not valid for physical items."
        /// </summary>
        public static LocalisableString OrderItemsVirtualOnly(string provider) => new TranslatableString(getKey(@"order.items.virtual_only"), @"`{0}` payment is not valid for physical items.", provider);

        /// <summary>
        /// "Trying to accept payment for an order in the wrong state `{0}`."
        /// </summary>
        public static LocalisableString OrderStatusNotCheckout(string state) => new TranslatableString(getKey(@"order.status.not_checkout"), @"Trying to accept payment for an order in the wrong state `{0}`.", state);

        /// <summary>
        /// "Trying to refund payment for an order in the wrong state `{0}`."
        /// </summary>
        public static LocalisableString OrderStatusNotPaid(string state) => new TranslatableString(getKey(@"order.status.not_paid"), @"Trying to refund payment for an order in the wrong state `{0}`.", state);

        /// <summary>
        /// "`{0}` param does not match"
        /// </summary>
        public static LocalisableString ParamInvalid(string param) => new TranslatableString(getKey(@"param.invalid"), @"`{0}` param does not match", param);

        /// <summary>
        /// "Pending payment is not an echeck. ({0})"
        /// </summary>
        public static LocalisableString PaypalNotEcheck(string actual) => new TranslatableString(getKey(@"paypal.not_echeck"), @"Pending payment is not an echeck. ({0})", actual);

        /// <summary>
        /// "Payment amount does not match: {0} != {1}"
        /// </summary>
        public static LocalisableString PurchaseCheckoutAmount(string actual, string expected) => new TranslatableString(getKey(@"purchase.checkout.amount"), @"Payment amount does not match: {0} != {1}", actual, expected);

        /// <summary>
        /// "Payment is not in USD. ({0})"
        /// </summary>
        public static LocalisableString PurchaseCheckoutCurrency(string type) => new TranslatableString(getKey(@"purchase.checkout.currency"), @"Payment is not in USD. ({0})", type);

        /// <summary>
        /// "Received order transaction id is malformed"
        /// </summary>
        public static LocalisableString OrderNumberMalformed => new TranslatableString(getKey(@"order_number.malformed"), @"Received order transaction id is malformed");

        /// <summary>
        /// "external_id contains wrong user id"
        /// </summary>
        public static LocalisableString OrderNumberUserIdMismatch => new TranslatableString(getKey(@"order_number.user_id_mismatch"), @"external_id contains wrong user id");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}