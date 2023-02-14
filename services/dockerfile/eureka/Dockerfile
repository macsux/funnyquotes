# -----------------------------------------------------------------------------
# Netflix Eureka Server Windows Image
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/powershell:lts-nanoserver-1809 AS downloader
RUN pwsh -Command \
    Invoke-WebRequest  https://start.spring.io/starter.zip \
        -OutFile eureka.zip \
        -Body @{ \
            type='gradle-project'; \
            groupId='eurekaserver.docker'; \
            artifactId='eurekaserver'; \
            applicationName='EurekaServer'; \
            dependencies='cloud-eureka-server'}; \
    Expand-Archive eureka.zip c:/eureka ; 

FROM openjdk:11-nanoserver-1809 AS builder
COPY --from=downloader C:/eureka c:/eureka 
COPY ./src c:/eureka/src
ENV GRADLE_OPTS -Dorg.gradle.daemon=false
WORKDIR C:/eureka/src/main/java/eurekaserver/docker/eurekaserver
# a bug with WSL case sensitivity
RUN rename eurekaserver.java EurekaServer.java 
WORKDIR c:/eureka
RUN .\gradlew bootJar
WORKDIR C:/eureka/build/libs/
RUN rename eurekaserver-0.0.1-SNAPSHOT.jar eurekaserver.jar

FROM openjdk:11.0.10-jre-nanoserver-1809
COPY --from=builder C:/eureka/build/libs/ c:/eureka 
EXPOSE 8761
WORKDIR c:/eureka
ENTRYPOINT ["java.exe", "-jar", "eurekaserver.jar"]
