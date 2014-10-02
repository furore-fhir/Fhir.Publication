﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class FileRendering : IWork
    {
        Source source;
        string targetfile;
        IRenderer renderer;

        public FileRendering(Source source, string targetfile, IRenderer renderer)
        {
            this.source = source;
            this.targetfile = targetfile;
            this.renderer = renderer;
        }

        public void Execute()
        {
            source.Context.EnsureTarget();
            using (Stream input = File.OpenRead(source.FullPath))
            using (Stream output = File.OpenWrite(targetfile))
            {
                renderer.Render(source, input, output);
            }
        }
    }
}
