using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;

namespace VRSimTk
{
    public class DataUtil : MonoBehaviour
    {
        static public bool DataSyncAvailable()
        {
            // Return false if no game object with DataSync component is selected.
            return FindObjectOfType<DataSync>() != null;
        }

        static public List<EntityData> GetEntities()
        {
            List<EntityData> entities = new List<EntityData>( FindObjectsOfType<EntityData>());
            return entities;
        }

        static public EntityData FindEntity(string id)
        {
            List<EntityData> entities = GetEntities();
            return entities.Find(e => e.id == id);
        }

        static public string CreateNewId(Object obj, string baseId = null)
        {
            string newId = baseId ?? string.Empty;
            int hyphenPos = newId.LastIndexOf('-');
            if (hyphenPos >= 0)
            {
                newId = newId.Remove(hyphenPos);
            }
            if (newId.Length > 0)
            {
                newId += '-';
            }
            newId += obj.GetInstanceID().ToString("X8");
            return newId;
        }

        static public string CreateNewEntityId(EntityData baseEntity)
        {
            return CreateNewId( baseEntity, baseEntity.id);
        }

        static public bool ReadFromXml<T>(ref T scenarioData, string url)
        {
            try
            {
                // Load XmlSceneData from XML
                XmlSerializer serializer = new XmlSerializer(typeof(VrXmlSceneData));
                FileStream stream = new FileStream(url, FileMode.Open);
                scenarioData = (T)serializer.Deserialize(stream);
                stream.Close();
            }
            catch (IOException)
            {
                Debug.LogErrorFormat("Unable to read XML file {0}", url);
                return false;
            }
            return true;
        }

        static public bool WriteToXml(object scenarioData, string url)
        {
            try
            {
                // Setup XML output settings
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "    ";
                settings.Encoding = Encoding.UTF8;
                settings.CheckCharacters = true;
                // Write data to XML
                XmlSerializer serializer = new XmlSerializer(typeof(VrXmlSceneData));
                FileStream stream = new FileStream(url, FileMode.Create);
                XmlWriter w = XmlWriter.Create(stream, settings);
                serializer.Serialize(w, scenarioData);
                stream.Close();
            }
            catch (IOException)
            {
                Debug.LogErrorFormat("Unable to write XML file {0}", url);
                return false;
            }
            return true;
        }
    }
}
