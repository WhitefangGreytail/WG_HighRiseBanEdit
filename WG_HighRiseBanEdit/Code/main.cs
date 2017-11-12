using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using System.Diagnostics;
using Boformer.Redirection;
using ColossalFramework;
using ColossalFramework.Plugins;

namespace WG_HighRiseBanEdit
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private const int RES = 0;
        private const int COM = 0;
        private const int IND = 0;
        private const int INDEX = 0;
        private const int OFFICE = 0;
        public const String XML_FILE = "WG_HighRise.xml";
        public static string currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + LoadingExtension.XML_FILE;

        private readonly Dictionary<MethodInfo, Redirector> redirectsOnLoaded = new Dictionary<MethodInfo, Redirector>();
        private readonly Dictionary<MethodInfo, Redirector> redirectsOnCreated = new Dictionary<MethodInfo, Redirector>();

        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;
        private static Stopwatch sw;

        public override void OnCreated(ILoading loading)
        {
            if (!isModEnabled)
            {
                isModEnabled = true;
                sw = Stopwatch.StartNew();

                Redirect(true);
                readFromXML();

                sw.Stop();
                UnityEngine.Debug.Log("WG_HighRiseBan: Successfully loaded in " + sw.ElapsedMilliseconds + " ms."); 
            }
        }


        public override void OnReleased()
        {
            if (isModEnabled)
            {
                isModEnabled = false;

                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionOne();
                    xml.writeXML(currentFileLocation, DataStore.getInstance());
                    DataStore.releaseInstance();
                }
                catch (Exception e)
                {
                    Debugging.panelMessage(e.Message);
                }

                RevertRedirect(true);
            }
        }


        public override void OnLevelUnloading()
        {
            if (isLevelLoaded)
            {
                isLevelLoaded = false;
            }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                if (isModEnabled && !isLevelLoaded)
                {
                    isLevelLoaded = true;
                    Debugging.panelMessage("Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");
                }
                Debugging.releaseBuffer();
            }
        }

        private void Redirect(bool onCreated)
        {
            var redirects = onCreated ? redirectsOnCreated : redirectsOnLoaded;
            redirects.Clear();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                try
                {
                    var r = RedirectionUtil.RedirectType(type, onCreated);
                    if (r != null)
                    {
                        foreach (var pair in r)
                        {
                            redirects.Add(pair.Key, pair.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"An error occured while applying {type.Name} redirects!");
                    UnityEngine.Debug.Log(e.StackTrace);
                }
            }
        }

        private void RevertRedirect(bool onCreated)
        {
            var redirects = onCreated ? redirectsOnCreated : redirectsOnLoaded;
            foreach (var kvp in redirects)
            {
                try
                {
                    kvp.Value.Revert();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"An error occured while reverting {kvp.Key.Name} redirect!");
                    UnityEngine.Debug.Log(e.StackTrace);
                }
            }
            redirects.Clear();
        }

        /// <summary>
        ///
        /// </summary>
        private void readFromXML()
        {
            currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;
            bool fileAvailable = File.Exists(currentFileLocation);

            if (fileAvailable)
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionOne();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(currentFileLocation);

                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);

                    reader.readXML(doc, DataStore.getInstance());
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.bufferWarning("The following exception(s) were detected while loading the XML file. Some (or all) values may not be loaded.");
                    Debugging.bufferWarning(e.Message);
                    UnityEngine.Debug.LogException(e);
                }
            }
            else
            {
                UnityEngine.Debug.Log("Configuration file not found. Will output new file to : " + currentFileLocation);
            }
        }
    }
}
