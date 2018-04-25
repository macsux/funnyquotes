cf create-service p-mysql 100mb mysql-FunnyQuotes
cf create-service p-service-registry standard eureka
cf create-service p-config-server standard config-server -c gitconfig.json
cf create-service p-circuit-breaker-dashboard standard hystrix