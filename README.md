# THPS.API

This is a mostly standard .NET Core 3.1 project, so I won't go into how to build it specifically, but instead mention the project specific things

## Mongo DB
The QScript db must be configured and accessible from the project. This is needed for script key resolving, and also save file serialization.

The database name is currently hardcoded to "QScript".

## Generating an API Key

### HTTP Auth Private Key
This key can be of any length

`
openssl genrsa -out http_auth_key.pem 2048
`

Take the base64 string, and make it into 1 line. That string is value to set APIKeyPrivateKey to.

### Generating an HTTP Auth Key for requests

This next part needs improvement, but basically comment out the Authorize attribute in the APIKeyController, and make a request to GenerateKey to make an admin key. This is for your first key, any key made after will not require this, so its easier to just do it locally.

`
curl -X POST "http://localhost:5000/api/APIKey/GenerateKey" -H  "accept: text/plain" -H  "Content-Type: application/json-patch+json" -d "{  \"name\": \"dev test key\",  \"roles\": [    \"Admin\"  ]}"