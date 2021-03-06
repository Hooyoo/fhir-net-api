﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hl7.Fhir.Model;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml;
using Hl7.Fhir.Support;

namespace Hl7.Fhir.Serialization
{
    public partial class FhirSerializer
    {
        internal static void Serialize(object instance, IFhirWriter writer)
        {
            new ResourceWriter(writer).Serialize(instance);
        }

        internal static string SerializeToXml(object instance)
        {
            return xmlWriterToString(xw => FhirSerializer.Serialize(instance, new XmlFhirWriter(xw)));
        }

        internal static string SerializeToJson(object instance)
        {
            return jsonWriterToString(jw => FhirSerializer.Serialize(instance, new JsonDomFhirWriter(jw)));
        }

        internal static byte[] SerializeToXmlBytes(object instance)
        {
            return xmlWriterToBytes(xw => FhirSerializer.Serialize(instance, new XmlFhirWriter(xw)));
        }

        internal static byte[] SerializeToJsonBytes(object instance)
        {
            return jsonWriterToBytes(jw => FhirSerializer.Serialize(instance, new JsonDomFhirWriter(jw)));
        }

        public static string SerializeResourceToXml(Resource resource)
        {
            return SerializeToXml(resource);
        }

        public static string SerializeTagListToXml(TagList list)
        {
            return SerializeToXml(list);
        }
            
        public static byte[] SerializeResourceToXmlBytes(Resource resource)
        {
            return SerializeToXmlBytes(resource);
        }

        public static byte[] SerializeTagListToXmlBytes(TagList list)
        {
            return SerializeToXmlBytes(list);
        }

        public static string SerializeResourceToJson(Resource resource)
        {
            return SerializeToJson(resource);
        }

        public static string SerializeTagListToJson(TagList list)
        {
            return SerializeToJson(list);
        }

        public static byte[] SerializeResourceToJsonBytes(Resource resource)
        {
            return SerializeToJsonBytes(resource);
        }

        public static byte[] SerializeTagListToJsonBytes(TagList list)
        {
            return SerializeToJsonBytes(list);
        }


        public static void SerializeResource(Resource resource, JsonWriter writer)
        {
            FhirSerializer.Serialize(resource, new JsonDomFhirWriter(writer));
        }

        public static void SerializeTagList(TagList list, JsonWriter jw)
        {
            FhirSerializer.Serialize(list, new JsonDomFhirWriter(jw));
        }

        public static void SerializeTagList(TagList list, XmlWriter xw)
        {
            FhirSerializer.Serialize(list, new XmlFhirWriter(xw));
        }

        public static void SerializeResource(Resource resource, XmlWriter writer)
        {
            FhirSerializer.Serialize(resource, new XmlFhirWriter(writer));
        }


        public static void SerializeBundle(Bundle bundle, JsonWriter writer, bool summary = false)
        {
            BundleJsonSerializer.WriteTo(bundle, writer, summary);
        }

        public static void SerializeBundle(Bundle bundle, XmlWriter writer, bool summary = false)
        {
            BundleXmlSerializer.WriteTo(bundle, writer, summary);
        }

        public static string SerializeBundleToJson(Bundle bundle, bool summary = false)
        {
            return jsonWriterToString(jw => BundleJsonSerializer.WriteTo(bundle, jw, summary));
        }

        public static string SerializeBundleToXml(Bundle bundle, bool summary = false)
        {
            return xmlWriterToString(xw => BundleXmlSerializer.WriteTo(bundle, xw, summary));
        }

        public static byte[] SerializeBundleToJsonBytes(Bundle bundle, bool summary = false)
        {
            return jsonWriterToBytes(jw => BundleJsonSerializer.WriteTo(bundle, jw, summary));
        }

        public static byte[] SerializeBundleToXmlBytes(Bundle bundle, bool summary = false)
        {
            return xmlWriterToBytes(xw => BundleXmlSerializer.WriteTo(bundle, xw, summary));
        }

        public static void SerializeBundleEntry(BundleEntry entry, JsonWriter writer, bool summary = false)
        {
            BundleJsonSerializer.WriteTo(entry, writer, summary);
        }

        public static void SerializeBundleEntry(BundleEntry entry, XmlWriter writer, bool summary = false)
        {
            BundleXmlSerializer.WriteTo(entry, writer,summary);
        }

        public static string SerializeBundleEntryToJson(BundleEntry entry, bool summary = false)
        {
            return jsonWriterToString(jw=>BundleJsonSerializer.WriteTo(entry, jw, summary));
        }

        public static string SerializeBundleEntryToXml(BundleEntry entry, bool summary = false)
        {
            return xmlWriterToString(xw => BundleXmlSerializer.WriteTo(entry, xw, summary));
        }

        public static byte[] SerializeBundleEntryToJsonBytes(BundleEntry entry, bool summary = false)
        {
            return jsonWriterToBytes(jw => BundleJsonSerializer.WriteTo(entry, jw, summary));
        }

        public static byte[] SerializeBundleEntryToXmlBytes(BundleEntry entry, bool summary = false)
        {
            return xmlWriterToBytes(xw => BundleXmlSerializer.WriteTo(entry, xw, summary));
        }

        private static byte[] xmlWriterToBytes(Action<XmlWriter> serializer)
        {
            MemoryStream stream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false) };
            XmlWriter xw = XmlWriter.Create(stream, settings);

            serializer(xw);

            xw.Flush();

            return stream.ToArray();
        }


        private static byte[] jsonWriterToBytes(Action<JsonWriter> serializer)
        {
            MemoryStream stream = new MemoryStream();

            var sw = new StreamWriter(stream, new UTF8Encoding(false));
            JsonWriter jw = new JsonTextWriter(sw);

            serializer(jw);

            jw.Flush();

            return stream.ToArray();

        }

        private static string jsonWriterToString(Action<JsonWriter> serializer)
        {
            StringBuilder resultBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(resultBuilder);
            JsonWriter jw = new JsonTextWriter(sw);

            serializer(jw);
            jw.Flush();
            jw.Close();

            return resultBuilder.ToString();
        }


        private static string xmlWriterToString(Action<XmlWriter> serializer)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriter xw = XmlWriter.Create(sb);

            serializer(xw);

            xw.Flush();
           

            return sb.ToString();
        }
    }
}
