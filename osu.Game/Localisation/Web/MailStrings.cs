// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class MailStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Mail";

        /// <summary>
        /// "Just letting you know that there has been a new update in beatmap &quot;{0}&quot; since your last visit."
        /// </summary>
        public static LocalisableString BeatmapsetUpdateNoticeNew(string title) => new TranslatableString(getKey(@"beatmapset_update_notice.new"), @"Just letting you know that there has been a new update in beatmap ""{0}"" since your last visit.", title);

        /// <summary>
        /// "New update for beatmap &quot;{0}&quot;"
        /// </summary>
        public static LocalisableString BeatmapsetUpdateNoticeSubject(string title) => new TranslatableString(getKey(@"beatmapset_update_notice.subject"), @"New update for beatmap ""{0}""", title);

        /// <summary>
        /// "If you no longer wish to watch this beatmap, you can either click the &quot;Unwatch&quot; link found in the page above, or from the modding watchlist page:"
        /// </summary>
        public static LocalisableString BeatmapsetUpdateNoticeUnwatch => new TranslatableString(getKey(@"beatmapset_update_notice.unwatch"), @"If you no longer wish to watch this beatmap, you can either click the ""Unwatch"" link found in the page above, or from the modding watchlist page:");

        /// <summary>
        /// "Visit the discussion page here:"
        /// </summary>
        public static LocalisableString BeatmapsetUpdateNoticeVisit => new TranslatableString(getKey(@"beatmapset_update_notice.visit"), @"Visit the discussion page here:");

        /// <summary>
        /// "Regards,"
        /// </summary>
        public static LocalisableString CommonClosing => new TranslatableString(getKey(@"common.closing"), @"Regards,");

        /// <summary>
        /// "Hi {0},"
        /// </summary>
        public static LocalisableString CommonHello(string user) => new TranslatableString(getKey(@"common.hello"), @"Hi {0},", user);

        /// <summary>
        /// "Please reply to this email IMMEDIATELY if you did not request this change."
        /// </summary>
        public static LocalisableString CommonReport => new TranslatableString(getKey(@"common.report"), @"Please reply to this email IMMEDIATELY if you did not request this change.");

        /// <summary>
        /// "More new supporter benefits will appear over time, as well!"
        /// </summary>
        public static LocalisableString DonationThanksBenefitMore => new TranslatableString(getKey(@"donation_thanks.benefit_more"), @"More new supporter benefits will appear over time, as well!");

        /// <summary>
        /// "If you have any questions or feedback, don&#39;t hesitate to reply to this mail; I&#39;ll get back to you as soon as possible!"
        /// </summary>
        public static LocalisableString DonationThanksFeedback => new TranslatableString(getKey(@"donation_thanks.feedback"), @"If you have any questions or feedback, don't hesitate to reply to this mail; I'll get back to you as soon as possible!");

        /// <summary>
        /// "It is thanks to people like you that osu! is able to keep the game and community running smoothly without any advertisements or forced payments."
        /// </summary>
        public static LocalisableString DonationThanksKeepFree => new TranslatableString(getKey(@"donation_thanks.keep_free"), @"It is thanks to people like you that osu! is able to keep the game and community running smoothly without any advertisements or forced payments.");

        /// <summary>
        /// "Your support keeps osu! running for around {0}! It may not seem like much, but it all adds up :)."
        /// </summary>
        public static LocalisableString DonationThanksKeepRunning(string minutes) => new TranslatableString(getKey(@"donation_thanks.keep_running"), @"Your support keeps osu! running for around {0}! It may not seem like much, but it all adds up :).", minutes);

        /// <summary>
        /// "Thanks, osu! &lt;3s you"
        /// </summary>
        public static LocalisableString DonationThanksSubject => new TranslatableString(getKey(@"donation_thanks.subject"), @"Thanks, osu! <3s you");

        /// <summary>
        /// "A community-provided translation for informational purposes follows:"
        /// </summary>
        public static LocalisableString DonationThanksTranslation => new TranslatableString(getKey(@"donation_thanks.translation"), @"A community-provided translation for informational purposes follows:");

        /// <summary>
        /// "Your giftee(s) will now have access to osu!direct and many other supporter benefits."
        /// </summary>
        public static LocalisableString DonationThanksBenefitGift => new TranslatableString(getKey(@"donation_thanks.benefit.gift"), @"Your giftee(s) will now have access to osu!direct and many other supporter benefits.");

        /// <summary>
        /// "You will now have access to osu!direct and many other supporter benefits for {0}."
        /// </summary>
        public static LocalisableString DonationThanksBenefitSelf(string duration) => new TranslatableString(getKey(@"donation_thanks.benefit.self"), @"You will now have access to osu!direct and many other supporter benefits for {0}.", duration);

        /// <summary>
        /// "Thanks a lot for your {0} towards osu!."
        /// </summary>
        public static LocalisableString DonationThanksSupportDefault(string support) => new TranslatableString(getKey(@"donation_thanks.support._"), @"Thanks a lot for your {0} towards osu!.", support);

        /// <summary>
        /// "support"
        /// </summary>
        public static LocalisableString DonationThanksSupportFirst => new TranslatableString(getKey(@"donation_thanks.support.first"), @"support");

        /// <summary>
        /// "continued support"
        /// </summary>
        public static LocalisableString DonationThanksSupportRepeat => new TranslatableString(getKey(@"donation_thanks.support.repeat"), @"continued support");

        /// <summary>
        /// "Just letting you know that there has been a new reply in &quot;{0}&quot; since your last visit."
        /// </summary>
        public static LocalisableString ForumNewReplyNew(string title) => new TranslatableString(getKey(@"forum_new_reply.new"), @"Just letting you know that there has been a new reply in ""{0}"" since your last visit.", title);

        /// <summary>
        /// "[osu!] New reply for topic &quot;{0}&quot;"
        /// </summary>
        public static LocalisableString ForumNewReplySubject(string title) => new TranslatableString(getKey(@"forum_new_reply.subject"), @"[osu!] New reply for topic ""{0}""", title);

        /// <summary>
        /// "If you no longer wish to watch this topic, you can either click the &quot;Unsubscribe topic&quot; link found at the bottom of the topic above, or from topic subscriptions management page:"
        /// </summary>
        public static LocalisableString ForumNewReplyUnwatch => new TranslatableString(getKey(@"forum_new_reply.unwatch"), @"If you no longer wish to watch this topic, you can either click the ""Unsubscribe topic"" link found at the bottom of the topic above, or from topic subscriptions management page:");

        /// <summary>
        /// "Jump straight to the latest reply using the following link:"
        /// </summary>
        public static LocalisableString ForumNewReplyVisit => new TranslatableString(getKey(@"forum_new_reply.visit"), @"Jump straight to the latest reply using the following link:");

        /// <summary>
        /// "Your verification code is:"
        /// </summary>
        public static LocalisableString PasswordResetCode => new TranslatableString(getKey(@"password_reset.code"), @"Your verification code is:");

        /// <summary>
        /// "Either you or someone pretending to be you has requested a password reset on your osu! account."
        /// </summary>
        public static LocalisableString PasswordResetRequested => new TranslatableString(getKey(@"password_reset.requested"), @"Either you or someone pretending to be you has requested a password reset on your osu! account.");

        /// <summary>
        /// "osu! account recover"
        /// </summary>
        public static LocalisableString PasswordResetSubject => new TranslatableString(getKey(@"password_reset.subject"), @"osu! account recover");

        /// <summary>
        /// "We have received your payment and are preparing your order for shipping. It may take a few days for us to send it out, depending on the quantity of orders. You can follow the progress of your order here, including tracking details where available:"
        /// </summary>
        public static LocalisableString StorePaymentCompletedPrepareShipping => new TranslatableString(getKey(@"store_payment_completed.prepare_shipping"), @"We have received your payment and are preparing your order for shipping. It may take a few days for us to send it out, depending on the quantity of orders. You can follow the progress of your order here, including tracking details where available:");

        /// <summary>
        /// "We have received your payment and are currently processing your order. You can follow the progress of your order here:"
        /// </summary>
        public static LocalisableString StorePaymentCompletedProcessing => new TranslatableString(getKey(@"store_payment_completed.processing"), @"We have received your payment and are currently processing your order. You can follow the progress of your order here:");

        /// <summary>
        /// "If you have any questions, don&#39;t hesitate to reply to this email."
        /// </summary>
        public static LocalisableString StorePaymentCompletedQuestions => new TranslatableString(getKey(@"store_payment_completed.questions"), @"If you have any questions, don't hesitate to reply to this email.");

        /// <summary>
        /// "Shipping"
        /// </summary>
        public static LocalisableString StorePaymentCompletedShipping => new TranslatableString(getKey(@"store_payment_completed.shipping"), @"Shipping");

        /// <summary>
        /// "We received your osu!store order!"
        /// </summary>
        public static LocalisableString StorePaymentCompletedSubject => new TranslatableString(getKey(@"store_payment_completed.subject"), @"We received your osu!store order!");

        /// <summary>
        /// "Thanks for your osu!store order!"
        /// </summary>
        public static LocalisableString StorePaymentCompletedThankYou => new TranslatableString(getKey(@"store_payment_completed.thank_you"), @"Thanks for your osu!store order!");

        /// <summary>
        /// "Total"
        /// </summary>
        public static LocalisableString StorePaymentCompletedTotal => new TranslatableString(getKey(@"store_payment_completed.total"), @"Total");

        /// <summary>
        /// "The person who gifted you this tag may choose to remain anonymous, so they have not been mentioned in this notification."
        /// </summary>
        public static LocalisableString SupporterGiftAnonymousGift => new TranslatableString(getKey(@"supporter_gift.anonymous_gift"), @"The person who gifted you this tag may choose to remain anonymous, so they have not been mentioned in this notification.");

        /// <summary>
        /// "But you likely already know who it is ;)."
        /// </summary>
        public static LocalisableString SupporterGiftAnonymousGiftMaybeNot => new TranslatableString(getKey(@"supporter_gift.anonymous_gift_maybe_not"), @"But you likely already know who it is ;).");

        /// <summary>
        /// "Thanks to them, you have access to osu!direct and other osu!supporter benefits for the next {0}."
        /// </summary>
        public static LocalisableString SupporterGiftDuration(string duration) => new TranslatableString(getKey(@"supporter_gift.duration"), @"Thanks to them, you have access to osu!direct and other osu!supporter benefits for the next {0}.", duration);

        /// <summary>
        /// "You can find out more details on these features here:"
        /// </summary>
        public static LocalisableString SupporterGiftFeatures => new TranslatableString(getKey(@"supporter_gift.features"), @"You can find out more details on these features here:");

        /// <summary>
        /// "Someone has just gifted you an osu!supporter tag!"
        /// </summary>
        public static LocalisableString SupporterGiftGifted => new TranslatableString(getKey(@"supporter_gift.gifted"), @"Someone has just gifted you an osu!supporter tag!");

        /// <summary>
        /// "You have been gifted an osu!supporter tag!"
        /// </summary>
        public static LocalisableString SupporterGiftSubject => new TranslatableString(getKey(@"supporter_gift.subject"), @"You have been gifted an osu!supporter tag!");

        /// <summary>
        /// "This is a confirmation email to inform you that your osu! email address has been changed to: &quot;{0}&quot;."
        /// </summary>
        public static LocalisableString UserEmailUpdatedChangedTo(string email) => new TranslatableString(getKey(@"user_email_updated.changed_to"), @"This is a confirmation email to inform you that your osu! email address has been changed to: ""{0}"".", email);

        /// <summary>
        /// "Please ensure that you received this email at your new address to prevent losing access your osu! account in the future."
        /// </summary>
        public static LocalisableString UserEmailUpdatedCheck => new TranslatableString(getKey(@"user_email_updated.check"), @"Please ensure that you received this email at your new address to prevent losing access your osu! account in the future.");

        /// <summary>
        /// "For security reasons, this email has been sent both to your new and old email address."
        /// </summary>
        public static LocalisableString UserEmailUpdatedSent => new TranslatableString(getKey(@"user_email_updated.sent"), @"For security reasons, this email has been sent both to your new and old email address.");

        /// <summary>
        /// "osu! email change confirmation"
        /// </summary>
        public static LocalisableString UserEmailUpdatedSubject => new TranslatableString(getKey(@"user_email_updated.subject"), @"osu! email change confirmation");

        /// <summary>
        /// "Your account is suspected to have been compromised, has recent suspicious activity or a VERY weak password. As a result, we require you to set a new password. Please make sure to choose a SECURE password."
        /// </summary>
        public static LocalisableString UserForceReactivationMain => new TranslatableString(getKey(@"user_force_reactivation.main"), @"Your account is suspected to have been compromised, has recent suspicious activity or a VERY weak password. As a result, we require you to set a new password. Please make sure to choose a SECURE password.");

        /// <summary>
        /// "You can perform the reset from {0}"
        /// </summary>
        public static LocalisableString UserForceReactivationPerformReset(string url) => new TranslatableString(getKey(@"user_force_reactivation.perform_reset"), @"You can perform the reset from {0}", url);

        /// <summary>
        /// "Reason:"
        /// </summary>
        public static LocalisableString UserForceReactivationReason => new TranslatableString(getKey(@"user_force_reactivation.reason"), @"Reason:");

        /// <summary>
        /// "osu! Account Reactivation Required"
        /// </summary>
        public static LocalisableString UserForceReactivationSubject => new TranslatableString(getKey(@"user_force_reactivation.subject"), @"osu! Account Reactivation Required");

        /// <summary>
        /// "Just letting you know that there have been new updates on items you are watching."
        /// </summary>
        public static LocalisableString UserNotificationDigestNew => new TranslatableString(getKey(@"user_notification_digest.new"), @"Just letting you know that there have been new updates on items you are watching.");

        /// <summary>
        /// "Change email notification preferences:"
        /// </summary>
        public static LocalisableString UserNotificationDigestSettings => new TranslatableString(getKey(@"user_notification_digest.settings"), @"Change email notification preferences:");

        /// <summary>
        /// "New osu! notifications"
        /// </summary>
        public static LocalisableString UserNotificationDigestSubject => new TranslatableString(getKey(@"user_notification_digest.subject"), @"New osu! notifications");

        /// <summary>
        /// "This is just a confirmation that your osu! password has been changed."
        /// </summary>
        public static LocalisableString UserPasswordUpdatedConfirmation => new TranslatableString(getKey(@"user_password_updated.confirmation"), @"This is just a confirmation that your osu! password has been changed.");

        /// <summary>
        /// "osu! password change confirmation"
        /// </summary>
        public static LocalisableString UserPasswordUpdatedSubject => new TranslatableString(getKey(@"user_password_updated.subject"), @"osu! password change confirmation");

        /// <summary>
        /// "Your verification code is:"
        /// </summary>
        public static LocalisableString UserVerificationCode => new TranslatableString(getKey(@"user_verification.code"), @"Your verification code is:");

        /// <summary>
        /// "You can enter the code with or without spaces."
        /// </summary>
        public static LocalisableString UserVerificationCodeHint => new TranslatableString(getKey(@"user_verification.code_hint"), @"You can enter the code with or without spaces.");

        /// <summary>
        /// "Alternatively, you can also visit this link below to finish verification:"
        /// </summary>
        public static LocalisableString UserVerificationLink => new TranslatableString(getKey(@"user_verification.link"), @"Alternatively, you can also visit this link below to finish verification:");

        /// <summary>
        /// "If you did not request this, please REPLY IMMEDIATELY as your account may be in danger."
        /// </summary>
        public static LocalisableString UserVerificationReport => new TranslatableString(getKey(@"user_verification.report"), @"If you did not request this, please REPLY IMMEDIATELY as your account may be in danger.");

        /// <summary>
        /// "osu! account verification"
        /// </summary>
        public static LocalisableString UserVerificationSubject => new TranslatableString(getKey(@"user_verification.subject"), @"osu! account verification");

        /// <summary>
        /// "An action performed on your account from {0} requires verification."
        /// </summary>
        public static LocalisableString UserVerificationActionFromDefault(string country) => new TranslatableString(getKey(@"user_verification.action_from._"), @"An action performed on your account from {0} requires verification.", country);

        /// <summary>
        /// "unknown country"
        /// </summary>
        public static LocalisableString UserVerificationActionFromUnknownCountry => new TranslatableString(getKey(@"user_verification.action_from.unknown_country"), @"unknown country");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}