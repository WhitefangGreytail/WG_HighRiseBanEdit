using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;
using System.Xml;
using System.IO;

namespace WG_HighRiseBanEdit
{
    public class HighRiseBanMod : IUserMod
    {
        public string Name
        {
            get { return "WG High Rise Ban Mod v1.0"; }
        }
        public string Description
        {
            get { return "Removes level 5 and instead ban based on model height."; }
        }

        private void EventSave()
        {
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionOne();
                xml.writeXML(LoadingExtension.currentFileLocation, DataStore.getInstance());
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }
        }

        private void newResHeight(string c)
        {
            int value = Convert.ToInt32(c);
            DataStore.getInstance().resHeightLimit = value;
        }


        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelper concrete = helper as UIHelper;

            if (File.Exists(LoadingExtension.currentFileLocation))
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionOne();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(LoadingExtension.currentFileLocation);
                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                    reader.readXML(doc, DataStore.getInstance());
                }
                catch (Exception)
                {
                }
            }


            UIHelperBase group = concrete.AddGroup("Population modifiers");
            group.AddButton("Save", EventSave);
            
            group.AddTextfield("Res Height", Convert.ToString(DataStore.getInstance().resHeightLimit), newResHeight);
        }
    }
}


