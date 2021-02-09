using System;

namespace Hl7.Fhir.Common.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class SwaggerUploadFile : Attribute
    {
        public SwaggerUploadFile(string contentType)
        {
            ContentType = contentType;
        }

        /// <summary>
        /// Content type of the file e.g. image/png
        /// </summary>
        public string ContentType { get; }
    }
}
