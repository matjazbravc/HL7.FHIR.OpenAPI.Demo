using Hl7.Fhir.Common.Core.Csv.Models;
using System.Collections.Generic;

namespace Hl7.Fhir.OpenAPI.Services
{
    public interface ICitizenshipService
    {
        Dictionary<string, Citizenship> Citizenships { get; }

        Citizenship GetCitizenship(string code);

        void Initialize();
    }
}