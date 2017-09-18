using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UtaFormatix
{
    static class Helper
    {
        /// <summary>
        /// Find the first child with corresponding name
        /// </summary>
        /// <param name="e"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static XElement FirstChild(this XElement e, string childName)
        {
            return e.Descendants(e.GetDefaultNamespace() + childName).FirstOrDefault();
        }

        /// <summary>
        /// Find childs with corresponding name
        /// </summary>
        /// <param name="e"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> Childs(this XElement e, string childName)
        {
            return e.Descendants(e.GetDefaultNamespace() + childName);
        }
    }
}
