#!/bin/bash
/opt/mssql/bin/sqlservr &

echo "Waiting for SQL Server start..."

for i in {1..30}; do
    /opt/mssql-tools/bin/sqlcmd -S localhost -U "$DATABASE_USER" -P "$DATABASE_PASSWORD" -Q "SELECT 1" &>/dev/null
    if [ $? -eq 0 ]; then
        echo "SQL Server is up!"
        break
    fi
    sleep 2
done


DAC_FILE=$(find /Dacpac -maxdepth 1 -name "*.dacpac" | head -n 1)

if [ -f "$DAC_FILE" ]; then
    echo "Found Dacpac: $DAC_FILE"
    /opt/sqlpackage/sqlpackage \
        /Action:Publish \
        /SourceFile:"$DAC_FILE" \
        /TargetServerName:localhost \
        /TargetUser:"$DATABASE_USER" \
        /TargetPassword:"$DATABASE_PASSWORD" \
        /TargetDatabaseName:"$DATABASE_NAME" \
		/TargetTrustServerCertificate:true \
        /p:CreateNewDatabase=true
    echo "Dacpac applied."
else
    echo "Dacpac not Found!"
fi

wait