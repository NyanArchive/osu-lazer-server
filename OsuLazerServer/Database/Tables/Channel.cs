using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OsuLazerServer.Database.Tables;

[Table("channels")]
public class Channel
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Required]
    public int Id { get; set; }


    [Column("name")] public string Name { get; set; }
    
    [Column("type")] public ChannelType Type { get; set; }
    
    [Column("allowed_write")] public bool AllowedWrite { get; set; }
    
    [Column("description")] public  string Description { get; set; }
}

public enum ChannelType
{
    PM,
    PUBLIC
}