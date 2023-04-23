using System.ComponentModel.DataAnnotations;

namespace Poc.FirmwareUploadOutbox.Features;

public record ClientRequest(Guid Id, [Required] string Name);