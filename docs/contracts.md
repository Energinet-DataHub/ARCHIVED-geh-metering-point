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
| metering_point_id | string |  | Unique metering point identifier of the metering point which has its settlement details changed. |
| metering_method | string |  | This indicates if the metering point is now physical, virtual, or calculated. |
| effective_date | string |  | The date on which the metering method is changed. |

<a name=".MeteringPointCancelled"></a>

### MeteringPointCancelled

Represents the cancellation of a metering point. This can only happen for MPs who have never had a supplier.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier. |

<a name=".MeteringPointClosed"></a>

### MeteringPointClosed

Represents the closing of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier. |
| effective_date | string |  | The date on which the metering point is closed. |

<a name=".MeteringPointConnected"></a>

### MeteringPointConnected

Represents the connection of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier. |
| effective_date | string |  | The date on which the metering point is connected. |

<a name=".MeteringPointCreated"></a>

### MeteringPointCreated

Represents the creation of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier. |
| metering_point_type | string |  | Defines the type of metering point created. |
| metering_grid_area | string |  | Indicates the metering grid area in which the metering point is created. |
| to_grid | string |  | If an Exchange MP is created, this field indicates the outgoing grid. |
| from_grid | string |  | If an Exchange MP is created, this field indicates the incoming grid. |
| settlement_method | string |  | If a Consumption or Net loss correction metering point is created, this field indicates the settlement method. |
| metering_method | string |  | This indicates if the metering point created is physical, virtual, or calculated. |
| meter_reading_periodicity | string |  | This indicates the reading periodicity. |
| connection_state | string |  | This indicates the connection state upon creation. |
| net_settlement_group | string |  | This indicates the net settlement group. |
| product | string |  | This indicates the energy product. |
| quantity_unit | string |  | This indicates the quantity unit, eg. kWh. |
| parent_metering_point_id | string |  | Unique identifier of associated parent metering point. This field is empty if no parent is set upon creation. |
| effective_date | string |  | The date on which the metering point is created. |

<a name=".MeteringPointDisconnected"></a>

### MeteringPointDisconnected

Represents the disconnection of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier. |
| effective_date | string |  | The date on which the metering point is disconnected. |

<a name=".NetSettlementGroupChanged"></a>

### NetSettlementGroupChanged

Represents the changing of the Net Settlement Group.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier of the metering point which has its net settlement group changed. |
| net_settlement_group | string |  | This indicates if the new net settlement group. |
| effective_date | string |  | The date on which the net settlement group is changed. |

<a name=".ParentCoupled"></a>

### ParentCoupled

Represents the coupling of a parent to a eligible metering point.
If a parent is changed, ie. one is removed and another is coupled, both the coupling and the decoupling messages are sent.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier of the child metering point. |
| parent_metering_point_id | string |  | Unique identifier of associated parent metering point. |
| effective_date | string |  | The date on which the parent is coupled. |

<a name=".ParentDecoupled"></a>

### ParentDecoupled

Represents the decoupling of a parent to a eligible metering point.
If a parent is changed, ie. one is removed and another is coupled, both the coupling and the decoupling messages are sent.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier of the metering point which has the existing parent decoupled. |
| effective_date | string |  | The date on which the parent is decoupled. |

<a name=".SettlementDetailsChanged"></a>

### SettlementDetailsChanged

Represents the changing of settlement details. Either the settlement method or the reading periodicity will be changed, or both will be changed.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | string |  | Unique metering point identifier of the metering point which has its metering method changed. |
| settlement_method | string |  | The settlement method of the metering point. |
| meter_reading_periodicity | string |  | The reading periodicity of the metering point. |
| effective_date | string |  | The date on which the settlement details are change. |
