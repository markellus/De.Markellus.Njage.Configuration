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
    public class njSubConfigurationParser : njAbstractConfigNodeParser
    {
        public override string GetConfigTypeAttribute()
        {
            return "config";
        }

        public override Type GetConfigType()
        {
            return typeof(njSubConfiguration);
        }

        public override object Parse(XmlElement node)
        {
            njSubConfiguration configSub = new njSubConfiguration(node);
            configSub.LoadConfiguration();

            return configSub;
        }

        public override bool Apply(ref XmlElement node, object value)
        {
            node.RemoveAll();
            node.SetAttribute("type", GetConfigTypeAttribute());

            njSubConfiguration configSub = value as njSubConfiguration;

            if (configSub != null)
            {
                configSub.SaveConfiguration();

                foreach (XmlElement nodeSub in configSub.SubRoot)
                {
                    node.AppendChild(node.OwnerDocument.ImportNode(nodeSub, true));
                }
            }

            return configSub != null;
        }
    }
}