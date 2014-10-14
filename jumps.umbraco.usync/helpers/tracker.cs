﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.language;

using jumps.umbraco.usync.Models;

using Umbraco.Core.Logging;

namespace jumps.umbraco.usync.helpers
{
    /// <summary>
    ///  change tracker - using MD5 hashes to compare items and files
    /// </summary>
    public static class tracker
    {
        public static bool DocTypeChanged(XElement node)
        {
            string filehash = XmlDoc.ReCalculateHash(node);
            if (string.IsNullOrEmpty(filehash))
                return true;

            XElement name = node.Element("Info").Element("Alias");
            if (name == null)
                return true;

            // get it...
            var docType = DocumentType.GetByAlias(name.Value);
            if (docType == null)
                return true;

            XmlDocument xmlDoc = helpers.XmlDoc.CreateDoc();
            xmlDoc.AppendChild(docType.ToXml(xmlDoc));

            string dbMD5 = helpers.XmlDoc.CalculateMD5Hash(xmlDoc);
            return (!filehash.Equals(dbMD5));
        }

        public static bool MediaTypeChanged(XElement node)
        {
            string filehash = XmlDoc.ReCalculateHash(node);
            if (string.IsNullOrEmpty(filehash))
                return true;

            XElement name = node.Element("Info").Element("Alias");
            if (name == null)
                return true;

            var item = MediaType.GetByAlias(name.Value);
            if (item == null)
                return true;

            XmlDocument doc = XmlDoc.CreateDoc();
            doc.AppendChild(MediaTypeHelper.ToXml(doc, item));
            var dbMD5 = XmlDoc.CalculateMD5Hash(doc);

            return (!filehash.Equals(dbMD5));
        }

        public static bool MacroChanged(XElement node)
        {
            string filehash = XmlDoc.ReCalculateHash(node);
            if (string.IsNullOrEmpty(filehash))
                return true;

            XElement name = node.Element("name");
            if (name == null)
                return true;

            var macro = Macro.GetByAlias(name.Value);
            if (macro == null)
                return true;

            XmlDocument doc = XmlDoc.CreateDoc();
            doc.AppendChild(macro.ToXml(doc));
            var dbMD5 = XmlDoc.CalculateMD5Hash(doc);

            return (!filehash.Equals(dbMD5));

        }

        public static bool DataTypeChanged(XElement node)
        {
            string filehash = XmlDoc.ReCalculateHash(node, true);

            if (string.IsNullOrEmpty(filehash))
                return true;

            XAttribute defId = node.Attribute("Definition");
            if (defId == null)
                return true;

            Guid defGuid = new Guid(defId.Value);

            if (!CMSNode.IsNode(defGuid))
                return true;
           
            DataTypeDefinition dtd = DataTypeDefinition.GetDataTypeDefinition(defGuid);
            if ( dtd == null )
                return true;

            XElement dbNode = dtd.SyncExport();
            var dbMD5 = XmlDoc.CalculateMD5Hash(dbNode, true);

            return (!filehash.Equals(dbMD5));
        }

        public static bool TemplateChanged(XElement node)
        {
            var hashProps = new string[] { "Name", "Alias", "Master" };
            string filehash = XmlDoc.ReCalculateHash(node, hashProps);
            if (string.IsNullOrEmpty(filehash))
                return true;

            XElement alias = node.Element("Alias");
            if (alias == null)
                return true;

            var item = Template.GetByAlias(alias.Value);
            if (item == null)
                return true;

            // for a template - we never change the contents - lets just md5 the two 
            // properties we care about (and save having to load the thing from disk?

            XmlDocument doc = XmlDoc.CreateDoc();
            doc.AppendChild(item.ToXml(doc));
            var dbNode = XElement.Load(new XmlNodeReader(doc));
            var dbMD5 = XmlDoc.ReCalculateHash(dbNode, hashProps);

            return (!filehash.Equals(dbMD5));
        }

        public static bool StylesheetChanged(XElement node)
        {
            var hashProps = new string[] {"Name", "FileName", "Properties"};
            string filehash = XmlDoc.ReCalculateHash(node, hashProps);
            if (string.IsNullOrEmpty(filehash))
                return true;

            XElement name = node.Element("Name");
            if (name == null)
                return true;

            var item = StyleSheet.GetByName(name.Value);
            if (item == null)
                return true;

            XmlDocument doc = XmlDoc.CreateDoc();
            doc.AppendChild(item.ToXml(doc));
            var dbNode = XElement.Load(new XmlNodeReader(doc));
            var dbMD5 = XmlDoc.ReCalculateHash(dbNode, hashProps) ;

            return (!filehash.Equals(dbMD5));
        }

        public static bool LanguageChanged(XElement node)
        {
            var name = node.Attribute("CultureAlias");
            if (name == null)
                return true;

            var lang = Language.GetByCultureCode(name.Value);
            if (lang == null)
                return true;

            var friendlyName = node.Attribute("FriendlyName");
            if (friendlyName == null)
                return true;

            return (!lang.FriendlyName.Equals(friendlyName.Value));
        }

        static Dictionary<string, Dictionary.DictionaryItem> dictionaryItems = null;

        public static bool DictionaryChanged(XElement node)
        {
            string filehash = XmlDoc.CalculateDictionaryHash(node);
            if (string.IsNullOrEmpty(filehash))
                return true;

            /// to do - a nice way of checking dictionary items for changes...
            if (dictionaryItems == null)
            {
                dictionaryItems = new Dictionary<string,Dictionary.DictionaryItem>();
                foreach (Dictionary.DictionaryItem item in Dictionary.getTopMostItems)
                {
                    dictionaryItems.Add(item.key, item);
                }
            }

            var key = node.Attribute("Key");
            if ( key == null )
                return true;

            if (!dictionaryItems.ContainsKey(key.Value))
                return true;

            // here we have the key - so is this the same? 
            XmlDocument doc = XmlDoc.CreateDoc();
            doc.AppendChild(dictionaryItems[key.Value].ToXml(doc));
            var dbNode = XElement.Load(new XmlNodeReader(doc));
            string dbMD5 = XmlDoc.CalculateDictionaryHash(dbNode);
            
            return (!filehash.Equals(dbMD5));
        }
    }
}