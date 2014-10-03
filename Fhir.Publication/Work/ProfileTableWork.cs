﻿using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hl7.Fhir.Publication
{
    public class ProfileTableWork : IWork
    {
        public Context Context { get; set; }

        public void Execute()
        {
            var generator = new ProfileTableGenerator(Context.TargetDir, "hoepsakee", false);
            string s = File.ReadAllText(Context.FullPath);
            var profile = (Profile)FhirParser.ParseResourceFromXml(s);
            var xmldoc = generator.generate(profile, false);
            File.WriteAllText(Context.TargetFullPath, xmldoc.ToString(SaveOptions.DisableFormatting));
        }
    }

}