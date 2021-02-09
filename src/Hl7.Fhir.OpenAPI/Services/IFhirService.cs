﻿using Hl7.Fhir.Rest;

namespace Hl7.Fhir.OpenAPI.Services
{
    public interface IFhirService
    {
        FhirClient Client { get; set; }
        
        string Endpoint { get; set; }

        void Initialize();
    }
}