using Hl7.Fhir.Common.Core.Errors.Base;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Hl7.Fhir.Common.Core.Errors
{
	[ExcludeFromCodeCoverage]
	public class NotFoundError : ApiError
	{
		public NotFoundError()
			: base(StatusCodes.Status404NotFound, HttpStatusCode.NotFound.ToString())
		{
		}
		
		public NotFoundError(string message)
			: base(StatusCodes.Status404NotFound, HttpStatusCode.NotFound.ToString(), message)
		{
		}
	}
}
