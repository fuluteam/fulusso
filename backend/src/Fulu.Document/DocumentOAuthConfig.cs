
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace  ICH.Document
{

    public class DocumentOAuthConfig 
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DocumentOAuthConfig()
        {
            this.ScopeName = "api1";
            this.ClientId = "js_oauth";
            this.ClientSecret = "secret";
            this.ScopeSeparator = " ";
            this.AppName = "document";
            this.Realm = "realm";
        }
        /// <summary>
        /// scope
        /// </summary>
        public string ScopeName { get; set; }
        /// <summary>
        /// clientId
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// clientSecret
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ScopeSeparator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AppName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Realm { get; set; }
    }
}
