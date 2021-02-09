using Hl7.Fhir.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.OpenAPI.Services
{
    public interface IMedicationService
    {
        Task<List<Medication>> GetMedicationDataForPatientAsync(string patientId);
    }
}