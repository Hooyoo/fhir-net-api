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
    /// Group of multiple entities
    /// </summary>
    [FhirType("Group", IsResource=true)]
    [DataContract]
    public partial class Group : Hl7.Fhir.Model.Resource
    {
        /// <summary>
        /// Types of resources that are part of group
        /// </summary>
        [FhirEnumeration("GroupType")]
        public enum GroupType
        {
            [EnumLiteral("person")]
            Person, // Group contains "person" Patient resources.
            [EnumLiteral("animal")]
            Animal, // Group contains "animal" Patient resources.
            [EnumLiteral("practitioner")]
            Practitioner, // Group contains healthcare practitioner resources.
            [EnumLiteral("device")]
            Device, // Group contains Device resources.
            [EnumLiteral("medication")]
            Medication, // Group contains Medication resources.
            [EnumLiteral("substance")]
            Substance, // Group contains Substance resources.
        }
        
        /// <summary>
        /// null
        /// </summary>
        [FhirType("GroupCharacteristicComponent")]
        [DataContract]
        public partial class GroupCharacteristicComponent : Hl7.Fhir.Model.Element
        {
            /// <summary>
            /// Kind of characteristic
            /// </summary>
            [FhirElement("code", Order=40)]
            [Cardinality(Min=1,Max=1)]
            [DataMember]
            public Hl7.Fhir.Model.CodeableConcept Code { get; set; }
            
            /// <summary>
            /// Value held by characteristic
            /// </summary>
            [FhirElement("value", Order=50, Choice=ChoiceType.DatatypeChoice)]
            [AllowedTypes(typeof(Hl7.Fhir.Model.CodeableConcept),typeof(Hl7.Fhir.Model.FhirBoolean),typeof(Hl7.Fhir.Model.Quantity),typeof(Hl7.Fhir.Model.Range))]
            [Cardinality(Min=1,Max=1)]
            [DataMember]
            public Hl7.Fhir.Model.Element Value { get; set; }
            
            /// <summary>
            /// Group includes or excludes
            /// </summary>
            [FhirElement("exclude", Order=60)]
            [Cardinality(Min=1,Max=1)]
            [DataMember]
            public Hl7.Fhir.Model.FhirBoolean ExcludeElement { get; set; }
            
            [NotMapped]
            [IgnoreDataMemberAttribute]
            public bool? Exclude
            {
                get { return ExcludeElement != null ? ExcludeElement.Value : null; }
                set
                {
                    if(value == null)
                      ExcludeElement = null; 
                    else
                      ExcludeElement = new Hl7.Fhir.Model.FhirBoolean(value);
                }
            }
            
        }
        
        
        /// <summary>
        /// Unique id
        /// </summary>
        [FhirElement("identifier", Order=70)]
        [DataMember]
        public Hl7.Fhir.Model.Identifier Identifier { get; set; }
        
        /// <summary>
        /// person | animal | practitioner | device | medication | substance
        /// </summary>
        [FhirElement("type", Order=80)]
        [Cardinality(Min=1,Max=1)]
        [DataMember]
        public Code<Hl7.Fhir.Model.Group.GroupType> TypeElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public Hl7.Fhir.Model.Group.GroupType? Type
        {
            get { return TypeElement != null ? TypeElement.Value : null; }
            set
            {
                if(value == null)
                  TypeElement = null; 
                else
                  TypeElement = new Code<Hl7.Fhir.Model.Group.GroupType>(value);
            }
        }
        
        /// <summary>
        /// Descriptive or actual
        /// </summary>
        [FhirElement("actual", Order=90)]
        [Cardinality(Min=1,Max=1)]
        [DataMember]
        public Hl7.Fhir.Model.FhirBoolean ActualElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public bool? Actual
        {
            get { return ActualElement != null ? ActualElement.Value : null; }
            set
            {
                if(value == null)
                  ActualElement = null; 
                else
                  ActualElement = new Hl7.Fhir.Model.FhirBoolean(value);
            }
        }
        
        /// <summary>
        /// Kind of Group members
        /// </summary>
        [FhirElement("code", Order=100)]
        [DataMember]
        public Hl7.Fhir.Model.CodeableConcept Code { get; set; }
        
        /// <summary>
        /// Label for Group
        /// </summary>
        [FhirElement("name", Order=110)]
        [DataMember]
        public Hl7.Fhir.Model.FhirString NameElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public string Name
        {
            get { return NameElement != null ? NameElement.Value : null; }
            set
            {
                if(value == null)
                  NameElement = null; 
                else
                  NameElement = new Hl7.Fhir.Model.FhirString(value);
            }
        }
        
        /// <summary>
        /// Number of members
        /// </summary>
        [FhirElement("quantity", Order=120)]
        [DataMember]
        public Hl7.Fhir.Model.Integer QuantityElement { get; set; }
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public int? Quantity
        {
            get { return QuantityElement != null ? QuantityElement.Value : null; }
            set
            {
                if(value == null)
                  QuantityElement = null; 
                else
                  QuantityElement = new Hl7.Fhir.Model.Integer(value);
            }
        }
        
        /// <summary>
        /// Trait of group members
        /// </summary>
        [FhirElement("characteristic", Order=130)]
        [Cardinality(Min=0,Max=-1)]
        [DataMember]
        public List<Hl7.Fhir.Model.Group.GroupCharacteristicComponent> Characteristic { get; set; }
        
        /// <summary>
        /// Who is in group
        /// </summary>
        [FhirElement("member", Order=140)]
        [Cardinality(Min=0,Max=-1)]
        [DataMember]
        public List<Hl7.Fhir.Model.ResourceReference> Member { get; set; }
        
    }
    
}
