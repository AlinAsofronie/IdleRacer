using System.Runtime.CompilerServices;

// Exposes internal simulation helpers (e.g. per-car run details such as the maximum
// speed reached) to the Edit Mode test assembly, so tests can verify internal invariants
// (like the TopSpeed clamp) without widening the public production API.
[assembly: InternalsVisibleTo("IdleRacer.Racing.Tests.EditMode")]
