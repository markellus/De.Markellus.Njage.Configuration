/***********************************************************/
/* NJAGE Engine - Configuration Framework                  */
/*                                                         */
/* Copyright 2020 Marcel Bulla. All rights reserved.       */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System;
using System.Xml;
using De.Markellus.Njage.NetInternals;

namespace De.Markellus.Njage.Configuration.Parsers
{
    internal class njEnumParser : njAbstractConfigNodeParser
    {
        public override string GetConfigTypeAttribute()
        {
            return "enum";
        }

        public override Type GetConfigType()
        {
            return typeof(Enum);
        }

        public override bool HasGenericComponents()
        {
            return false;
        }

        public override bool CanParseChildren()
        {
            return true;
        }

        public override object Parse(XmlElement node)
        {
            string strValue = node.GetAttribute("value");
            string strDefinition = node.GetAttribute("definition");

            if (string.IsNullOrEmpty(strValue) || string.IsNullOrEmpty(strDefinition))
            {
                return null;
            }

            Type tEnum = njReflectiveEnumerator.GetTypeFromString(strDefinition);

            if (tEnum != null && Enum.TryParse(tEnum, strValue, out object result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public override bool Apply(ref XmlElement node, object value)
        {
            node.SetAttribute("type", "enum");
            node.SetAttribute("definition", value.GetType().FullName);
            node.SetAttribute("value", value.ToString());

            return true;
        }
    }
}
