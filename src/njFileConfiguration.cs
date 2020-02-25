using System;
using System.IO;
using System.Xml;
using static De.Markellus.Njage.Debugging.njLogger;

namespace De.Markellus.Njage.Configuration
{
    /// <summary>
    /// Eine Sammlung von Einstellungen, basierend auf einem XML-Dokument.
    /// </summary>
    public class njFileConfiguration : njAbstractConfiguration, IDisposable
    {
        private Stream _streamBackend;
        private XmlDocument _document;

        /// <summary>
        /// Erstellt eine neue yfConfigCollection.
        /// </summary>
        /// <param name="strPath">Pfad zu einer Konfigurationsdatei. Wenn die Datei nicht existiert oder nicht geladen
        /// werden kann ist die Sammlung ungültig.</param>
        /// <param name="bWriteable">true wenn es möglich sein soll die Konfiguration zu überschreiben, ansonsten false.</param>
        public njFileConfiguration(string strPath, bool bWriteable = false)
        {
            CanWrite = bWriteable;

            if (!File.Exists(strPath))
            {
                if (CanWrite)
                {
                    _streamBackend = new FileStream(strPath, FileMode.CreateNew);
                    CreateConfiguration();
                }
                else
                {
                    _streamBackend = null;
                }
            }
            else
            {
                _streamBackend = new FileStream(strPath, FileMode.Open);
            }
        }

        /// <summary>
        /// Erstellt eine neue yfConfigCollection.
        /// </summary>
        /// <param name="stream">Ein Stream, aus dem die Konfigurationsdatei ausgelesen werden soll. Wenn die Datei nicht existiert oder nicht geladen
        /// werden kann ist die Sammlung ungültig.</param>
        /// <param name="bWriteable">true wenn es möglich sein soll die Konfiguration zu überschreiben, ansonsten false.</param>
        public njFileConfiguration(Stream stream, bool bWriteable = false)
        {
            _streamBackend = stream;
            CanWrite = bWriteable;

            if (_streamBackend.Length == 0 && CanWrite)
            {
                CreateConfiguration();
            }
        }

        /// <summary>
        /// Erstellt eine neue Konfiguration und schreibt Diese in den Stream.
        /// </summary>
        private void CreateConfiguration()
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<njageconfig version=\"1.1\"/>");
            document.Save(_streamBackend);
        }

        protected override void LoadXmlStructure(out XmlElement nodeRoot)
        {
            try
            {
                _streamBackend.Position = 0;
                _document = new XmlDocument();
                _document.Load(_streamBackend);
            }
            catch
            {
                njWarn("Configuration could not be loaded!");
                IsValid = false;
                nodeRoot = null;
                return;
            }

            nodeRoot = _document.DocumentElement;
            if (nodeRoot.Name != "njageconfig")
            {
                njWarn("The file contains no configuration or is incorrect!");
            }
            else if (_document.DocumentElement.GetAttribute("version") != "1.1")
            {
                njError("Format version is not supported!");
                IsValid = false;
            }
            else
            {
                IsValid = true;
            }
        }

        protected override void WriteXmlStructure(XmlElement nodeRoot)
        {
            _streamBackend.Position = 0;
            _document.Save(_streamBackend);
            _streamBackend.SetLength(_streamBackend.Position);
        }

        public void Dispose()
        {
            _streamBackend?.Dispose();
        }
    }
}
