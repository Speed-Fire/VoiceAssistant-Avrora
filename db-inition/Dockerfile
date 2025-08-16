
FROM mcr.microsoft.com/mssql/server:2025-latest

# Копируем с предыдущего этапа .dacpac
COPY ["db-inition/VoiceAssistant.Server.Database.dacpac", "/Dacpac/VoiceAssistant.Server.Database.dacpac"]

USER root

# Установка sqlpackage для применения .dacpac
RUN apt-get update && \
    apt-get install -y unzip libunwind8 libicu70 curl && \
    curl -L https://aka.ms/sqlpackage-linux -o /sqlpackage.zip && \
    unzip /sqlpackage.zip -d /opt/sqlpackage && \
    chmod a+x /opt/sqlpackage/sqlpackage && \
    rm /sqlpackage.zip

# Копируем скрипт и делаем его исполняемым
COPY ["db-inition/init-db.sh", "/init-db.sh"]
RUN chmod +x /init-db.sh

# Запуск контейнера — поднимаем SQL Server и применяем .dacpac
ENTRYPOINT ["/bin/bash", "/init-db.sh"]