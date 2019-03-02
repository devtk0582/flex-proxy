using System;

namespace FlexProxy.Core
{
    public static class Constants
    {
        public const string CONTENT_TYPE_MULTIPART_FORM_DATA_IDENTIFIER = "multipart/form-data";

        public const string CONTENT_TYPE_FORM_DATA_IDENTIFIER = "application/x-www-form-urlencoded";

        public const string CONTENT_TYPE_JAVASCRIPT = "javascript";

        public const string CONTENT_TYPE_JSON = "json";

        public const string CONTENT_TYPE_HTML_IDENTIFIER = "text/html";

        public const string MULTIPART_FORM_BOUNDARY_INDEX = "boundary=";

        public const string MULTIPART_FORM_SEPARATOR = "--";

        public const string MULTIPART_FORM_NAME_REG = @"(?<=name\=\"")(.*?)(?=\"")";

        public const string MULTIPART_FORM_CONTENT_TYPE_REG = @"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)";

        public const string MULTIPART_FORM_FILE_NAME_REG = @"(?<=filename\=\"")(.*?)(?=\"")";

        public const string HTTP_REQUEST_HEADER_REQUESTED_WITH = "X-Requested-With";

        public const string AJAX_HEADER_IDENTIFIER = "XMLHttpRequest";

        public const string EVENT_LOG_SESSION_ID = "proxy.event.sessionId";

        public const string ERROR_UNABLE_TO_CONNECT_TO_FLEX_PROXY = "Unable to connect to flex proxy";
        
        public const string HTTP_CONTEXT_COOKIES_TO_ENCODE_KEY = "CookiesToEncode";

        public const string HTTP_CONTEXT_TRANSPORT_ITEMS = "TransportItems";

        public const string REQUEST_TRACELOGGER_NAME = "TracerLogger";

        public const int REQUEST_TRACELOGGER_INTERVAL = 30000;

        public const string PROXY_LOGGER_NAME = "ProxyLogger";
    }
}
