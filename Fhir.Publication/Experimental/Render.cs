﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class RenderProcessor : IProcessor
    {
        public IRenderer Renderer { get; set; }
        public RenderProcessor(IRenderer renderer)
        {
            this.Renderer = renderer;
        }

        public void Process(Document input, Stage stage)
        {
            Log.Debug("Rendering {0} \n from: {1} \n ..to: {2}.", Renderer.GetType().Name, input.SourceFullPath, input.TargetFullPath);
            Document output = stage.CreateDocumentBasedOn(input);
            Renderer.Render(input, output);
        }
    }

}
