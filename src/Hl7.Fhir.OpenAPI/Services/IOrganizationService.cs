using Hl7.Fhir.Model;
using System.Threading.Tasks;

namespace Hl7.Fhir.OpenAPI.Services
{
    public interface IOrganizationService
    {
        Task<Organization> AddOrganizationAsync(string id, string name, string contactPhone);

        Task<Organization> SearchByIdentifierAsync(string identifier);
    }
}