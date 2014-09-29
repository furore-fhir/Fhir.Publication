﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Hl7.Fhir.DocumenterTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = (args.Count() == 1) ? args[0] : Directory.GetCurrentDirectory();

            string sourcedir = dir + "\\Source";
            string targetdir = dir + "\\Generated";

            
            MappingList mappings = new MappingList();
            mappings.Map(".md", ".html", new MarkdownRenderer());


            FhirDocumentation.Generate(sourcedir, targetdir);
            
        }
    }
}
