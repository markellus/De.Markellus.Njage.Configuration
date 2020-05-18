/***********************************************************/
/* NJAGE Engine - Configuration Framework                  */
/*                                                         */
/* Copyright 2020 Marcel Bulla. All rights reserved.       */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System;
using System.Xml;

namespace De.Markellus.Njage.Configuration
{
    /// <summary>
    /// Base class for a parser of a certain type of setting.
    /// </summary>
    public abstract class njAbstractConfigNodeParser
    {
        /// <summary>
        /// Returns the value of the type attribute of this setting.
        /// </summary>
        public abstract string GetConfigTypeAttribute();

        /// <summary>
        /// Returns which type this class handles.
        /// </summary>
        public abstract Type GetConfigType();

        /// <summary>
        /// Returns true if the type handled by this class contains generic components.
        /// </summary>
        public virtual bool HasGenericComponents()
        {
            return false;
        }

        /// <summary>
        /// Returns true if this class can also parse types that are children of the type handled by this class.
        /// Child types will only be parsed if there is no other parser that supports the child type directly.
        /// </summary>
        public virtual bool CanParseChildren()
        {
            return false;
        }

        /// <summary>
        /// Creates a new instance of the target type from the passed XML node.
        /// </summary>
        /// <param name="node">The XML node from which the value is to be read.</param>
        public abstract object Parse(XmlElement node);

        /// <summary>
        /// Writes the transferred object instance to the configuration file.
        /// </summary>
        /// <param name="value">The value to be written to the configuration file.</param>
        /// <param name="node">The XML node in which the value is to be written.</param>
        public abstract bool Apply(ref XmlElement node, object value);
    }
}
