# MeteringPoints

<!---[![codecov](https://codecov.io/gh/Energinet-DataHub/geh-metering-point/branch/main/graph/badge.svg?token=XR3CF7GC90)](https://codecov.io/gh/Energinet-DataHub/geh-metering-point)--->

## Intro

The metering point domain is in charge of maintaining grid areas and creating the different metering point types in their respective grid areas, as well as maintaining the metering point state.
Furthermore the domain supports metering point master data updates not related to an energy supplier or consumer updates.

## Getting Started

Learn how to get started with Green Energy Hub [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/getting-started.md).

Learn how to get a development environment up and running for development, debugging, testing or evaluation purposes [here](docs/development.md).

## Processes

These are the processes maintained by this domain.

| Process                                                                      |
| ---------------------------------------------------------------------------- |
| [Create Metering Point](docs/business-processes/create-metering-point.md) |
| [Submission of master data – grid access provider](docs/business-processes/submission-of-master-data-grid-acess-provider.md)                |
| [Close down metering point](docs/business-processes/close-down-metering-point.md)                                               |
| [Connection of metering point with status new](docs/business-processes/connection-of-metering-point-with-status-new.md)                                             |
| [Disconnect or reconnect metering point](docs/business-processes/disconnect-or-reconnect-metering-point.md)  
| [Change of settlement method](docs/business-processes/change-of-settlement-method.md)                                                        |
| [Update production obligation](docs/business-processes/update-production-obligation.md)                                                              |
| [Request service from grid access provider](docs/business-processes/request-service-from-grid-access-provider.md)                             |
| ....                                                                         |

## Architecture


![MP domain Architecture]![MP arkitektur](https://user-images.githubusercontent.com/72008816/157625877-aa97547c-3dd5-41de-b71f-751308328054.PNG)


## Context Streams

![MP context stream]![MP context stream](https://user-images.githubusercontent.com/72008816/157625911-28450684-d8ec-4526-b7f7-010c78f4aea4.PNG)


## Domain Roadmap

* A market actor can create all metering point types - Done
* A market actor can update metering point master data on all metering point types - Done
* A market actor can connect all metering point types - Done
* A market actor can disconnect and reconnect metering points - Done
* We can send messages to market actors through MessageHub - Done
* We publish all metering point created events
* A market actor can close down a metering point - In progress
* A Market actor can access a GUI and search for metering points they own - Done
* A market actor can access a GUI and view the status of their processes - In progress

## Where can I get more help?

Please see the [community documentation](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/COMMUNITY.md)
