using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Helpers for adding the stable PowerLab ProblemDetails extensions.
    /// </summary>
    public static class PowerLabProblemDetailsExtensions
    {
        public static PowerLabProblemDetailsDto WithValidationIssues(
            this PowerLabProblemDetailsDto problemDetails,
            IReadOnlyList<ValidationIssueDto> issues)
        {
            problemDetails.ValidationIssues = issues;
            return problemDetails;
        }
    }
}
