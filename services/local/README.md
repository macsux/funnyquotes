A collection of services to setup up local development environment via docker-compose. 

#### How to use

##### Start

```
docker-compose up -d <SERVICENAME...>
```

*You can specify more then one service name. Omit to start everything*

##### Stop

```
docker-compose down
```

#### Included services:

- `config-server` - [Spring Cloud Config Server](https://cloud.spring.io/spring-cloud-config) 
  - http://localhost:8888
  - Looks for config files inside `../config` directory
- `eureka` - [Spring Cloud Eureka](https://spring.io/projects/spring-cloud-netflix) 
  - http://localhost:8761
- `hystrix` - [Spring Cloud Circuit Breaker Dashboard](https://cloud.spring.io/spring-cloud-static/Edgware.SR6/multi/multi__circuit_breaker_hystrix_dashboard.html) 
  - http://localhost:7979
- `rabbitmq` - [RabbitMQ](https://www.rabbitmq.com/)
  - Server: localhost:5672
  - Credentials: guest/guest
  - Management UI: http://localhost:8084
- `eventstore` - [EventStore](https://eventstore.org/)
  - Server: localhost:1113
  - Credentials: admin/changeit
  - UI: http://localhost:2113
- `mysql` - MySQL
  - Server: localhost:3306
  - Credentials: root / (blank)
- `phpmyadmin` - [PhpMyAdmin](https://www.phpmyadmin.net/), UI to interact with `mysql` container
  - UI: http://localhost:8083
- `redis` - Redis
  - localhost:6379
- `redis-commander` - [Redis Commander](https://github.com/joeferner/redis-commander) - UI for redis
  - http://localhost:8081
- `zipkin` - Zipkin - distributed tracing
  - http://localhost:9411
- `mongo` - MongoDB
  - Server: localhost:27017
  - Credentials: admin/admin
- `mongo-express` - [Mongo Express](https://github.com/mongo-express/mongo-express) (web based GUI)
  - http://localhost:8082
- `omnidb` - OmniDB - multi-db web based GUI (postgres, mysql)
  - http://localhost:8085
  - Credentials: admin/admin
  - Note: when connecting to servers, use container service name not localhost
- `postgres` - PostgreSQL
  - Server: localhost:5432
  - Credentials: admin/admin
- `concourse` - ConcourseCI
  - Server: http://localhost:8087
  - Credentials: test/test