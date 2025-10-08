**Task 1**
Determine the architectural style of the solution. Provide an explanation for your choice.

It may be considered a client–server monolith with a shared database - even though there are multiple separate applications, they all connect directly to the same database. because of that, this design is not microservices or SOA, and there is no dedicated business logic layer, so it is not n‑tier.

**Task 2**
Draw the architectural diagram of the solution as if you were designing it from scratch. Provide pros and cons of the two solutions.

The main idea is to separate business logic from the presentation, and for that Application Layer service is added. That service is the main point that handles API requests from the other applications. If needed, DAL can be separated further into independent layer.
