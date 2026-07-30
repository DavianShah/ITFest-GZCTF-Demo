using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GZCTF.Models.Data;

[Index(nameof(GameId), nameof(Category), IsUnique = true)]
public class SpeedrunCategory
{
    [Key]
    public int Id { get; set; }
    public int GameId { get; set; }
    public ChallengeCategory Category { get; set; }
    public bool Used { get; set; }
    public bool Included { get; set; } = true;
    public Game Game { get; set; } = null!;
}
