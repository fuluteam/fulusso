using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FuLu.Passport.Domain.Entities
{
    [Table("client")]
    public class ClientEntity : IEntity
    {
        [Column("id")]
        public string Id { get; set; }
        [Column("full_name")]
        public string FullName { get; set; }
        [Column("client_id"),Key]
        public int ClientId { get; set; }
        [Column("client_secret")]
        public string ClientSecret { get; set; }
        [Column("host_url")]
        public string HostUrl { get; set; }
        [Column("redirect_uri")]
        public string RedirectUri { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}
