using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.Web;

namespace integraMobile.Infrastructure
{
    public static class Utils
    {
        /// <summary>
        /// Remove all xmlns:* instances from the passed XmlDocument to simplify our xpath expressions
        /// </summary>
        public static XDocument RemoveNamespaces(XDocument oldXml)
        {
            try
            {
                oldXml.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();
                return oldXml;
            }
            catch (XmlException error)
            {
                throw new XmlException(error.Message + " at Utils.RemoveNamespaces");
            }
        }

        /// <summary>
        /// Remove all xmlns:* instances from the passed XmlDocument to simplify our xpath expressions
        /// </summary>
        public static XDocument RemoveNamespaces(string oldXml)
        {
            XDocument newXml = XDocument.Parse(oldXml);
            return RemoveNamespaces(newXml);
        }

        /// <summary>
        /// Converts a string that has been HTML-enconded for HTTP transmission into a decoded string.
        /// </summary>
        /// <param name="escapedString">String to decode.</param>
        /// <returns>Decoded (unescaped) string.</returns>
        public static string UnescapeString(string escapedString)
        {
            return HttpUtility.HtmlDecode(escapedString);
        }
    }
}
