# Log Api

This is a very simple web api that displays the incomming requests. I recently needed something like this so I can debug my NodeMcu microcontroller. I wanted to see what I obtained and if I am able to send it somewhere.

![](assets/LogApi.gif)

## Installation 

The easiest way to use Log Api is with Docker.

### Docker:

1. Run:
```
docker pull hasanhasanov/log-api:latest
docker run -p 8080:80 hasanhasanov/log-api:latest
```
2. Open a browser and navigate to: http://localhost:8080/requests.html

### Dotnet

1. Clone or download the project
2. Build using Release configuration
3. Open cmd and navigate to LogApi\src\LogApi\LogApi\bin\Release\net5.0
4. Run:

```
dotnet LogApi.dll
```
5. Open a browser and navigate to: http://localhost:5000/requests.html

## Usage example

You can make any request to the port that you setup the project. It will collect information about the request and you can then see this information realtime by navigating to the /requests.html page.

The web api also has a background job which by default triggers every 10 mins. This job cleans the oldest requests in order to prevent ever growing memory. You are free to change this trigger time using the configs.

The application also has a session. It will disconnect the user after 30 mins. You can change that value too.

### Configs

You can override any of the configs by changing them in appSettings.json or through environment variables.

| Parameter  | Default Value | Description |
| ------------- | ------------- |------------- |
| SocketAliveMinutes  | 30  | How long you will remain connected to the socket.  |
| RequestCleanerInMinutes  |  10  | How often should the cleaner run. |
| MaximumRequestsToKeep  | 1000 | How many requests to keep after the cleaner. |

## Meta

* Hasan Hasanov – [@hmhasanov](https://twitter.com/hmhasanov)
* Blog - [Hasan Hasanov](https://hasan-hasanov.com/)

## Contributing

1. Fork it (<https://github.com/yourname/yourproject/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request
