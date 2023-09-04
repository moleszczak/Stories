# Stories

## Description

Stories is a service that allow cooperation with service https://hacker-news.firebaseio.com/v0/.
You can easily get N best ranked stories with detail information about them.

## Instalation

Clone code from repository into local environment.
Go to local folder where code is cloned and execute commands below.

```dotnet run --project Stories```

Service shuld start and start responding to request.

## Usage

To get list of N best stories open command line tool and execute command below replaing N with your own number.
Number must be selected in range 1 to 200.

```curl -X GET https://localhost:7124/Stories/N -H 'accept: text/plain'```

## Roadmap

Authentication to allow only authenticated users access service.
Better error handling
 - dedicated handler for timeouts or too many requests response from https://hacker-news.firebaseio.com/v0/
