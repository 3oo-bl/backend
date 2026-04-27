using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProfitableViewApp.DTOS;

[Table("Users")]
public class UserDTO
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; } //ДОСТУПНО ИЗ ЗАПРОСА, НАДО ЧТО-ТО С ЭТИМ СДЕЛАТЬ
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public int PreferencesId { get; set; }
    public PrefsWeigthsDTO Preferences { get; set; }
}