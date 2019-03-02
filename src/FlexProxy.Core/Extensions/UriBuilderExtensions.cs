using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Extensions
{
    public static class UriBuilderExtensions
    {
        public static string ReplaceHost(this UriBuilder uriBuilder, string oldHost, string newHost, string newScheme = null)
        {
            oldHost = oldHost?.ToLower();
            newHost = newHost?.ToLower();
            if (string.IsNullOrEmpty(oldHost) || string.IsNullOrEmpty(newHost))
            {
                return string.Empty;
            }

            if (uriBuilder.Uri.IsDefaultPort)
            {
                uriBuilder.Port = -1;
            }

            if (uriBuilder.Host.Equals(oldHost, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(newScheme))
                {
                    uriBuilder.Scheme = newScheme;
                }

                uriBuilder.Host = newHost;
            }

            return uriBuilder.Uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
        }
    }
}
