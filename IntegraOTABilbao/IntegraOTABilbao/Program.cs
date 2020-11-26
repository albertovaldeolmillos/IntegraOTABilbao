using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraOTABilbao
{
    class Program
    {
        static void Main(string[] args)
        {
            //Para ejecutar este servicio es necesario:
            //  1- tener acceso al equipo de Desarrollo de OtaBilbao local (192.168.71.5 ---administrador/Gertek.2019)
            //  2- tener arrancado el servicio integraMobileWS.asmx en el IIS Express local (localhost://5675) 
            IntegraMovileWSLocal.integraMobileWSSoapClient clienttariff = new IntegraMovileWSLocal.integraMobileWSSoapClient("integraMobileWSSoap");
            var usuT = clienttariff.ClientCredentials.UserName;//usu=null, psw=null
            //clienttariff.ClientCredentials.UserName.UserName = @"bilbokoudala\extappota";
            //clienttariff.ClientCredentials.UserName.Password = "eER8TncC";
            string XML_ReqTA = @"<ipark_in><u>aliciaruana@hotmail.com</u><SessionID>NOVACIO</SessionID><f>49998006</f><cityID>300001</cityID><d>090211090119</d><vers>1.0</vers><ah>B53AB639FF6EEEB1</ah><em>BILBAOTAO</em></ipark_in>";
            string XML_ResponseTAw = clienttariff.QueryFinePaymentQuantity(XML_ReqTA);
        }
    }
}
