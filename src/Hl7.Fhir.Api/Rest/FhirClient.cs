﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hl7.Fhir;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Hl7.Fhir.Search;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Rest;
using System.Threading;



namespace Hl7.Fhir.Rest
{
    public class FhirClient
    {
        private Uri _endpoint;

        /// <summary>
        /// Creates a new client using a default endpoint
        /// </summary>
        public FhirClient(Uri endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            if (!endpoint.IsAbsoluteUri) throw new ArgumentException("endpoint", "Endpoint must be absolute");

            _endpoint = endpoint;
            PreferredFormat = ResourceFormat.Xml;
        }

        public FhirClient(string endpoint)
            : this(new Uri(endpoint))
        {
        }

        public ResourceFormat PreferredFormat { get; set; }
        public bool UseFormatParam { get; set; }

        public FhirResponse LastResponseDetails { get; private set; }

        /// <summary>
        /// Contact the endpoint's Conformance statement to configure the client
        /// to  the capabilities of the server
        /// </summary>
        //public void Configure()
        //{
        //    // Set preferred serialization format
        //    throw new NotImplementedException();
        //}
        

        /// <summary>
        /// The default endpoint for use with operations that use discrete id/version parameters
        /// instead of explicit uri endpoints.
        /// </summary>
        public Uri Endpoint 
        {
            get
            {
                return _endpoint != null ? _endpoint : null; 
            }
        }


        private Uri makeAbsolute(Uri location=null)
        {
            // If called without a location, just return the base endpoint
            if (location == null) return Endpoint;

            // If the location is absolute, verify whether it is within the endpoint
            if (location.IsAbsoluteUri)
            {
                if (!new RestUrl(Endpoint).IsEndpointFor(location)) 
                    throw Error.Argument("location", "Url is not located on this FhirClient's endpoint");
            }
            else
                // Else, make location absolute within the endpoint
                location = new Uri(Endpoint, location);

            return location;
        }


        /// <summary>
        /// Get a conformance statement for the system
        /// </summary>
        /// <param name="useOptionsVerb">If true, uses the Http OPTIONS verb to get the conformance, otherwise uses the /metadata endpoint</param>
        /// <returns>A Conformance resource. Throws an exception if the operation failed.</returns>
        public ResourceEntry<Conformance> Conformance(bool useOptionsVerb = false)
        {
            RestUrl url = useOptionsVerb ? new RestUrl(Endpoint) : new RestUrl(Endpoint).WithMetadata();

            var req = new FhirRequest(url.Uri, useOptionsVerb ? "OPTIONS" : "GET");
            return doRequest(req, HttpStatusCode.OK, resp => resp.BodyAsEntry<Conformance>());
        }


        /// <summary>
        /// Create a resource on a FHIR endpoint
        /// </summary>
        /// <param name="resource">The resource instance to create</param>
        /// <param name="tags">Optional. List of Tags to add to the created instance.</param>
        /// <param name="refresh">Optional. When true, fetches the newly created resource from the server.</param>
        /// <returns>A ResourceEntry containing the metadata (id, selflink) associated with the resource as created on the server, or an exception if the create failed.</returns>
        /// <typeparam name="TResource">The type of resource to create</typeparam>
        /// <remarks>The Create operation normally does not return the posted resource, but just its metadata. Specifying
        /// refresh=true ensures the return value contains the Resource as stored by the server.
        /// </remarks>
        public ResourceEntry<TResource> Create<TResource>(TResource resource, IEnumerable<Tag> tags = null, bool refresh = false) where TResource : Resource, new()
        {
            if (resource == null) throw Error.ArgumentNull("resource");

            return internalCreate<TResource>(resource, tags, null, refresh);
        }


        /// <summary>
        /// Create a resource with a given id on the FHIR endpoint
        /// </summary>
        /// <param name="resource">The resource instance to create</param>
        /// <param name="id">Optional. A client-assigned logical id for the newly created resource.</param>
        /// <param name="tags">Optional. List of Tags to add to the created instance.</param>
        /// <param name="refresh">Optional. When true, fetches the newly created resource from the server.</param>
        /// <returns>A ResourceEntry containing the metadata (id, selflink) associated with the resource as created on the server, or an exception if the create failed.</returns>
        /// <typeparam name="TResource">The type of resource to create</typeparam>
        /// <remarks>The Create operation normally does not return the posted resource, but just its metadata. Specifying
        /// refresh=true ensures the return value contains the Resource as stored by the server.
        /// </remarks>
        public ResourceEntry<TResource> Create<TResource>(TResource resource, string id, IEnumerable<Tag> tags = null, bool refresh = false) where TResource : Resource, new()
        {
            if (resource == null) throw Error.ArgumentNull("resource");
            if (id == null) throw Error.ArgumentNull("id", "Must supply a client-assigned id");

            return internalCreate<TResource>(resource, tags, id, refresh);
        }


        private ResourceEntry<TResource> internalCreate<TResource>(TResource resource, IEnumerable<Tag> tags, string id, bool refresh) where TResource : Resource, new()
        {
            var collection = typeof(TResource).GetCollectionName();
            FhirRequest req = null;

            if (id == null)
            {
                // A normal create
                var rl = new RestUrl(Endpoint).ForCollection(collection);
                req = new FhirRequest(rl.Uri, "POST");
            }
            else
            {
                // A create at a specific id => PUT to that address
                var ri = ResourceIdentity.Build(Endpoint, collection, id);
                req = new FhirRequest(ri, "PUT");
            }

            req.SetBody(resource, PreferredFormat);
            if(tags != null) req.SetTagsInHeader(tags);
            var entry = doRequest(req, id == null ? HttpStatusCode.OK : HttpStatusCode.Created, resp => resp.BodyAsEntry<TResource>());

            // If asked for it, immediately get the contents *we just posted*, so use the actually created version
            if (refresh) entry = Refresh(entry, versionSpecific: true);
            return entry;

        }

        /// <summary>
        /// Refreshes the data and metadata for a given ResourceEntry.
        /// </summary>
        /// <param name="entry">The entry to refresh. It's id property will be used to fetch the latest version of the Resource.</param>
        /// <typeparam name="TResource">The type of resource to refresh</typeparam>
        /// <returns>A resource entry containing up-to-date data and metadata.</returns>
        public ResourceEntry<TResource> Refresh<TResource>(ResourceEntry<TResource> entry) where TResource : Resource, new()
        {
            return Refresh<TResource>(entry, false);                
        }


        internal ResourceEntry<TResource> Refresh<TResource>(ResourceEntry<TResource> entry, bool versionSpecific = false) where TResource : Resource, new()
        {
            if (entry == null) throw Error.ArgumentNull("entry");

            if (!versionSpecific)
                return Read<TResource>(entry.Id);
            else
                return Read<TResource>(entry.SelfLink);
        }


        /// <summary>
        /// Fetches a typed resource from a FHIR resource endpoint.
        /// </summary>
        /// <param name="endpoint">The url of the Resource to fetch. This can be a Resource id url or a version-specific
        /// Resource url.</param>
        /// <typeparam name="TResource">The type of resource to read</typeparam>
        /// <returns>The requested resource as a ResourceEntry&lt;T&gt;. This operation will throw an exception
        /// if the resource has been deleted or does not exist. The specified may be relative or absolute, if it is an abolute
        /// url, it must reference an address within the endpoint.</returns>
        public ResourceEntry<TResource> Read<TResource>(Uri location) where TResource : Resource, new()
        {
            if (location == null) throw Error.ArgumentNull("location");

            var req = new FhirRequest(makeAbsolute(location));                
            return doRequest(req, HttpStatusCode.OK, resp => resp.BodyAsEntry<TResource>());
        }


        /// <summary>
        /// Reads a resource from a FHIR resource endpoint.
        /// </summary>
        /// <param name="endpoint">The url of the Resource to fetch. This can be a Resource id url or a version-specific
        /// Resource url.</param>
        /// <returns>The requested resource as an untyped ResourceEntry. The ResourceEntry.Resource, which is of type
        /// object, must be cast to the correct Resource type to access its properties.
        /// The specified may be relative or absolute, if it is an abolute
        /// url, it must reference an address within the endpoint.</returns>
        public ResourceEntry Read(Uri location)
        {
            if (location == null) throw Error.ArgumentNull("location");

            var collection = getCollectionFromLocation(location);

            var req = new FhirRequest(makeAbsolute(location));
            return doRequest(req, HttpStatusCode.OK, resp => resp.BodyAsEntry(collection));
        }


        private static string getCollectionFromLocation(Uri location)
        {
            var collection = new ResourceIdentity(location).Collection;
            if (collection == null) throw Error.Argument("location", "Must be a FHIR REST url containing the resource type in its path");

            return collection;
        }

        private static string getIdFromLocation(Uri location)
        {
            var id = new ResourceIdentity(location).Id;
            if (id == null) throw Error.Argument("location", "Must be a FHIR REST url containing the logical id in its path");

            return id;
        }


        ///// <summary>
        ///// Fetches a typed resource, given its id and optionally its version.
        ///// </summary>
        ///// <param name="id">Id of the Resource to fetch.</param>
        ///// <param name="versionId">Optional. The version of the Resource to fetch.</param>
        ///// <typeparam name="TResource">The type of resource to read</typeparam>
        ///// <returns>The requested resource as a ResourceEntry&lt;T&gt;. This operation will throw an exception
        ///// if the resource has been deleted or does not exist.</returns>
        //public ResourceEntry<TResource> Read<TResource>(string id, string versionId=null) where TResource : Resource, new()
        //{
        //    if (id == null) throw new ArgumentNullException("id");

        //    var ri = ResourceIdentity.Build(Endpoint,typeof(TResource).GetCollectionName(), id, versionId);
        //    return Read<TResource>(ri);
        //}


        /// <summary>
        /// Update (or create) a resource at a given endpoint
        /// </summary>
        /// <param name="entry">A ResourceEntry containing the resource to update</param>
        /// <param name="refresh">Optional. When true, fetches the newly updated resource from the server.</param>
        /// <typeparam name="TResource">The type of resource that is being updated</typeparam>
        /// <returns>The resource as updated on the server. Throws an exception when the update failed,
        /// in particular when an update conflict is detected and the server returns a HTTP 409. When the ResourceEntry
        /// passed as the argument does not have a SelfLink, the server may return a HTTP 412 to indicate it
        /// requires version-aware updates.</returns>
        public ResourceEntry<TResource> Update<TResource>(ResourceEntry<TResource> entry, bool refresh=false)
                        where TResource : Resource, new()
        {
            if (entry == null) throw Error.ArgumentNull("entry");
            if (entry.Resource == null) throw Error.Argument("entry","Entry does not contain a Resource to update");
            if (entry.Id == null) throw Error.Argument("entry","Entry needs a non-null entry.id to send the update to");

            var req = new FhirRequest(entry.Id, "PUT");
            req.SetBody(entry.Resource,PreferredFormat);
            if(entry.Tags != null) req.SetTagsInHeader(entry.Tags);

            // Always supply the version we are updating if we have one. Servers may require this.
            if (entry.SelfLink != null) req.SetContentLocation(entry.SelfLink);

            // This might be an update of a resource that doesn't yet exist, so accept a status Created too
            var updated = doRequest(req, new HttpStatusCode[] { HttpStatusCode.Created, HttpStatusCode.OK }, resp => resp.BodyAsEntry<TResource>());

            // If asked for it, immediately get the contents *we just posted*, so use the actually created version
            if (refresh) updated = Refresh(updated, versionSpecific: true);

            return updated;
        }

     
        /// <summary>
        /// Delete a resource at the given endpoint.
        /// </summary>
        /// <param name="location">endpoint of the resource to delete</param>
        /// <returns>Throws an exception when the delete failed, though this might
        /// just mean the server returned 404 (the resource didn't exist before) or 410 (the resource was
        /// already deleted).</returns>
        public void Delete(Uri location)
        {
            if (location == null) throw Error.ArgumentNull("location");

            var req = new FhirRequest(makeAbsolute(location), "DELETE");
            doRequest(req, HttpStatusCode.NoContent, resp => true);
        }



        /// <summary>
        /// Delete a resource represented by the entry
        /// </summary>
        /// <param name="entry">Entry containing the id of the resource to delete</param>
        /// <returns>Throws an exception when the delete failed, though this might
        /// just mean the server returned 404 (the resource didn't exist before) or 410 (the resource was
        /// already deleted).</returns>
        public void Delete(ResourceEntry entry)
        {
            if (entry == null) throw Error.ArgumentNull("entry");
            if (entry.Id == null) throw Error.Argument("entry", "Entry must have an id");

            Delete(entry.Id);
        }

     

        /// <summary>
        /// Retrieve the version history for a specific resource type
        /// </summary>
        /// <param name="since">Optional. Returns only changes after the given date</param>
        /// <param name="pageSize">Optional. Asks server to limit the number of entries per page returned</param>
        /// <typeparam name="TResource">The type of Resource to get the history for</typeparam>
        /// <returns>A bundle with the history for the indicated instance, may contain both 
        /// ResourceEntries and DeletedEntries.</returns>
	    public Bundle TypeHistory<TResource>(DateTimeOffset? since = null, int? pageSize = null) where TResource : Resource, new()
        {          
            var collection = typeof(TResource).GetCollectionName();

            return internalHistory(collection, null, since, pageSize);
        }


        /// <summary>
        /// Retrieve the version history for a resource at a given location
        /// </summary>
        /// <param name="location">The address of the resource to get the history for</param>
        /// <param name="since">Optional. Returns only changes after the given date</param>
        /// <param name="pageSize">Optional. Asks server to limit the number of entries per page returned</param>
        /// <returns>A bundle with the history for the indicated instance, may contain both 
        /// ResourceEntries and DeletedEntries.</returns>
        public Bundle History(Uri location, DateTimeOffset? since = null, int? pageSize = null)
        {
            if (location == null) throw Error.ArgumentNull("location");

            var collection = getCollectionFromLocation(location);
            var id = getIdFromLocation(location);

            return internalHistory(collection, id, since, pageSize);
        }


        /// <summary>
        /// Retrieve the version history for a resource in a ResourceEntry
        /// </summary>
        /// <param name="entry">The ResourceEntry representing the Resource to get the history for</param>
        /// <param name="since">Optional. Returns only changes after the given date</param>
        /// <param name="pageSize">Optional. Asks server to limit the number of entries per page returned</param>
        /// <returns>A bundle with the history for the indicated instance, may contain both 
        /// ResourceEntries and DeletedEntries.</returns>
        public Bundle History(BundleEntry entry, DateTimeOffset? since = null, int? pageSize = null)
        {
            if (entry == null) throw Error.ArgumentNull("entry");

            return History(entry.Id, since, pageSize);
        }


        /// <summary>
        /// Retrieve the full version history of the server
        /// </summary>
        /// <param name="since">Optional. Returns only changes after the given date</param>
        /// <param name="pageSize">Optional. Asks server to limit the number of entries per page returned</param>
        /// <returns>A bundle with the history for the indicated instance, may contain both 
        /// ResourceEntries and DeletedEntries.</returns>
        public Bundle WholeSystemHistory(DateTimeOffset? since = null, int? pageSize = null)
        {
            return internalHistory(null, null, since, pageSize);
        }


        private Bundle internalHistory(string collection = null, string id = null, DateTimeOffset? since = null, int? pageSize = null)
        {
            RestUrl location = null;

            if(collection == null)
                location = new RestUrl(Endpoint).ServerHistory();
            else
            {
                location = (id == null) ?
                    new RestUrl(_endpoint).CollectionHistory(collection) :
                    new RestUrl(_endpoint).ResourceHistory(collection, id);
            }

            if (since != null) location = location.AddParam(HttpUtil.HISTORY_PARAM_SINCE, PrimitiveTypeConverter.ConvertTo<string>(since.Value));
            if (pageSize != null) location = location.AddParam(HttpUtil.HISTORY_PARAM_COUNT, pageSize.ToString());

            return fetchBundle(location.Uri);
        }


        /// <summary>
        /// Fetches a bundle from a FHIR resource endpoint. 
        /// </summary>
        /// <param name="location">The url of the endpoint which returns a Bundle</param>
        /// <returns>The Bundle as received by performing a GET on the endpoint. This operation will throw an exception
        /// if the operation does not result in a HttpStatus OK.</returns>
        private Bundle fetchBundle(Uri location)
        {
            var req = new FhirRequest(makeAbsolute(location), "GET");
            return doRequest(req, HttpStatusCode.OK, resp => resp.BodyAsBundle());
        }


        /// <summary>
        /// Validates whether the contents of the resource would be acceptable as an update
        /// </summary>
        /// <param name="entry">The entry containing the updated Resource to validate</param>
        /// <param name="result">Contains the OperationOutcome detailing why validation failed, or null if validation succeeded</param>
        /// <returns>True when validation was successful, false otherwise. Note that this function may still throw exceptions if non-validation related
        /// failures occur.</returns>
        public bool TryValidateUpdate<TResource>(ResourceEntry<TResource> entry, out OperationOutcome result) where TResource : Resource, new()
        {
            if (entry == null) throw Error.ArgumentNull("entry");
            if (entry.Resource == null) throw Error.Argument("entry","Entry does not contain a Resource to validate");
            if (entry.Id == null) throw Error.Argument("enry", "Entry needs a non-null entry.id to use for validation");

            var id = new ResourceIdentity(entry.Id);
            var url = new RestUrl(Endpoint).Validate(id.Collection, id.Id);
            result = doValidate(url.Uri, entry.Resource, entry.Tags);

            return result == null || !result.Success();
        }


        /// <summary>
        /// Validates whether the contents of the resource would be acceptable as a create
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="resource">The entry containing the Resource data to use for the validation</param>
        /// <param name="result">Contains the OperationOutcome detailing why validation failed, or null if validation succeeded</param>
        /// <param name="tags">Optional list of tags to attach to the resource</param>
        /// <returns>True when validation was successful, false otherwise. Note that this function may still throw exceptions if non-validation related
        /// failures occur.</returns>
        public bool TryValidateCreate<TResource>(TResource resource, out OperationOutcome result, IEnumerable<Tag> tags = null) where TResource : Resource, new()
        {
            if (resource == null) throw new ArgumentNullException("resource");

            var collection = typeof(Resource).GetCollectionName();
            var url = new RestUrl(_endpoint).Validate(collection);

            result = doValidate(url.Uri, resource, tags);
            return result == null || !result.Success();
        }


        private OperationOutcome doValidate(Uri url, Resource data, IEnumerable<Tag> tags)
        {
            var req = new FhirRequest(url, "POST");

            req.SetBody(data, PreferredFormat);
            if(tags != null) req.SetTagsInHeader(tags);

            try
            {
                doRequest(req, HttpStatusCode.OK, resp => true);
                return null;
            }
            catch (FhirOperationException foe)
            {
                if (foe.Outcome != null)
                    return foe.Outcome;
                else
                    throw foe;
            }
        }

               
        
        /// <summary>
        /// Search for Resources of a certain type that match the given criteria
        /// </summary>
        /// <param name="criteria">Optional. The search parameters to filter the resources on</param>
        /// <param name="sort">Optional. The element to sort on</param>
        /// <param name="includes">Optional. A list of include paths</param>
        /// <param name="pageSize">Optional. Asks server to limit the number of entries per page returned</param>
        /// <typeparam name="TResource">The type of resource to list</typeparam>
        /// <returns>A Bundle with all resources found by the search, or an empty Bundle if none were found.</returns>
        /// <remarks>All parameters are optional, leaving all parameters empty will return an unfiltered list 
        /// of all resources of the given Resource type</remarks>
        public Bundle Search<TResource>(SearchParam[] criteria = null, string sort = null, string[] includes = null, int? pageSize = null) where TResource : Resource, new()
        {
            return internalSearch(typeof(TResource).GetCollectionName(), criteria, sort, includes, pageSize);
        }

        /// <summary>
        /// Search for Resources of a certain type that match the given criteria
        /// </summary>
        /// <param name="resource">The type of resource to search for</param>
        /// <param name="criteria">Optional. The search parameters to filter the resources on</param>
        /// <param name="sort">Optional. The element to sort on</param>
        /// <param name="includes">Optional. A list of include paths</param>
        /// <param name="pageSize">Optional. Asks server to limit the number of entries per page returned</param>
        /// <typeparam name="TResource">The type of resource to list</typeparam>
        /// <returns>A Bundle with all resources found by the search, or an empty Bundle if none were found.</returns>
        /// <remarks>Except for 'resource', all parameters are optional, leaving all parameters empty will return an unfiltered 
        /// list of all resources of the given Resource type</remarks>
        public Bundle Search(string resource, SearchParam[] criteria = null, string sort = null, string[] includes = null, int? pageSize = null)
        {
            if (resource == null) throw Error.ArgumentNull("resource");

            return internalSearch(resource, criteria, sort, includes, pageSize);
        }

        public Bundle WholeSystemSearch(SearchParam[] criteria = null, string sort = null, string[] includes = null, int? pageSize = null)
        {
            return internalSearch(null, criteria, sort, includes, pageSize);
        }

        /// <summary>
        /// Search for resources based on a resource's id.
        /// </summary>
        /// <param name="id">The id of the resource to search for</param>
        /// <param name="includes">Zero or more include paths</param>
        /// <typeparam name="TResource">The type of resource to search for</typeparam>
        /// <returns>A Bundle with the BundleEntry as identified by the id parameter or an empty
        /// Bundle if the resource wasn't found.</returns>
        /// <remarks>This operation is similar to Read, but additionally,
        /// it is possible to specify include parameters to include resources in the bundle that the
        /// returned resource refers to.</remarks>
        public Bundle SearchById<TResource>(string id, string sort = null, string[] includes = null, int? pageSize = null) where TResource : Resource, new()
        {
            if (id == null) throw Error.ArgumentNull("id");

            return SearchById(typeof(TResource).GetCollectionName(), id, sort, includes, pageSize);
        }

        public Bundle SearchById(string resource, string id, string sort = null, string[] includes = null, int? pageSize = null)
        {
            if (resource == null) throw Error.ArgumentNull("resource");
            if (id == null) throw Error.ArgumentNull("id");

            return internalSearch(resource, new SearchParam[] { new SearchParam(HttpUtil.SEARCH_PARAM_ID, id) }, sort, includes, pageSize);
        }


        private Bundle internalSearch(string collection = null, SearchParam[] criteria = null, string sort = null, string[] includes = null, int? pageSize = null)
        {
            RestUrl url = null;

            if (collection != null)
                // Since there is confusion between using /resource/?param, /resource?param, use
                // the /resource/search?param instead
                url = new RestUrl(Endpoint).Search(collection);
            else
                url = new RestUrl(Endpoint);

            if (pageSize.HasValue)
                url.AddParam(HttpUtil.SEARCH_PARAM_COUNT, pageSize.Value.ToString());

            if (sort != null)
                url.AddParam(HttpUtil.SEARCH_PARAM_SORT, sort);

            if (criteria != null)
            {
                foreach (var criterium in criteria)
                    url.AddParam(criterium.QueryKey, criterium.QueryValue);
            }

            if (includes != null)
            {
                foreach (string includeParam in includes)
                    url.AddParam(HttpUtil.SEARCH_PARAM_INCLUDE, includeParam);
            }

            return fetchBundle(url.Uri);
        }


        /// <summary>
        /// Uses the FHIR paging mechanism to go navigate around a series of paged result Bundles
        /// </summary>
        /// <param name="current">The bundle as received from the last response</param>
        /// <param name="direction">Optional. Direction to browse to, default is the next page of results.</param>
        /// <returns>A bundle containing a new page of results based on the browse direction, or null if
        /// the server did not have more results in that direction.</returns>
        public Bundle Continue(Bundle current, PageDirection direction = PageDirection.Next)
        {
            if (current == null) throw Error.ArgumentNull("current");
            if (current.Links == null) return null;

            Uri continueAt = null;

            switch (direction)
            {
                case PageDirection.First:
                    continueAt = current.Links.FirstLink; break;
                case PageDirection.Previous:
                    continueAt = current.Links.PreviousLink; break;
                case PageDirection.Next:
                    continueAt = current.Links.NextLink; break;
                case PageDirection.Last:
                    continueAt = current.Links.LastLink; break;
            }

            if (continueAt != null)
                return fetchBundle(continueAt);
            else
                return null;
        }


        /// <summary>
        /// Send a set of creates, updates and deletes to the server to be processed in one transaction
        /// </summary>
        /// <param name="bundle">The bundled creates, updates and delted</param>
        /// <returns>A bundle as returned by the server after it has processed the transaction, or null
        /// if an error occurred.</returns>
        public Bundle Transaction(Bundle bundle)
        {
            if (bundle == null) throw new ArgumentNullException("bundle");

            var req = new FhirRequest(Endpoint, "POST");
            req.SetBody(bundle, PreferredFormat);
            return doRequest(req, HttpStatusCode.OK, resp => resp.BodyAsBundle());
        }


        /// <summary>
        /// Send a document bundle
        /// </summary>
        /// <param name="bundle">A bundle containing a Document</param>
        /// <remarks>The bundle must declare it is a Document, use Bundle.SetBundleType() to do so.</remarks>
        public void Document(Bundle bundle)
        {
            if (bundle == null) throw Error.ArgumentNull("bundle");
            if(bundle.GetBundleType() != BundleType.Document)
                throw Error.Argument("bundle", "The bundle passed to the Document endpoint needs to be a document (use SetBundleType to do so)");

            var url = new RestUrl(Endpoint).ToDocument();

            // Documents are merely "accepted"
            var req = new FhirRequest(url.Uri, "POST");
            req.SetBody(bundle, PreferredFormat);
            doRequest(req, HttpStatusCode.NoContent, resp => true );
        }


        /// <summary>
        /// Send a Document or Message bundle to a server's Mailbox
        /// </summary>
        /// <param name="bundle">The Document or Message be sent</param>
        /// <returns>A return message as a Bundle</returns>
        /// <remarks>The bundle must declare it is a Document or Message, use Bundle.SetBundleType() to do so.</remarks>       
        public Bundle DeliverToMailbox(Bundle bundle)
        {
            if (bundle == null) throw Error.ArgumentNull("bundle");
            if( bundle.GetBundleType() != BundleType.Document && bundle.GetBundleType() != BundleType.Message)
                throw Error.Argument("bundle", "The bundle passed to the Mailbox endpoint needs to be a document or message (use SetBundleType to do so)");

            var url = new RestUrl(_endpoint).ToMailbox();

            var req = new FhirRequest(url.Uri, "POST");
            req.SetBody(bundle, PreferredFormat);

            return doRequest(req, HttpStatusCode.OK, resp => resp.BodyAsBundle());
        }


        /// <summary>
        /// Get all tags known by the FHIR server
        /// </summary>
        /// <returns>A list of Tags</returns>
        public IEnumerable<Tag> WholeSystemTags()
        {
            return internalGetTags(null, null, null);
        }


        /// <summary>
        /// Get all tags known by the FHIR server for a given resource type
        /// </summary>
        /// <returns>A list of all Tags present on the server</returns>
        public IEnumerable<Tag> TypeTags<TResource>() where TResource : Resource, new()
        {
            return internalGetTags(typeof(TResource).GetCollectionName(), null, null);
        }


        /// <summary>
        /// Get all tags known by the FHIR server for a given resource type
        /// </summary>
        /// <returns>A list of Tags occuring for the given resource type</returns>
        public IEnumerable<Tag> TypeTags(string type)
        {
            if (type == null) throw Error.ArgumentNull("type");

            return internalGetTags(type, null, null);
        }


        /// <summary>
        /// Get the tags for a resource (or resource version) at a given location
        /// </summary>
        /// <param name="location">The url of the Resource to get the tags for. This can be a Resource id url or a version-specific
        /// Resource url.</param>
        /// <returns>A list of Tags for the resource instance</returns>
        public IEnumerable<Tag> Tags(Uri location)
        {
            if (location == null) throw Error.ArgumentNull("location");

            var collection = getCollectionFromLocation(location);
            var id = getIdFromLocation(location);
            var version = new ResourceIdentity(location).VersionId;

            return internalGetTags(collection, id, version);
        }


        private IEnumerable<Tag> internalGetTags(string collection, string id, string version)
        {
            RestUrl location = null;

            if(collection == null)
                location = location.ServerTags();
            else
            {
                if(id == null)
                    location = location.CollectionTags(collection);
                else
                    location = location.ResourceTags(collection,id,version);
            }

            var req = new FhirRequest(location.Uri, "GET");
            var result = doRequest(req, HttpStatusCode.OK, resp => resp.BodyAsTagList());
            return result.Category;
        }


        /// <summary>
        /// Add one or more tags to a resource at a given location
        /// </summary>
        /// <param name="location">The url of the Resource to affix the tags to. This can be a Resource id url or a version-specific
        /// <param name="tags"></param>
        /// <remarks>Affixing tags to a resource (or version of the resource) is not considered an update, so does not create a new version.</remarks>
        public void AffixTags(Uri location, IEnumerable<Tag> tags)
        {
            if (location == null) throw Error.ArgumentNull("location");
            if (tags == null) throw Error.ArgumentNull("tags");

            var collection = getCollectionFromLocation(location);
            var id = getIdFromLocation(location);
            var version = new ResourceIdentity(location).VersionId;

            var rl = new RestUrl(Endpoint).ResourceTags(collection, id, version);

            var req = new FhirRequest(rl.Uri,"POST");
            req.SetBody(new TagList(tags), PreferredFormat);
            
            doRequest(req, HttpStatusCode.OK, resp => true);
        }


        /// <summary>
        /// Remove one or more tags from a resource at a given location
        /// </summary>
        /// <param name="location">The url of the Resource to remove the tags from. This can be a Resource id url or a version-specific
        /// <param name="tags"></param>
        /// <remarks>Removing tags to a resource (or version of the resource) is not considered an update, so does not create a new version.</remarks>
        public void DeleteTags(Uri location, IEnumerable<Tag> tags)
        {
            if (location == null) throw Error.ArgumentNull("location");
            if (tags == null) throw Error.ArgumentNull("tags");

            var collection = getCollectionFromLocation(location);
            var id = getIdFromLocation(location);
            var version = new ResourceIdentity(location).VersionId;

            var rl = new RestUrl(Endpoint).DeleteResourceTags(collection, id, version);

            var req = new FhirRequest(rl.Uri, "POST");
            req.SetBody(new TagList(tags), PreferredFormat);

            doRequest(req, HttpStatusCode.OK, resp => true);
        }


        private T doRequest<T>(FhirRequest request, HttpStatusCode success, Func<FhirResponse,T> onSuccess)
        {
            return doRequest<T>(request, new HttpStatusCode[] { success }, onSuccess);
        }


        private T doRequest<T>(FhirRequest request, HttpStatusCode[] success, Func<FhirResponse,T> onSuccess)
        {
            request.UseFormatParameter = this.UseFormatParam;
            var response = request.GetResponse(PreferredFormat);

            if (success.Contains(response.Result))
                return onSuccess(response);
            else
            {
                // Try to parse the body as an OperationOutcome resource, but it is no
                // problem if it's something else, or there is no parseable body at all

                OperationOutcome outcome = null;

                try
                {
                    outcome = response.BodyAsEntry<OperationOutcome>().Resource;
                }
                catch
                {
                    // failed, so the body does not contain an OperationOutcome.
                    // Put the raw body as a message in the OperationOutcome as a fallback scenario
                    var body = response.BodyAsString();
                    if( !String.IsNullOrEmpty(body) )
                        outcome = OperationOutcome.ForMessage(body);
                }

                if (outcome != null)
                    throw new FhirOperationException("Operation failed with status code " + LastResponseDetails.Result, outcome);
                else
                    throw new FhirOperationException("Operation failed with status code " + LastResponseDetails.Result);
            }
        }      
    }

    public enum PageDirection
    {
        First,
        Previous,
        Next,
        Last
    }


}