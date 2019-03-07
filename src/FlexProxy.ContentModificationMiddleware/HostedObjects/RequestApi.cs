using FlexProxy.Core;
using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IRequestApi
    {
        string GetUrl();
        void ReplaceUrl(string oldValue, string newValue);
        string GetContentType();
        string GetHttpMethod();
        string GetHeader(string name);
        void AddHeader(string name, string value);
        void UpdateHeader(string name, string value);
        void RemoveHeader(string name);
        string GetCookie(string name);
        void EncodeCookie(string key);
        void RemoveSpecialHeader(string name);
        void AddCookie(string name, string value);
        void UpdateCookie(string name, string value);
        void RemoveCookie(string name);
    }

    public class RequestApi : IRequestApi
    {
        private IHttpContextAccessor _httpContextAccessor;

        public RequestApi(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUrl()
        {
            return UriHelper.GetEncodedUrl(_httpContextAccessor.HttpContext.Request);
        }

        public void ReplaceUrl(string oldValue, string newValue)
        {
            if (_httpContextAccessor.HttpContext.Request.QueryString.HasValue)
            {
                var queryString = _httpContextAccessor.HttpContext.Request.QueryString.Value;

                _httpContextAccessor.HttpContext.Request.QueryString = new QueryString(queryString.Replace(oldValue, newValue));
            }
        }

        public string GetContentType()
        {
            return _httpContextAccessor.HttpContext.Request.ContentType;
        }

        public string GetHttpMethod()
        {
            return _httpContextAccessor.HttpContext.Request.Method;
        }

        public string GetHeader(string name)
        {
            return _httpContextAccessor.HttpContext.Request.Headers[name];
        }

        public void AddHeader(string name, string value)
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;

            if (!httpRequest.Headers.ContainsKey(name))
            {
                httpRequest.Headers.Add(name, value);
            }
        }

        public void UpdateHeader(string name, string value)
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;

            if (httpRequest.Headers.ContainsKey(name))
            {
                httpRequest.Headers[name] = value;
            }
        }

        public void RemoveHeader(string name)
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;

            if (httpRequest.Headers[name] != StringValues.Empty)
            {
                httpRequest.Headers.Remove(name);
            }
        }

        public string GetCookie(string name)
        {
            return _httpContextAccessor.HttpContext.Request.Cookies[name];
        }

        public void EncodeCookie(string key)
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;

            if (!httpRequest.Cookies.ContainsKey(key))
            {
                return;
            }

            var cookiesToEncode = _httpContextAccessor.HttpContext.Items[Constants.HTTP_CONTEXT_COOKIES_TO_ENCODE_KEY] as List<string>;

            if (cookiesToEncode == null)
            {
                _httpContextAccessor.HttpContext.Items[Constants.HTTP_CONTEXT_COOKIES_TO_ENCODE_KEY] = new List<string>() { key };
            }
            else
            {
                cookiesToEncode.Add(key);
            }
        }

        public void RemoveSpecialHeader(string name)
        {
            var transportHeaders = GetTransportItems(_httpContextAccessor.HttpContext);

            transportHeaders?.Headers.Add(name);
        }

        public void AddCookie(string name, string value)
        {
            var transportHeaders = GetTransportItems(_httpContextAccessor.HttpContext);

            transportHeaders?.Cookies.Add(
                new TransientCookie()
                {
                    Name = name,
                    Value = value,
                    Action = TransientCookieAction.Add
                }
            );

        }

        public void UpdateCookie(string name, string value)
        {
            var transportHeaders = GetTransportItems(_httpContextAccessor.HttpContext);

            transportHeaders?.Cookies.Add(
                new TransientCookie()
                {
                    Name = name,
                    Value = value,
                    Action = TransientCookieAction.Update
                }
            );
        }

        public void RemoveCookie(string name)
        {
            var transportHeaders = GetTransportItems(_httpContextAccessor.HttpContext);

            transportHeaders?.Cookies.Add(
                new TransientCookie()
                {
                    Name = name,
                    Action = TransientCookieAction.Delete
                }
            );
        }

        private TransportItems GetTransportItems(HttpContext httpContext)
        {
            if (!httpContext.Items.ContainsKey(Constants.HTTP_CONTEXT_TRANSPORT_ITEMS))
            {
                httpContext.Items.Add(Constants.HTTP_CONTEXT_TRANSPORT_ITEMS, new TransportItems());
            }

            return httpContext.Items[Constants.HTTP_CONTEXT_TRANSPORT_ITEMS] as TransportItems;
        }
    }
}
