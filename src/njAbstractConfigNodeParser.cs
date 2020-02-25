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
    /// Basisklasse für einen Parser eines bestimmten Konfigurationstyps.
    /// </summary>
    public abstract class njAbstractConfigNodeParser
    {
        /// <summary>
        /// Gibt zurück, welchen Wert das type-Attribut dieser Konfiguration hat.
        /// </summary>
        /// <returns></returns>
        public abstract string GetConfigTypeAttribute();

        /// <summary>
        /// Gibt zurück, welchen Typ diese Klasse behandelt.
        /// </summary>
        public abstract Type GetConfigType();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool HasGenericComponents()
        {
            return false;
        }

        public virtual bool CanParseChildren()
        {
            return false;
        }

        /// <summary>
        /// Erstellt eine neue Instanz des Zieltyps aus dem übergebenen Wert.
        /// </summary>
        /// <param name="node">Der XML-Knoten, aus dem der Wert ausgelesen werden soll.</param>
        public abstract object Parse(XmlElement node);

        /// <summary>
        /// Schreibt die übergebene Objektinstanz in die Konfigurationsdatei.
        ///
        /// Vorbedingungen:
        ///     * Das value vom Tyo <see cref="GetConfigType"/>() ist, ist sichergestellt.
        /// </summary>
        /// <param name="value">Der Wert, der in die Konfigurationsdatei geschrieben werden soll.</param>
        /// <param name="node">Der XML-Knoten, in den der Wert geschrieben werden soll.</param>
        public abstract bool Apply(ref XmlElement node, object value);
    }
}
