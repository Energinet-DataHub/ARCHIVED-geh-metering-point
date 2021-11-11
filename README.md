# MeteringPoints

## Intro

The metering point domain is in charge of maintaining grid areas and creating the different metering point types in their respective grid areas, as well as maintaining the metering point state.
Furthermore the domain supports metering point master data updates not related to an energy supplier or consumer updates.

These are the processes maintained by this domain.

| Process                                                                      |
| ---------------------------------------------------------------------------- |
| [Create Metering Point](docs/business-processes/create-metering-point.md) |
| [Submission of master data – grid access provider](docs/business-processes/submission-of-master-data-grid-acess-provider.md)                |
| [Close down metering point](docs/business-processes/close-down-metering-point.md)                                               |
| [Connection of metering point with status new](docs/business-processes/connection-of-metering-point-with-status-new.md)                                             |
| [Disconnect or reconnect metering point](docs/business-processes/disconnect-or-reconnect-metering-point.md)                                                            |
| [Change of settlement method](docs/business-processes/change-of-settlement-method.md)                                                        |
| [Update production obligation](docs/business-processes/update-production-obligation.md)                                                              |
| [Request service from grid access provider](docs/business-processes/request-service-from-grid-access-provider.md)                             |
| ....                                                                         |

## Architecture

<img width="615" alt="MP domain Architecture" src="https://user-images.githubusercontent.com/25637982/117973312-87033780-b32c-11eb-9232-32c90cdb0fdb.PNG">

## Context Streams

<img width="412" alt="MP context stream" src="https://user-images.githubusercontent.com/25637982/114844794-6dc5a480-9ddb-11eb-9603-56d6c36a15af.PNG">

## Domain Roadmap

In current program increment we are working on the following>

* A market actor can create all metering point types
* A market actor can update metering point master data on all metering point types
* We can send messages to market actors through MessageHub
* We can send metering point master data updates to future and current energy supplier
* A market actor can cancel their request for create MP
* We publish all metering point created events

## Getting Started

[Read here how to get started](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/getting-started.md).

## Where can I get more help?

Please see the [community documentation](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/COMMUNITY.md)
