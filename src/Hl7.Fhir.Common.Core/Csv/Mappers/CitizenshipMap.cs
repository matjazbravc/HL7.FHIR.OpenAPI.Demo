﻿using CsvHelper.Configuration;
using Hl7.Fhir.Common.Core.Csv.Models;

namespace Hl7.Fhir.Common.Core.Csv.Mappers
{
    public sealed class CitizenshipMap : ClassMap<Citizenship>
    {
        public CitizenshipMap()
        {
            Map(m => m.Code).Name("Code");
            Map(m => m.Explanation).Name("Explanation");
            Map(m => m.From).Name("From");
            Map(m => m.Through).Name("Through");
        }
    }
}
