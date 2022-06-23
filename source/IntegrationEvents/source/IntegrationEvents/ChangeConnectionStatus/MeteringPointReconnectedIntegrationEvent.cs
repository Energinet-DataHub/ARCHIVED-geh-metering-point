// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: ChangeConnectionStatus/MeteringPointReconnectedIntegrationEvent.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Energinet.DataHub.MeteringPoints.IntegrationEvents.ChangeConnectionStatus {

  /// <summary>Holder for reflection information generated from ChangeConnectionStatus/MeteringPointReconnectedIntegrationEvent.proto</summary>
  public static partial class MeteringPointReconnectedIntegrationEventReflection {

    #region Descriptor
    /// <summary>File descriptor for ChangeConnectionStatus/MeteringPointReconnectedIntegrationEvent.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static MeteringPointReconnectedIntegrationEventReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CkVDaGFuZ2VDb25uZWN0aW9uU3RhdHVzL01ldGVyaW5nUG9pbnRSZWNvbm5l",
            "Y3RlZEludGVncmF0aW9uRXZlbnQucHJvdG8aH2dvb2dsZS9wcm90b2J1Zi90",
            "aW1lc3RhbXAucHJvdG8ifQoYTWV0ZXJpbmdQb2ludFJlY29ubmVjdGVkEhgK",
            "EG1ldGVyaW5ncG9pbnRfaWQYASABKAkSEwoLZ3Nybl9udW1iZXIYAiABKAkS",
            "MgoOZWZmZWN0aXZlX2RhdGUYAyABKAsyGi5nb29nbGUucHJvdG9idWYuVGlt",
            "ZXN0YW1wQkyqAklFbmVyZ2luZXQuRGF0YUh1Yi5NZXRlcmluZ1BvaW50cy5J",
            "bnRlZ3JhdGlvbkV2ZW50cy5DaGFuZ2VDb25uZWN0aW9uU3RhdHVzYgZwcm90",
            "bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Google.Protobuf.WellKnownTypes.TimestampReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Energinet.DataHub.MeteringPoints.IntegrationEvents.ChangeConnectionStatus.MeteringPointReconnected), global::Energinet.DataHub.MeteringPoints.IntegrationEvents.ChangeConnectionStatus.MeteringPointReconnected.Parser, new[]{ "MeteringpointId", "GsrnNumber", "EffectiveDate" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  ///*
  /// This message is sent out when a metering point is disconnected.
  /// </summary>
  public sealed partial class MeteringPointReconnected : pb::IMessage<MeteringPointReconnected>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<MeteringPointReconnected> _parser = new pb::MessageParser<MeteringPointReconnected>(() => new MeteringPointReconnected());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<MeteringPointReconnected> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Energinet.DataHub.MeteringPoints.IntegrationEvents.ChangeConnectionStatus.MeteringPointReconnectedIntegrationEventReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MeteringPointReconnected() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MeteringPointReconnected(MeteringPointReconnected other) : this() {
      meteringpointId_ = other.meteringpointId_;
      gsrnNumber_ = other.gsrnNumber_;
      effectiveDate_ = other.effectiveDate_ != null ? other.effectiveDate_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MeteringPointReconnected Clone() {
      return new MeteringPointReconnected(this);
    }

    /// <summary>Field number for the "meteringpoint_id" field.</summary>
    public const int MeteringpointIdFieldNumber = 1;
    private string meteringpointId_ = "";
    /// <summary>
    /// Unique identification for metering point
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string MeteringpointId {
      get { return meteringpointId_; }
      set {
        meteringpointId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "gsrn_number" field.</summary>
    public const int GsrnNumberFieldNumber = 2;
    private string gsrnNumber_ = "";
    /// <summary>
    /// Business facing metering point identifier
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string GsrnNumber {
      get { return gsrnNumber_; }
      set {
        gsrnNumber_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "effective_date" field.</summary>
    public const int EffectiveDateFieldNumber = 3;
    private global::Google.Protobuf.WellKnownTypes.Timestamp effectiveDate_;
    /// <summary>
    /// Date which the metering point is connected
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Google.Protobuf.WellKnownTypes.Timestamp EffectiveDate {
      get { return effectiveDate_; }
      set {
        effectiveDate_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as MeteringPointReconnected);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(MeteringPointReconnected other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (MeteringpointId != other.MeteringpointId) return false;
      if (GsrnNumber != other.GsrnNumber) return false;
      if (!object.Equals(EffectiveDate, other.EffectiveDate)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (MeteringpointId.Length != 0) hash ^= MeteringpointId.GetHashCode();
      if (GsrnNumber.Length != 0) hash ^= GsrnNumber.GetHashCode();
      if (effectiveDate_ != null) hash ^= EffectiveDate.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (MeteringpointId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(MeteringpointId);
      }
      if (GsrnNumber.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(GsrnNumber);
      }
      if (effectiveDate_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(EffectiveDate);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (MeteringpointId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(MeteringpointId);
      }
      if (GsrnNumber.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(GsrnNumber);
      }
      if (effectiveDate_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(EffectiveDate);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (MeteringpointId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(MeteringpointId);
      }
      if (GsrnNumber.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(GsrnNumber);
      }
      if (effectiveDate_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(EffectiveDate);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(MeteringPointReconnected other) {
      if (other == null) {
        return;
      }
      if (other.MeteringpointId.Length != 0) {
        MeteringpointId = other.MeteringpointId;
      }
      if (other.GsrnNumber.Length != 0) {
        GsrnNumber = other.GsrnNumber;
      }
      if (other.effectiveDate_ != null) {
        if (effectiveDate_ == null) {
          EffectiveDate = new global::Google.Protobuf.WellKnownTypes.Timestamp();
        }
        EffectiveDate.MergeFrom(other.EffectiveDate);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            MeteringpointId = input.ReadString();
            break;
          }
          case 18: {
            GsrnNumber = input.ReadString();
            break;
          }
          case 26: {
            if (effectiveDate_ == null) {
              EffectiveDate = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(EffectiveDate);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            MeteringpointId = input.ReadString();
            break;
          }
          case 18: {
            GsrnNumber = input.ReadString();
            break;
          }
          case 26: {
            if (effectiveDate_ == null) {
              EffectiveDate = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(EffectiveDate);
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
