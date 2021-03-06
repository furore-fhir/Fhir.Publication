﻿using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hl7.Fhir.Publication
{

    public class ProfileProcessor : IProcessor
    {
        public ISelector Influx { get; set; }

        public void Process(Document input, Stage output)
        {
            input.Context.EnsureTarget(); // omdat er eerst plaatjes worden gegenereerd voorafgaand aan het document zelf.

            var generator = new ProfileTableGenerator(new ProfileKnowledgeProvider(input.Name, input.Context.Target.Directory));
            var profile = (Profile)FhirParser.ParseResourceFromXml(input.Text);
            Document result = input.CloneMetadata();
            var xmldoc = generator.Generate(profile, extensionsOnly: false);
            result.Text = xmldoc.ToString(SaveOptions.DisableFormatting);
            output.Post(result);
        }
    }
}
