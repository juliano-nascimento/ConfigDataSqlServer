using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlServerConfigData.ConfigData
{
    public static class Connection
    {
        public static string GetConnection(IConfiguration configuration)
        {
            //string conn = string.Empty;

            switch (configuration.GetSection("AppSettings").GetSection("Ambiente").Value.ToUpper())
            {
                case "PRD":
                    return configuration.GetConnectionString("ConnectionPrd");
                case "HOM":
                    return configuration.GetConnectionString("Connection");
                default:
                    return configuration.GetConnectionString("Connection");
            }
        }
    }
}
