using System;
using WebsiteTemplate.Utilities;

namespace Marketplace.SiteSpecific
{
    public class AppSettings : ApplicationSettingsCore
    {
        public override bool EnableAuditing => false; // Will audit everything

        public override bool DebugStartup => false; //  will create a window to allow debugging when starting if true

        public override bool UpdateDatabase => true; // Set to true first time to create tables. Also set to true after making changes

        public override string ApplicationPassPhrase => "xxxxxxxxxxxxxxxx"; // This is used for encrypting/decrypting password inputs. Should be different for each project
                                                                            // Must be at least 16 characters (128bits)

        public override Type GetApplicationStartupType => typeof(AppStartup);

        public override string SystemEmailAddress => "system@example.com";

        public override bool TokenEndpointAllowInsecureHttpRequests => true;

        public override string GetApplicationName()
        {
            return "Marketplace";
        }
    }
}