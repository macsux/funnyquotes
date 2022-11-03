# -----------------------------------------------------------------------------
# Spring Config Server Windows Image
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/powershell:lts-nanoserver-1809 AS downloader
RUN pwsh -Command \
    Invoke-WebRequest  https://start.spring.io/starter.zip \
        -OutFile app.zip \
        -Body @{ \
            type='gradle-project'; \
            groupId='configserver.docker'; \
            artifactId='configserver'; \
            applicationName='ConfigServer'; \
            dependencies='cloud-config-server'}; \
    Expand-Archive app.zip c:/app ; 

FROM openjdk:11-nanoserver-1809 AS builder
COPY --from=downloader C:/app c:/app 
COPY ./src c:/app/src
ENV GRADLE_OPTS -Dorg.gradle.daemon=false
WORKDIR C:/app/src/main/java/configserver/docker/configserver
# a bug with WSL case sensitivity
RUN rename configserver.java ConfigServer.java 
WORKDIR c:/app
RUN .\gradlew bootJar
WORKDIR C:/app/build/libs/
RUN rename configserver-0.0.1-SNAPSHOT.jar configserver.jar

FROM openjdk:11.0.10-jre-nanoserver-1809
COPY --from=builder C:/app/build/libs/ c:/app 
EXPOSE 8888
WORKDIR c:/app
ENTRYPOINT ["java.exe", "-jar", "configserver.jar"]
