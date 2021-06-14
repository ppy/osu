// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class StoreStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Store";

        /// <summary>
        /// "Warehouse"
        /// </summary>
        public static LocalisableString AdminWarehouse => new TranslatableString(getKey(@"admin.warehouse"), @"Warehouse");

        /// <summary>
        /// "Checkout"
        /// </summary>
        public static LocalisableString CartCheckout => new TranslatableString(getKey(@"cart.checkout"), @"Checkout");

        /// <summary>
        /// "{0} item in cart (${1})|{0} items in cart (${1})"
        /// </summary>
        public static LocalisableString CartInfo(string countDelimited, string subtotal) => new TranslatableString(getKey(@"cart.info"), @"{0} item in cart (${1})|{0} items in cart (${1})", countDelimited, subtotal);

        /// <summary>
        /// "I want to check out more goodies before completing the order"
        /// </summary>
        public static LocalisableString CartMoreGoodies => new TranslatableString(getKey(@"cart.more_goodies"), @"I want to check out more goodies before completing the order");

        /// <summary>
        /// "shipping fees"
        /// </summary>
        public static LocalisableString CartShippingFees => new TranslatableString(getKey(@"cart.shipping_fees"), @"shipping fees");

        /// <summary>
        /// "Shopping Cart"
        /// </summary>
        public static LocalisableString CartTitle => new TranslatableString(getKey(@"cart.title"), @"Shopping Cart");

        /// <summary>
        /// "total"
        /// </summary>
        public static LocalisableString CartTotal => new TranslatableString(getKey(@"cart.total"), @"total");

        /// <summary>
        /// "Uh oh, there are problems with your cart preventing a checkout!"
        /// </summary>
        public static LocalisableString CartErrorsNoCheckoutLine1 => new TranslatableString(getKey(@"cart.errors_no_checkout.line_1"), @"Uh oh, there are problems with your cart preventing a checkout!");

        /// <summary>
        /// "Remove or update items above to continue."
        /// </summary>
        public static LocalisableString CartErrorsNoCheckoutLine2 => new TranslatableString(getKey(@"cart.errors_no_checkout.line_2"), @"Remove or update items above to continue.");

        /// <summary>
        /// "Your cart is empty."
        /// </summary>
        public static LocalisableString CartEmptyText => new TranslatableString(getKey(@"cart.empty.text"), @"Your cart is empty.");

        /// <summary>
        /// "Return to the {0} to find some goodies!"
        /// </summary>
        public static LocalisableString CartEmptyReturnLinkDefault(string link) => new TranslatableString(getKey(@"cart.empty.return_link._"), @"Return to the {0} to find some goodies!", link);

        /// <summary>
        /// "store listing"
        /// </summary>
        public static LocalisableString CartEmptyReturnLinkLinkText => new TranslatableString(getKey(@"cart.empty.return_link.link_text"), @"store listing");

        /// <summary>
        /// "Uh oh, there are problems with your cart!"
        /// </summary>
        public static LocalisableString CheckoutCartProblems => new TranslatableString(getKey(@"checkout.cart_problems"), @"Uh oh, there are problems with your cart!");

        /// <summary>
        /// "Click here to go edit it."
        /// </summary>
        public static LocalisableString CheckoutCartProblemsEdit => new TranslatableString(getKey(@"checkout.cart_problems_edit"), @"Click here to go edit it.");

        /// <summary>
        /// "The payment was cancelled."
        /// </summary>
        public static LocalisableString CheckoutDeclined => new TranslatableString(getKey(@"checkout.declined"), @"The payment was cancelled.");

        /// <summary>
        /// "We are currently overwhelmed with orders! You are welcome to place your order, but please expect an **additional 1-2 week delay** while we catch up with existing orders."
        /// </summary>
        public static LocalisableString CheckoutDelayedShipping => new TranslatableString(getKey(@"checkout.delayed_shipping"), @"We are currently overwhelmed with orders! You are welcome to place your order, but please expect an **additional 1-2 week delay** while we catch up with existing orders.");

        /// <summary>
        /// "Your cart appears to be out of date and has been reloaded, please try again."
        /// </summary>
        public static LocalisableString CheckoutOldCart => new TranslatableString(getKey(@"checkout.old_cart"), @"Your cart appears to be out of date and has been reloaded, please try again.");

        /// <summary>
        /// "Checkout with Paypal"
        /// </summary>
        public static LocalisableString CheckoutPay => new TranslatableString(getKey(@"checkout.pay"), @"Checkout with Paypal");

        /// <summary>
        /// "checkout"
        /// </summary>
        public static LocalisableString CheckoutTitleCompact => new TranslatableString(getKey(@"checkout.title_compact"), @"checkout");

        /// <summary>
        /// "You have incomplete checkouts, click {0} to view them."
        /// </summary>
        public static LocalisableString CheckoutHasPendingDefault(string link) => new TranslatableString(getKey(@"checkout.has_pending._"), @"You have incomplete checkouts, click {0} to view them.", link);

        /// <summary>
        /// "here"
        /// </summary>
        public static LocalisableString CheckoutHasPendingLinkText => new TranslatableString(getKey(@"checkout.has_pending.link_text"), @"here");

        /// <summary>
        /// "A previous checkout was started but did not finish."
        /// </summary>
        public static LocalisableString CheckoutPendingCheckoutLine1 => new TranslatableString(getKey(@"checkout.pending_checkout.line_1"), @"A previous checkout was started but did not finish.");

        /// <summary>
        /// "Resume your checkout by selecting a payment method."
        /// </summary>
        public static LocalisableString CheckoutPendingCheckoutLine2 => new TranslatableString(getKey(@"checkout.pending_checkout.line_2"), @"Resume your checkout by selecting a payment method.");

        /// <summary>
        /// "save {0}%"
        /// </summary>
        public static LocalisableString Discount(string percent) => new TranslatableString(getKey(@"discount"), @"save {0}%", percent);

        /// <summary>
        /// "As your payment was an eCheck, please allow up to 10 extra days for the payment to clear through PayPal!"
        /// </summary>
        public static LocalisableString InvoiceEcheckDelay => new TranslatableString(getKey(@"invoice.echeck_delay"), @"As your payment was an eCheck, please allow up to 10 extra days for the payment to clear through PayPal!");

        /// <summary>
        /// "invoice"
        /// </summary>
        public static LocalisableString InvoiceTitleCompact => new TranslatableString(getKey(@"invoice.title_compact"), @"invoice");

        /// <summary>
        /// "Your payment has not yet been confirmed!"
        /// </summary>
        public static LocalisableString InvoiceStatusProcessingTitle => new TranslatableString(getKey(@"invoice.status.processing.title"), @"Your payment has not yet been confirmed!");

        /// <summary>
        /// "If you have already paid, we may still be waiting to receive confirmation of your payment. Please refresh this page in a minute or two!"
        /// </summary>
        public static LocalisableString InvoiceStatusProcessingLine1 => new TranslatableString(getKey(@"invoice.status.processing.line_1"), @"If you have already paid, we may still be waiting to receive confirmation of your payment. Please refresh this page in a minute or two!");

        /// <summary>
        /// "If you encountered a problem during checkout, {0}"
        /// </summary>
        public static LocalisableString InvoiceStatusProcessingLine2Default(string link) => new TranslatableString(getKey(@"invoice.status.processing.line_2._"), @"If you encountered a problem during checkout, {0}", link);

        /// <summary>
        /// "click here to resume your checkout"
        /// </summary>
        public static LocalisableString InvoiceStatusProcessingLine2LinkText => new TranslatableString(getKey(@"invoice.status.processing.line_2.link_text"), @"click here to resume your checkout");

        /// <summary>
        /// "Cancel Order"
        /// </summary>
        public static LocalisableString OrderCancel => new TranslatableString(getKey(@"order.cancel"), @"Cancel Order");

        /// <summary>
        /// "This order will be cancelled and payment will not be accepted for it. The payment provider might not release any reserved funds immediately. Are you sure?"
        /// </summary>
        public static LocalisableString OrderCancelConfirm => new TranslatableString(getKey(@"order.cancel_confirm"), @"This order will be cancelled and payment will not be accepted for it. The payment provider might not release any reserved funds immediately. Are you sure?");

        /// <summary>
        /// "This order cannot be cancelled at this time."
        /// </summary>
        public static LocalisableString OrderCancelNotAllowed => new TranslatableString(getKey(@"order.cancel_not_allowed"), @"This order cannot be cancelled at this time.");

        /// <summary>
        /// "View Invoice"
        /// </summary>
        public static LocalisableString OrderInvoice => new TranslatableString(getKey(@"order.invoice"), @"View Invoice");

        /// <summary>
        /// "No orders to view."
        /// </summary>
        public static LocalisableString OrderNoOrders => new TranslatableString(getKey(@"order.no_orders"), @"No orders to view.");

        /// <summary>
        /// "Order placed {0}"
        /// </summary>
        public static LocalisableString OrderPaidOn(string date) => new TranslatableString(getKey(@"order.paid_on"), @"Order placed {0}", date);

        /// <summary>
        /// "Resume Checkout"
        /// </summary>
        public static LocalisableString OrderResume => new TranslatableString(getKey(@"order.resume"), @"Resume Checkout");

        /// <summary>
        /// "The checkout link for this order has expired."
        /// </summary>
        public static LocalisableString OrderShopifyExpired => new TranslatableString(getKey(@"order.shopify_expired"), @"The checkout link for this order has expired.");

        /// <summary>
        /// "{0} for {1} ({2})"
        /// </summary>
        public static LocalisableString OrderItemDisplayNameSupporterTag(string name, string username, string duration) => new TranslatableString(getKey(@"order.item.display_name.supporter_tag"), @"{0} for {1} ({2})", name, username, duration);

        /// <summary>
        /// "Quantity"
        /// </summary>
        public static LocalisableString OrderItemQuantity => new TranslatableString(getKey(@"order.item.quantity"), @"Quantity");

        /// <summary>
        /// "You cannot modify your order as it has been cancelled."
        /// </summary>
        public static LocalisableString OrderNotModifiableExceptionCancelled => new TranslatableString(getKey(@"order.not_modifiable_exception.cancelled"), @"You cannot modify your order as it has been cancelled.");

        /// <summary>
        /// "You cannot modify your order while it is being processed."
        /// </summary>
        public static LocalisableString OrderNotModifiableExceptionCheckout => new TranslatableString(getKey(@"order.not_modifiable_exception.checkout"), @"You cannot modify your order while it is being processed.");

        /// <summary>
        /// "Order is not modifiable"
        /// </summary>
        public static LocalisableString OrderNotModifiableExceptionDefault => new TranslatableString(getKey(@"order.not_modifiable_exception.default"), @"Order is not modifiable");

        /// <summary>
        /// "You cannot modify your order as it has already been delivered."
        /// </summary>
        public static LocalisableString OrderNotModifiableExceptionDelivered => new TranslatableString(getKey(@"order.not_modifiable_exception.delivered"), @"You cannot modify your order as it has already been delivered.");

        /// <summary>
        /// "You cannot modify your order as it has already been paid for."
        /// </summary>
        public static LocalisableString OrderNotModifiableExceptionPaid => new TranslatableString(getKey(@"order.not_modifiable_exception.paid"), @"You cannot modify your order as it has already been paid for.");

        /// <summary>
        /// "You cannot modify your order while it is being processed."
        /// </summary>
        public static LocalisableString OrderNotModifiableExceptionProcessing => new TranslatableString(getKey(@"order.not_modifiable_exception.processing"), @"You cannot modify your order while it is being processed.");

        /// <summary>
        /// "You cannot modify your order as it has already been shipped."
        /// </summary>
        public static LocalisableString OrderNotModifiableExceptionShipped => new TranslatableString(getKey(@"order.not_modifiable_exception.shipped"), @"You cannot modify your order as it has already been shipped.");

        /// <summary>
        /// "Cancelled"
        /// </summary>
        public static LocalisableString OrderStatusCancelled => new TranslatableString(getKey(@"order.status.cancelled"), @"Cancelled");

        /// <summary>
        /// "Preparing"
        /// </summary>
        public static LocalisableString OrderStatusCheckout => new TranslatableString(getKey(@"order.status.checkout"), @"Preparing");

        /// <summary>
        /// "Delivered"
        /// </summary>
        public static LocalisableString OrderStatusDelivered => new TranslatableString(getKey(@"order.status.delivered"), @"Delivered");

        /// <summary>
        /// "Paid"
        /// </summary>
        public static LocalisableString OrderStatusPaid => new TranslatableString(getKey(@"order.status.paid"), @"Paid");

        /// <summary>
        /// "Pending confirmation"
        /// </summary>
        public static LocalisableString OrderStatusProcessing => new TranslatableString(getKey(@"order.status.processing"), @"Pending confirmation");

        /// <summary>
        /// "Shipped"
        /// </summary>
        public static LocalisableString OrderStatusShipped => new TranslatableString(getKey(@"order.status.shipped"), @"Shipped");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString ProductName => new TranslatableString(getKey(@"product.name"), @"Name");

        /// <summary>
        /// "This item is currently out of stock. Check back later!"
        /// </summary>
        public static LocalisableString ProductStockOut => new TranslatableString(getKey(@"product.stock.out"), @"This item is currently out of stock. Check back later!");

        /// <summary>
        /// "Unfortunately this item is out of stock. Use the dropdown to choose a different type or check back later!"
        /// </summary>
        public static LocalisableString ProductStockOutWithAlternative => new TranslatableString(getKey(@"product.stock.out_with_alternative"), @"Unfortunately this item is out of stock. Use the dropdown to choose a different type or check back later!");

        /// <summary>
        /// "Add to Cart"
        /// </summary>
        public static LocalisableString ProductAddToCart => new TranslatableString(getKey(@"product.add_to_cart"), @"Add to Cart");

        /// <summary>
        /// "Notify me when available!"
        /// </summary>
        public static LocalisableString ProductNotify => new TranslatableString(getKey(@"product.notify"), @"Notify me when available!");

        /// <summary>
        /// "you will be notified when we have new stock. click {0} to cancel"
        /// </summary>
        public static LocalisableString ProductNotificationSuccess(string link) => new TranslatableString(getKey(@"product.notification_success"), @"you will be notified when we have new stock. click {0} to cancel", link);

        /// <summary>
        /// "here"
        /// </summary>
        public static LocalisableString ProductNotificationRemoveText => new TranslatableString(getKey(@"product.notification_remove_text"), @"here");

        /// <summary>
        /// "This product is already in stock!"
        /// </summary>
        public static LocalisableString ProductNotificationInStock => new TranslatableString(getKey(@"product.notification_in_stock"), @"This product is already in stock!");

        /// <summary>
        /// "gift to player"
        /// </summary>
        public static LocalisableString SupporterTagGift => new TranslatableString(getKey(@"supporter_tag.gift"), @"gift to player");

        /// <summary>
        /// "You need to be {0} to get an osu!supporter tag!"
        /// </summary>
        public static LocalisableString SupporterTagRequireLoginDefault(string link) => new TranslatableString(getKey(@"supporter_tag.require_login._"), @"You need to be {0} to get an osu!supporter tag!", link);

        /// <summary>
        /// "signed in"
        /// </summary>
        public static LocalisableString SupporterTagRequireLoginLinkText => new TranslatableString(getKey(@"supporter_tag.require_login.link_text"), @"signed in");

        /// <summary>
        /// "Enter a username to check availability!"
        /// </summary>
        public static LocalisableString UsernameChangeCheck => new TranslatableString(getKey(@"username_change.check"), @"Enter a username to check availability!");

        /// <summary>
        /// "Checking availability of {0}..."
        /// </summary>
        public static LocalisableString UsernameChangeChecking(string username) => new TranslatableString(getKey(@"username_change.checking"), @"Checking availability of {0}...", username);

        /// <summary>
        /// "You need to be {0} to change your name!"
        /// </summary>
        public static LocalisableString UsernameChangeRequireLoginDefault(string link) => new TranslatableString(getKey(@"username_change.require_login._"), @"You need to be {0} to change your name!", link);

        /// <summary>
        /// "signed in"
        /// </summary>
        public static LocalisableString UsernameChangeRequireLoginLinkText => new TranslatableString(getKey(@"username_change.require_login.link_text"), @"signed in");

        /// <summary>
        /// "Xsolla is an authorised&lt;br&gt;global distributor of osu!"
        /// </summary>
        public static LocalisableString XsollaDistributor => new TranslatableString(getKey(@"xsolla.distributor"), @"Xsolla is an authorised<br>global distributor of osu!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}