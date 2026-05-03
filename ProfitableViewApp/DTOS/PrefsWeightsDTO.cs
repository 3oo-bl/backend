using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProfitableViewApp.DTOS;

[Table("PrefsWeights")]
public class PrefsWeightsDTO
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    public float Price { get; set; }
    public float Delivery { get; set; }
    public float Rating { get; set; }
}