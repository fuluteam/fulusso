using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Redis;

namespace FuLu.Passport.Domain.Entities
{
    [Table("user")]
    public class UserEntity : IEntity
    {
        [RedisKey("id", true)]
        [Column("id")]
        public string Id { get; set; }
        [RedisKey("username")]
        [Column("username")]
        public string UserName { get; set; }
        [Column("nickname")]
        public string NickName { get; set; }
        [RedisKey("phone")]
        [Column("phone")]
        public string Phone { get; set; }
        [Column("gender")]
        public int Gender { get; set; }
        [Column("figure_url")]
        public string FigureUrl { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("birthday")]
        public DateTime? Birthday { get; set; }
        //[Column("activated")]
        //public bool Activated { get; set; }
        [Column("locked")]
        public bool Locked { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
        [Column("last_login_time")]
        public DateTime? LastLoginTime { get; set; }
        [Column("last_login_ip")]
        public string LastLoginIp { get; set; }
        [Column("last_login_address")]
        public string LastLoginAddress { get; set; }
        [Column("login_count")]
        public int LoginCount { get; set; }
        [Column("login_error_count")]
        public int LoginErrorCount { get; set; }
        [Column("last_try_login_time")]
        public DateTime? LastTryLoginTime { get; set; }
        [Column("register_client_id")]
        public int RegisterClientId { get; set; }
        [Column("register_client_name")]
        public string RegisterClientName { get; set; }
        [Column("register_ip")]
        public string RegisterIp { get; set; }
        [Column("register_address")]
        public string RegisterAddress { get; set; }
        [Column("register_time")]
        public DateTime RegisterTime { get; set; }
        [Column("enabled_two_factor")]
        public bool EnabledTwoFactor { get; set; }
    }
}
