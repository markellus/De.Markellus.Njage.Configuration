using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace De.Markellus.Njage.Configuration.Parsers
{
    internal class njStringParser : njAbstractConfigNodeParser
    {
        public override string GetConfigTypeAttribute()
        {
            return "string";
        }

        public override Type GetConfigType()
        {
            return typeof(string);
        }

        public override bool HasGenericComponents()
        {
            return false;
        }

        public override object Parse(XmlElement node)
        {
            return node.GetAttribute("value");
        }

        public override bool Apply(ref XmlElement node, object value)
        {
            node.SetAttribute("type", GetConfigTypeAttribute());
            node.SetAttribute("value", (string)value);
            return true;
        }
    }
}
