﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class TestRenderer : IRenderer
    {

        public void Render(StreamReader reader, StreamWriter writer)
        {
            string content = reader.ReadToEnd();
            content = "Hello World!!!\n" + content;
            writer.Write(content);
        }
    }
}
