using System;
using System.Text.RegularExpressions;

namespace Klab.Toolkit.ValueObjects;

/// <summary>
/// Represents an email address.
/// </summary>
public record Email
{
    private const string EmailRegex = "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])";

    /// <summary>
    /// value of the email address.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Create valid email address.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Empty E-Mail Adress is not possible");
        }

        if (!Regex.IsMatch(email, EmailRegex))
        {
            throw new ArgumentException("E-Mail is invalid");
        }

        return new Email(email);
    }

    private Email(string value)
    {
        Value = value;
    }
}
