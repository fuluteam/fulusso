using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Fulu.Passport.Domain.Entities
{
    [Table("external_user")]
    public class ExternalUserEntity : IEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 通行证用户主键
        /// </summary>
        [Column("userid")]
        public string UserId { get; set; }

        /// <summary>
        /// 第三方用户id
        /// </summary>
        [Column("provider_key")]
        public string ProviderKey { get; set; }

        /// <summary>
        /// 第三方类型
        /// </summary>
        [Column("login_provider")]
        public string LoginProvider { get; set; }

        /// <summary>
        /// 应用Id
        /// </summary>
        [Column("client_id")]
        public int ClientId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Column("nickname")]
        public string Nickname { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_date")]
        public DateTime? CreateDate { get; set; }
    }
}
