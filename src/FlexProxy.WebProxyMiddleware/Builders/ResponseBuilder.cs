using FlexProxy.Core.Extensions;
using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FlexProxy.WebProxyMiddleware.Builders
{
    public interface IResponseBuilder
    {
        void CopyHeaders(HttpContext context, HttpWebResponse remoteResponse, HostMappingOptions mapping);
    }

    public class ResponseBuilder : IResponseBuilder
    {
        public void CopyHeaders(HttpContext context, HttpWebResponse remoteResponse, HostMappingOptions mapping)
        {
            var originalResponse = context.Response;

            originalResponse.StatusCode = (int)remoteResponse.StatusCode;

            string cookiePrefix = string.Empty;

            for (var i = 0; i < remoteResponse.Headers.Count; i++)
            {
                var headerKey = remoteResponse.Headers.GetKey(i);

                switch (headerKey)
                {
                    case HttpHeaders.SetCookie:
                        foreach (var value in remoteResponse.Headers.GetValues(i))
                        {
                            originalResponse.Headers.Append(HttpHeaders.SetCookie, Regex.Replace(value, mapping.DownstreamHost, mapping.ServingHost, RegexOptions.IgnoreCase));
                        }
                        break;
                    case HttpHeaders.AccessControlAllowOrigin:
                    case HttpHeaders.Location:
                        if (Uri.IsWellFormedUriString(remoteResponse.Headers[headerKey], UriKind.Absolute))
                        {
                            originalResponse.Headers.Add(headerKey, new UriBuilder(remoteResponse.Headers[headerKey]).ReplaceHost(mapping.DownstreamHost, mapping.ServingHost, mapping.ServingScheme));
                        }
                        else
                        {
                            originalResponse.Headers.Add(headerKey, remoteResponse.Headers[headerKey].Replace(mapping.DownstreamHost, mapping.ServingHost));
                        }
                        break;
                    case HttpHeaders.ContentLength:
                    case HttpHeaders.Connection:
                    case HttpHeaders.KeepAlive:
                    case HttpHeaders.ProxyAuthenticate:
                    case HttpHeaders.Trailer:
                    case HttpHeaders.TransferEncoding:
                    case HttpHeaders.Upgrade:
                    case HttpHeaders.WWWAuthenticate:
                    case HttpHeaders.ContentEncoding:
                        break;
                    default:
                        originalResponse.Headers.Add(headerKey, remoteResponse.Headers[headerKey]);
                        break;
                }
            }
        }
    }
}
