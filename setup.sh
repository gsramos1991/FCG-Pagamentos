# Espere até que o SQL Server esteja pronto para aceitar conexões
/opt/mssql/bin/sqlservr &
PID=$!
sleep 30 # Dá tempo para o serviço subir

# Use 'sqlcmd' para rodar comandos SQL
/opt/mssql/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q 

# Mate o processo do SQL Server para que o CMD principal do Dockerfile assuma.
kill $PID
wait $PID