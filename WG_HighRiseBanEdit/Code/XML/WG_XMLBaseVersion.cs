using System;
using System.Xml;

namespace WG_HighRiseBanEdit
{
    public abstract class WG_XMLBaseVersion
    {
        public WG_XMLBaseVersion()
        {
        }

        public abstract void readXML(XmlDocument doc, DataStore store);
        public abstract bool writeXML(string fullPathFileName, DataStore store);
    }
}