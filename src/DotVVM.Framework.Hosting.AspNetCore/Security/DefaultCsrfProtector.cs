using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using DotVVM.Framework.AspNetCore.Hosting;

namespace DotVVM.Framework.Security
{
    /// <summary>
    /// Implements synchronizer token pattern for CSRF protection.
    /// <para>The token is generated based on Session ID (random 256-bit value persisted in cookie), 
    /// Request identity (full URI) and User identity (user name, if authenticated).</para>
    /// <para>Value of stored Session ID and the token itself is encrypted and signed.</para>
    /// </summary>
    public class DefaultCsrfProtector : ICsrfProtector
    {
        private const int SID_LENGTH = 32; // 256-bit identifier
        private const string PURPOSE_SID = "DotVVM.Framework.Security.DefaultCsrfProtector.SID"; // Key derivation label for protecting SID
        private const string PURPOSE_TOKEN = "DotVVM.Framework.Security.DefaultCsrfProtector.Token"; // Key derivation label for protecting token

        private IDataProtectionProvider protectionProvider;

        public DefaultCsrfProtector(DotvvmConfiguration configuration)
        {
            this.protectionProvider = configuration.ServiceLocator.GetService<IDataProtectionProvider>();
        }

        public string GenerateToken(IDotvvmRequestContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Get SID
            var sid = this.GetOrCreateSessionId(context);

            // Construct protector with purposes
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            var protector = this.protectionProvider.CreateProtector(PURPOSE_TOKEN, userIdentity, requestIdentity);

            // Get token
            var tokenData = protector.Protect(sid);

            // Return encoded token
            return Convert.ToBase64String(tokenData);
        }

        public void VerifyToken(IDotvvmRequestContext context, string token)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(token)) throw new SecurityException("CSRF protection token is missing.");

            // Construct protector with purposes
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            var protector = this.protectionProvider.CreateProtector(PURPOSE_TOKEN, userIdentity, requestIdentity);

            // Get token
            byte[] tokenSid;
            try
            {
                var tokenData = Convert.FromBase64String(token);
                tokenSid = protector.Unprotect(tokenData);
            }
            catch (Exception ex)
            {
                // Incorrect Base64 formatting of crypto protection error
                throw new SecurityException("CSRF protection token is invalid.", ex);
            }

            // Get SID from cookie and compare with token one
            var cookieSid = this.GetOrCreateSessionId(context, canGenerate: false); // should not generate new token
            if (!cookieSid.SequenceEqual(tokenSid)) throw new SecurityException("CSRF protection token is invalid.");
        }

        private byte[] GetOrCreateSessionId(IDotvvmRequestContext context, bool canGenerate = true)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
			var originalHttpContext = context.GetAspNetCoreContext();
			var sessionIdCookieName = GetSessionIdCookieName(context);
            if (string.IsNullOrWhiteSpace(sessionIdCookieName)) throw new FormatException("Configured SessionIdCookieName is missing or empty.");

            // Get cookie manager
            var mgr = new ChunkingCookieManager(); // TODO: Make this configurable

            // Construct protector with purposes
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            var protector = this.protectionProvider.CreateProtector(PURPOSE_SID);

            // Get cookie value
            var sidCookieValue = mgr.GetRequestCookie(originalHttpContext, sessionIdCookieName);

            if (!string.IsNullOrWhiteSpace(sidCookieValue))
            {
                // Try to read from cookie
                try
                {
                    var protectedSid = Convert.FromBase64String(sidCookieValue);
                    var sid = protector.Unprotect(protectedSid);
                    return sid;
                }
                catch (Exception ex)
                {
                    // Incorrect Base64 formatting of crypto protection error
                    // Generate new one or thow error if can't
                    if (!canGenerate)
                        throw new SecurityException("Value of the SessionID cookie is corrupted or has been tampered with.", ex);
                    // else suppress error and generate new SID
                }
            }

            // No SID - generate and protect new one

            if(canGenerate)
            {
				var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
                var sid = new byte[SID_LENGTH];
                rng.GetBytes(sid);
                var protectedSid = protector.Protect(sid);

                // Save to cookie
                sidCookieValue = Convert.ToBase64String(protectedSid);
                mgr.AppendResponseCookie(
					originalHttpContext,
                    sessionIdCookieName,                                // Configured cookie name
                    sidCookieValue,                                     // Base64-encoded SID value
                    new CookieOptions
                    {
                        HttpOnly = true,                                // Don't allow client script access
                        Secure = context.HttpContext.Request.IsHttps   // If request goes trough HTTPS, mark as secure only
                    });

                // Return newly generated SID
                return sid;
            }
            else
            {
                throw new SecurityException("SessionID cookie is missing, so can't verify CSRF token.");
            }
        }

        private string GetSessionIdCookieName(IDotvvmRequestContext context)
        {
            var domain = context.HttpContext.Request.Url.Host;
            if (context.HttpContext.Request.Url.Port != (context.HttpContext.Request.IsHttps ? 443 : 80))
            {
                domain += "-" + context.HttpContext.Request.Url.Port;
            }
            return string.Format(context.Configuration.Security.SessionIdCookieName, domain);
        }
    }
}