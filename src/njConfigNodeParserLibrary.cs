/***********************************************************/
/* NJAGE Engine - Configuration Framework                  */
/*                                                         */
/* Copyright 2020 Marcel Bulla. All rights reserved.       */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using De.Markellus.Njage.NetInternals;
using static De.Markellus.Njage.Debugging.njLogger;

namespace De.Markellus.Njage.Configuration
{
    /// <summary>
    /// Static interface, knows all parsers and can use them to process data types and objects
    /// without the caller having to know the exact implementation.
    /// </summary>
    public static class njConfigNodeParserLibrary
    {
        private static bool _bAutoRegistered;
        private static Mutex _mutex;

        /// <summary>
        /// Dictionary with available configuration parsers, sorted by their corresponding xml attribute.
        /// </summary>
        private static readonly Dictionary<string, njAbstractConfigNodeParser> _dicParsersByAttribute;

        /// <summary>
        /// Dictionary with available configuration parsers, sorted by their corresponding type.
        /// </summary>
        private static readonly Dictionary<Type, njAbstractConfigNodeParser> _dicParsersByType;

        static njConfigNodeParserLibrary()
        {
            _dicParsersByAttribute = new Dictionary<string, njAbstractConfigNodeParser>();
            _dicParsersByType = new Dictionary<Type, njAbstractConfigNodeParser>();
            _bAutoRegistered = false;
            _mutex = new Mutex();
        }

        /// <summary>
        /// Automatically scans loaded assemblies for configuration parsers and adds them to the dictionaries.
        /// Assemblies which are loaded after this method has been called will also be scanned.
        /// This method can only be called once!
        /// </summary>
        public static void AutoRegisterParsers()
        {
            _mutex.WaitOne();

            static void LambdaRegister(Assembly assembly)
            {
                foreach (njAbstractConfigNodeParser parser in njReflectiveEnumerator
                    .GetInstancesOfType<njAbstractConfigNodeParser>(assembly))
                {
                    RegisterParser(parser);
                }
            }

            if (_bAutoRegistered)
            {
                njWarn("njConfigNodeParserLibrary.AutoRegisterParsers() was called multiple times!");
                _mutex.ReleaseMutex();
                return;
            }

            _bAutoRegistered = true;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) LambdaRegister(assembly);
            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => LambdaRegister(args.LoadedAssembly);

            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Registers a new configuration parser.
        /// Do not use this method if <see cref="AutoRegisterParsers"/>() has been called.
        /// </summary>
        /// <param name="parser">The new configuration parser.</param>
        public static void RegisterParser(njAbstractConfigNodeParser parser)
        {
            _mutex.WaitOne();

            njInfo($"Config Parser registered: {parser.GetType().Name}, Target: {parser.GetConfigType()}, type attribute: {parser.GetConfigTypeAttribute()}");

            if (!_dicParsersByAttribute.TryAdd(parser.GetConfigTypeAttribute(), parser) ||
                !_dicParsersByType.TryAdd(parser.GetConfigType(), parser))
            {
                njWarn(
                    $"There is more than one config parser for {parser.GetConfigTypeAttribute()}, additional parsers will be ignored!");
            }

            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Tries to parse a xml node and returns the parsed object.
        /// </summary>
        /// <param name="node">The node which holds the object information.</param>
        /// <returns>A new object which type corresponds to the type given in the type xml attribute,
        /// or null if parsing the node has failed.</returns>
        public static object Parse(XmlElement node)
        {
            string strType = node.GetAttribute("type");

            if (string.IsNullOrEmpty(strType))
            {
                return null;
            }

            _mutex.WaitOne();
            bool bHasParser = _dicParsersByAttribute.TryGetValue(strType, out njAbstractConfigNodeParser parser);
            _mutex.ReleaseMutex();

            if(bHasParser)
            {
                try
                {
                    if (parser.Parse(node) is object result)
                    {
                        return result;
                    }
                    else
                    {
                        njWarn($"The key {node.Name} could not be read with the desired type!");
                        return null;
                    }
                }
                catch
                {
                    njError($"An error occurred while reading {node.Name}! Wrong type specification?");
                    return null;
                }
            }

            njError($"The key {node.Name} of type {strType} could not be read: Unknown data type!");
            return null;
        }

        /// <summary>
        /// Saves an object into a xml node.
        /// </summary>
        /// <param name="node">The node in which the object should be saved.</param>
        /// <param name="value">The object itself.</param>
        /// <returns>true, if the object was saved successfully, otherwise false.</returns>
        public static bool Apply(XmlElement node, object value)
        {
            _mutex.WaitOne();
            bool bHasParser = _dicParsersByType.TryGetValue(value.GetType(), out njAbstractConfigNodeParser parser);

            if (!bHasParser)
            {
                parser = _dicParsersByType.FirstOrDefault(kvp =>
                    kvp.Value.CanParseChildren() && kvp.Value.GetConfigType().IsAssignableFrom(value.GetType())).Value;
                bHasParser = parser != null;
            }

            _mutex.ReleaseMutex();

            if (bHasParser)
            {
                try
                {
                    if (!parser.Apply(ref node, value))
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    njError($"An error occurred while writing {value.GetType()}!");
                    return false;
                }

                return true;
            }

            njError($"There is no suitable config parser for objects of type {value.GetType()}!");
            return false;
        }

        /// <summary>
        /// Returns a type object that matches a type string inside a xml node.
        /// </summary>
        /// <param name="strAttribute">The type attribute as string</param>
        /// <returns>The type object if a parser was found, otherwise null.</returns>
        public static Type GetParserTypeFromAttribute(string strAttribute)
        {
            return _dicParsersByAttribute.TryGetValue(strAttribute, out njAbstractConfigNodeParser parser)
                ? parser.GetConfigType()
                : null;
        }

        /// <summary>
        /// Returns an attribute string that matches a type object.
        /// </summary>
        /// <param name="type">The type object</param>
        /// <returns>The attribute string if a parser was found, otherwise null.</returns>
        public static string GetParserAttributeFromType(Type type)
        {
            return _dicParsersByType.TryGetValue(type, out njAbstractConfigNodeParser parser)
                ? parser.GetConfigTypeAttribute()
                : _dicParsersByAttribute.FirstOrDefault(kvp =>
                    kvp.Value.CanParseChildren() && kvp.Value.GetConfigType().IsAssignableFrom(type)).Key;
        }

        /// <summary>
        /// Checks if there is a generic parser for a given tpe attribute,
        /// which may be able to parse a xml node into a generic object.
        /// </summary>
        /// <param name="strAttribute">The type attribute as string</param>
        /// <returns>true, if a generic parser is available, otherwise false.</returns>
        public static bool GetParserHasGenericComponentsFromAttribute(string strAttribute)
        {
            return _dicParsersByAttribute.TryGetValue(strAttribute, out njAbstractConfigNodeParser parser) &&
                   parser.HasGenericComponents();
        }
    }
}
