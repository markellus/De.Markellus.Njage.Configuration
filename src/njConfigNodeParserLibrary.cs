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
using System.Threading;
using System.Xml;
using De.Markellus.Njage.NetInternals;
using static De.Markellus.Njage.Debugging.njLogger;

namespace De.Markellus.Njage.Configuration
{
    public static class njConfigNodeParserLibrary
    {
        private static bool _bAutoRegistered;
        private static Mutex _mutex;

        /// <summary>
        /// List with available configuration types
        /// </summary>
        private static readonly Dictionary<string, njAbstractConfigNodeParser> _dicParsersByAttribute;

        private static readonly Dictionary<Type, njAbstractConfigNodeParser> _dicParsersByType;

        static njConfigNodeParserLibrary()
        {
            _dicParsersByAttribute = new Dictionary<string, njAbstractConfigNodeParser>();
            _dicParsersByType = new Dictionary<Type, njAbstractConfigNodeParser>();
            _bAutoRegistered = false;
            _mutex = new Mutex();
        }

        public static void AutoRegisterParsers()
        {
            _mutex.WaitOne();

            if (_bAutoRegistered)
            {
                njWarn("njConfigNodeParserLibrary.AutoRegisterParsers() was called multiple times!");
                _mutex.ReleaseMutex();
                return;
            }

            _bAutoRegistered = true;

            foreach (njAbstractConfigNodeParser parser in njReflectiveEnumerator
                .GetInstancesOfType<njAbstractConfigNodeParser>())
            {
                RegisterParser(parser);
            }

            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Registers a new configuration parser.
        /// </summary>
        /// <param name="parser"></param>
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

        public static Type GetParserTypeFromAttribute(string strAttribute)
        {
            return _dicParsersByAttribute.TryGetValue(strAttribute, out njAbstractConfigNodeParser parser)
                ? parser.GetConfigType()
                : null;
        }

        public static string GetParserAttributeFromType(Type type)
        {
            return _dicParsersByType.TryGetValue(type, out njAbstractConfigNodeParser parser)
                ? parser.GetConfigTypeAttribute()
                : _dicParsersByAttribute.FirstOrDefault(kvp =>
                    kvp.Value.CanParseChildren() && kvp.Value.GetConfigType().IsAssignableFrom(type)).Key;
        }

        public static bool GetParserHasGenericComponentsFromAttribute(string strAttribute)
        {
            return _dicParsersByAttribute.TryGetValue(strAttribute, out njAbstractConfigNodeParser parser) &&
                   parser.HasGenericComponents();
        }
    }
}
