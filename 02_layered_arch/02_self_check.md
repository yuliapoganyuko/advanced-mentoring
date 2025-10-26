**Questions for the self-check:**





1. Name examples of the layered architecture. Do they differ or just extend each other?

Two main examples are layered and clean architecture, where application is divided into layers, each serving a specific role for separation of concerns (presentation, business logic, infrastructure, etc). 

In layered architecture, the flow is typically: presentation -> business logic -> infrastructure, and each layer only depends on the one below it. 

Clean architecture is an extended version of layered architecture, where the layers are "reversed" and the domain logic sits at the center independently of presentation or database. in this model, dependencies point inwards toward the domain.





2\. Is the below layered architecture correct and why? Is it possible from C to use B? from A to use C?

This layered architecture example is incorrect because there is a cyclic dependency between B and C. In proper layered architecture, C should not depend on B, as this violates the "dependency flows only downward" rule. Regarding A -> C, in an open layered architecture it is allowed to skip intermediate layers (in this case, B is skipped).





3\. Is DDD a type of layered architecture? What is Anemic model? Is it really an antipattern?

DDD is not necessarily layered architecture, it is more of an approach that helps to align the software model closely with the real-world business domain. However, DDD often adopts some type of layered architecture. 

Anemic model is a model that does not contain any business logic, only fields or properties to store the data. While it is generally considered an antipattern in DDD, it can be a reasonable choice in simple systems.





4\. What are architectural anti-patterns? Discuss at least three, think of any on your current or previous projects.

Architectural anti-patterns are common bad practices that introduce negative consequences.

On my current project, I have observed cargo cult programming, particularly in a couple of older applications written in C++. Since most of the developers did not write the original code and lack C++ experience, they apply solutions similar to already existing ones without fully understanding them. 

Also, there are some boat anchors. For example, after a major modernization, some old classes were kept for a transitional period on purpose. Even though it is considered an anti-pattern, it has been useful in situations when there are unexpected issues that require comparison with the old implementation.

On my previous legacy projects, lava flow was quite common. Because the application was used in the critical business processes for years, management was reluctant to approve refactorings that might introduce risk.





5\. What do Testability, Extensibility and Scalability NFRs mean. How would you ensure you reached them? Does Clean Architecture cover these NFRs?

Testability measures to what degree the software component can be effectively tested.

Extensibility indicates how easily a software component can be extended with new functionality, and how much effort is required to do so without breaking existing logic.

Scalability shows whether the software component can handle increased load by adding resources.

Clean architecture supports testability and extensibility due to inverted dependencies and isolated inner layer that defines interfaces. However, scalability is not inherently addressed by the clean architecture.

