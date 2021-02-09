using Hl7.Fhir.Common.Contracts.Dto;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Common.Contracts.Converters
{
    /// <summary>
    /// Observation to ObservationDto converter
    /// </summary>
    public class ObservationToDtoCoverter : IConverter<Observation, ObservationDto>, IConverter<IList<Observation>, IList<ObservationDto>>
    {
        private readonly ILogger<ObservationToDtoCoverter> _logger;

        public ObservationToDtoCoverter(ILogger<ObservationToDtoCoverter> logger)
        {
            _logger = logger;
        }

        public ObservationDto Convert(Observation observation)
        {
            _logger.LogDebug(nameof(Convert));
            var coding = observation.Code.Coding.FirstOrDefault();
            var value = (Quantity)observation.Value;
            var effective = (FhirDateTime)observation.Effective;
            var result = new ObservationDto
            {
                Id = observation.Id,
                System = coding.System,
                Code = coding.Code,
                Name = observation.Code.Text,
                Unit = value.Code,
                Value = (decimal)value.Value,
                Effective = effective.ToDateTimeOffset(new TimeSpan(1, 0, 0))
            };
            return result;
        }

        public IList<ObservationDto> Convert(IList<Observation> observations)
        {
            _logger.LogDebug("ConvertList");
            return observations.Select(cmp =>
            {
                var observationDto = Convert(cmp);
                return observationDto;
            }).ToList();
        }
    }
}
