using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Diff;

namespace De.Markellus.Njage.Configuration
{
    [TestFixture]
    public class njFileConfigurationTests
    {
        [SetUp]
        public void Init()
        {
            njConfigNodeParserLibrary.AutoRegisterParsers();
            if (Directory.Exists("./TestData-TestWrite"))
            {
                Directory.Delete("./TestData-TestWrite", true);
            }
        }

        [Test]
        public void ReadOnlyNonExistantConfiguration()
        {
            AssertFileDoesNotExist("./ReadOnlyNonExistantConfiguration.xml");
            var config = new njFileConfiguration("./ReadOnlyNonExistantConfiguration.xml", false);
            config.LoadConfiguration();
            Assert.IsFalse(config.IsValid);
            config.Dispose();
        }

        [Test]
        public void CreateNonExistantConfiguration()
        {
            var config = OpenTestConfig("CreateNonExistantConfiguration.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);

            CloseTestConfig("./CreateNonExistantConfiguration.xml", config);
        }

        [Test]
        public void CreateNonExistantConfigurationStream()
        {
            AssertFileDoesNotExist("./CreateNonExistantConfiguration.xml");
            var config = new njFileConfiguration(new FileStream("./CreateNonExistantConfiguration.xml", FileMode.CreateNew), true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            config.Dispose();
        }

        [Test]
        public void OpenSimpleConfigOneEntry()
        {
            var config = OpenTestConfig("SimpleConfigOneEntry.xml", false);
            
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(1, config?.Count, "Falsche Anzahl an Items");

            CloseTestConfig("SimpleConfigOneEntry.xml", config);
        }

        [Test]
        public void OpenSimpleConfigOneEntryStream()
        {
            var config = new njFileConfiguration(new FileStream("./TestData/SimpleConfigOneEntry.xml", FileMode.Open), false);
            
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(1, config?.Count, "Falsche Anzahl an Items");
            
            config.Dispose();
        }

        [Test]
        public void ReadSimpleConfigOneEntry()
        {
            var config = OpenTestConfig("SimpleConfigOneEntry.xml", false);
            
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual("a string value", config.Get<string>("setting1"), "Falscher Wert");
            
            CloseTestConfig("SimpleConfigOneEntry.xml", config);
        }

        [Test]
        public void ReadSimpleConfigOneEntryNotExisting()
        {
            var config = OpenTestConfig("SimpleConfigOneEntry.xml", false);
            
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(null, config.Get<string>("setting250"), "Falscher Wert");
            
            CloseTestConfig("SimpleConfigOneEntry.xml", config);
        }

        [Test]
        public void ReadSimpleConfigOneEntryExistingWrongType()
        {
            var config = OpenTestConfig("SimpleConfigOneEntry.xml", false);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(0, config.Get<int>("setting1"), "Falscher Wert");
            
            CloseTestConfig("SimpleConfigOneEntry.xml", config);
        }

        [Test]
        public void WriteSimpleConfigOneEntry()
        {
            var config = OpenTestConfig("WriteSimpleConfigOneEntry.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            config.Set("setting1", "another string value");
            Assert.AreEqual("another string value", config.Get<string>("setting1"), "Falscher Wert");

            CloseTestConfig("WriteSimpleConfigOneEntry.xml", config);
        }

        [Test]
        public void WriteSimpleConfigOneEntryAddNodeSameName()
        {
            var config = OpenTestConfig("WriteSimpleConfigOneEntryAddNodeSameName.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            config.Set("setting1", "another string value", 1);
            Assert.AreEqual("a string value", config.Get<string>("setting1"), "Falscher Wert");
            Assert.AreEqual("another string value", config.Get<string>("setting1", "", 1), "Falscher Wert");
            
            CloseTestConfig("WriteSimpleConfigOneEntryAddNodeSameName.xml", config);
        }
        
        [Test]
        public void WriteSimpleConfigOneEntryRemoveNodeSameName()
        {
            var config = OpenTestConfig("WriteSimpleConfigOneEntryRemoveNodeSameName.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(2, config.Count);
            Assert.AreEqual("a string value", config.Get<string>("setting1"), "Falscher Wert");
            Assert.AreEqual("another string value", config.Get<string>("setting1", "", 1), "Falscher Wert");
            
            config.Set("setting1", null, 0);
            Assert.AreEqual("another string value", config.Get<string>("setting1", "", 0), "Falscher Wert");
            
            CloseTestConfig("WriteSimpleConfigOneEntryRemoveNodeSameName.xml", config);
        }
        
        [Test]
        public void WriteSimpleConfigOneEntryRemoveNodeNonExistant()
        {
            var config = OpenTestConfig("SimpleConfigOneEntry.xml", false);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(1, config.Count);
            Assert.AreEqual("a string value", config.Get<string>("setting1"), "Falscher Wert");

            config.Set("setting2", null, 0);
            Assert.AreEqual("", config.Get<string>("setting2", "", 0), "Falscher Wert");
            Assert.AreEqual(null, config.Get<string>("setting2", null, 0), "Falscher Wert");
            config.Set("setting1", null, 1);
            Assert.AreEqual("", config.Get<string>("setting1", "", 1), "Falscher Wert");
            Assert.AreEqual(null, config.Get<string>("setting1", null, 1), "Falscher Wert");
            
            CloseTestConfig("SimpleConfigOneEntry.xml", config);
        }

        [Test]
        public void WriteSimpleConfigOneEntryWrongType()
        {
            var config = OpenTestConfig("SimpleConfigOneEntry.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            config.Set("setting1", 21);
            Assert.AreEqual("a string value", config.Get<string>("setting1"), "Falscher Wert");

            CloseTestConfig("SimpleConfigOneEntry.xml", config);
        }

        [Test]
        public void WriteSimpleConfigOneEntryWrongTypeAllowOverride()
        {
            var config = OpenTestConfig("WriteSimpleConfigOneEntryTypeOverride.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual("a string value", config.Get<string>("setting1"), "Falscher Wert");
            config.Set("setting1", 21, 0, true);
            Assert.AreEqual(21, config.Get<int>("setting1"), "Falscher Wert");
            
            CloseTestConfig("WriteSimpleConfigOneEntryTypeOverride.xml", config);
        }

        [Test]
        public void ReadListConfig()
        {
            var config = OpenTestConfig("ListConfig.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(1, config.Count);

            var list = config.Get<List<string>>("infolist");
            Assert.NotNull(list, "Liste konnte nicht ausgelesen werden");
            Assert.AreEqual(3, list.Count, "Liste hat falsche Länge");
            Assert.AreEqual("first index", list[0], "Falscher Wert");
            Assert.AreEqual("second index", list[1], "Falscher Wert");
            Assert.AreEqual("third index", list[2], "Falscher Wert");
            
            CloseTestConfig("ListConfig.xml", config);
        }

        [Test]
        public void ReadListConfigWrongItem()
        {
            var config = OpenTestConfig("ListConfigWrongItem.xml", false);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(0, config.Count);

            var list = config.Get<List<string>>("infolist");
            Assert.Null(list, "Kaputte Liste eingelesen");
            
            CloseTestConfig("ListConfigWrongItem.xml", config);
        }

        [Test]
        public void ReadMultiListConfig()
        {
            var config = OpenTestConfig("MultiListConfig.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(1, config.Count);

            var list = config.Get<List<List<string>>>("infolist");
            Assert.NotNull(list, "Liste konnte nicht ausgelesen werden");
            Assert.AreEqual(2, list.Count, "Liste hat falsche Länge");
            Assert.AreEqual(3, list[0].Count, "Innere Liste hat falsche Länge");
            Assert.AreEqual(3, list[1].Count, "Innere Liste hat falsche Länge");
            Assert.AreEqual("first index", list[0][0], "Falscher Wert");
            Assert.AreEqual("second index", list[0][1], "Falscher Wert");
            Assert.AreEqual("third index", list[0][2], "Falscher Wert");
            Assert.AreEqual("first index 2", list[1][0], "Falscher Wert");
            Assert.AreEqual("second index 2", list[1][1], "Falscher Wert");
            Assert.AreEqual("third index 2", list[1][2], "Falscher Wert");
            
            CloseTestConfig("MultiListConfig.xml", config);
        }

        [Test]
        public void ReadMultiListConfigWrongItemOuter()
        {
            var config = OpenTestConfig("MultiListConfigWrongItemOuter.xml", false);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(0, config.Count);

            var list = config.Get<List<string>>("infolist");
            Assert.Null(list, "Kaputte Liste eingelesen");
            
            CloseTestConfig("MultiListConfigWrongItemOuter.xml", config);
        }

        [Test]
        public void ReadMultiListConfigWrongItemOuter2()
        {
            var config = OpenTestConfig("MultiListConfigWrongItemOuter2.xml", false);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(0, config.Count);

            var list = config.Get<List<string>>("infolist");
            Assert.Null(list, "Kaputte Liste eingelesen");
            
            CloseTestConfig("MultiListConfigWrongItemOuter2.xml", config);
        }

        [Test]
        public void ReadMultiListConfigWrongItemOuter3()
        {
            var config = OpenTestConfig("MultiListConfigWrongItemOuter3.xml", false);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(0, config.Count);

            var list = config.Get<List<string>>("infolist");
            Assert.Null(list, "Kaputte Liste eingelesen");
            
            CloseTestConfig("MultiListConfigWrongItemOuter3.xml", config);
        }

        public enum TestEnum
        {
            FirstEntry,
            SecondEntry
        }

        [Test]
        public void ReadListEnum()
        {
            var config = OpenTestConfig("ListEnum.xml", false);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(1, config.Count);

            var e = config.Get<TestEnum>("text");
            Assert.AreEqual(TestEnum.FirstEntry, e, "Falscher Wert");
            
            CloseTestConfig("ListEnum.xml", config);
        }

        [Test]
        public void WriteListEnum()
        {
            var config = OpenTestConfig("ListEnum.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(1, config.Count);

            config.Set("text", TestEnum.SecondEntry);
            var e = config.Get<TestEnum>("text");
            Assert.AreEqual(TestEnum.SecondEntry, e, "Falscher Wert");

            CloseTestConfig("ListEnum.xml", config);
        }

        [Test]
        public void DeleteOneItem()
        {
            var config = OpenTestConfig("DeleteOneItem.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(3, config.Count);
            Assert.AreEqual("first index", config.Get("item", "", 0), "Falscher Wert");
            Assert.AreEqual("second index", config.Get("item", "", 1), "Falscher Wert");
            Assert.AreEqual("third index", config.Get("item", "", 2), "Falscher Wert");
            
            config.Delete("item", 1);
            
            Assert.AreEqual(2, config.Count);
            Assert.AreEqual("first index", config.Get("item", "", 0), "Falscher Wert");
            Assert.AreEqual("third index", config.Get("item", "", 1), "Falscher Wert");
            
            CloseTestConfig("DeleteOneItem.xml", config);
        }

        [Test]
        public void DeleteAllItems()
        {
            var config = OpenTestConfig("DeleteAllItems.xml", true);
            config.LoadConfiguration();
            Assert.IsTrue(config.IsValid);
            Assert.AreEqual(4, config.Count);
            Assert.AreEqual("first index", config.Get("item", "", 0), "Falscher Wert");
            Assert.AreEqual("second index", config.Get("item", "", 1), "Falscher Wert");
            Assert.AreEqual("third index", config.Get("item", "", 2), "Falscher Wert");
            Assert.AreEqual("something else", config.Get("itemother", "", 0), "Falscher Wert");
            
            config.DeleteAll("item");
            
            Assert.AreEqual(1, config.Count);
            Assert.AreEqual("something else", config.Get("itemother", "", 0), "Falscher Wert");
            
            CloseTestConfig("DeleteAllItems.xml", config);
        }

        //TODO: Diverse Get-Fälle: falscher Datentyp geschirben, Fehler beim Parsen, falscher Wert im value-Attribut, etc.


        private njFileConfiguration OpenTestConfig(string strName, bool bWriteable)
        {
            if (!bWriteable)
            {
                return new njFileConfiguration("./TestData/" + strName, false);
            }
            else
            {
                AssertFileDoesNotExist("./TestData-TestWrite/" + strName);
                AssertDirectoryExists("./TestData-TestWrite/");
                if (File.Exists("./TestData/" + strName))
                {
                    File.Copy("./TestData/" + strName, "./TestData-TestWrite/" + strName);
                }
                return new njFileConfiguration("./TestData-TestWrite/" + strName, true);
            }
        }

        private void CloseTestConfig(string strName, njFileConfiguration config)
        {
            if (!config.CanWrite)
            {
                config.Dispose();
            }
            else
            {
                config.SaveConfiguration();
                config.Dispose();
                
                Assert.IsTrue(File.Exists("./TestData-Verify/" + strName), "Verify-Datei fehlt!");

                Diff d = DiffBuilder.Compare(Input.FromString(File.ReadAllText("./TestData-TestWrite/" + strName)))
                    .WithTest(Input.FromString(File.ReadAllText("./TestData-Verify/" + strName)))
                    .Build();
                Assert.IsFalse(d.HasDifferences(), "Datei ist nach dem Schreibvorgang nicht identisch mit Verify-Datei");
                File.Delete("./TestData-TestWrite/" + strName);
            }
        }

        private void AssertFileDoesNotExist(string strFile)
        {
            if (File.Exists(strFile))
            {
                File.Delete(strFile);
            }
        }

        private void AssertDirectoryExists(string strDir)
        {
            if (!Directory.Exists(strDir))
            {
                Directory.CreateDirectory(strDir);
            }
        }
    }
}
