using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Fulu.Passport.Domain.Entities
{
    [Table("operator_log")]
    public class OperatorLogEntity : IEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column("id")]
        public int Id { get; set; }
        /// <summary>
        /// 应用Id
        /// </summary>
        [Column("client_id")]
        public int ClientId { get; set; }
        /// <summary>
        /// 用户主键
        /// </summary>
        [Column("userid")]
        public string UserId { get; set; }
        /// <summary>
        /// 动作
        /// </summary>
        [Column("content")]
        public string Content { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        [Column("ip")]
        public string Ip { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        [Column("create_date")]
        public DateTime CreateDate { get; set; }
   
    }
}
