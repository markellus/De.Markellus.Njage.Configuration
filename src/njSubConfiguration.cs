/***********************************************************/
/* NJAGE Engine - Configuration Framework                  */
/*                                                         */
/* Copyright 2020 Marcel Bulla. All rights reserved.       */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System.Xml;

namespace De.Markellus.Njage.Configuration
{
    /// <summary>
    /// A subset of another Configuration.
    /// </summary>
    public class njSubConfiguration : njAbstractConfiguration
    {
        /// <summary>
        /// The root element of the subset, which is a child element of the parent configuration.
        /// </summary>
        internal XmlElement SubRoot { get; }

        /// <summary>
        /// Creates a new configuration based on a XML node.
        /// </summary>
        /// <param name="nodeSubRoot"></param>
        internal njSubConfiguration(XmlElement nodeSubRoot)
        {
            SubRoot = nodeSubRoot;
            CanWrite = true;
        }
        
        protected override void LoadXmlStructure(out XmlElement nodeRoot)
        {
            //just return our root
            nodeRoot = SubRoot;
            IsValid = true;
        }

        protected override void WriteXmlStructure(XmlElement nodeRoot)
        {
            //nothing to do here.
        }
    }

    public static class njSubConfigurationExtensions
    {
        /// <summary>
        /// Creates a subset configuration inside an existing configuration as a setting.
        /// </summary>
        /// <param name="configParent">The parent configuration</param>
        /// <param name="strIdentifier">The identifier of the setting.</param>
        /// <param name="iIndex">An index, if there is more than one setting available via the given
        /// <see cref="strIdentifier"/>.</param>
        public static njSubConfiguration CreateChild(this njAbstractConfiguration configParent, string strIdentifier,
            int iIndex = 0)
        {
            njSubConfiguration configSub = new njSubConfiguration(configParent.CreateDocumentNode());
            configSub.LoadConfiguration();
            configParent.Set(strIdentifier, configSub, iIndex);
            return configSub;
        }

        /// <summary>
        /// Returns a subset configuration.
        /// This method is equivalent to Get(strIdentifier, CreateChild(strIdentifier, iIndex), iIndex).
        /// </summary>
        /// <param name="configParent">The parent configuration</param>
        /// <param name="strIdentifier">The identifier of the setting.</param>
        /// <param name="iIndex">An index, if there is more than one setting available via the given
        /// <see cref="strIdentifier"/>.</param>
        public static njSubConfiguration GetSubConfig(this njAbstractConfiguration configParent, string strIdentifier,
            int iIndex = 0)
        {
            return configParent.Get<njSubConfiguration>(strIdentifier, null, iIndex) ??
                   (configParent.CanWrite ? configParent.CreateChild(strIdentifier, iIndex) : null);
        }
    }
}