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
    internal class njIntParser : njAbstractConfigNodeParser
    {
        public override string GetConfigTypeAttribute()
        {
            return "int";
        }

        public override Type GetConfigType()
        {
            return typeof(int);
        }

        public override object Parse(XmlElement node)
        {
            if (int.TryParse(node.GetAttribute("value"), out int iResult))
            {
                return iResult;
            }

            return null;
        }

        public override bool Apply(ref XmlElement node, object value)
        {
            node.SetAttribute("type", GetConfigTypeAttribute());
            node.SetAttribute("value", value.ToString());
            return true;
        }
    }
}