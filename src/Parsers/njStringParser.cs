/***********************************************************/
/* NJAGE Engine - Configuration Framework                  */
/*                                                         */
/* Copyright 2020 Marcel Bulla. All rights reserved.       */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System;
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
