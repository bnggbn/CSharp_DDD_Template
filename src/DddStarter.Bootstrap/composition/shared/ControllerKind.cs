namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Identifies which controller adapter drives the application entry point.
/// </summary>
public enum ControllerKind
{
    /// <summary>Interactive console adapter.</summary>
    Console,

    /// <summary>Command-line adapter.</summary>
    Cli,

    /// <summary>API adapter.</summary>
    Api
}
