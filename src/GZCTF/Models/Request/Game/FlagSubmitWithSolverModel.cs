using System.ComponentModel.DataAnnotations;

namespace GZCTF.Models.Request.Game;

/// <summary>
/// Multipart flag submission with a solver file
/// </summary>
public class FlagSubmitWithSolverModel : FlagSubmitModel
{
    /// <summary>
    /// Solver source code or archive
    /// </summary>
    [Required]
    public IFormFile SolverFile { get; set; } = null!;
}
