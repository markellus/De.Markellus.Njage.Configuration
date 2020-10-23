/***********************************************************/
/* NJAGE Engine - Configuration Framework                  */
/*                                                         */
/* Copyright 2020 Marcel Bulla. All rights reserved.       */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace De.Markellus.Njage.Configuration
{
    /// <summary>
    /// Contains settings that are applied to all objects in this namespace.
    /// </summary>
    public static class njGlobalSettings
    {
        /// <summary>
        /// The name of the root node of a config file.
        /// </summary>
        public static string RootNodeName = "njageconfig";

        /// <summary>
        /// Determines whether the version attribute of a config file should be ignored.
        /// </summary>
        public static bool IgnoreConfigVersion = false;
    }
}
