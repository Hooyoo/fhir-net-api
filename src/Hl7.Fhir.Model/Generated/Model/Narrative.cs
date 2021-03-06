﻿using System;
using System.Collections.Generic;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Validation;
using System.Linq;
using System.Runtime.Serialization;

/*
  Copyright (c) 2011-2013, HL7, Inc.
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

//
// Generated on Mon, Feb 3, 2014 11:56+0100 for FHIR v0.80
//
namespace Hl7.Fhir.Model
{
    /// <summary>
    /// A human-readable formatted text, including images
    /// </summary>
    [FhirType("Narrative")]
    [DataContract]
    public partial class Narrative : Hl7.Fhir.Model.Element
    {
        /// <summary>
        /// The status of a resource narrative
        /// </summary>
        [FhirEnumeration("NarrativeStatus")]
        public enum NarrativeStatus
        {
            [EnumLiteral("generated")]
            Generated, // The contents of the narrative are entirely generated from the structured data in the resource.
            [EnumLiteral("extensions")]
            Extensions, // The contents of the narrative are entirely generated from the structured data in the resource and some of the content is generated from extensions.
            [EnumLiteral("additional")]
            Additional, // The contents of the narrative contain additional information not found in the structured data.
            [EnumLiteral("empty")]
            Empty, // the contents of the narrative are some equivalent of "No human-readable text provided for this resource".
        }
        
        /// <summary>
        /// generated | extensions | additional
        /// </summary>
        [FhirElement("status", Order=40)]
        [Cardinality(Min=1,Max=1)]
        [DataMember]
        public Code<Hl7.Fhir.Model.Narrative.NarrativeStatus> StatusElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public Hl7.Fhir.Model.Narrative.NarrativeStatus? Status
        {
            get { return StatusElement != null ? StatusElement.Value : null; }
            set
            {
                if(value == null)
                  StatusElement = null; 
                else
                  StatusElement = new Code<Hl7.Fhir.Model.Narrative.NarrativeStatus>(value);
            }
        }
        
        /// <summary>
        /// Limited xhtml content
        /// </summary>
        [FhirElement("div", XmlSerialization=XmlSerializationHint.XhtmlElement, Order=50)]
        [Cardinality(Min=1,Max=1)]
        [NarrativeXhtmlPattern]
        [DataMember]
        public string Div { get; set; }
        
    }
    
}
