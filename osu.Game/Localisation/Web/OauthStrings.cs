// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class OauthStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Oauth";

        /// <summary>
        /// "Cancel"
        /// </summary>
        public static LocalisableString Cancel => new TranslatableString(getKey(@"cancel"), @"Cancel");

        /// <summary>
        /// "is requesting permission to access your account."
        /// </summary>
        public static LocalisableString AuthoriseRequest => new TranslatableString(getKey(@"authorise.request"), @"is requesting permission to access your account.");

        /// <summary>
        /// "This application will be able to:"
        /// </summary>
        public static LocalisableString AuthoriseScopesTitle => new TranslatableString(getKey(@"authorise.scopes_title"), @"This application will be able to:");

        /// <summary>
        /// "Authorisation Request"
        /// </summary>
        public static LocalisableString AuthoriseTitle => new TranslatableString(getKey(@"authorise.title"), @"Authorisation Request");

        /// <summary>
        /// "Are you sure you want to revoke this client&#39;s permissions?"
        /// </summary>
        public static LocalisableString AuthorizedClientsConfirmRevoke => new TranslatableString(getKey(@"authorized_clients.confirm_revoke"), @"Are you sure you want to revoke this client's permissions?");

        /// <summary>
        /// "This application can:"
        /// </summary>
        public static LocalisableString AuthorizedClientsScopesTitle => new TranslatableString(getKey(@"authorized_clients.scopes_title"), @"This application can:");

        /// <summary>
        /// "Owned by {0}"
        /// </summary>
        public static LocalisableString AuthorizedClientsOwnedBy(string user) => new TranslatableString(getKey(@"authorized_clients.owned_by"), @"Owned by {0}", user);

        /// <summary>
        /// "No Clients"
        /// </summary>
        public static LocalisableString AuthorizedClientsNone => new TranslatableString(getKey(@"authorized_clients.none"), @"No Clients");

        /// <summary>
        /// "Revoke Access"
        /// </summary>
        public static LocalisableString AuthorizedClientsRevokedFalse => new TranslatableString(getKey(@"authorized_clients.revoked.false"), @"Revoke Access");

        /// <summary>
        /// "Access Revoked"
        /// </summary>
        public static LocalisableString AuthorizedClientsRevokedTrue => new TranslatableString(getKey(@"authorized_clients.revoked.true"), @"Access Revoked");

        /// <summary>
        /// "Client ID"
        /// </summary>
        public static LocalisableString ClientId => new TranslatableString(getKey(@"client.id"), @"Client ID");

        /// <summary>
        /// "Application Name"
        /// </summary>
        public static LocalisableString ClientName => new TranslatableString(getKey(@"client.name"), @"Application Name");

        /// <summary>
        /// "Application Callback URL"
        /// </summary>
        public static LocalisableString ClientRedirect => new TranslatableString(getKey(@"client.redirect"), @"Application Callback URL");

        /// <summary>
        /// "Reset client secret"
        /// </summary>
        public static LocalisableString ClientReset => new TranslatableString(getKey(@"client.reset"), @"Reset client secret");

        /// <summary>
        /// "Failed to reset client secret"
        /// </summary>
        public static LocalisableString ClientResetFailed => new TranslatableString(getKey(@"client.reset_failed"), @"Failed to reset client secret");

        /// <summary>
        /// "Client Secret"
        /// </summary>
        public static LocalisableString ClientSecret => new TranslatableString(getKey(@"client.secret"), @"Client Secret");

        /// <summary>
        /// "Show client secret"
        /// </summary>
        public static LocalisableString ClientSecretVisibleFalse => new TranslatableString(getKey(@"client.secret_visible.false"), @"Show client secret");

        /// <summary>
        /// "Hide client secret"
        /// </summary>
        public static LocalisableString ClientSecretVisibleTrue => new TranslatableString(getKey(@"client.secret_visible.true"), @"Hide client secret");

        /// <summary>
        /// "Register a new OAuth application"
        /// </summary>
        public static LocalisableString NewClientHeader => new TranslatableString(getKey(@"new_client.header"), @"Register a new OAuth application");

        /// <summary>
        /// "Register application"
        /// </summary>
        public static LocalisableString NewClientRegister => new TranslatableString(getKey(@"new_client.register"), @"Register application");

        /// <summary>
        /// "By using the API you are agreeing to the {0}."
        /// </summary>
        public static LocalisableString NewClientTermsOfUseDefault(string link) => new TranslatableString(getKey(@"new_client.terms_of_use._"), @"By using the API you are agreeing to the {0}.", link);

        /// <summary>
        /// "Terms of Use"
        /// </summary>
        public static LocalisableString NewClientTermsOfUseLink => new TranslatableString(getKey(@"new_client.terms_of_use.link"), @"Terms of Use");

        /// <summary>
        /// "Are you sure you want to delete this client?"
        /// </summary>
        public static LocalisableString OwnClientsConfirmDelete => new TranslatableString(getKey(@"own_clients.confirm_delete"), @"Are you sure you want to delete this client?");

        /// <summary>
        /// "Are you sure you want to reset the client secret? This will revoke all existing tokens."
        /// </summary>
        public static LocalisableString OwnClientsConfirmReset => new TranslatableString(getKey(@"own_clients.confirm_reset"), @"Are you sure you want to reset the client secret? This will revoke all existing tokens.");

        /// <summary>
        /// "New OAuth Application"
        /// </summary>
        public static LocalisableString OwnClientsNew => new TranslatableString(getKey(@"own_clients.new"), @"New OAuth Application");

        /// <summary>
        /// "No Clients"
        /// </summary>
        public static LocalisableString OwnClientsNone => new TranslatableString(getKey(@"own_clients.none"), @"No Clients");

        /// <summary>
        /// "Delete"
        /// </summary>
        public static LocalisableString OwnClientsRevokedFalse => new TranslatableString(getKey(@"own_clients.revoked.false"), @"Delete");

        /// <summary>
        /// "Deleted"
        /// </summary>
        public static LocalisableString OwnClientsRevokedTrue => new TranslatableString(getKey(@"own_clients.revoked.true"), @"Deleted");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}