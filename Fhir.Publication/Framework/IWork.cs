﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public interface IWork
    {
        Context Context { get; set; }
        void Execute();
    }

}