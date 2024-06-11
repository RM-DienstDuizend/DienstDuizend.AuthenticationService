using FluentValidation;

namespace DienstDuizend.AuthenticationService.Common.Extensions;

public static class FluentValidationExtensions
{

	/// <summary>
	/// Defines a uniqueness validator on the current rule builder, but only for string properties.
	/// Validation will fail if the contents of the string contains more then the set amount of allowed duplicate characters
	/// </summary>
	/// <typeparam name="T">Type of object being validated</typeparam>
	/// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
	/// <param name="count">The minimal amount of required unique characters in the string</param>
	/// <returns></returns>
	public static IRuleBuilderOptions<T, string> MaxDuplicateChars<T>(this IRuleBuilder<T, string> ruleBuilder, int maxAllowedDuplicates)
	{
		return ruleBuilder
			.Must(v => v.GroupBy(c => c).All(g => g.Count() <= maxAllowedDuplicates))
			.WithMessage($"Your password cannot contain more than {maxAllowedDuplicates} duplicate characters.");
	}

	public static IRuleBuilderOptions<T, string> IsContainedIn<T>(this IRuleBuilder<T, string> ruleBuilder, IEnumerable<string> source)
	{
		return ruleBuilder
			.Must(v => source.Contains(v));
	}

	public static IRuleBuilderOptions<T, string> IsNotContainedIn<T>(this IRuleBuilder<T, string> ruleBuilder, IEnumerable<string> source)
	{
		return ruleBuilder
			.Must(v => !source.Contains(v)); // Negate the Contains check
	}

}