$keyvault1 = 'hello-aue-kv'
$keyvault2 = 'hello-ase-kv'
$key = 'key1'
$plainText = 'The quick brown fox'

# Create a Key
az keyvault key create --vault-name $keyvault1 --name $key --kty 'RSA' --size 2048

# Encrypt plaintext in Primary Vault
$cipherText = ( az keyvault key encrypt --vault-name $keyvault1 --name $key --algorithm 'RSA-OAEP' --value $plainText --data-type 'plaintext' | ConvertFrom-Json).result

# Backup the Key
az keyvault key backup --vault-name $keyvault1 --name $key --file "./$($key).bak"

# Restore the Key to Secondary Vault
az keyvault key restore --vault-name $keyvault2 --file "./$($key).bak"

# Decrypt ciphertext
$plainText2 = ( az keyvault key decrypt --vault-name $keyvault1 --name $key --algorithm 'RSA-OAEP' --value $cipherText --data-type 'plaintext' | ConvertFrom-Json).result

if($plainText -eq $plainText2) { Write-Host "$plainText = $plainText2" -ForegroundColor Green } 
else { Write-Host "$plainText != $plainText2" -ForegroundColor Red }
