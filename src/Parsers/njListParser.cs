/***********************************************************/
/* NJAGE Engine - Configuration Framework                  */
/*                                                         */
/* Copyright 2020 Marcel Bulla. All rights reserved.       */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Markellus.Njage.NetInternals;

namespace De.Markellus.Njage.Configuration.Parsers
{
    internal class njListParser : njAbstractConfigNodeParser
    {
        public override string GetConfigTypeAttribute()
        {
            return "list";
        }

        public override Type GetConfigType()
        {
            return typeof(IList);
        }

        public override bool HasGenericComponents()
        {
            return true;
        }

        public override bool CanParseChildren()
        {
            return true;
        }

        public override object Parse(XmlElement node)
        {
            string strContent = node.GetAttribute("content");

            IList listResult = null;
            bool bFirst = true;
            Type tComparer = null;

            foreach (XmlElement nodeSub in node.ChildNodes)
            {
                if (nodeSub.LocalName != "item")
                {
                    return null;
                }

                object obj = njConfigNodeParserLibrary.Parse(nodeSub);

                if (bFirst)
                {
                    bFirst = false;

                    if (njConfigNodeParserLibrary.GetParserHasGenericComponentsFromAttribute(strContent))
                    {
                        tComparer = nodeSub.GetAttribute("type") == strContent ? obj.GetType() : null;
                    }
                    else
                    {
                        tComparer = njConfigNodeParserLibrary.GetParserTypeFromAttribute(strContent);
                    }

                    if (tComparer == null)
                    {
                        return null;
                    }

                    listResult = (IList)njGenericRuntimeCaller.Invoke(typeof(List<>), tComparer, node.ChildNodes.Count);
                }

                Type tOther = obj.GetType();
                if (tOther.IsAssignableFrom(tComparer) && tComparer.IsAssignableFrom(tOther))
                {
                    listResult.Add(obj);
                }
                else
                {
                    return null;
                }
            }

            return listResult;
        }

        public override bool Apply(ref XmlElement node, object value)
        {
            IList listValue = (IList) value;
            Type type = value.GetType();
            string strContent = njConfigNodeParserLibrary.GetParserAttributeFromType(type.GenericTypeArguments[0]);

            if (string.IsNullOrEmpty(strContent))
            {
                return false;
            }

            node.RemoveAll();
            node.SetAttribute("type", "list");
            node.SetAttribute("content", strContent);

            foreach (object obj in listValue)
            {
                 XmlElement nodeSub = node.OwnerDocument.CreateElement("item");
                node.AppendChild(nodeSub);
                if (!njConfigNodeParserLibrary.Apply(nodeSub, obj))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
