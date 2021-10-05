# Protocol Documentation

<a name="top"></a>

## Table of Contents

- [Contracts/IntegrationEventContract.proto](#Contracts/IntegrationEventContract.proto)
    - [IntegrationEventEnvelope](#.IntegrationEventEnvelope)
    - [MeteringMethodChanged](#.MeteringMethodChanged)
    - [MeteringPointCancelled](#.MeteringPointCancelled)
    - [MeteringPointClosed](#.MeteringPointClosed)
    - [MeteringPointDisconnected](#.MeteringPointDisconnected)
    - [NetSettlementGroupChanged](#.NetSettlementGroupChanged)
    - [ParentCoupled](#.ParentCoupled)
    - [ParentDecoupled](#.ParentDecoupled)
    - [SettlementDetailsChanged](#.SettlementDetailsChanged)
  
- [IntegrationEvents/Connect/MeteringPointConnectedIntegrationEvent.proto](#IntegrationEvents/Connect/MeteringPointConnectedIntegrationEvent.proto)
    - [MeteringPointConnected](#.MeteringPointConnected)
  
- [IntegrationEvents/CreateMeteringPoint/Consumption/ConsumptionMeteringPointCreated.proto](#IntegrationEvents/CreateMeteringPoint/Consumption/ConsumptionMeteringPointCreated.proto)
    - [ConsumptionMeteringPointCreated](#.ConsumptionMeteringPointCreated)
  
    - [ConsumptionMeteringPointCreated.ConnectionState](#.ConsumptionMeteringPointCreated.ConnectionState)
    - [ConsumptionMeteringPointCreated.MeterReadingPeriodicity](#.ConsumptionMeteringPointCreated.MeterReadingPeriodicity)
    - [ConsumptionMeteringPointCreated.MeteringMethod](#.ConsumptionMeteringPointCreated.MeteringMethod)
    - [ConsumptionMeteringPointCreated.NetSettlementGroup](#.ConsumptionMeteringPointCreated.NetSettlementGroup)
    - [ConsumptionMeteringPointCreated.ProductType](#.ConsumptionMeteringPointCreated.ProductType)
    - [ConsumptionMeteringPointCreated.SettlementMethod](#.ConsumptionMeteringPointCreated.SettlementMethod)
    - [ConsumptionMeteringPointCreated.UnitType](#.ConsumptionMeteringPointCreated.UnitType)
  
- [IntegrationEvents/CreateMeteringPoint/Production/ProductionMeteringPointCreated.proto](#IntegrationEvents/CreateMeteringPoint/Production/ProductionMeteringPointCreated.proto)
    - [ProductionMeteringPointCreated](#.ProductionMeteringPointCreated)
  
    - [ProductionMeteringPointCreated.ConnectionState](#.ProductionMeteringPointCreated.ConnectionState)
    - [ProductionMeteringPointCreated.MeterReadingPeriodicity](#.ProductionMeteringPointCreated.MeterReadingPeriodicity)
    - [ProductionMeteringPointCreated.MeteringMethod](#.ProductionMeteringPointCreated.MeteringMethod)
    - [ProductionMeteringPointCreated.NetSettlementGroup](#.ProductionMeteringPointCreated.NetSettlementGroup)
    - [ProductionMeteringPointCreated.ProductType](#.ProductionMeteringPointCreated.ProductType)
    - [ProductionMeteringPointCreated.UnitType](#.ProductionMeteringPointCreated.UnitType)
  
- [IntegrationEvents/CreateMeteringPoint/Exchange/ExchangeMeteringPointCreated.proto](#IntegrationEvents/CreateMeteringPoint/Exchange/ExchangeMeteringPointCreated.proto)
    - [ExchangeMeteringPointCreated](#.ExchangeMeteringPointCreated)
  
    - [ExchangeMeteringPointCreated.ConnectionState](#.ExchangeMeteringPointCreated.ConnectionState)
    - [ExchangeMeteringPointCreated.MeterReadingPeriodicity](#.ExchangeMeteringPointCreated.MeterReadingPeriodicity)
    - [ExchangeMeteringPointCreated.MeteringMethod](#.ExchangeMeteringPointCreated.MeteringMethod)
    - [ExchangeMeteringPointCreated.ProductType](#.ExchangeMeteringPointCreated.ProductType)
    - [ExchangeMeteringPointCreated.UnitType](#.ExchangeMeteringPointCreated.UnitType)
  
- [Scalar Value Types](#scalar-value-types)

<a name="Contracts/IntegrationEventContract.proto"></a>
<p align="right"><a href="#top">Top</a></p>

## Contracts/IntegrationEventContract.proto

Metering Point Domain related messages.

<a name=".IntegrationEventEnvelope"></a>

### IntegrationEventEnvelope

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| meteringPointCreatedMessage | [MeteringPointCreated](#MeteringPointCreated) |  |  |
| meteringPointDisconnectedMessage | [MeteringPointDisconnected](#MeteringPointDisconnected) |  |  |
| meteringPointClosedMessage | [MeteringPointClosed](#MeteringPointClosed) |  |  |
| meteringPointCancelledMessage | [MeteringPointCancelled](#MeteringPointCancelled) |  |  |
| parentCoupledMessage | [ParentCoupled](#ParentCoupled) |  |  |
| parentDecoupledMessage | [ParentDecoupled](#ParentDecoupled) |  |  |
| settlementDetailsChangedMessage | [SettlementDetailsChanged](#SettlementDetailsChanged) |  |  |
| meteringMethodChangedMessage | [MeteringMethodChanged](#MeteringMethodChanged) |  |  |
| netSettlementGroupChangedMessage | [NetSettlementGroupChanged](#NetSettlementGroupChanged) |  |  |

<a name=".MeteringMethodChanged"></a>

### MeteringMethodChanged (Preliminary)

Represents the changing of the Metering Method.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique metering point identifier of the metering point which has its settlement details changed. |
| gsrn_number | [string](#string) |  | Business facing metering point identifier. |
| metering_method | [string](#string) |  | This indicates if the metering point is now physical, virtual, or calculated. |
| effective_date | [string](#string) |  | The date on which the metering method is changed. |

<a name=".MeteringPointCancelled"></a>

### MeteringPointCancelled (Preliminary)

Represents the cancellation of a metering point. This can only happen for MPs who have never had a supplier.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique metering point identifier. |
| gsrn_number | [string](#string) |  | Business facing metering point identifier. |

<a name=".MeteringPointClosed"></a>

### MeteringPointClosed (Preliminary)

Represents the closing of a metering point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique metering point identifier. |
| gsrn_number | [string](#string) |  | Business facing metering point identifier. |
| effective_date | [string](#string) |  | The date on which the metering point is closed. |

<a name=".MeteringPointDisconnected"></a>

### MeteringPointDisconnected (Preliminary)
Represents the disconnection of a metering point.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique metering point identifier. |
| gsrn_number | [string](#string) |  | Business facing metering point identifier. |
| effective_date | [string](#string) |  | The date on which the metering point is disconnected. |

<a name=".NetSettlementGroupChanged"></a>

### NetSettlementGroupChanged (Preliminary)

Represents the changing of the Net Settlement Group.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique metering point identifier of the metering point which has its net settlement group changed. |
| gsrn_number | [string](#string) |  | Business facing metering point identifier. |
| net_settlement_group | [string](#string) |  | This indicates if the new net settlement group. |
| effective_date | [string](#string) |  | The date on which the net settlement group is changed. |

<a name=".ParentCoupled"></a>

### ParentCoupled (Preliminary)

Represents the coupling of a parent to a eligible metering point.
If a parent is changed, ie. one is removed and another is coupled, both the coupling and the decoupling messages are sent.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique metering point identifier of the child metering point. |
| child_gsrn_number | [string](#string) |  | Business facing metering point identifier of child metering point. |
| parent_metering_point_id | [string](#string) |  | Unique identifier of associated parent metering point. |
| parent?gsrn_number | [string](#string) |  | Business facing metering point identifier of coupled parent metering point. |
| effective_date | [string](#string) |  | The date on which the parent is coupled. |

<a name=".ParentDecoupled"></a>

### ParentDecoupled (Preliminary)

Represents the decoupling of a parent to a eligible metering point.
If a parent is changed, ie. one is removed and another is coupled, both the coupling and the decoupling messages are sent.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique metering point identifier of the child metering point which has the existing parent decoupled. |
| gsrn_number | [string](#string) |  | Business facing metering point identifier of decoupled child. |
| effective_date | [string](#string) |  | The date on which the parent is decoupled. |

<a name=".SettlementDetailsChanged"></a>

### SettlementDetailsChanged (Preliminary)

Represents the changing of settlement details. Either the settlement method or the reading periodicity will be changed, or both will be changed.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique metering point identifier of the metering point which has its metering method changed. |
| gsrn_number | [string](#string) |  | Business facing metering point identifier. |
| settlement_method | [string](#string) |  | The settlement method of the metering point. |
| meter_reading_periodicity | [string](#string) |  | The reading periodicity of the metering point. |
| effective_date | [string](#string) |  | The date on which the settlement details are change. |

<a name="IntegrationEvents/Connect/MeteringPointConnectedIntegrationEvent.proto"></a>
<p align="right"><a href="#top">Top</a></p>

## IntegrationEvents/Connect/MeteringPointConnectedIntegrationEvent.proto

<a name=".MeteringPointConnected"></a>

### MeteringPointConnected

This message is sent out when a metering point is connected.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| meteringpoint_id | [string](#string) |  | Unique metering point identifier |
| gsrn_number | [string](#string) |  | Business facing metering point identifier |
| effective_date | [string](#string) |  | Date which the metering point is connected. |

<a name="IntegrationEvents/CreateMeteringPoint/Consumption/ConsumptionMeteringPointCreated.proto"></a>
<p align="right"><a href="#top">Top</a></p>

## IntegrationEvents/CreateMeteringPoint/Consumption/ConsumptionMeteringPointCreated.proto

<a name=".ConsumptionMeteringPointCreated"></a>

### ConsumptionMeteringPointCreated

This message is sent out when a Consumption metering point is created.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique identification for metering point |
| gsrn_number | [string](#string) |  | Business facing metering point identifier |
| grid_area_code | [string](#string) |  | Signifies which grid area a metering point belongs to |
| settlement_method | [ConsumptionMeteringPointCreated.SettlementMethod](#ConsumptionMeteringPointCreated.SettlementMethod) |  |  |
| metering_method | [ConsumptionMeteringPointCreated.MeteringMethod](#ConsumptionMeteringPointCreated.MeteringMethod) |  | Metering method denotes how energy quantity is calculated in other domain |
| meter_reading_periodicity | [ConsumptionMeteringPointCreated.MeterReadingPeriodicity](#ConsumptionMeteringPointCreated.MeterReadingPeriodicity) |  | Denotes how often a energy quantity is read on a metering point |
| net_settlement_group | [ConsumptionMeteringPointCreated.NetSettlementGroup](#ConsumptionMeteringPointCreated.NetSettlementGroup) |  | Denotes the net settlement group |
| product | [ConsumptionMeteringPointCreated.ProductType](#ConsumptionMeteringPointCreated.ProductType) |  |  |
| effective_date | [google.protobuf.Timestamp](#google.protobuf.Timestamp) |  | The date on which the metering point is created |
| connection_state | [ConsumptionMeteringPointCreated.ConnectionState](#ConsumptionMeteringPointCreated.ConnectionState) |  | Denotes which connection state a metering point is created with. For a consumption metering point this is always &#34;New&#34; |
| unit_type | [ConsumptionMeteringPointCreated.UnitType](#ConsumptionMeteringPointCreated.UnitType) |  | Denotes the unit type. For a production metering point this is always a variation of watt/hour |

<a name=".ConsumptionMeteringPointCreated.ConnectionState"></a>

### ConsumptionMeteringPointCreated.ConnectionState

| Name | Number | Description |
| ---- | ------ | ----------- |
| CS_NEW | 0 | Always created with connection state new |

<a name=".ConsumptionMeteringPointCreated.MeterReadingPeriodicity"></a>

### ConsumptionMeteringPointCreated.MeterReadingPeriodicity

| Name | Number | Description |
| ---- | ------ | ----------- |
| MRP_HOURLY | 0 |  |
| MRP_QUARTERLY | 1 |  |

<a name=".ConsumptionMeteringPointCreated.MeteringMethod"></a>

### ConsumptionMeteringPointCreated.MeteringMethod

| Name | Number | Description |
| ---- | ------ | ----------- |
| MM_PHYSICAL | 0 |  |
| MM_VIRTUAL | 1 |  |
| MM_CALCULATED | 2 |  |

<a name=".ConsumptionMeteringPointCreated.NetSettlementGroup"></a>

### ConsumptionMeteringPointCreated.NetSettlementGroup

| Name | Number | Description |
| ---- | ------ | ----------- |
| NSG_ZERO | 0 |  |
| NSG_ONE | 1 |  |
| NSG_TWO | 2 |  |
| NSG_THREE | 3 |  |
| NSG_SIX | 4 |  |
| NSG_NINETYNINE | 5 |  |

<a name=".ConsumptionMeteringPointCreated.ProductType"></a>

### ConsumptionMeteringPointCreated.ProductType

| Name | Number | Description |
| ---- | ------ | ----------- |
| PT_TARIFF | 0 |  |
| PT_FUELQUANTITY | 1 |  |
| PT_POWERACTIVE | 2 |  |
| PT_POWERREACTIVE | 3 |  |
| PT_ENERGYACTIVE | 4 |  |
| PT_ENERGYREACTIVE | 5 |  |

<a name=".ConsumptionMeteringPointCreated.SettlementMethod"></a>

### ConsumptionMeteringPointCreated.SettlementMethod

| Name | Number | Description |
| ---- | ------ | ----------- |
| SM_FLEX | 0 |  |
| SM_PROFILED | 1 |  |
| SM_NONPROFILED | 2 |  |

<a name=".ConsumptionMeteringPointCreated.UnitType"></a>

### ConsumptionMeteringPointCreated.UnitType

| Name | Number | Description |
| ---- | ------ | ----------- |
| UT_WH | 0 | Watt per hour |
| UT_KWH | 1 | Kilowatt per hour |
| UT_MWH | 2 | Megawatt per hour |
| UT_GWH | 3 | Gigawatt per hour |

<a name="IntegrationEvents/CreateMeteringPoint/Production/ProductionMeteringPointCreated.proto"></a>
<p align="right"><a href="#top">Top</a></p>

## IntegrationEvents/CreateMeteringPoint/Production/ProductionMeteringPointCreated.proto

<a name=".ProductionMeteringPointCreated"></a>

### ProductionMeteringPointCreated

This message is sent out when a Consumption metering point is created.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique identification for metering point |
| gsrn_number | [string](#string) |  | Business facing metering point identifier |
| grid_area_code | [string](#string) |  | Signifies which grid area a metering point belongs to |
| metering_method | [ProductionMeteringPointCreated.MeteringMethod](#ProductionMeteringPointCreated.MeteringMethod) |  | Metering method denotes where energy quantities originate |
| meter_reading_periodicity | [ProductionMeteringPointCreated.MeterReadingPeriodicity](#ProductionMeteringPointCreated.MeterReadingPeriodicity) |  | Denotes how often a energy quantity is read on a metering point |
| net_settlement_group | [ProductionMeteringPointCreated.NetSettlementGroup](#ProductionMeteringPointCreated.NetSettlementGroup) |  | Denotes the net settlement group |
| product | [ProductionMeteringPointCreated.ProductType](#ProductionMeteringPointCreated.ProductType) |  | Denotes the product type |
| connection_state | [ProductionMeteringPointCreated.ConnectionState](#ProductionMeteringPointCreated.ConnectionState) |  | Denotes which connection state a metering point is created with. For a production metering point this is always &#34;New&#34; |
| unit_type | [ProductionMeteringPointCreated.UnitType](#ProductionMeteringPointCreated.UnitType) |  | Denotes the unit type. For a production metering point this is always a variation of watt/hour |
| effective_Date | [google.protobuf.Timestamp](#google.protobuf.Timestamp) |  | The date on which the metering point is created |

<a name=".ProductionMeteringPointCreated.ConnectionState"></a>

### ProductionMeteringPointCreated.ConnectionState

| Name | Number | Description |
| ---- | ------ | ----------- |
| CS_NEW | 0 | Always created with connection state new |

<a name=".ProductionMeteringPointCreated.MeterReadingPeriodicity"></a>

### ProductionMeteringPointCreated.MeterReadingPeriodicity

| Name | Number | Description |
| ---- | ------ | ----------- |
| MRP_HOURLY | 0 | Read every hour |
| MRP_QUARTERLY | 1 | Read every 15 minutes |

<a name=".ProductionMeteringPointCreated.MeteringMethod"></a>

### ProductionMeteringPointCreated.MeteringMethod

| Name | Number | Description |
| ---- | ------ | ----------- |
| MM_PHYSICAL | 0 | Has a physical meter associated with it |
| MM_VIRTUAL | 1 | Does not have a physical meter associated with it |
| MM_CALCULATED | 2 | Does not have a physical meter associated with it |

<a name=".ProductionMeteringPointCreated.NetSettlementGroup"></a>

### ProductionMeteringPointCreated.NetSettlementGroup

| Name | Number | Description |
| ---- | ------ | ----------- |
| NSG_ZERO | 0 |  |
| NSG_ONE | 1 |  |
| NSG_TWO | 2 |  |
| NSG_THREE | 3 |  |
| NSG_SIX | 4 |  |
| NSG_NINETYNINE | 5 |  |

<a name=".ProductionMeteringPointCreated.ProductType"></a>

### ProductionMeteringPointCreated.ProductType

| Name | Number | Description |
| ---- | ------ | ----------- |
| PT_TARIFF | 0 |  |
| PT_FUELQUANTITY | 1 |  |
| PT_POWERACTIVE | 2 |  |
| PT_POWERREACTIVE | 3 |  |
| PT_ENERGYACTIVE | 4 |  |
| PT_ENERGYREACTIVE | 5 |  |

<a name=".ProductionMeteringPointCreated.UnitType"></a>

### ProductionMeteringPointCreated.UnitType

| Name | Number | Description |
| ---- | ------ | ----------- |
| UT_WH | 0 | Watt per hour |
| UT_KWH | 1 | Kilowatt per hour |
| UT_MWH | 2 | Megawatt per hour |
| UT_GWH | 3 | Gigawatt per hour |

<a name="IntegrationEvents/CreateMeteringPoint/Exchange/ExchangeMeteringPointCreated.proto"></a>
<p align="right"><a href="#top">Top</a></p>

## IntegrationEvents/CreateMeteringPoint/Exchange/ExchangeMeteringPointCreated.proto

<a name=".ExchangeMeteringPointCreated"></a>

### ExchangeMeteringPointCreated

This message is sent out when a Consumption metering point is created.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| metering_point_id | [string](#string) |  | Unique identification for metering point |
| gsrn_number | [string](#string) |  | Business facing metering point identifier |
| grid_area_code | [string](#string) |  | Signifies which grid area a metering point belongs to |
| to_grid_area_code | [string](#string) |  | Denotes which grid area energy is exchanged to |
| from_grid_area_code | [string](#string) |  | Denotes which grid area energy is exchange from |
| metering_method | [ExchangeMeteringPointCreated.MeteringMethod](#ExchangeMeteringPointCreated.MeteringMethod) |  | Metering method denotes where energy quantities originate |
| meter_reading_periodicity | [ExchangeMeteringPointCreated.MeterReadingPeriodicity](#ExchangeMeteringPointCreated.MeterReadingPeriodicity) |  | Denotes how often a energy quantity is read on a metering point |
| product | [ExchangeMeteringPointCreated.ProductType](#ExchangeMeteringPointCreated.ProductType) |  | Denotes the product type |
| connection_state | [ExchangeMeteringPointCreated.ConnectionState](#ExchangeMeteringPointCreated.ConnectionState) |  | Denotes which connection state a metering point is created with. For an exchange metering point this is always &#34;New&#34; |
| unit_type | [ExchangeMeteringPointCreated.UnitType](#ExchangeMeteringPointCreated.UnitType) |  | Denotes the unit type. For a production metering point this is always a variation of watt/hour |
| effective_date | [google.protobuf.Timestamp](#google.protobuf.Timestamp) |  | The date on which the metering point is created |

<a name=".ExchangeMeteringPointCreated.ConnectionState"></a>

### ExchangeMeteringPointCreated.ConnectionState

| Name | Number | Description |
| ---- | ------ | ----------- |
| CS_NEW | 0 | Always created with connection state new |

<a name=".ExchangeMeteringPointCreated.MeterReadingPeriodicity"></a>

### ExchangeMeteringPointCreated.MeterReadingPeriodicity

| Name | Number | Description |
| ---- | ------ | ----------- |
| MRP_HOURLY | 0 | Read every hour |
| MRP_QUARTERLY | 1 | Read every 15 minutes |

<a name=".ExchangeMeteringPointCreated.MeteringMethod"></a>

### ExchangeMeteringPointCreated.MeteringMethod

| Name | Number | Description |
| ---- | ------ | ----------- |
| MM_PHYSICAL | 0 | Has a physical meter associated with it |
| MM_VIRTUAL | 1 | Does not have a physical meter associated with it |
| MM_CALCULATED | 2 | Does not have a physical meter associated with it |

<a name=".ExchangeMeteringPointCreated.ProductType"></a>

### ExchangeMeteringPointCreated.ProductType

| Name | Number | Description |
| ---- | ------ | ----------- |
| PT_TARIFF | 0 |  |
| PT_FUELQUANTITY | 1 |  |
| PT_POWERACTIVE | 2 |  |
| PT_POWERREACTIVE | 3 |  |
| PT_ENERGYACTIVE | 4 |  |
| PT_ENERGYREACTIVE | 5 |  |

<a name=".ExchangeMeteringPointCreated.UnitType"></a>

### ExchangeMeteringPointCreated.UnitType

| Name | Number | Description |
| ---- | ------ | ----------- |
| UT_WH | 0 | Watt per hour |
| UT_KWH | 1 | Kilowatt per hour |
| UT_MWH | 2 | Megawatt per hour |
| UT_GWH | 3 | Gigawatt per hour |

## Scalar Value Types

| .proto Type | Notes | C++ | Java | Python | Go | C# | PHP | Ruby |
| ----------- | ----- | --- | ---- | ------ | -- | -- | --- | ---- |
| <a name="double" /> double |  | double | double | float | float64 | double | float | Float |
| <a name="float" /> float |  | float | float | float | float32 | float | float | Float |
| <a name="int32" /> int32 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint32 instead. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="int64" /> int64 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint64 instead. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="uint32" /> uint32 | Uses variable-length encoding. | uint32 | int | int/long | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="uint64" /> uint64 | Uses variable-length encoding. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum or Fixnum (as required) |
| <a name="sint32" /> sint32 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int32s. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sint64" /> sint64 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int64s. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="fixed32" /> fixed32 | Always four bytes. More efficient than uint32 if values are often greater than 2^28. | uint32 | int | int | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="fixed64" /> fixed64 | Always eight bytes. More efficient than uint64 if values are often greater than 2^56. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum |
| <a name="sfixed32" /> sfixed32 | Always four bytes. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sfixed64" /> sfixed64 | Always eight bytes. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="bool" /> bool |  | bool | boolean | boolean | bool | bool | boolean | TrueClass/FalseClass |
| <a name="string" /> string | A string must always contain UTF-8 encoded or 7-bit ASCII text. | string | String | str/unicode | string | string | string | String (UTF-8) |
| <a name="bytes" /> bytes | May contain any arbitrary sequence of bytes. | string | ByteString | str | []byte | ByteString | string | String (ASCII-8BIT) |
