**Questions for the self-check:**





1\. What is Message Based Architecture? What is the difference between Message Based Architecture and Event Based Architecture?
Message-based architecture enables asynchronous communication between distributed systems or applications through messages. It consists of three key components: a message publisher, a message consumer, and a queue for temporarily storing messages.



Event-based architecture can be viewed as a sub-type of message-based architecture, as it also uses messages for communication. The key distinction is that an event represents a notification that can have multiple subscribers, with the publisher not requiring an action in response, while in message-based architecture, the message is directed to a specific recipient, with the sender expecting the message to be processed by a consumer.







2\. What is Message Broker? How do message brokers work?

A message broker is middleware software that acts as an intermediary in distributed systems by receiving messages from producers and routing them to consumers. Advanced message brokers support additional configuration options such as message validation, duplicate detection, failed delivery handling and dead lettering. Message brokers store messages persistently and provide delivery guarantees based on their configuration.



How message brokers work: A message broker receives a message from a producer and places it into the appropriate queue or topic. The message is then routed to the correct consumer(s). Once the consumer processes and acknowledges the message, it is removed from the queue.







3\. When should you use message brokers?

Message brokers are appropriate for distributed systems where components need to communicate asynchronously without expecting immediate processing. This approach is beneficial for decoupling components, enabling independent scaling and load balancing, handling traffic spikes through buffering, allowing independent component development, and providing high reliability.







4\. Name and describe any distribution pattern.

Two most common patterns are point-to-point and publisher-subscriber.

Point-to-point: There is a 1-to-1 relationship between the publisher and consumer. Messages are sent through queues, and each message is consumed by exactly one consumer. An action or response is typically expected from the consumer.

Publisher-subscriber: There is a 1-to-many relationship between the publisher and consumers. Messages are broadcast through topics, and multiple subscribers can receive the same message. This pattern functions more as a notification mechanism, where action from consumers is not required.







5\. What are the advantages and disadvantages of using message broker?

Pros:

* decouples components
* enables independent scaling and load balancing
* handles traffic spikes through buffering
* allows independent component development
* provides high reliability
* supports failed delivery handling and dead lettering
* supports message validation
* supports duplicate detection

Cons:

* adds complexity
* requires additional infrastructure and its management and monitoring
* can cause communication latency







6\. What is the difference between Queue and Topic?

A queue is used for point-to-point communication where each message is delivered to a single consumer, while a topic is used for publish-subscribe communication where messages are broadcast to multiple subscribers.







7\. What are the typical failures in MBA? How can you address them? What is Saga pattern?

* failure to receive/deliver a message - can be addressed through message broker configuration (retry policies, delivery guarantees)
* duplicate messages - can be fixed using message broker duplicate detection or implementing specific consumer-side logic
* dual write problem occurs when a write action needs to be performed alongside message publishing in a transactional manner - can be fixed by using transactional outbox pattern, where message publishing is triggered by a database update



Saga pattern is used for managing distributed transactions across multiple distributed services through messaging. Instead of a single ACID transaction, the operation is broken down into multiple local transactions. In case of failure, affected components are notified through messaging so that the applied changes can be compensated or undone, enabling eventual consistency.

