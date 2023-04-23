using System.ComponentModel.DataAnnotations;

namespace Poc.AsyncApiAndOutbox.Features;

public record ClientRequest(Guid Id, [Required] string Name);