﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;

namespace Hl7.Fhir.Publication.Profile
{
    internal class ProfileKnowledgeProvider
    {
//        private StructureLoader _loader;

        public const string DSTU1URL = "http://www.hl7.org/implement/standards/fhir/";

        private string baseName;

        private IArtifactSource _source;

        public ProfileKnowledgeProvider(string baseName, string imageOutputDirectory, IArtifactSource source)
        {
            this.baseName = baseName;  
            ImageOutputDirectory = imageOutputDirectory;
            _source = source;
        }


        //TODO: Determine dynamically based on core profiles?
        public bool isDataType(String value)
        {
            return new[] { "Identifier", "HumanName", "Address", "ContactPoint", "Timing", "Quantity", "Attachment", "Range",
                  "Period", "Ratio", "CodeableConcept", "Coding", "SampledData", "Age", "Distance", "Duration", "Count", "Money" }.Contains(value);
        }

        //TODO: Determine based on core profiles
        public bool isPrimitive(String value)
        {
            return new[] { "boolean", "integer", "decimal", "base64Binary", "instant", "string", "date", "dateTime", "code", "oid", "uuid", "id" }.Contains(value);
        }


        public bool isReference(String value)
        {
            return value == "Reference";
        }


        public string GetLinkForElementDefinition(StructureDefinition structure, ElementDefinition element)
        {
            return GetLinkForProfileDict(structure) + "#" + MakeElementDictAnchor(s, element);
        }

        public static string GetLinkForProfileDict(StructureDefinition structure)
        {
            return GetProfilePageName(structure) + "-definition" + ".html";
        }

        private static string GetProfilePageName(StructureDefinition structure)
        {
            return TokenizeName(structure.Name).ToLower();
        }


        public string MakeElementDictAnchor(ElementDefinition element)
        {
            return element.Name ?? element.Path;
        }


        public StructureDefinition GetExtensionDefinition(string url)
        {
            if (url.StartsWith("#"))
            {
                //TODO
                throw new NotImplementedException("Contained extensions not done yet");
            }
            else
            {
                var cr = _source.ReadConformanceResource(url) as StructureDefinition; 
                if(cr != null && cr.Type == StructureDefinition.StructureDefinitionType.Extension)
                {
                    if(cr.Snapshot == null)
                        throw new NotImplementedException("No snapshot representation on extension for url " + url);

                    return cr;
                }
                else
                    return null;
            }
        }


        public string GetLinkForExtensionDefinition(string extensionUrl)
        {
            var extd = GetExtensionDefinition(extensionUrl);

            if (extd != null) return extd.Url;

            return extensionUrl;
        }


        public string GetLinkForBinding(ElementDefinition.ElementDefinitionBindingComponent binding)
        {
            if (binding.ValueSet == null)
                return null;

            String reference = binding.ValueSet is FhirUri ? 
                    ((FhirUri)binding.ValueSet).Value : ((ResourceReference)binding.ValueSet).Reference;

            if (reference.StartsWith("http://hl7.org/fhir/v3/vs/"))
                return MakeSpecLink("v3/" + reference.Substring(26) + "/index.html");
            else if (reference.StartsWith("http://hl7.org/fhir/vs/"))
                return MakeSpecLink(reference.Substring(23) + ".html");
            else if (reference.StartsWith("http://hl7.org/fhir/v2/vs/"))
                return MakeSpecLink("v2/" + reference.Substring(26) + "/index.html");
            else
                return reference + ".html";
        }

        public string MakeSpecLink(string p)
        {
            return SpecUrl() + p;
        }


        public string SpecUrl()
        {
            return DSTU1URL;
            //if (version.StartsWith("0.8") || version == null) return DSTU1URL;

            //throw new NotImplementedException("Do not know the URL to specification version  " + version);
        }


        //public string GetLinkForProfileReference(Profile profile, string p)
        //{
        //    if (p.StartsWith(CORE_TYPE_PROFILEREFERENCE_PREFIX))
        //    {
        //        string rn = p.Substring(CORE_TYPE_PROFILEREFERENCE_PREFIX.Length);
        //        return GetLinkForTypeDocu(rn);
        //    }
        //    else if (p.StartsWith("#"))
        //        return GetLinkForLocalStructure(profile, p.Substring(1));
        //    else
        //        return p;
        //}




        //internal Model.ValueSet GetValueSet(string url)
        //{
        //    if (!url.StartsWith("http")) url = "http://local/" + url;
        //    return _loader.ArtifactSource.ReadResourceArtifact(new Uri(url)) as ValueSet;
        //}

        internal string GetLinkForTypeDocu(string typename)
        {
            if (typename == "*")
                return SpecUrl() + "datatypes.html#open";
            else if (isDataType(typename) || isPrimitive(typename))
                return SpecUrl() + "datatypes.html#" + typename.ToLower();
            else if (typename == "Any")
                return SpecUrl() + "resourcelist.html";
            else if (ModelInfo.IsKnownResource(typename))
                return SpecUrl() + typename.ToLower() + ".html";
            else if (typename == "Extension")
                return SpecUrl() + "extensibility.html#Extension";
            else
                throw new NotImplementedException("Don't know how to link to specification page for type " + typename);
        }

        internal bool HasLinkForTypeDocu(string typename)
        {
            return typename == "*" || isDataType(typename) || isPrimitive(typename) || typename == "Extension" || ModelInfo.IsKnownResource(typename);
        }



      



        //internal string GetLabelForProfileReference(Profile profile, string p)
        //{
        //    if (p.StartsWith(CORE_TYPE_PROFILEREFERENCE_PREFIX))
        //        return p.Substring(CORE_TYPE_PROFILEREFERENCE_PREFIX.Length);
        //    else
        //        return p;
        //}

        //internal string GetLinkForExtensionDefinition(Profile profile, Profile.ProfileExtensionDefnComponent extension)       
        //{
        //    return GetLinkForExtensionDefinition(profile, extension.Code);
        //}

       


        //internal string GetLinkForElementDefinition(Profile.ProfileStructureComponent s, Profile profile, Profile.ElementComponent element)
        //{
        //    return GetLinkForProfileDict(profile) + "#" + MakeElementDictAnchor(s,element);
        //}

        //internal string GetLinkForLocalStructure(Profile profile, Profile.ProfileStructureComponent structure)
        //{
        //    return GetLinkForLocalStructure(profile, structure.Name);
        //}

        //internal string GetLinkForLocalStructure(Profile profile, string name)
        //{
        //    return GetProfilePageName(profile) + "-" + TokenizeName(name).ToLower() + ".html";
        //}






        //public string GetLinkForProfileTable(Profile profile)
        //{
        //    return GetProfilePageName(profile) + ".html";
        //}




        internal static String TokenizeName(String cs)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < cs.Length; i++)
            {
                char c = cs[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_')
                    s.Append(c);
                else if (c != ' ')
                    s.Append("." + c.ToString());
            }

            return s.ToString();
        }



       




        public const string V2_SYSTEM_PREFIX = "http://hl7.org/fhir/v2/";
        public const string V3_SYSTEM_PREFIX = "http://hl7.org/fhir/v3/";
        public const string FHIR_SYSTEM_PREFIX = "http://hl7.org/fhir/";

        //internal ValueSet GetValueSetForSystem(string system)
        //{
        //    string valuesetUri = null;

        //    if (system.StartsWith(V2_SYSTEM_PREFIX))
        //        valuesetUri = V2_SYSTEM_PREFIX + "vs/" + system.Substring(V2_SYSTEM_PREFIX.Length);
        //    else if (system.StartsWith(V3_SYSTEM_PREFIX))
        //        valuesetUri = V3_SYSTEM_PREFIX + "vs/" + system.Substring(V3_SYSTEM_PREFIX.Length);
        //    else if (system.StartsWith(FHIR_SYSTEM_PREFIX))
        //        valuesetUri = FHIR_SYSTEM_PREFIX + "vs/" + system.Substring(FHIR_SYSTEM_PREFIX.Length);

        //    if (valuesetUri != null)
        //        return GetValueSet(valuesetUri);
        //    else
        //        return null;
        //}

        public string ImageOutputDirectory { get; set; }

        public string ImageLinkPath { get { return "../dist/images"; } }

        /**
         * There are circumstances where the table has to present in the absence of a stable supporting infrastructure.
         * and the file paths cannot be guaranteed. For these reasons, you can tell the builder to inline all the graphics
         * (all the styles are inlined anyway, since the table fbuiler has even less control over the styling
         *  
         */
        public bool InlineGraphics { get; set; }
    }

}
