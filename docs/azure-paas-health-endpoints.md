# List of Azure PaaS health endpoints

I collect undocumented Azure PaaS health endpoints.

| Azure service | Endpoint(s) | FQ URL | Example | Remarks |
| ------------- | ----------- | ------ | ------- | ------- |
| Azure Key Vault | `/healthstatus` | `https://(Key Vault Name).vault.azure.net/healthstatus` | <https://hello-aue-kv.vault.azure.net/healthstatus> | |
| Azure API Management | `/status-0123456789abcdef`<br/>`/internal-status-0123456789abcdef` | `https://(APIM Name).azure-api.net/status-0123456789abcdef`<br/>`https://(APIM Name).azure-api.net/internal-status-0123456789abcdef` | <https://hello-apim.azure-api.net/status-0123456789abcdef><br/><https://hello-apim.azure-api.net/internal-status-0123456789abcdef> | |
