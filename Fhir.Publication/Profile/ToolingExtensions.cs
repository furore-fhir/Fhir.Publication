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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Hl7.Fhir.Publication.Profile
{
    internal static class ToolingExtensions
    {
        public const String EXT_SUBSUMES = "http://hl7.org/fhir/StructureDefinition/valueset-subsumes";
        private const String EXT_OID = "http://hl7.org/fhir/StructureDefinition/valueset-oid";
        public const String EXT_DEPRECATED = "http://hl7.org/fhir/StructureDefinition/valueset-deprecated";
        public const String EXT_DEFINITION = "http://hl7.org/fhir/StructureDefinition/valueset-definition";
        public const String EXT_COMMENT = "http://hl7.org/fhir/StructureDefinition/valueset-comments";
        private const String EXT_IDENTIFIER = "http://hl7.org/fhir/StructureDefinition/identifier";
        private const String EXT_TRANSLATION = "http://hl7.org/fhir/StructureDefinition/translation";
        public const String EXT_ISSUE_SOURCE = "http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-source";
        public const String EXT_DISPLAY_HINT = "http://hl7.org/fhir/StructureDefinition/structuredefinition-display-hint";

        // unregistered?

        public const String EXT_FLYOVER = "http://hl7.org/fhir/Profile/questionnaire-extensions#flyover";
        private const String EXT_QTYPE = "http://www.healthintersections.com.au/fhir/Profile/metadata#type";
        private const String EXT_EXPANSION_CLOSED = "http://hl7.org/fhir/Profile/questionnaire-extensions#closed";
        private const String EXT_QREF = "http://www.healthintersections.com.au/fhir/Profile/metadata#reference";
        private const String EXTENSION_FILTER_ONLY = "http://www.healthintersections.com.au/fhir/Profile/metadata#expandNeedsFilter";
        private const String EXT_TYPE = "http://www.healthintersections.com.au/fhir/Profile/metadata#type";
        private const String EXT_REFERENCE = "http://www.healthintersections.com.au/fhir/Profile/metadata#reference";
        private const String EXT_ALLOWABLE_UNITS = "http://hl7.org/fhir/StructureDefinition/elementdefinition-allowedUnits";

        public static bool? GetDeprecated(this ValueSet.ConceptDefinitionComponent c)
        {
            return c.GetBoolExtension(EXT_DEPRECATED);
        }

        public static String GetComment(this ValueSet.ConceptDefinitionComponent c)
        {
            return c.GetStringExtension(EXT_COMMENT);
        }

        //public static String getDisplayHint(this Element def)
        //{
        //    return ReadStringExtension(def, EXT_DISPLAY_HINT);
        //}


        public static IEnumerable<Code> GetSubsumes(this ValueSet.ConceptDefinitionComponent c)
        {
            return c.GetExtensions(EXT_SUBSUMES).Where(ext => ext.Value is Code).Select(ext => (Code)ext.Value);
        }
    }
}
