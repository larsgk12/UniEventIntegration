namespace UniEventIntegration.Options;

/// <summary>
/// Validates the configuration options for the AuthManager.
/// </summary>
public sealed class AuthManagerOptionsValidation : IValidateOptions<AuthManagerOptions>
{
    /// <summary>
    /// Validates the AuthManager options.
    /// </summary>
    /// <param name="name">The name of the options being validated.</param>
    /// <param name="options">The AuthManager options to validate.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating the result of the validation.</returns>
    public ValidateOptionsResult Validate(string? name, AuthManagerOptions options)
    {
        if (options.IdentityUri is null)
            return ValidateOptionsResult.Fail("The 'IdentityUri'-value was not found when validating configuration options.");
        if (options.IdentityUri.ToString()[^1] != '/')
            return ValidateOptionsResult.Fail("The 'IdentityUri'-value must end with a slash (/).");
        if (string.IsNullOrWhiteSpace(options.ClientId))
            return ValidateOptionsResult.Fail("The 'ClientId'-value was not found when validating configuration options.");
        if (string.IsNullOrWhiteSpace(options.Scopes))
            return ValidateOptionsResult.Fail("The 'Scope'-value was not found when validating configuration options.");

        return ValidateOptionsResult.Success;
    }
}
