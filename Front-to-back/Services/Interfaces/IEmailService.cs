using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Front_to_back.Services.Interfaces
{
    public interface IEmailService
    {
        void Send(string to, string subject, string html, string from = null);
    }
}
