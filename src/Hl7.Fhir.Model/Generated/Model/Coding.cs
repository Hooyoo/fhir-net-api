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
    /// A reference to a code defined by a terminology system
    /// </summary>
    [FhirType("Coding")]
    [DataContract]
    public partial class Coding : Hl7.Fhir.Model.Element
    {
        /// <summary>
        /// Identity of the terminology system
        /// </summary>
        [FhirElement("system", Order=40)]
        [DataMember]
        public Hl7.Fhir.Model.FhirUri SystemElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public System.Uri System
        {
            get { return SystemElement != null ? SystemElement.Value : null; }
            set
            {
                if(value == null)
                  SystemElement = null; 
                else
                  SystemElement = new Hl7.Fhir.Model.FhirUri(value);
            }
        }
        
        /// <summary>
        /// Version of the system - if relevant
        /// </summary>
        [FhirElement("version", Order=50)]
        [DataMember]
        public Hl7.Fhir.Model.FhirString VersionElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public string Version
        {
            get { return VersionElement != null ? VersionElement.Value : null; }
            set
            {
                if(value == null)
                  VersionElement = null; 
                else
                  VersionElement = new Hl7.Fhir.Model.FhirString(value);
            }
        }
        
        /// <summary>
        /// Symbol in syntax defined by the system
        /// </summary>
        [FhirElement("code", Order=60)]
        [DataMember]
        public Hl7.Fhir.Model.Code CodeElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public string Code
        {
            get { return CodeElement != null ? CodeElement.Value : null; }
            set
            {
                if(value == null)
                  CodeElement = null; 
                else
                  CodeElement = new Hl7.Fhir.Model.Code(value);
            }
        }
        
        /// <summary>
        /// Representation defined by the system
        /// </summary>
        [FhirElement("display", Order=70)]
        [DataMember]
        public Hl7.Fhir.Model.FhirString DisplayElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public string Display
        {
            get { return DisplayElement != null ? DisplayElement.Value : null; }
            set
            {
                if(value == null)
                  DisplayElement = null; 
                else
                  DisplayElement = new Hl7.Fhir.Model.FhirString(value);
            }
        }
        
        /// <summary>
        /// If this code was chosen directly by the user
        /// </summary>
        [FhirElement("primary", Order=80)]
        [DataMember]
        public Hl7.Fhir.Model.FhirBoolean PrimaryElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public bool? Primary
        {
            get { return PrimaryElement != null ? PrimaryElement.Value : null; }
            set
            {
                if(value == null)
                  PrimaryElement = null; 
                else
                  PrimaryElement = new Hl7.Fhir.Model.FhirBoolean(value);
            }
        }
        
        /// <summary>
        /// Set this coding was chosen from
        /// </summary>
        [FhirElement("valueSet", Order=90)]
        [DataMember]
        public Hl7.Fhir.Model.ResourceReference ValueSet { get; set; }
        
    }
    
}
