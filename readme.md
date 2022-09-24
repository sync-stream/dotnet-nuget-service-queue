## SyncStream.Service.Queue
This project contains a collection of library services
for interacting with *RabbitMQ* with optional allowances
for alias messaging with *AWS S3*.

## Installation
```shell
dotnet add package 'SyncStream.Service.Queue'
```

## Alias Messaging with S3
A fancy way of saying that queue messages and their payloads
are stored on AWS S3 as a `json` file.  Once that file has
has been stored, the object key is then sent to RabbitMQ as
opposed to the entire payload.  S3 aliasing is implicitly
enabled by providing a `QueueSimpleStorageServiceConfiguration`
instance.

## Example

##### Queue Configuration C#
```csharp
QueueConfiguration configuration = new QueueConfiguration
{
  Name = "friendly-name",
  Endpoint = "name-in-rabbit-mq",
  Hostname = "192.168.1.1",
  Secure = false,
  
  // Include this to enable S3 aliasing
  SimpleStorageService = new QueueSimpleStorageServiceConfiguration
  {
    AccessKeyId = "<aws-access_key_id>",
    Bucket = "<bucket-name>",
    KmsKeyId = "<aws-kms-key-id>",
    Region = "us-east-1",
    SecretAccessKey = "<aws-secret_access_key>"
  },
  
  Password = "SuP3Rs3cR3tP4$5w0rd",
  Username =  "me"
};
```

##### Queue Configuration `appsettings.json`
```json lines
{
  "Queues": [
    {
      "Name": "friendly-name",
      "Endpoint": "name-in-rabbit-mq",
      "Hostname": "192.168.1.1",
      "Secure": false,
      "SimpleStorageService": {
        "AccessKeyId": "<aws-access_key_id>",
        "Bucket": "<bucket-name>",
        "KmsKeyId": "<aws-kms-key-id>",
        "Region": "us-east-1",
        "SecretAccessKey": "<aws-secret_access_key>"
      },
      "Password": "SuP3Rs3cR3tP4$5w0rd",
      "Username": "me"
    }
  ],
}
```
