// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Identity.Model
{
    public sealed class SettingsData
    {
        public string SiteName { get; set; }

        public string FooterText { get; set; }

        public string PrivacyPolicyUrl { get; set; }

        public string LegalUrl { get; set; }

        public string BootstrapUrl { get; set; }

        public string TermsOfServiceUrl { get; set; }

        public string StylesUrl { get; set; }

        public string EmailConfirmationText { get; set; }

        public string EmailConfirmationSubject { get; set; }

        public string EmailPasswordResetText { get; set; }

        public string EmailPasswordResetSubject { get; set; }

        public string SmptSender { get; set; }

        public string SmtpServer { get; set; }

        public string SmtpUsername { get; set; }

        public string SmtpPassword { get; set; }

        public string GoogleAnalyticsId { get; set; }

        public string[] Logo { get; set; }
    }
}
