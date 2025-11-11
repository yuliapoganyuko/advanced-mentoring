**Questions for the self-check:**





1\. Explain the difference between terms: REST and RESTful. What are the six constraints?
REST is an architectural style, RESTful is API that applies REST constraints.

* Uniform interface: the API must expose a consistent, well-defined contract and enforce it
* Client–server: client and server can evolve independently as long as they respect the agreed interface
* Stateless: the server keeps no session context between requests; the client is responsible for managing state
* Cacheable: responses should indicate whether they can be cached, improving performance when appropriate
* Layered system: the architecture can be composed of layers without affecting clients
* Code on demand: optionally, the API may return executable code to extend client functionality







2\. HTTP Request Methods (the difference) and HTTP Response codes. What is idempotency? Is HTTP the only protocol supported by the REST?

* Get - gets the resource without modifying it
* Post - creates a new resource
* Put - updates existing resource
* Delete - deletes resource



HTTP Response codes are three-digit statuses returned with the response that indicate if the request was performed successfully or not. Codes are combined by categories: 1\*\* - info, 2\*\* - success, 3\*\* - redirection, 4\*\* - client-side error, 5\*\* - server error.



Idempotency means that repeatedly making the same request with the same parameters has the same result. Get, Put and Delete are considered idempotent.



In theory, REST can be implemented over protocols other than HTTP. However, in practice RESTful services almost always use HTTP.







3\. What are the advantages of statelessness in RESTful services?

* All data needed for each request comes from the client, so there is no dependence on stored session state
* Scaling out is easier as any server can handle any request at any time
* Responses are easier to cache as they aren’t tied to a specific session
* API is less complex as there is no need in server-side state synchronization







4\. How can caching be organized in RESTful services?

GET responses are cacheable by default; POST responses can be cacheable, but only if they explicitly include a Expires header or a Cache-Control header; PUT and DELETE are always non-cacheable. Any cacheable response should provide a validator - typically an ETag or Last-Modified header, so conditional requests can be performed to ensure that the data is fresh. Cache can live in the client, intermediate proxies, or CDNs.







5\. How can versioning be organized in RESTful services?

It’s generally recommended that breaking changes (changes to request or response formats or removing endpoints) should be introduced in a new major version.

Version are exposed to clients via the URI path, a query parameter, or headers (Accept or custom). On the backend, the mechanism should be implemented to route different versions to corresponding handlers.







6\. What are the best practices of resource naming?

* Use nouns for resource URIs and avoid verbs in the path
* Name collection resources with plural nouns and individual document resources with singular nouns
* Use nested collections only when it is appropriate
* Lowercase is preferred
* For multi-word names, use hyphens instead of underscores
* Don't over-nest the resources
* Don't use trailing slash in URI
* Use query parameters for filtering and sorting







7\. What are OpenAPI and Swagger? What implementations/libraries for .NET exist? When would you prefer to generate API docs automatically and when manually?

OpenAPI is standard for documenting HTTP/REST APIs: it describes available endpoints, operations, inputs/outputs and metadata in a OpenAPI schema format, and it is typically published as JSON. Swagger is a tooling that implements the OpenAPI specification; however, the terms they are often used interchangeably.



For .NET, common implementations are Swashbuckle and NSwag, Microsoft OpenAPI library is also available for lower-level use cases.



Generating API docs automatically is preferable for big APIs or those updated often. However, if additional details need to be included, documentation may be updated manually.







8\. What is OData? When will you choose to follow it and when not?

OData is a protocol for building and consuming HTTP APIs, and it has 5 key pillars: standardised data access, resource-centric approach, rich querying, published metadata and schema, interoperability. It is preferable to follow it if any of the aspects described above are important for the API. However, it may be unnecessary if there is a highly custom behaviour, non data-centric operations, non-structured data/streaming involved, or querying needs to be more particular.







9\. What is Richardson Maturity Model? Is it always a good idea to reach the 3rd level of maturity?

The Richardson Maturity Model describes how RESTful services evolve through three levels:

1st: Resources: exposing distinct resources, each with its own URI

2nd: HTTP verbs and status codes: using appropriate HTTP method for every action, returning meaningful status codes

3rd: Hypermedia controls: making API self-discoverable by including relevant links in responses

Even though level 3 provides the complete REST benefits, sometimes it isn’t worth the added complexity; level 2 often offers a good balance.







10\. What does pros and cons REST have in comparison with other web API types?

Pros of REST:

* Simple due to being built on HTTP
* Supported by many tools
* Easy caching, thus better performance
* Requests are stateless, so it's easier to scale horizontally



Cons of REST:

* Under- or over-fetching, which means that client cannot choose what fields to return
* Often many requests need to be performed to get related data
* When API is about commands and not resources, the resource-centric style can seem forced



