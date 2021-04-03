# Log Api

This is a very simple web api that displays the incomming requests. I recently needed something like this so I can debug my NodeMCU microcontroller. I wanted to see what I obtain and if I am able to send it somewhere.

![](header.png)

## Installation 

The easiest way to use Log Api is using Docker.

### Docker:

```
Docker pull goes here
```

### Dotnet

```
Dotnet command goes here
```

## Usage example

You can make any request to the port that you setup the project. It will collect information about the request. You can then see this information realtime by navigating to the /requests.html page.

### Configs

You can override any of the configs by changing them in appSettings.json or through environment variables.

| Parameter  | Default Value | Description |
| ------------- | ------------- |------------- |
| SocketAliveMinutes  | 10  | How long you will remain connected to the socket.  |
| RequestCleanerInMinutes  |  10  | How often should the cleaner run. |
| MaximumRequestsToKeep  | 1000 | How many requests to keep after the cleaner. |

## Meta

Hasan Hasanov – [@hmhasanov](https://twitter.com/hmhasanov)
Blog - [Hasan Hasanov](https://hasan-hasanov.com/)

## Contributing

1. Fork it (<https://github.com/yourname/yourproject/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request
