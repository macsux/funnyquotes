wget -nv https://dl.influxdata.com/telegraf/releases/telegraf-1.14.3-static_linux_amd64.tar.gz 2>&1
tar --extract --file=telegraf-1.14.3-static_linux_amd64.tar.gz --strip-components 2 ./telegraf/telegraf
./telegraf --config telegraf.conf