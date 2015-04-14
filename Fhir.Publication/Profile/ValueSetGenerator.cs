﻿/*
Copyright (c) 2011+, HL7, Inc
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright notice, this 
   list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, 
   this list of conditions and the following disclaimer in the documentation 
   and/or other materials provided with the distribution.
 * Neither the name of HL7 nor the names of its contributors may be used to 
   endorse or promote products derived from this software without specific 
   prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
POSSIBILITY OF SUCH DAMAGE.

*/

// Classes in this file are updated to match instance/utils/NarrativeGenerator.java commit #5196 in the Java tooling trunk

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Navigation;
using Hl7.Fhir.Support;
using MarkdownDeep;

namespace Hl7.Fhir.Publication.Profile
{
    internal class ValueSetGenerator
    {
        ProfileKnowledgeProvider _pkp;

        private StringBuilder xhtml = new StringBuilder();

        internal ValueSetGenerator(ProfileKnowledgeProvider pkp)
        {
            _pkp = pkp;
        }


        public XElement generate(ValueSet vs)
        {
            var result = new XElement(XmlNs.XHTMLNS + "div");

            if (vs.Expansion != null)
            {
                if (vs.Define == null && vs.Compose == null)
                {
                    throw new NotImplementedException("Expansion HTML generation not yet supported");
                    //TODO: port generateExpansion(x, vs);
                }
                else
                    throw new Exception("Error: should not encounter value set expansion at this point");
            }

            //Integer count = countMembership(vs);
            //if (count == null)
            //    x.addTag("p").addText("This value set does not contain a fixed number of concepts");
            //else
            //    x.addTag("p").addText("This value set contains " + count.toString() + " concepts");


            bool hasExtensions = false;

            if (vs.Define != null)
                hasExtensions = generateDefinition(result, vs);

            if (vs.Compose != null)
            {
                throw new NotImplementedException();
                //hasExtensions = generateComposition(result, vs) || hasExtensions;
            }

            //inject(vs, result, hasExtensions ? Narrative.NarrativeStatus.Extensions :  Narrative.NarrativeStatus.Generated);

            return result;
        }


        private bool generateDefinition(XElement x, ValueSet vs)
        {
            bool hasExtensions = false;
            var mymaps = new Dictionary<ConceptMap, ValueSet>();

            foreach (var a in _pkp.GetConceptMapsForSource(vs.Url))
            {
                var targetVs = _pkp.GetValueSet(a.TargetAsString());
                mymaps.Add(a, targetVs);
            }

            var langs = new List<String>();

            var h = x.AddTag("h2");
            h.AddText(vs.Name);

            var p = x.AddTag("p");
            smartAddText(p, vs.Description);
            if (!String.IsNullOrEmpty(vs.Copyright)) generateCopyright(x, vs);

            p = x.AddTag("p");
            p.AddText("This value set defines its own terms in the system " + vs.Define.System);
            var t = x.AddTag("table").SetAttribute("class", "codes");

            bool commentS = false;
            bool deprecated = false;
            bool display = false;
            bool heirarchy = false;

            if (vs.Define.Concept != null)
            {
                foreach (var c in vs.Define.Concept)
                {
                    commentS = commentS || conceptsHaveComments(c);
                    deprecated = deprecated || conceptsHaveDeprecated(c);
                    display = display || conceptsHaveDisplay(c);
                    heirarchy = heirarchy || !c.Concept.IsNullOrEmpty();

                    scanLangs(c, langs);
                }
            }

            addMapHeaders(addTableHeaderRowStandard(t, heirarchy, display, true, commentS, deprecated), mymaps);

            foreach (var c in vs.Define.Concept)
                hasExtensions = addDefineRowToTable(t, c, 0, heirarchy, display, commentS, deprecated, mymaps) || hasExtensions;

            if (langs.Count > 0)
            {
                langs.Sort();
                x.AddTag("p").AddTag("b").AddText("Additional Language Displays");
                t = x.AddTag("table").SetAttribute("class", "codes");
                var tr = t.AddTag("tr");
                tr.AddTag("td").AddTag("b").AddText("Code");

                foreach (var lang in langs)
                    tr.AddTag("td").AddTag("b").AddText(lang);

                foreach (var c in vs.Define.Concept)
                {
                    addLanguageRow(c, t, langs);
                }
            }

            return hasExtensions;
        }

        private bool conceptsHaveComments(ValueSet.ConceptDefinitionComponent c)
        {
            if (!String.IsNullOrEmpty(c.GetComment())) return true;

            return c.Concept.Any(g => conceptsHaveComments(g));
        }

        private bool conceptsHaveDeprecated(ValueSet.ConceptDefinitionComponent c)
        {
            if (c.GetDeprecated().HasValue) return true;

            return c.Concept.Any(g => conceptsHaveDeprecated(g));
        }

        private bool conceptsHaveDisplay(ValueSet.ConceptDefinitionComponent c)
        {
            if (!String.IsNullOrEmpty(c.Display)) return true;

            return c.Concept.Any(g => conceptsHaveDisplay(g));
        }

        private void smartAddText(XElement p, String text)
        {
            if (text == null)
                return;

            String[] lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0) p.AddTag("br");
                p.AddText(lines[i]);
            }
        }


        private void generateCopyright(XElement x, ValueSet vs)
        {
            var p = x.AddTag("p");
            p.AddTag("b").AddText("Copyright Statement:");
            smartAddText(p, " " + vs.Copyright);
        }

        private void scanLangs(ValueSet.ConceptDefinitionComponent c, List<String> langs)
        {
            if (c.Designation != null)
            {
                foreach (var designation in c.Designation)
                {
                    String lang = designation.Language;

                    if (langs != null && !langs.Contains(lang))
                        langs.Add(lang);
                }
            }

            if (c.Concept != null)
            {
                foreach (var g in c.Concept)
                    scanLangs(g, langs);
            }
        }

        private void addMapHeaders(XElement tr, Dictionary<ConceptMap, ValueSet> mymaps)
        {
            foreach (var m in mymaps.Keys)
            {
                var td = tr.AddTag("td");
                var b = td.AddTag("b");
                var a = b.AddTag("a");
                a.SetAttribute("href", _pkp.GetLinkForValueSet(mymaps[m].Url));
                a.AddText(m.Description);
            }
        }


        private XElement addTableHeaderRowStandard(XElement t, bool hasHeirarchy, bool hasDisplay, bool definitions, bool comments, bool deprecated)
        {
            var tr = t.AddTag("tr");
            if (hasHeirarchy)
                tr.AddTag("td").AddTag("b").AddText("Lvl");
            tr.AddTag("td").AddTag("b").AddText("Code");
            if (hasDisplay)
                tr.AddTag("td").AddTag("b").AddText("Display");
            if (definitions)
                tr.AddTag("td").AddTag("b").AddText("Definition");
            if (deprecated)
                tr.AddTag("td").AddTag("b").AddText("Deprecated");
            if (comments)
                tr.AddTag("td").AddTag("b").AddText("Comments");
            return tr;
        }



        private bool addDefineRowToTable(XElement t, ValueSet.ConceptDefinitionComponent c, int i, bool hasHeirarchy,
            bool hasDisplay, bool comment, bool deprecated, Dictionary<ConceptMap, ValueSet> maps)
        {
            bool hasExtensions = false;
            var tr = t.AddTag("tr");
            var td = tr.AddTag("td");

            if (hasHeirarchy)
            {
                td.AddText((i + 1).ToString());
                td = tr.AddTag("td");
                var s = new String('\u00A0', i * 2);
                td.AddText(s);
            }

            td.AddText(c.Code);
            XElement a;

            if (c.CodeElement != null)
            {
                a = td.AddTag("a");
                a.SetAttribute("name", ProfileKnowledgeProvider.TokenizeName(c.Code));
                a.AddText(" ");
            }

            if (hasDisplay)
            {
                td = tr.AddTag("td");
                if (c.DisplayElement != null)
                    td.AddText(c.Display);
            }

            td = tr.AddTag("td");

            if (c != null)
                smartAddText(td, c.Definition);

            if (deprecated)
            {
                td = tr.AddTag("td");
                String s = PrimitiveTypeConverter.ConvertTo<string>(c.GetDeprecated());

                if (s != null)
                {
                    smartAddText(td, s);
                    hasExtensions = true;
                }
            }

            if (comment)
            {
                td = tr.AddTag("td");
                String s = c.GetComment();

                if (s != null)
                {
                    smartAddText(td, s);
                    hasExtensions = true;
                }
            }

            foreach (var m in maps.Keys)
            {
                td = tr.AddTag("td");
                var mappings = findMappingsForCode(c.Code, m);
                bool first = true;

                foreach (var mapping in mappings)
                {
                    if (!first)
                        td.AddTag("br");
                    first = false;

                    var span = td.AddTag("span");
                    span.SetAttribute("title", mapping.EquivalenceElement != null ? mapping.Equivalence.ToString() : "");
                    span.AddText(getCharForEquivalence(mapping));
                    a = td.AddTag("a");
                    a.SetAttribute("href", _pkp.GetLinkForValueSet(maps[m].Url) + "#" + mapping.Code);
                    a.AddText(mapping.Code);
                    if (!String.IsNullOrEmpty(mapping.Comments))
                        td.AddTag("i").AddText("(" + mapping.Comments + ")");
                }
            }

            foreach (var e in c.GetSubsumes())
            {
                hasExtensions = true;
                tr = t.AddTag("tr");
                td = tr.AddTag("td");
                String s = new String('.', i * 2);
                td.AddText(s);
                a = td.AddTag("a");
                a.SetAttribute("href", "#" + ProfileKnowledgeProvider.TokenizeName(e.Value));
                a.AddText(c.Code);
            }

            foreach (var cc in c.Concept)
            {
                hasExtensions = addDefineRowToTable(t, cc, i + 1, hasHeirarchy, hasDisplay, comment, deprecated, maps) || hasExtensions;
            }

            return hasExtensions;

        }

        private IEnumerable<ConceptMap.ConceptMapElementMapComponent> findMappingsForCode(String code, ConceptMap map)
        {
            if (map.Element != null)
            {
                return map.Element.SelectMany(c => c.Map);
            }
            else
                return Enumerable.Empty<ConceptMap.ConceptMapElementMapComponent>();
        }


        private String getCharForEquivalence(ConceptMap.ConceptMapElementMapComponent mapping)
        {
            if (mapping.EquivalenceElement == null)
                return "";

            switch (mapping.Equivalence)
            {
                case ConceptMap.ConceptMapEquivalence.Equal: return "=";
                case ConceptMap.ConceptMapEquivalence.Equivalent: return "~";
                case ConceptMap.ConceptMapEquivalence.Wider: return ">";
                case ConceptMap.ConceptMapEquivalence.Narrower: return "<";
                case ConceptMap.ConceptMapEquivalence.Inexact: return "><";
                case ConceptMap.ConceptMapEquivalence.Unmatched: return "-";
                case ConceptMap.ConceptMapEquivalence.Disjoint: return "!=";
                default: return "?";
            }
        }


        private void addLanguageRow(ValueSet.ConceptDefinitionComponent c, XElement t, List<String> langs)
        {
            var tr = t.AddTag("tr");
            tr.AddTag("td").AddText(c.Code);

            foreach (String lang in langs)
            {
                ValueSet.ConceptDefinitionDesignationComponent d = null;

                foreach (var designation in c.Designation)
                {
                    if (lang == designation.Language)
                        d = designation;
                }

                tr.AddTag("td").AddText(d == null ? "" : d.Value);
            }
        }


        // private bool generateComposition(XElement x, ValueSet vs)
        // {
        //     bool hasExtensions = false;

        //     if (vs.Define == null)
        //     {
        //         var h = new XElement(XmlNs.XHTMLNS + "h2", new XText("Value Set for codes in " + vs.Name));
        //         var p = new XElement(XmlNs.XHTMLNS + "p");
        //         smartAddText(p, vs.Description);
        //         x.Add(h, p);

        //         if (vs.Copyright != null)
        //             generateCopyright(x, vs);

        //         x.Add(new XElement(XmlNs.XHTMLNS + "p", "This value set includes codes defined in other code systems, using the following rules:"));
        //     }
        //     else
        //     {
        //         x.Add(new XElement(XmlNs.XHTMLNS + "p", "In addition, this value set includes codes defined in other code systems, using the following rules:"));
        //     }

        //     var ul = new XElement(XmlNs.XHTMLNS + "ul");
        //     x.Add(ul);

        //     XElement li;

        //     if (vs.Compose.Import != null)
        //     {
        //         foreach (var imp in vs.Compose.Import)
        //         {
        //             li = new XElement(XmlNs.XHTMLNS + "li");
        //             ul.Add(li);

        //             li.Add(new XText("Import all the codes that are part of "));
        //             AddVsRef(imp, li);
        //         }
        //     }

        //     if (vs.Compose.Include != null)
        //     {
        //         foreach (var inc in vs.Compose.Include)
        //             hasExtensions = genInclude(ul, inc, "Include") || hasExtensions;
        //     }

        //     if (vs.Compose.Exclude != null)
        //     {
        //         foreach (var exc in vs.Compose.Exclude)
        //             hasExtensions = genInclude(ul, exc, "Exclude") || hasExtensions;
        //     }

        //     return hasExtensions;
        // }


        //private bool genInclude(XElement ul, ValueSet.ConceptSetComponent inc, String type) 
        //{
        //     bool hasExtensions = false;
        //     var li = new XElement(XmlNs.XHTMLNS + "li");  ul.Add(li);

        //     var e = _pkp.GetValueSetForSystem(inc.System);
        // //    AtomEntry<? extends Resource> e = context.getCodeSystems().get(inc.getSystem());

        //     if ( (inc.Code == null || !inc.Code.Any()) &&  (inc.Filter == null || !inc.Filter.Any()))
        //     { 
        //         li.Add(new XText(type+" all codes defined in "));
        //         addCsRef(inc, li, e);
        //     } 
        //     else 
        //     { 
        //         if (inc.CodeElement != null && inc.CodeElement.Any())
        //         {
        //             li.Add(new XText(type+" these codes as defined in "));
        //             addCsRef(inc, li, e);

        //             var t = new XElement(XmlNs.XHTMLNS+"table");  li.Add(t);
        //             bool hasComments = false;

        //             foreach (var c in inc.CodeElement) 
        //             {
        //                 hasComments = hasComments || c.GetExtension(ToolingExtensions.EXT_COMMENT) != null;
        //             }

        //             if (hasComments)
        //                 hasExtensions = true;

        //             addTableHeaderRowStandard(t, hasComments, false);

        //             foreach(var c in inc.CodeElement) 
        //             {
        //                 var tr = new XElement(XmlNs.XHTMLNS + "tr"); t.Add(tr);
        //                 tr.Add(new XElement(XmlNs.XHTMLNS+"td", new XText(c.Value)));

        //                 ValueSet.ValueSetDefineConceptComponent cc = getConceptForCode(e, c.Value, inc.System);

        //                 XElement td = new XElement(XmlNs.XHTMLNS+"td"); tr.Add(td);
        //                 if (cc != null && !String.IsNullOrEmpty(cc.Display))
        //                     td.Add(new XText(cc.Display));

        //                 //if (!Utilities.noString(c.getDisplay()))  DSTU2
        //                 //    td.addText(c.getDisplay());
        //                 //else if (cc != null && !Utilities.noString(cc.getDisplay()))
        //                 //    td.addText(cc.getDisplay());

        //                 td = new XElement(XmlNs.XHTMLNS+"td"); tr.Add(td);

        //                 if (c.GetExtension(ToolingExtensions.EXT_DEFINITION) != null)
        //                     smartAddText(td, ToolingExtensions.ReadStringExtension(c, ToolingExtensions.EXT_DEFINITION));
        //                 else if (cc != null && !String.IsNullOrEmpty(cc.Definition))
        //                     smartAddText(td, cc.Definition);
        //                 else
        //                     ; // No else in the java code!!

        //                 if (c.GetExtension(ToolingExtensions.EXT_COMMENT) != null) 
        //                 {
        //                     var tdn = new XElement(XmlNs.XHTMLNS+"td"); tr.Add(td);
        //                     smartAddText(tdn, "Note: "+ ToolingExtensions.ReadStringExtension(c, ToolingExtensions.EXT_COMMENT));
        //                 }
        //             }
        //         }

        //         if (inc.Filter != null)
        //         {
        //             foreach (var f in inc.Filter)
        //             {
        //                 li.Add(new XText(type + " codes from "));
        //                 addCsRef(inc, li, e);

        //                 // TODO: Java code does not allow for f.Op to be null, but it is optional
        //                 li.Add(new XText(" where " + f.Property + " " + describe(f.Op.GetValueOrDefault()) + " "));
        //                 if (e != null && codeExistsInValueSet(e, f.Value))
        //                 {
        //                     li.Add(new XElement(XmlNs.XHTMLNS + "a",
        //                         new XText(f.Value), new XAttribute("href", prefix + getCsRef(inc.System) + "#" + ProfileKnowledgeProvider.TokenizeName(f.Value))));
        //                 }
        //                 else
        //                     li.Add(new XText(f.Value));

        //                 String disp = f.getDisplayHint();
        //                 if (disp != null)
        //                     li.Add(new XText(" (" + disp + ")"));
        //             }
        //         }
        //     }

        //    return hasExtensions;
        //}

        //private void addCsRef(ValueSet.ConceptSetComponent inc, XElement li, ValueSet cs)
        //{
        //     String reference = null;

        //    if (cs != null) 
        //    {
        //        reference = inc.System;
        //    }       

        //    if (cs != null && reference != null) 
        //    {
        //     if (!String.IsNullOrEmpty(prefix) && reference.StartsWith("http://hl7.org/fhir/"))
        //         reference = reference.Substring(20)+"/index.html";

        //        XElement a =new XElement(XmlNs.XHTMLNS+"a"); li.Add(a);
        //         a.Add(new XAttribute("href", prefix+reference.Replace("\\", "/")));

        //        a.Add(new XText(inc.System));
        //    }
        //    else 
        //     li.Add(new XText(inc.System));
        //}


        //private string getCsRef(string p)
        //{
        //    //TODO: do something smart here
        //    return p;
        //}


        //private ValueSet.ValueSetDefineConceptComponent getConceptForCode(ValueSet vs, String code, String system)
        //{
        //    if (vs == null)
        //    {
        //        //TODO: include additional terminology services
        //        //if (context.getTerminologyServices() != null)
        //        //    return context.getTerminologyServices().getCodeDefinition(system, code);
        //        //else
        //        return null;
        //    }

        //    if (vs.Define == null)
        //        return null;

        //    foreach (var c in vs.Define.Concept)
        //    {
        //        var v = getConceptForCode(c, code);

        //        if (v != null)
        //            return v;
        //    }

        //    return null;
        //}


        //private ValueSet.ValueSetDefineConceptComponent getConceptForCode(ValueSet.ValueSetDefineConceptComponent c, String code)
        //{
        //    if (code == c.Code)
        //        return c;

        //    if (c.Concept == null) return null;

        //    foreach (var cc in c.Concept)
        //    {
        //        var v = getConceptForCode(cc, code);
        //        if (v != null)
        //            return v;
        //    }
        //    return null;
        //}


        //private bool codeExistsInValueSet(ValueSet vs, String code)
        //{
        //    if (vs.Define == null || vs.Define.Concept == null) return false;

        //    foreach (var c in vs.Define.Concept)
        //    {
        //        if (inConcept(code, c))
        //            return true;
        //    }
        //    return false;
        //}


        //private bool inConcept(String code, ValueSet.ValueSetDefineConceptComponent c)
        //{
        //    if (c.Code == code)
        //        return true;

        //    foreach (var g in c.Concept)
        //    {
        //        if (inConcept(code, g))
        //            return true;
        //    }

        //    return false;
        //}

        //private string prefix = "http://nos.nl";


        // private String describe(ValueSet.FilterOperator opSimple)
        // {
        //     switch (opSimple)
        //     {
        //         case ValueSet.FilterOperator.Equal: return " = ";
        //         case ValueSet.FilterOperator.IsA: return " is-a ";
        //         case ValueSet.FilterOperator.IsNotA: return " is-not-a ";
        //         case ValueSet.FilterOperator.Regex: return " matches (by regex) ";
        //         case ValueSet.FilterOperator.In: return " in ";
        //         case ValueSet.FilterOperator.NotIn: return " not in ";
        //     }
        //     return null;
        // }



        // private void AddVsRef(String value, XElement li)
        // {
        //     var a = new XElement(XmlNs.XHTMLNS + "a");
        //     a.Add(new XAttribute("href", value), new XText(value));
        //     li.Add(a);

        //     //AtomEntry<? extends Resource> vs = context.getValueSets().get(value);
        //     //if (vs == null) 
        //     //  vs = context.getCodeSystems().get(value); 
        //     //if (vs != null) {
        //     //  String ref= vs.getLinks().get("path");
        //     //  XhtmlNode a = li.addTag("a");
        //     //  a.setAttribute("href", prefix+ref.replace("\\", "/"));
        //     //  a.addText(value);
        //     //} else if (value.equals("http://snomed.info/sct") || value.equals("http://snomed.info/id")) {
        //     //  XhtmlNode a = li.addTag("a");
        //     //  a.setAttribute("href", value);
        //     //  a.addText("SNOMED-CT");      
        //     //}
        //     //else 
        //     //  li.addText(value);
        // }
    }
}

