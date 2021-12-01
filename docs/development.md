# Getting Started as a Developer

These are the basic steps to set up your own environment for development.

1. Set up local test environment
2. Create database
3. Test by executing a business process
4. Set up infrastructure in Azure using Terraform
5. Publish Azure Functions

FAQ

1. Why do I need to install all that to run test locally?

## Set up local test environment

Setup third party tools as documented in the [prerequisites here](https://github.com/Energinet-DataHub/geh-core/blob/main/source/TestCommon/documents/functionapp-testcommon.md)

## Create database

Build and execute the command line tool `ApplyDBMigrationsApp` with connection string to your deployed database. The tool is in [this solution](../source/Energinet.DataHub.MeteringPoints.sln). Detailed instructions are [here](../source/Energinet.DataHub.MeteringPoints.ApplyDBMigrationsApp/README.md).

The connection string can be found in the configuration of the metering point functions in the Azure portal.

## Test by executing integration tests

Run integration tests as [documented here](https://github.com/Energinet-DataHub/green-force-art/blob/main/wiki/datahub/environments/integrationtest.md)

## Test by executing a business process

Ask for an invitation to our Postman Team. There's a workspace for metering point endpoints.

## Set up infrastructure in Azure using Terraform

TBD

## Publish Azure Functions

TBD

## FAQ

### Why do I need to install all that to run test locally?

To ensure that we all install the tools in the same way/location, we can assume that in our shared setup.
