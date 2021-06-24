# Protocol Documentation

## Table of Contents

- [IntegrationEventContract.proto](#IntegrationEventContract.proto)
    - [MeteringMethodChanged](#.MeteringMethodChanged)
    - [MeteringPointCancelled](#.MeteringPointCancelled)
    - [MeteringPointClosed](#.MeteringPointClosed)
    - [MeteringPointConnected](#.MeteringPointConnected)
    - [MeteringPointCreated](#.MeteringPointCreated)
    - [MeteringPointDisconnected](#.MeteringPointDisconnected)
    - [NetSettlementGroupChanged](#.NetSettlementGroupChanged)
    - [ParentCoupled](#.ParentCoupled)
    - [ParentDecoupled](#.ParentDecoupled)
    - [SettlementDetailsChanged](#.SettlementDetailsChanged)

<a name="IntegrationEventContract.proto"></a>

## IntegrationEventContract.proto

Metering Point Domain related messages.

<a name=".MeteringMethodChanged"></a>

### MeteringMethodChanged

Represents the changing of the Metering Method.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier of the metering point which has its settlement details changed. |
| meteringMethod | [string](#string) |  | This indicates if the metering point is now physical, virtual, or calculated. |
| EffectiveDate | [string](#string) |  | The date on which the metering method is changed. |

<a name=".MeteringPointCancelled"></a>

### MeteringPointCancelled

Represents the cancellation of a metering point. This can only happen for MPs who have never had a supplier.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier. |

<a name=".MeteringPointClosed"></a>

### MeteringPointClosed

Represents the closing of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier. |
| EffectiveDate | [string](#string) |  | The date on which the metering point is closed. |

<a name=".MeteringPointConnected"></a>

### MeteringPointConnected

Represents the connection of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier. |
| EffectiveDate | [string](#string) |  | The date on which the metering point is connected. |

<a name=".MeteringPointCreated"></a>

### MeteringPointCreated

Represents the creation of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier. |
| meteringPointType | [string](#string) |  | Defines the type of metering point created. |
| meteringGridArea | [string](#string) |  | Indicates the metering grid area in which the metering point is created. |
| toGrid | [string](#string) |  | If an Exchange MP is created, this field indicates the outgoing grid. |
| fromGrid | [string](#string) |  | If an Exchange MP is created, this field indicates the incoming grid. |
| settlementMethod | [string](#string) |  | If a Consumption or Net loss correction metering point is created, this field indicates the settlement method. |
| meteringMethod | [string](#string) |  | This indicates if the metering point created is physical, virtual, or calculated. |
| meterReadingPeriodicity | [string](#string) |  | This indicates the reading periodicity. |
| connectionState | [string](#string) |  | This indicates the connection state upon creation. |
| netSettlementGroup | [string](#string) |  | This indicates the net settlement group. |
| product | [string](#string) |  | This indicates the energy product. |
| quantityUnit | [string](#string) |  | This indicates the quantity unit, eg. kWh. |
| parentMeteringPointGsrn | [string](#string) |  | Unique identifier of associated parent metering point. This field is empty if no parent is set upon creation. |
| EffectiveDate | [string](#string) |  | The date on which the metering point is created. |

<a name=".MeteringPointDisconnected"></a>

### MeteringPointDisconnected

Represents the disconnection of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier. |
| EffectiveDate | [string](#string) |  | The date on which the metering point is disconnected. |

<a name=".NetSettlementGroupChanged"></a>

### NetSettlementGroupChanged

Represents the changing of the Net Settlement Group.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier of the metering point which has its net settlement group changed. |
| netSettlementGroup | [string](#string) |  | This indicates if the new net settlement group. |
| EffectiveDate | [string](#string) |  | The date on which the net settlement group is changed. |

<a name=".ParentCoupled"></a>

### ParentCoupled

Represents the coupling of a parent to an eligible metering point.
If a parent is changed, ie. one is removed and another is coupled, both the coupling and the decoupling messages are sent.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier of the child metering point. |
| parentMeteringPointGsrn | [string](#string) |  | Unique identifier of associated parent metering point. |
| EffectiveDate | [string](#string) |  | The date on which the parent is coupled. |

<a name=".ParentDecoupled"></a>

### ParentDecoupled

Represents the decoupling of a parent to an eligible metering point.
If a parent is changed, ie. one is removed and another is coupled, both the coupling and the decoupling messages are sent.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier of the metering point which has the existing parent decoupled. |
| EffectiveDate | [string](#string) |  | The date on which the parent is decoupled. |

<a name=".SettlementDetailsChanged"></a>

### SettlementDetailsChanged

Represents the changing of settlement details. Either the settlement method or the reading periodicity will be changed, or both will be changed.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Gsrn | [string](#string) |  | Unique metering point identifier of the metering point which has its metering method changed. |
| settlementMethod | [string](#string) |  | The settlement method of the metering point. |
| meterReadingPeriodicity | [string](#string) |  | The reading periodicity of the metering point. |
| EffectiveDate | [string](#string) |  | The date on which the settlement details are change. |
