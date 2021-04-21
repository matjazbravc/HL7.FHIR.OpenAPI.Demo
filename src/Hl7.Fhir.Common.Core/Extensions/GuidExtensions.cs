using System;

namespace Hl7.Fhir.Common.Core.Extensions
{
    public static class GuidExtensions
    {
        public static string ToFhirId(this Guid me)
        {
            return me.ToString("n");
        }

        public static string ToFhirId(this Guid? me)
        {
            return me?.ToString("n");
        }
    }
}
