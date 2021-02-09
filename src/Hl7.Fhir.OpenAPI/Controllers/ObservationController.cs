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
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hl7.Fhir.OpenAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [EnableCors("EnableCORS")]
    [Route("api/[controller]")]
    public class ObservationController : BaseController<ObservationController>
    {
        private readonly IFhirService _fhirService;
        private readonly IObservationService _observationService;
        private readonly IConverter<Observation, ObservationDto> _observationToDtoConverter;
        private readonly IConverter<IList<Observation>, IList<ObservationDto>> _observationsToDtoConverter;

        public ObservationController(IFhirService fhirService, IObservationService observationService,
            IConverter<Observation, ObservationDto> observationToDtoConverter,
            IConverter<IList<Observation>, IList<ObservationDto>> observationsToDtoConverter)
        {
            _fhirService = fhirService;
            _observationService = observationService;
            _observationToDtoConverter = observationToDtoConverter;
            _observationsToDtoConverter = observationsToDtoConverter;
            _fhirService.Initialize();
        }

        /// <summary>
        /// Gets all Patient's observations
        /// </summary>
        /// <param name="patientId">Patient resource Id</param>
        /// <returns>List of observations</returns>
        [HttpGet("{patientId}", Name = "GetObservationsForPatientAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ObservationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetObservationsForPatientAsync(string patientId)
        {
            Logger.LogDebug(nameof(GetObservationsForPatientAsync));
            if (ModelState.IsValid)
            {
                var response = await _observationService.GetObservationsForPatientAsync(patientId).ConfigureAwait(false);
                if (response == null)
                {
                    return NotFound(new NotFoundError("The observations for a patient was not found"));
                }
                var result = _observationsToDtoConverter.Convert(response);
                return Ok(result);
            }
            return BadRequest();
        }

        /// <summary>
        /// Gets Patient's hemoglobin observations
        /// </summary>
        /// <param name="patientId">Patient resource Id</param>
        /// <returns>List of hemoglobin observations</returns>
        [HttpGet("GetHemoglobinObservations/{patientId}", Name = "GetHemoglobinObservationsForPatientAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ObservationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHemoglobinObservationsForPatientAsync(string patientId)
        {
            Logger.LogDebug(nameof(GetHemoglobinObservationsForPatientAsync));
            if (ModelState.IsValid)
            {
                var response = await _observationService.GetHemoglobinObservationsForPatientAsync(patientId).ConfigureAwait(false);
                if (response == null)
                {
                    return NotFound(new NotFoundError("The observations for a patient was not found"));
                }
                var result = _observationsToDtoConverter.Convert(response);
                return Ok(result);
            }
            return BadRequest();
        }

        /// <summary>
        /// Gets Patient's red blood cell (RBCs) count observations
        /// </summary>
        /// <param name="patientId">Patient resource Id</param>
        /// <returns>List of red bloodcell count observations</returns>
        [HttpGet("GetRedBloodCellCountObservations/{patientId}", Name = "GetRedBloodCellCountForPatientAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ObservationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRedBloodCellCountObservationsForPatientAsync(string patientId)
        {
            Logger.LogDebug(nameof(GetRedBloodCellCountObservationsForPatientAsync));
            if (ModelState.IsValid)
            {
                var response = await _observationService.GetRedBloodCellCountObservationsForPatientAsync(patientId).ConfigureAwait(false);
                if (response == null)
                {
                    return NotFound(new NotFoundError("The observations for a patient was not found"));
                }
                var result = _observationsToDtoConverter.Convert(response);
                return Ok(result);
            }
            return BadRequest();
        }

        /// <summary>
        /// Gets Patient's white blood cell (WBCs) count observations
        /// </summary>
        /// <param name="patientId">Patient resource Id</param>
        /// <returns>List of white bloodcell count observations</returns>
        [HttpGet("GetWhiteBloodCellCountObservations/{patientId}", Name = "GetWhiteBloodCellCountForPatientAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ObservationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWhiteBloodCellCountObservationsForPatientAsync(string patientId)
        {
            Logger.LogDebug(nameof(GetWhiteBloodCellCountObservationsForPatientAsync));
            if (ModelState.IsValid)
            {
                var response = await _observationService.GetWhiteBloodCellCountObservationsForPatientAsync(patientId).ConfigureAwait(false);
                if (response == null)
                {
                    return NotFound(new NotFoundError("The observations for a patient was not found"));
                }
                var result = _observationsToDtoConverter.Convert(response);
                return Ok(result);
            }
            return BadRequest();
        }

        /// <summary>
        /// Add hemoglobin observation
        /// </summary>
        /// <param name="patientId">Patient resource Id</param>
        /// <param name="value">Hemoglobin value</param>
        /// <returns>Return observation</returns>
        [HttpPost("AddHemoglobinObservation/{patientId}/{value}", Name = "AddHemoglobinObservationAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ObservationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddHemoglobinObservationAsync([Required] string patientId, [Required] decimal value)
        {
            Logger.LogDebug(nameof(AddHemoglobinObservationAsync));
            if (ModelState.IsValid)
            {
                var response = await _observationService.AddHemoglobinObservationAsync(patientId, value).ConfigureAwait(false);
                if (response == null)
                {
                    return NotFound(new NotFoundError("The observation was not created"));
                }
                var result = _observationToDtoConverter.Convert(response);
                return Ok(result);
            }
            else
            {
                var errors = ModelStateErrors();
                return BadRequest(errors);
            }
        }

        /// <summary>
        /// Add red blood cell count (RBCs) observation
        /// </summary>
        /// <returns>Return observation</returns>
        [HttpPost("AddRedBloodCellCountObservation/{patientId}/{value}", Name = "AddRedBloodCellCountObservationAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ObservationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddRedBloodCellCountObservationAsync([Required] string patientId, [Required] decimal value)
        {
            Logger.LogDebug(nameof(AddRedBloodCellCountObservationAsync));
            if (ModelState.IsValid)
            {
                var response = await _observationService.AddRedBloodCellCountObservationAsync(patientId, value).ConfigureAwait(false);
                if (response == null)
                {
                    return NotFound(new NotFoundError("The observation was not created"));
                }
                var result = _observationToDtoConverter.Convert(response);
                return Ok(result);
            }
            else
            {
                var errors = ModelStateErrors();
                return BadRequest(errors);
            }
        }

        /// <summary>
        /// Add white blood cell count (WBCs) observation
        /// </summary>
        /// <returns>Return observation</returns>
        [HttpPost("AddWhiteBloodCellCountObservation/{patientId}/{value}", Name = "AddWhiteBloodCellCountObservationAsync")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ObservationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddWhiteBloodCellCountObservationAsync([Required] string patientId, [Required] decimal value)
        {
            Logger.LogDebug(nameof(AddWhiteBloodCellCountObservationAsync));
            if (ModelState.IsValid)
            {
                var response = await _observationService.AddWhiteBloodCellCountObservationAsync(patientId, value).ConfigureAwait(false);
                if (response == null)
                {
                    return NotFound(new NotFoundError("The observation was not created"));
                }
                var result = _observationToDtoConverter.Convert(response);
                return Ok(result);
            }
            else
            {
                var errors = ModelStateErrors();
                return BadRequest(errors);
            }
        }
    }
}