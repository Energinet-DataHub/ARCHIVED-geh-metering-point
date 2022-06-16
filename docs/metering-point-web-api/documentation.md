# WebApi

The WebApi is a way to request data directly from the Metering Point domain. This is currently only used as a way to
serve data for the frontend BFF (Backend For Frontend) which acts as a proxy for any backend requests the frontend has
to perform.

## MeteringPoint Client

The MeteringPoint Client (Libraries/Energinet.DataHub.MeteringPoints.Client) is used to expose the WebApi endpoints to
the BFF.

See [Metering Point Client documentation](../metering-point-client/documentation.md) for further details.

## Backend For Frontend

The BFF can be found the [*Greenforce Frontend* repo](https://github.com/Energinet-DataHub/greenforce-frontend). At the
time of writing the solution file can be located under *apps/dh/api-dh*.

Anything that might impact the frontend itself (I.e. enums that need to be translated) requires the frontend itself to
be updated as well.
