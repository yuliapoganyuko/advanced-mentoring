**Questions for the self-check:**







1\.	What are the cons and pros of the Monolith architectural style?

Pros:

* Easy to develop (*for small monolith*)
* Easy to test
* Simple deployment (single package)

Cons:

* Tight coupling and thus harder to update and test
* Low agility
* *Scaling is quite expensive as it requires duplicating the entire application even if only a few endpoints are the bottleneck.*
* Reduced readability in large systems and thus harder maintenance
* Challenging to adopt new technologies or make major changes
* Continuous delivery setup is complex
* One failure can affect the entire application
* *Deploying a large monolith can require significant time and effort.*
* *Deployment often leads to downtime (partial or complete unavailability).*
* *Small changes require redeploying the entire application, causing downtime.*
* *The size of the application can slow down the start-up time. Which increases time for development and deployment.*





2\.	What are the cons and pros of the Microservices architectural style?

Pros:

* Low coupling, thus easier maintenance
* Simple scaling
* Easier continuous delivery (independent deployments for each microservice)
* Higher agility (teams work in parallel)
* Greater reliability (one failure generally does not break the whole application)
* Flexibility to use different languages/technologies per service
* Services can be rewritten/updated independently

Cons:

* Needs a lot of planning upfront
* Higher costs of infrastructure and its maintenance 
* Distribution tax:
	* *Higher communication overhead due to inter-service interactions*
	* *Harder to monitor, debug, and analyze logs*
	* *Challenges in maintaining strong consistency (reliance on eventual consistency)*
	* *Increased communication complexity (e.g., orchestration vs. choreography)*
* Often data is duplicated between microservices, and thus it is needed to synchronize the updates
* Needs mechanisms to handle service failures without interrupting other microservices
* More complex testing compared to monoliths
* *Duplication of code for transversal concerns (logging, error handling, etc.)*





3\.	What is the difference between SOA and Microservices?

* ESB usage: SOA relies on an Enterprise Service Bus to manage service interactions; microservices communicate directly.
* Business logic risk: ESB is intended to be logic-free, but can get bloated with business logic in practice.
* Communication: SOA often uses SOAP; microservices typically use HTTP/REST.
* Agility: ESB “wiring” in SOA reduces agility compared to microservices.

*Scope: SOA has an enterprise scope; Microservices focus on application-level concerns.
Reuse: SOA emphasizes service reuse; Microservices accept code duplication for decoupling.
Data: SOA uses a shared data source; Microservices give each service its own database.
Communication: SOA often relies on synchronous communication (e.g., SOAP); Microservices prefer asynchronous methods (e.g., REST or messaging protocols).
Technology Stack: SOA uses SOAP and schemas (e.g., XSD); Microservices favor lightweight protocols like HTTP/REST.
Integration: SOA relies on an Enterprise Service Bus; Microservices communicate directly.
Deployment: SOA services often require dedicated providers; Microservices can run on any machine.
Resilience: SOA synchronous calls introduce dependencies; Microservices prioritize independence.
Specialization: SOA services can range from general-purpose to specialized; Microservices focus on high specialization.*





4\.	\[Open question] What does hybrid architectural style mean? Think of your current and previous projects and try to describe which architectural styles they most likely followed.

Hybrid architectural style applies principles from different architecture styles in a single project. For example, Service-based hybrid is similar to SOA but often with a shared database instead of separate ones for each service. Or Hierarchical hybrid - it enforces rules on which microservices can call others, creating restrictions.



In my current project, there is a following system:

* Main .NET web application

&nbsp;	Contains both SOAP and REST services.

&nbsp;	One main database.

* Separate service solution (reporting, etc.)

&nbsp;	Reuses some libraries from the main solution.

&nbsp;	Uses the same main database.

* C++ desktop and mobile applications

&nbsp;	They have their own local databases that synchronize periodically with the main database.



It is not a fully monolith as several apps run separately; it is also not a pure microservices architecture as there is one DB shared across different services, and also same libraries are reused. It resembles hybrid N-tier + SOA architecture with shared DB.





5\.	Name several examples of the distributed architectures. What do ACID and BASE terms mean.

Distributed system is composed of multiple independent parts that appear as a single unit from the outside. The example would be microservices or SOA architectures. 



ACID transaction is a transaction that is Atomic (succeeds or fails completely, never partially), Consistent (applies all necessary database constraints), Isolated (data cannot be read by other transactions until in the correct state) and Durable (once completed, the result remains consistent until the next update).



BASE transaction is “Basically Available” and follows the principle of eventual consistency, but does not guarantee ACID properties. It is based on the idea that, unless modified again, updated data will eventually become consistent across the entire distributed system.





6\.	Name several use cases where Serverless architecture would be beneficial.

Serverless architecture would be beneficial when there is a specific trigger and the action itself is short, stateless and doesn’t require constantly running server. For example, sending notification about a new file in the storage, or running scheduled clean-up.





