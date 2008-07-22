using System;
using System.Collections;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;

namespace Glue.Web
{
    /// <summary>
    /// GlueException.
    /// </summary>
    public class GlueException : Exception
    {
        protected GlueException() { }
        public GlueException(string message) : base(message) {}
        public GlueException(string message, Exception innerException) : base(message, innerException) {}
    }

    // HTTP 404
    public class GlueNotFoundException : GlueException
    {
        public GlueNotFoundException() { }
        public GlueNotFoundException(string message) : base(message) {}
    }

    // HTTP 401
    public class GlueUnauthorizedException : GlueException
    {
        public GlueUnauthorizedException() { }
        public GlueUnauthorizedException(string message) : base(message) {}
    }

    // HTTP 403
    public class GlueForbiddenException : GlueException
    {
        public GlueForbiddenException() { }
        public GlueForbiddenException(string message) : base(message) {}
    }

    // HTTP 503
    public class GlueServiceUnavailableException : GlueException
    {
        public GlueServiceUnavailableException() { }
        public GlueServiceUnavailableException(string message) : base(message) {}
    }
}
