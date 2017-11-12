using System;
using System.IO;
using System.Xml;


namespace WG_HighRiseBanEdit
{
    public class XML_VersionOne: WG_XMLBaseVersion
    {
        private const string popNodeName = "population";

        /// <param name="doc"></param>
        public override void readXML(XmlDocument doc, DataStore store)
        {
            XmlElement root = doc.DocumentElement;


            foreach (XmlNode node in root.ChildNodes)
            {
                try
                {
                    if (node.Name.Equals(popNodeName))
                    {
                        readPopulationNode(node, store);
                    }
                }
                catch (Exception e)
                {
                    Debugging.bufferWarning(e.Message);
                    UnityEngine.Debug.LogException(e);
                }
            }
        } // end readXML


        /// <param name="fullPathFileName"></param>
        /// <returns></returns>
        public override bool writeXML(string fullPathFileName, DataStore store)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_PopOnlyMod");
            XmlAttribute attribute = xmlDoc.CreateAttribute("version");
            attribute.Value = "1";
            rootNode.Attributes.Append(attribute);

            xmlDoc.AppendChild(rootNode);

            XmlNode heightNode = xmlDoc.CreateElement(popNodeName);

            try
            {
                makeHeightNode(heightNode, xmlDoc);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }

            rootNode.AppendChild(heightNode);

            try
            {
                if (File.Exists(fullPathFileName))
                {
                    if (File.Exists(fullPathFileName + ".bak"))
                    {
                        File.Delete(fullPathFileName + ".bak");
                    }

                    File.Move(fullPathFileName, fullPathFileName + ".bak");
                }
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }

            try
            {
                xmlDoc.Save(fullPathFileName);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
                return false;  // Only time when we say there's an error
            }

            return true;
        } // end writeXML


        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="array"></param>
        private void makeHeightNode(XmlNode root, XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement("height");

            XmlAttribute attribute = xmlDoc.CreateAttribute("res");
            attribute.Value = Convert.ToString(DataStore.getInstance().resHeightLimit);
            node.Attributes.Append(attribute);

            root.AppendChild(node);
        }


        /// <param name="popNode"></param>`
        private void readPopulationNode(XmlNode popNode, DataStore store)
        {
            foreach (XmlNode node in popNode.ChildNodes)
            {
                try
                {
                    int temp = Convert.ToInt32(node.Attributes["res"].InnerText);

                    if (temp <= 0)
                    {
                        temp = 10;  // Bad person trying to give negative or div0 error. 
                    }
                    DataStore.getInstance().resHeightLimit = temp;
                }
                catch (Exception e)
                {
                    Debugging.bufferWarning("Height, part a: " + e.Message);
                }
            } // end foreach
        }
    }
}