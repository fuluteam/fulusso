using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fulu.Passport.Domain.Interface
{
    public interface IPassportClient
    {
        Task<(string code, string msg, string msgId)> SendSms(string receiveNumber, string messageContent, string signatureCode);
    }
}
