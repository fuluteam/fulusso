using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Authorization
{
   public enum UserValidate
   {
       /// <summary>
       /// 不启用验证。默认使用全局用户验证
       /// </summary>
        None=0,
       /// <summary>
       /// 开启用户验证
       /// </summary>
        On=1,
       /// <summary>
       /// 关闭用户验证
       /// </summary>
        Off= 2
    }

   public enum ClientValidate
   {
       /// <summary>
       /// 不启用验证。默认使用全局Client验证
       /// </summary>
       None = 0,
       /// <summary>
       /// 开启Client验证
       /// </summary>
       On = 1,
        /// <summary>
        /// 关闭Client验证
        /// </summary>
        Off = 2
   }

    /// <summary>
    /// 
    /// </summary>
    public class BasicAuthorizeAttribute : AuthorizeAttribute, IResourceFilter
    {
        public UserValidate UserValidate { get; set; }

        public ClientValidate ClientValidate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public BasicAuthorizeAttribute() : base("Basic")
        {
            UserValidate = UserValidate.None;
            ClientValidate = ClientValidate.None;
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
        }
    }

}