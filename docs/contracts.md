# Protocol Documentation

<a name="top"></a>

## Table of Contents

- [Metering Point domain contracts](#.Metering Point domain contracts)
    - [IntegrationEventEnvelope](#.IntegrationEventEnvelope)
    - [MeteringPointCancelledMessage](#.MeteringPointCancelledMessage)
    - [MeteringPointClosedMessage](#.MeteringPointClosedMessage)
    - [MeteringPointConnectedMessage](#.MeteringPointConnectedMessage)
    - [MeteringPointCreatedEventMessage](#.MeteringPointCreatedEventMessage)
    - [MeteringPointDisconnectedMessage](#.MeteringPointDisconnectedMessage)
    - [MeteringPointUpdatedMessage](#.MeteringPointUpdatedMessage)
    - [ParentCoupledMessage](#.ParentCoupledMessage)
    - [ParentDecoupledMessage](#.ParentDecoupledMessage)

<a name="source/Energinet.DataHub.MeteringPoints.Infrastructure/Integration/Contracts/IntegrationEventContract.proto"></a>
<p align="right"><a href="#top">Top</a></p>

## Metering Point domain contracts

Metering Point Domain related messages.

<a name=".MeteringPointCancelledMessage"></a>

### MeteringPointCancelledMessage

Represents the cancellation of a metering point. This can only happen for MPs who have never had a supplier.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | string |  | Unique metering point identifier. |

<a name=".MeteringPointClosedMessage"></a>

### MeteringPointClosedMessage

Represents the closing of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | string |  | Unique metering point identifier. |
| EffectiveDate | string |  | The date which the metering point is closed. |

<a name=".MeteringPointCreatedEventMessage"></a>

### MeteringPointCreatedEventMessage

Represents the creation of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | string |  | Unique metering point identifier. |
| meteringPointType | string |  | Defines the type of metering point created. |
| meteringGridArea | string |  | Indicates the metering grid area in which the metering point is created. |
| toGrid | string |  | If an Exchange MP is created, this field indicates the outgoing grid. |
| fromGrid | string |  | If an Exchange MP is created, this field indicates the incoming grid. |
| settlementMethod | string |  | If a Consumption or Net loss correction metering point is created, this field indicates the settlement method. |
| meteringMethod | string |  | This indicates if the metering point created is physical, virtual, or calculated. |
| meterReadingPeriodicity | string |  | This indicates the reading periodicity. |
| connectionState | string |  | This indicates the connection state upon creation. |
| netSettlementGroup | string |  | This indicates the net settlement group. |
| product | string |  | This indicates the energy product. |
| quantityUnit | string |  | This indicates the quantity unit, eg. kWh. |
| parentMeteringPointGsrn | string |  | Unique identifier of associated parent metering point. This field is empty if no parent is set upon creation. |
| EffectiveDate | string |  | The date which the metering point is created. |

<a name=".MeteringPointConnectedMessage"></a>

### MeteringPointConnectedMessage

Represents the connection of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | string |  | Unique metering point identifier. |
| EffectiveDate | string |  | The date which the metering point is connected. |

<a name=".MeteringPointDisconnectedMessage"></a>

### MeteringPointDisconnectedMessage

Represents the disconnection of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | string |  | Unique metering point identifier. |
| EffectiveDate | string |  | The date which the metering point is disconnected. |

<a name=".MeteringPointUpdatedMessage"></a>

### MeteringPointUpdatedMessage
Represents the update of a metering 
point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | string |  | Unique metering point identifier. |
| toGrid | string |  | This field indicates the outgoing grid. |
| fromGrid | string |  | This field indicates the incoming grid. |
| settlementMethod | string |  | This field indicates the settlement method. |
| meteringMethod | string |  | This indicates if the metering point is physical, virtual, or calculated. |
| meterReadingPeriodicity | string |  | This indicates the reading periodicity. |
| netSettlementGroup | string |  | This indicates the net settlement group. |
| product | string |  | This indicates the energy product. |
| quantityUnit | string |  | This indicates the quantity unit, eg. kWh. |
| EffectiveDate | string |  | The date which the metering point is updated. |

<a name=".ParentCoupledMessage"></a>

### ParentCoupledMessage

Represents the coupling of a parent to a eligible metering point.
If a parent is changed, ie. one is removed and another is coupled, both the coupling and the decoupling messages are sent.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | string |  | Unique metering point identifier of the child metering point. |
| parentMeteringPointGsrn | string |  | Unique identifier of associated parent metering point. |
| EffectiveDate | string |  | The date which the parent is coupled. |

<a name=".ParentDecoupledMessage"></a>

### ParentDecoupledMessage

Represents the decoupling of a parent to a eligible metering point.
If a parent is changed, ie. one is removed and another is coupled, both the coupling and the decoupling messages are sent.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | string |  | Unique metering point identifier og the metering point which has the existing parent decoupled. |
| EffectiveDate | string |  | The date which the parent is decoupled. |
