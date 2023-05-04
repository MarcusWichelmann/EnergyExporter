using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Options;
using RecursiveDataAnnotationsValidation;

namespace SolarEdgeExporter.Utils;

public class RecursiveDataAnnotationValidateOptions<TOptions> : IValidateOptions<TOptions> where TOptions : class {
    private readonly RecursiveDataAnnotationValidator _recursiveDataAnnotationValidator = new();

    public string Name { get; set; }

    public RecursiveDataAnnotationValidateOptions(string optionsBuilderName) {
        Name = optionsBuilderName;
    }

    public ValidateOptionsResult Validate(string? name, TOptions options) {
        if (name != Name)
            return ValidateOptionsResult.Skip;

        var validationResults = new List<ValidationResult>();
        if (_recursiveDataAnnotationValidator.TryValidateObjectRecursive(
                options,
                new ValidationContext(options, null, null),
                validationResults))
            return ValidateOptionsResult.Success;

        List<string> errors = validationResults.Select(
                r =>
                    $"Validation failed for members: '{string.Join(",", r.MemberNames)}' with the error: '{r.ErrorMessage}'.")
            .ToList();
        return ValidateOptionsResult.Fail(errors);
    }
}
