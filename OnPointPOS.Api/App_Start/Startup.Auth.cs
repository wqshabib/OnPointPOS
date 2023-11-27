using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using POSSUM.Api.Models;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.Identity.EntityFramework;
using POSSUM.Api.Providers;
using POSSUM.MasterData;
namespace POSSUM.Api
{
    public partial class Startup
    {

        static Startup()
        {
            PublicClientId = "self";

            UserManagerFactory = () => new ApplicationUserManager(new UserStore<MasterApplicationUser>(MasterDbContext.Create()));

            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId, UserManagerFactory),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true
            };
        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static Func<ApplicationUserManager> UserManagerFactory { get; set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864

        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(MasterDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.Use(async (context, next) =>
            {
                IOwinRequest req = context.Request;
                IOwinResponse res = context.Response;
                // for auth2 token requests
                if (req.Path.StartsWithSegments(new PathString("/Token")))
                {
                    // if there is an origin header
                    var origin = req.Headers.Get("Origin");
                    if (!string.IsNullOrEmpty(origin))
                    {
                        // allow the cross-site request
                        res.Headers.Set("Access-Control-Allow-Origin", origin);
                    }

                    // if this is pre-flight request
                    if (req.Method == "OPTIONS")
                    {
                        // respond immediately with allowed request methods and headers
                        res.StatusCode = 200;
                        res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Methods", "GET", "POST");
                        res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Headers", "authorization", "content-type");
                        // no further processing
                        return;
                    }
                }
                // continue executing pipeline
                await next();
            });

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //app.UseGoogleAuthentication();
        }


        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        /*     public void ConfigureAuth(IAppBuilder app)
             {
                 // Configure the db context, user manager and signin manager to use a single instance per request
                 app.CreatePerOwinContext(ApplicationDbContext.Create);
                 app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
                 app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

                 // Enable the application to use a cookie to store information for the signed in user
                 // and to use a cookie to temporarily store information about a user logging in with a third party login provider
                 // Configure the sign in cookie
                 app.UseCookieAuthentication(new CookieAuthenticationOptions
                 {
                     AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                     LoginPath = new PathString("/Account/Login"),
                     Provider = new CookieAuthenticationProvider
                     {
                         // Enables the application to validate the security stamp when the user logs in.
                         // This is a security feature which is used when you change a password or add an external login to your account.  
                         OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                             validateInterval: TimeSpan.FromMinutes(30),
                             regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                     }
                 });
                 app.Use(async (context, next) =>
                 {
                     IOwinRequest req = context.Request;
                     IOwinResponse res = context.Response;
                     // for auth2 token requests
                     if (req.Path.StartsWithSegments(new PathString("/Token")))
                     {
                         // if there is an origin header
                         var origin = req.Headers.Get("Origin");
                         if (!string.IsNullOrEmpty(origin))
                         {
                             // allow the cross-site request
                             res.Headers.Set("Access-Control-Allow-Origin", origin);
                         }

                         // if this is pre-flight request
                         if (req.Method == "OPTIONS")
                         {
                             // respond immediately with allowed request methods and headers
                             res.StatusCode = 200;
                             res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Methods", "GET", "POST");
                             res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Headers", "authorization", "content-type");
                             // no further processing
                             return;
                         }
                     }
                     // continue executing pipeline
                     await next();
                 });
                 app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

                 // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
                 app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

                 // Enables the application to remember the second login verification factor such as phone or email.
                 // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
                 // This is similar to the RememberMe option when you log in.
                 app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

                 // Uncomment the following lines to enable logging in with third party login providers
                 //app.UseMicrosoftAccountAuthentication(
                 //    clientId: "",
                 //    clientSecret: "");

                 //app.UseTwitterAuthentication(
                 //   consumerKey: "",
                 //   consumerSecret: "");

                 //app.UseFacebookAuthentication(
                 //   appId: "",
                 //   appSecret: "");

                 //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
                 //{
                 //    ClientId = "",
                 //    ClientSecret = ""
                 //});
             }*/
    }
}