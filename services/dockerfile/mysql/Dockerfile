# -----------------------------------------------------------------------------
# MySQL Windows Image
# -----------------------------------------------------------------------------

FROM mcr.microsoft.com/powershell:lts-nanoserver-1809 AS builder
ENV IMAGE_VERSION 5.7.24
ADD https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-${IMAGE_VERSION}-winx64.zip mysql.zip
RUN pwsh.exe -Command \
    Expand-Archive mysql.zip scratch ; \
    Remove-Item -Force mysql.zip  ; \
    Move-Item scratch\* C:\MySQL

FROM mcr.microsoft.com/windows/nanoserver:1809
COPY --from=builder C:/MySQL c:/MySQL
RUN C:\MySQL\bin\mysqld.exe --initialize-insecure --console --explicit_defaults_for_timestamp
EXPOSE 3306
ENTRYPOINT ["C:\\MySQL\\bin\\mysqld.exe", "--console"]
