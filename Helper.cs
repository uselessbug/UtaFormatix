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
            return e.Descendants(e.GetDefaultNamespace() + childName).FirstOrDefault() ??
                   e.Descendants(e.Name.Namespace + childName).FirstOrDefault();
        }

        /// <summary>
        /// Find childs with corresponding name
        /// </summary>
        /// <param name="e"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> Childs(this XElement e, string childName)
        {
            var d = e.Descendants(e.GetDefaultNamespace() + childName);
            return !d.Any() ? e.Descendants(e.Name.Namespace + childName) : d;
        }

        /// <summary>
        /// Apply Namespace for itself and all chlids
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="nameSpace"></param>
        /// <param name="overwrite"></param>
        public static void ApplyNamespace(this XElement parent, XNamespace nameSpace, bool overwrite = true)
        {
            if (nameSpace == XNamespace.None || nameSpace == "")
            {
                return;
            }
            if (overwrite || parent.Name.Namespace == XNamespace.None || parent.Name.Namespace == "")
            {
                parent.Name = nameSpace + parent.Name.LocalName;
            }
            foreach (XElement child in parent.Elements())
            {
                ApplyNamespace(child, nameSpace);
            }
        }
    }
}
