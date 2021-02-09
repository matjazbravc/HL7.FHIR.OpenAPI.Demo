using Hl7.Fhir.Common.Contracts.Converters;
using Hl7.Fhir.Common.Contracts.Dto;
using Hl7.Fhir.Common.Core.Errors;
using Hl7.Fhir.Model;
using Hl7.Fhir.OpenAPI.Controllers.Base;
using Hl7.Fhir.OpenAPI.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.OpenAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [EnableCors("EnableCORS")]
    [Route("api/[controller]")]
    public class MedicationController : BaseController<MedicationController>
    {
        private readonly IFhirService _fhirService;
        private readonly IMedicationService _medicationService;
        private readonly IConverter<Patient, PatientDetailDto> _patientToDtoConverter;

        public MedicationController(IFhirService fhirService, IMedicationService medicationService, IConverter<Patient, PatientDetailDto> patientToDtoConverter)
        {
            _fhirService = fhirService;
            _medicationService = medicationService;
            _patientToDtoConverter = patientToDtoConverter;
            _fhirService.Initialize();
        }

        /// <summary>
        /// Get Patient's Medications
        /// </summary>
        /// <param name="patientId">Patient resource Id</param>
        /// <returns>Return list of Medications</returns>
        [HttpGet("{id}", Name = "GetPatientMedications")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Medication>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPatientMedicationsAsync(string patientId)
        {
            Logger.LogDebug(nameof(GetPatientMedicationsAsync));
            if (ModelState.IsValid)
            {
                var medications = await _medicationService.GetMedicationDataForPatientAsync(patientId).ConfigureAwait(false);
                if (medications == null)
                {
                    return NotFound(new NotFoundError("The patient medications was not found"));
                }
                return Ok(medications);
            }
            return BadRequest();
        }
    }
}
