using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace ECommerceLiteUI.SystemVariableManager
{
    public static class SystemVariables
    {
        public static string EMAİL
        {
            get
            {
                try
                {
                    return
                       ConfigurationManager.AppSettings["ECommerceLiteMail"].ToString();
                }
                catch (Exception)
                {

                    throw new Exception("HATA:WebConfig dosyasında email bilgisi bulunamadı.");
                }
            }
        }
    }
}