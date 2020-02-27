/***********************************************************/
/* NJAGE Engine - Configuration Framework                  */
/*                                                         */
/* Copyright 2020 Marcel Bulla. All rights reserved.       */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static De.Markellus.Njage.Debugging.njLogger;

namespace De.Markellus.Njage.Configuration
{
    /// <summary>
    /// The base class for configurations
    /// </summary>
    public abstract class njAbstractConfiguration
    {
        /// <summary>
        /// The root element of the xml document
        /// </summary>
        private XmlElement _nodeRoot;

        /// <summary>
        /// A Dictionary with all loaded settings.
        /// </summary>
        private Dictionary<string, List<object>> _dicConfigs;

        /// <summary>
        /// Returns true if it is possible to overwrite this configuration.
        /// </summary>
        public bool CanWrite { get; protected set; }

        /// <summary>
        /// Returns the total count of settings.
        /// </summary>
        public int Count => _dicConfigs.Sum(x => x.Value.Count);

        /// <summary>
        /// Returns whether the configuration is valid.
        /// </summary>
        public bool IsValid { get; protected set; }

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        public void LoadConfiguration()
        {
            _dicConfigs = new Dictionary<string, List<object>>();
            LoadXmlStructure(out _nodeRoot);
            if (_nodeRoot != null && IsValid)
            {
                ReadConfigurationNode(_nodeRoot, "");
            }
        }

        /// <summary>
        /// Loads the xml structure of the the configuration.
        /// </summary>
        /// <param name="nodeRoot">The root element of the loaded xml configuration, or null if the configuration is
        /// invalid./param>
        protected abstract void LoadXmlStructure(out XmlElement nodeRoot);

        /// <summary>
        /// Saves the configuration. This will silently fail if the configuration is invalid or read only.
        /// </summary>
        public void SaveConfiguration()
        {
            if (!IsValid || !CanWrite)
            {
                njWarn($"Can not save configuation: IsValid={IsValid}, CanWrite={CanWrite}");
                return;
            }
            WriteConfigurationNode(_nodeRoot, "", new Dictionary<string, List<object>>(_dicConfigs));
            WriteXmlStructure(_nodeRoot);
        }

        /// <summary>
        /// Writes the xml structure of the configuration.
        /// </summary>
        /// <param name="nodeRoot">The root element of the loaded xml configuration.</param>
        protected abstract void WriteXmlStructure(XmlElement nodeRoot);

        /// <summary>
        /// Returns a setting.
        /// </summary>
        /// <param name="strIdentifier">The identifier of the setting.</param>
        /// <param name="defaultValue">The default value of the setting.</param>
        /// <param name="iIndex">An index, if there is more than one setting available via the given
        /// <see cref="strIdentifier"/>.</param>
        /// <typeparam name="T">The type of the setting.</typeparam>
        /// <returns>The setting or <see cref="defaultValue"/> if the setting does not exist or has a different type.</returns>
        public T Get<T>(string strIdentifier, T defaultValue = default, int iIndex = 0)
        {
            return _dicConfigs.TryGetValue(strIdentifier, out List<object> listNodes) && listNodes.Count > iIndex &&
                   listNodes[iIndex] is T tResult
                ? tResult
                : defaultValue;
        }

        /// <summary>
        /// Overrides a setting, if <see cref="CanWrite"/> is set to true.
        /// </summary>
        /// <param name="strIdentifier">The identifier of the setting.</param>
        /// <param name="value">The new value of the setting.</param>
        /// <param name="iIndex">An index, if there is more than one setting available via the given
        /// <see cref="strIdentifier"/>. If <see cref="iIndex"/> is set to -1, <see cref="value"/> will be appended
        /// after the existing settings.</param>
        /// <param name="bAllowTypeOverride">true, if <see cref="value"/> might be of another type than the original
        /// setting.</param>
        public void Set(string strIdentifier, object value, int iIndex = 0, bool bAllowTypeOverride = false)
        {
            if (!CanWrite)
            {
                njError($"Can not set value \"{strIdentifier}\": The Configuration is Read Only!");
            }
            else if (_dicConfigs.TryGetValue(strIdentifier, out List<object> listNodes))
            {
                if (listNodes.Count > iIndex && value != null && listNodes[iIndex].GetType() != value.GetType() &&
                    !bAllowTypeOverride)
                {
                    njError($"Can not set value \"{strIdentifier}\": The existing value has a different type!");
                }
                else if (iIndex > listNodes.Count)
                {
                    njError(
                        $"Can not set value \"{strIdentifier}\"::{iIndex}: There is no previous node \"{strIdentifier}\"::{iIndex - 1}!");
                }
                else if ((iIndex == listNodes.Count || iIndex == -1) && value != null)
                {
                    listNodes.Add(value);
                }
                else if (value == null)
                {
                    listNodes.RemoveAt(iIndex);
                }
                else
                {
                    listNodes[iIndex] = value;
                }
            }
            else if (value != null)
            {
                _dicConfigs[strIdentifier] = new List<object> {value};
            }
        }

        /// <summary>
        /// Deletes a setting, if <see cref="CanWrite"/> is set to true.
        /// </summary>
        /// <param name="strIdentifier">The identifier of the setting.</param>
        /// <param name="iIndex">An index, if there is more than one setting available via the given
        /// <see cref="strIdentifier"/>.</param>
        public void Delete(string strIdentifier, int iIndex = 0)
        {
            if (!CanWrite)
            {
                njError($"Can not delete value \"{strIdentifier}\": The Configuration is Read Only!");
            }
            Set(strIdentifier, null, iIndex);
        }

        /// <summary>
        /// Deletes all settings with a given identifier, if <see cref="CanWrite"/> is set to true.
        /// </summary>
        /// <param name="strIdentifier">The identifier of the setting.</param>
        public void DeleteAll(string strIdentifier)
        {
            if (!CanWrite)
            {
                njError($"Can not delete value \"{strIdentifier}\": The Configuration is Read Only!");
            }

            if (_dicConfigs.TryGetValue(strIdentifier, out List<object> listNodes))
            {
                listNodes.Clear();
            }
        }

        private void ReadConfigurationNode(XmlElement nodeCurrent, string strCurrentNode)
        {
            foreach (XmlElement nodeSub in nodeCurrent.ChildNodes)
            {
                if (!string.IsNullOrEmpty(nodeSub.GetAttribute("type")))
                {
                    object objParsed = njConfigNodeParserLibrary.Parse(nodeSub);

                    string strSubNode = GetSubNodeName(strCurrentNode, nodeSub);

                    if (objParsed != null)
                    {
                        if (!_dicConfigs.TryGetValue(strSubNode, out List<object> listNodes))
                        {
                            listNodes = new List<object>();
                            _dicConfigs.Add(strSubNode, listNodes);
                        }

                        listNodes.Add(objParsed);
                    }
                }
                else
                {
                    ReadConfigurationNode(nodeSub, strCurrentNode);
                }
            }
        }

        private void WriteConfigurationNode(XmlElement nodeCurrent, string strCurrentNode, Dictionary<string, List<object>> dicConfigs)
        {
            foreach (var kvp in _dicConfigs)
            {
                for(int i = 0; i < kvp.Value.Count; i++)
                {
                    WriteSubNode(_nodeRoot, kvp.Key, kvp.Value[i], i);
                }
            }
            RemoveUnappliedNodes(_nodeRoot);
        }

        private void WriteSubNode(XmlElement nodeCurrent, string strNode, object obj, int iNode)
        {
            string strPart = strNode.Split('.')[0];
            List<XmlElement> listNodeNext =
                nodeCurrent.Cast<XmlElement>().Where(nodeSub => nodeSub.LocalName == strPart).ToList();

            while (listNodeNext.Count <= iNode)
            {
                XmlElement node = nodeCurrent.OwnerDocument.CreateElement(strPart);
                nodeCurrent.AppendChild(node);
                listNodeNext.Add(node);
            }
            
            if (strNode.Contains('.'))
            {
                listNodeNext[iNode].SetAttribute("__Applied", "true");
                WriteSubNode(listNodeNext[iNode], strNode.Substring(strPart.Length), obj, iNode);
            }
            else
            {
                if (njConfigNodeParserLibrary.Apply(listNodeNext[iNode], obj))
                {
                    listNodeNext[iNode].SetAttribute("__AppliedRecursive", "true");
                }
            }
        }

        private void RemoveUnappliedNodes(XmlElement nodeCurrent)
        {
            for(int i = 0; i < nodeCurrent.ChildNodes.Count; i++)
            {
                XmlElement nodeSub = (XmlElement) nodeCurrent.ChildNodes[i];
                if (nodeSub.GetAttribute("__AppliedRecursive") == "true")
                {
                    nodeSub.RemoveAttribute("__AppliedRecursive");
                }
                else if (nodeSub.GetAttribute("__Applied") != "true")
                {
                    nodeCurrent.RemoveChild(nodeSub);
                    i--;
                }
                else
                {
                    nodeSub.RemoveAttribute("__Applied");
                    RemoveUnappliedNodes(nodeSub);
                }
            }
        }

        private string GetSubNodeName(string strCurrentNode, XmlElement nodeSub)
        {
            return strCurrentNode + (string.IsNullOrEmpty(strCurrentNode) ? "" : ".") + nodeSub.LocalName;
        }
    }
}
