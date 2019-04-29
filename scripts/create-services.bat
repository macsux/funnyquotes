cf create-service p.mysql db-small mysql-funnyquotes
cf create-service p-service-registry standard eureka
cf create-service p-config-server standard config-server -c gitconfig.json
cf create-service p-circuit-breaker-dashboard standard hystrix
rem cf create-service p-identity devsso sso