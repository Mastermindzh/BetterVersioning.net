namespace BetterVersioning.Net.Models;

public class BetterVersioningOptions
{

    /// <summary>
    /// Makes the Until version inclusive.
    /// Meaning that if you specify "[Until(6)" on a method that method will be included in version 6.
    /// Without this set to true, "[Until(6)" would mean "anything lower than 6"
    /// </summary>
    /// <value>false by default</value>
    public bool UntilInclusive { get; set; } = false;

    /// <summary>
    /// Detect duplicate routes caused by BetterVersioning.net
    /// </summary>
    /// <value>true by default</value>
    public bool DetectDuplicatesAtStartup { get; set; } = true;
}
