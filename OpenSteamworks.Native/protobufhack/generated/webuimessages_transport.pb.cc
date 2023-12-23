// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: webuimessages_transport.proto

#include "webuimessages_transport.pb.h"

#include <algorithm>

#include <google/protobuf/io/coded_stream.h>
#include <google/protobuf/extension_set.h>
#include <google/protobuf/wire_format_lite.h>
#include <google/protobuf/descriptor.h>
#include <google/protobuf/generated_message_reflection.h>
#include <google/protobuf/reflection_ops.h>
#include <google/protobuf/wire_format.h>
// @@protoc_insertion_point(includes)
#include <google/protobuf/port_def.inc>

PROTOBUF_PRAGMA_INIT_SEG
constexpr CTransportAuth_Authenticate_Request::CTransportAuth_Authenticate_Request(
  ::PROTOBUF_NAMESPACE_ID::internal::ConstantInitialized)
  : auth_key_(&::PROTOBUF_NAMESPACE_ID::internal::fixed_address_empty_string){}
struct CTransportAuth_Authenticate_RequestDefaultTypeInternal {
  constexpr CTransportAuth_Authenticate_RequestDefaultTypeInternal()
    : _instance(::PROTOBUF_NAMESPACE_ID::internal::ConstantInitialized{}) {}
  ~CTransportAuth_Authenticate_RequestDefaultTypeInternal() {}
  union {
    CTransportAuth_Authenticate_Request _instance;
  };
};
PROTOBUF_ATTRIBUTE_NO_DESTROY PROTOBUF_CONSTINIT CTransportAuth_Authenticate_RequestDefaultTypeInternal _CTransportAuth_Authenticate_Request_default_instance_;
constexpr CTransportAuth_Authenticate_Response::CTransportAuth_Authenticate_Response(
  ::PROTOBUF_NAMESPACE_ID::internal::ConstantInitialized){}
struct CTransportAuth_Authenticate_ResponseDefaultTypeInternal {
  constexpr CTransportAuth_Authenticate_ResponseDefaultTypeInternal()
    : _instance(::PROTOBUF_NAMESPACE_ID::internal::ConstantInitialized{}) {}
  ~CTransportAuth_Authenticate_ResponseDefaultTypeInternal() {}
  union {
    CTransportAuth_Authenticate_Response _instance;
  };
};
PROTOBUF_ATTRIBUTE_NO_DESTROY PROTOBUF_CONSTINIT CTransportAuth_Authenticate_ResponseDefaultTypeInternal _CTransportAuth_Authenticate_Response_default_instance_;
static ::PROTOBUF_NAMESPACE_ID::Metadata file_level_metadata_webuimessages_5ftransport_2eproto[2];
static constexpr ::PROTOBUF_NAMESPACE_ID::EnumDescriptor const** file_level_enum_descriptors_webuimessages_5ftransport_2eproto = nullptr;
static const ::PROTOBUF_NAMESPACE_ID::ServiceDescriptor* file_level_service_descriptors_webuimessages_5ftransport_2eproto[1];

const ::PROTOBUF_NAMESPACE_ID::uint32 TableStruct_webuimessages_5ftransport_2eproto::offsets[] PROTOBUF_SECTION_VARIABLE(protodesc_cold) = {
  PROTOBUF_FIELD_OFFSET(::CTransportAuth_Authenticate_Request, _has_bits_),
  PROTOBUF_FIELD_OFFSET(::CTransportAuth_Authenticate_Request, _internal_metadata_),
  ~0u,  // no _extensions_
  ~0u,  // no _oneof_case_
  ~0u,  // no _weak_field_map_
  PROTOBUF_FIELD_OFFSET(::CTransportAuth_Authenticate_Request, auth_key_),
  0,
  ~0u,  // no _has_bits_
  PROTOBUF_FIELD_OFFSET(::CTransportAuth_Authenticate_Response, _internal_metadata_),
  ~0u,  // no _extensions_
  ~0u,  // no _oneof_case_
  ~0u,  // no _weak_field_map_
};
static const ::PROTOBUF_NAMESPACE_ID::internal::MigrationSchema schemas[] PROTOBUF_SECTION_VARIABLE(protodesc_cold) = {
  { 0, 6, sizeof(::CTransportAuth_Authenticate_Request)},
  { 7, -1, sizeof(::CTransportAuth_Authenticate_Response)},
};

static ::PROTOBUF_NAMESPACE_ID::Message const * const file_default_instances[] = {
  reinterpret_cast<const ::PROTOBUF_NAMESPACE_ID::Message*>(&::_CTransportAuth_Authenticate_Request_default_instance_),
  reinterpret_cast<const ::PROTOBUF_NAMESPACE_ID::Message*>(&::_CTransportAuth_Authenticate_Response_default_instance_),
};

const char descriptor_table_protodef_webuimessages_5ftransport_2eproto[] PROTOBUF_SECTION_VARIABLE(protodesc_cold) =
  "\n\035webuimessages_transport.proto\032 google/"
  "protobuf/descriptor.proto\032\030steammessages"
  "_base.proto\032\030webuimessages_base.proto\"7\n"
  "#CTransportAuth_Authenticate_Request\022\020\n\010"
  "auth_key\030\001 \001(\t\"&\n$CTransportAuth_Authent"
  "icate_Response2r\n\rTransportAuth\022[\n\014Authe"
  "nticate\022$.CTransportAuth_Authenticate_Re"
  "quest\032%.CTransportAuth_Authenticate_Resp"
  "onse\032\004\200\227\"\003B\037H\001\200\001\001\252\002\027OpenSteamworks.Proto"
  "buf"
  ;
static const ::PROTOBUF_NAMESPACE_ID::internal::DescriptorTable*const descriptor_table_webuimessages_5ftransport_2eproto_deps[3] = {
  &::descriptor_table_google_2fprotobuf_2fdescriptor_2eproto,
  &::descriptor_table_steammessages_5fbase_2eproto,
  &::descriptor_table_webuimessages_5fbase_2eproto,
};
static ::PROTOBUF_NAMESPACE_ID::internal::once_flag descriptor_table_webuimessages_5ftransport_2eproto_once;
const ::PROTOBUF_NAMESPACE_ID::internal::DescriptorTable descriptor_table_webuimessages_5ftransport_2eproto = {
  false, false, 363, descriptor_table_protodef_webuimessages_5ftransport_2eproto, "webuimessages_transport.proto", 
  &descriptor_table_webuimessages_5ftransport_2eproto_once, descriptor_table_webuimessages_5ftransport_2eproto_deps, 3, 2,
  schemas, file_default_instances, TableStruct_webuimessages_5ftransport_2eproto::offsets,
  file_level_metadata_webuimessages_5ftransport_2eproto, file_level_enum_descriptors_webuimessages_5ftransport_2eproto, file_level_service_descriptors_webuimessages_5ftransport_2eproto,
};
PROTOBUF_ATTRIBUTE_WEAK ::PROTOBUF_NAMESPACE_ID::Metadata
descriptor_table_webuimessages_5ftransport_2eproto_metadata_getter(int index) {
  ::PROTOBUF_NAMESPACE_ID::internal::AssignDescriptors(&descriptor_table_webuimessages_5ftransport_2eproto);
  return descriptor_table_webuimessages_5ftransport_2eproto.file_level_metadata[index];
}

// Force running AddDescriptors() at dynamic initialization time.
PROTOBUF_ATTRIBUTE_INIT_PRIORITY static ::PROTOBUF_NAMESPACE_ID::internal::AddDescriptorsRunner dynamic_init_dummy_webuimessages_5ftransport_2eproto(&descriptor_table_webuimessages_5ftransport_2eproto);

// ===================================================================

class CTransportAuth_Authenticate_Request::_Internal {
 public:
  using HasBits = decltype(std::declval<CTransportAuth_Authenticate_Request>()._has_bits_);
  static void set_has_auth_key(HasBits* has_bits) {
    (*has_bits)[0] |= 1u;
  }
};

CTransportAuth_Authenticate_Request::CTransportAuth_Authenticate_Request(::PROTOBUF_NAMESPACE_ID::Arena* arena)
  : ::PROTOBUF_NAMESPACE_ID::Message(arena) {
  SharedCtor();
  RegisterArenaDtor(arena);
  // @@protoc_insertion_point(arena_constructor:CTransportAuth_Authenticate_Request)
}
CTransportAuth_Authenticate_Request::CTransportAuth_Authenticate_Request(const CTransportAuth_Authenticate_Request& from)
  : ::PROTOBUF_NAMESPACE_ID::Message(),
      _has_bits_(from._has_bits_) {
  _internal_metadata_.MergeFrom<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(from._internal_metadata_);
  auth_key_.UnsafeSetDefault(&::PROTOBUF_NAMESPACE_ID::internal::GetEmptyStringAlreadyInited());
  if (from._internal_has_auth_key()) {
    auth_key_.Set(::PROTOBUF_NAMESPACE_ID::internal::ArenaStringPtr::EmptyDefault{}, from._internal_auth_key(), 
      GetArena());
  }
  // @@protoc_insertion_point(copy_constructor:CTransportAuth_Authenticate_Request)
}

void CTransportAuth_Authenticate_Request::SharedCtor() {
auth_key_.UnsafeSetDefault(&::PROTOBUF_NAMESPACE_ID::internal::GetEmptyStringAlreadyInited());
}

CTransportAuth_Authenticate_Request::~CTransportAuth_Authenticate_Request() {
  // @@protoc_insertion_point(destructor:CTransportAuth_Authenticate_Request)
  SharedDtor();
  _internal_metadata_.Delete<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>();
}

void CTransportAuth_Authenticate_Request::SharedDtor() {
  GOOGLE_DCHECK(GetArena() == nullptr);
  auth_key_.DestroyNoArena(&::PROTOBUF_NAMESPACE_ID::internal::GetEmptyStringAlreadyInited());
}

void CTransportAuth_Authenticate_Request::ArenaDtor(void* object) {
  CTransportAuth_Authenticate_Request* _this = reinterpret_cast< CTransportAuth_Authenticate_Request* >(object);
  (void)_this;
}
void CTransportAuth_Authenticate_Request::RegisterArenaDtor(::PROTOBUF_NAMESPACE_ID::Arena*) {
}
void CTransportAuth_Authenticate_Request::SetCachedSize(int size) const {
  _cached_size_.Set(size);
}

void CTransportAuth_Authenticate_Request::Clear() {
// @@protoc_insertion_point(message_clear_start:CTransportAuth_Authenticate_Request)
  ::PROTOBUF_NAMESPACE_ID::uint32 cached_has_bits = 0;
  // Prevent compiler warnings about cached_has_bits being unused
  (void) cached_has_bits;

  cached_has_bits = _has_bits_[0];
  if (cached_has_bits & 0x00000001u) {
    auth_key_.ClearNonDefaultToEmpty();
  }
  _has_bits_.Clear();
  _internal_metadata_.Clear<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>();
}

const char* CTransportAuth_Authenticate_Request::_InternalParse(const char* ptr, ::PROTOBUF_NAMESPACE_ID::internal::ParseContext* ctx) {
#define CHK_(x) if (PROTOBUF_PREDICT_FALSE(!(x))) goto failure
  _Internal::HasBits has_bits{};
  while (!ctx->Done(&ptr)) {
    ::PROTOBUF_NAMESPACE_ID::uint32 tag;
    ptr = ::PROTOBUF_NAMESPACE_ID::internal::ReadTag(ptr, &tag);
    CHK_(ptr);
    switch (tag >> 3) {
      // optional string auth_key = 1;
      case 1:
        if (PROTOBUF_PREDICT_TRUE(static_cast<::PROTOBUF_NAMESPACE_ID::uint8>(tag) == 10)) {
          auto str = _internal_mutable_auth_key();
          ptr = ::PROTOBUF_NAMESPACE_ID::internal::InlineGreedyStringParser(str, ptr, ctx);
          #ifndef NDEBUG
          ::PROTOBUF_NAMESPACE_ID::internal::VerifyUTF8(str, "CTransportAuth_Authenticate_Request.auth_key");
          #endif  // !NDEBUG
          CHK_(ptr);
        } else goto handle_unusual;
        continue;
      default: {
      handle_unusual:
        if ((tag & 7) == 4 || tag == 0) {
          ctx->SetLastTag(tag);
          goto success;
        }
        ptr = UnknownFieldParse(tag,
            _internal_metadata_.mutable_unknown_fields<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(),
            ptr, ctx);
        CHK_(ptr != nullptr);
        continue;
      }
    }  // switch
  }  // while
success:
  _has_bits_.Or(has_bits);
  return ptr;
failure:
  ptr = nullptr;
  goto success;
#undef CHK_
}

::PROTOBUF_NAMESPACE_ID::uint8* CTransportAuth_Authenticate_Request::_InternalSerialize(
    ::PROTOBUF_NAMESPACE_ID::uint8* target, ::PROTOBUF_NAMESPACE_ID::io::EpsCopyOutputStream* stream) const {
  // @@protoc_insertion_point(serialize_to_array_start:CTransportAuth_Authenticate_Request)
  ::PROTOBUF_NAMESPACE_ID::uint32 cached_has_bits = 0;
  (void) cached_has_bits;

  cached_has_bits = _has_bits_[0];
  // optional string auth_key = 1;
  if (cached_has_bits & 0x00000001u) {
    ::PROTOBUF_NAMESPACE_ID::internal::WireFormat::VerifyUTF8StringNamedField(
      this->_internal_auth_key().data(), static_cast<int>(this->_internal_auth_key().length()),
      ::PROTOBUF_NAMESPACE_ID::internal::WireFormat::SERIALIZE,
      "CTransportAuth_Authenticate_Request.auth_key");
    target = stream->WriteStringMaybeAliased(
        1, this->_internal_auth_key(), target);
  }

  if (PROTOBUF_PREDICT_FALSE(_internal_metadata_.have_unknown_fields())) {
    target = ::PROTOBUF_NAMESPACE_ID::internal::WireFormat::InternalSerializeUnknownFieldsToArray(
        _internal_metadata_.unknown_fields<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(::PROTOBUF_NAMESPACE_ID::UnknownFieldSet::default_instance), target, stream);
  }
  // @@protoc_insertion_point(serialize_to_array_end:CTransportAuth_Authenticate_Request)
  return target;
}

size_t CTransportAuth_Authenticate_Request::ByteSizeLong() const {
// @@protoc_insertion_point(message_byte_size_start:CTransportAuth_Authenticate_Request)
  size_t total_size = 0;

  ::PROTOBUF_NAMESPACE_ID::uint32 cached_has_bits = 0;
  // Prevent compiler warnings about cached_has_bits being unused
  (void) cached_has_bits;

  // optional string auth_key = 1;
  cached_has_bits = _has_bits_[0];
  if (cached_has_bits & 0x00000001u) {
    total_size += 1 +
      ::PROTOBUF_NAMESPACE_ID::internal::WireFormatLite::StringSize(
        this->_internal_auth_key());
  }

  if (PROTOBUF_PREDICT_FALSE(_internal_metadata_.have_unknown_fields())) {
    return ::PROTOBUF_NAMESPACE_ID::internal::ComputeUnknownFieldsSize(
        _internal_metadata_, total_size, &_cached_size_);
  }
  int cached_size = ::PROTOBUF_NAMESPACE_ID::internal::ToCachedSize(total_size);
  SetCachedSize(cached_size);
  return total_size;
}

void CTransportAuth_Authenticate_Request::MergeFrom(const ::PROTOBUF_NAMESPACE_ID::Message& from) {
// @@protoc_insertion_point(generalized_merge_from_start:CTransportAuth_Authenticate_Request)
  GOOGLE_DCHECK_NE(&from, this);
  const CTransportAuth_Authenticate_Request* source =
      ::PROTOBUF_NAMESPACE_ID::DynamicCastToGenerated<CTransportAuth_Authenticate_Request>(
          &from);
  if (source == nullptr) {
  // @@protoc_insertion_point(generalized_merge_from_cast_fail:CTransportAuth_Authenticate_Request)
    ::PROTOBUF_NAMESPACE_ID::internal::ReflectionOps::Merge(from, this);
  } else {
  // @@protoc_insertion_point(generalized_merge_from_cast_success:CTransportAuth_Authenticate_Request)
    MergeFrom(*source);
  }
}

void CTransportAuth_Authenticate_Request::MergeFrom(const CTransportAuth_Authenticate_Request& from) {
// @@protoc_insertion_point(class_specific_merge_from_start:CTransportAuth_Authenticate_Request)
  GOOGLE_DCHECK_NE(&from, this);
  _internal_metadata_.MergeFrom<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(from._internal_metadata_);
  ::PROTOBUF_NAMESPACE_ID::uint32 cached_has_bits = 0;
  (void) cached_has_bits;

  if (from._internal_has_auth_key()) {
    _internal_set_auth_key(from._internal_auth_key());
  }
}

void CTransportAuth_Authenticate_Request::CopyFrom(const ::PROTOBUF_NAMESPACE_ID::Message& from) {
// @@protoc_insertion_point(generalized_copy_from_start:CTransportAuth_Authenticate_Request)
  if (&from == this) return;
  Clear();
  MergeFrom(from);
}

void CTransportAuth_Authenticate_Request::CopyFrom(const CTransportAuth_Authenticate_Request& from) {
// @@protoc_insertion_point(class_specific_copy_from_start:CTransportAuth_Authenticate_Request)
  if (&from == this) return;
  Clear();
  MergeFrom(from);
}

bool CTransportAuth_Authenticate_Request::IsInitialized() const {
  return true;
}

void CTransportAuth_Authenticate_Request::InternalSwap(CTransportAuth_Authenticate_Request* other) {
  using std::swap;
  _internal_metadata_.Swap<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(&other->_internal_metadata_);
  swap(_has_bits_[0], other->_has_bits_[0]);
  auth_key_.Swap(&other->auth_key_, &::PROTOBUF_NAMESPACE_ID::internal::GetEmptyStringAlreadyInited(), GetArena());
}

::PROTOBUF_NAMESPACE_ID::Metadata CTransportAuth_Authenticate_Request::GetMetadata() const {
  return GetMetadataStatic();
}


// ===================================================================

class CTransportAuth_Authenticate_Response::_Internal {
 public:
};

CTransportAuth_Authenticate_Response::CTransportAuth_Authenticate_Response(::PROTOBUF_NAMESPACE_ID::Arena* arena)
  : ::PROTOBUF_NAMESPACE_ID::Message(arena) {
  SharedCtor();
  RegisterArenaDtor(arena);
  // @@protoc_insertion_point(arena_constructor:CTransportAuth_Authenticate_Response)
}
CTransportAuth_Authenticate_Response::CTransportAuth_Authenticate_Response(const CTransportAuth_Authenticate_Response& from)
  : ::PROTOBUF_NAMESPACE_ID::Message() {
  _internal_metadata_.MergeFrom<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(from._internal_metadata_);
  // @@protoc_insertion_point(copy_constructor:CTransportAuth_Authenticate_Response)
}

void CTransportAuth_Authenticate_Response::SharedCtor() {
}

CTransportAuth_Authenticate_Response::~CTransportAuth_Authenticate_Response() {
  // @@protoc_insertion_point(destructor:CTransportAuth_Authenticate_Response)
  SharedDtor();
  _internal_metadata_.Delete<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>();
}

void CTransportAuth_Authenticate_Response::SharedDtor() {
  GOOGLE_DCHECK(GetArena() == nullptr);
}

void CTransportAuth_Authenticate_Response::ArenaDtor(void* object) {
  CTransportAuth_Authenticate_Response* _this = reinterpret_cast< CTransportAuth_Authenticate_Response* >(object);
  (void)_this;
}
void CTransportAuth_Authenticate_Response::RegisterArenaDtor(::PROTOBUF_NAMESPACE_ID::Arena*) {
}
void CTransportAuth_Authenticate_Response::SetCachedSize(int size) const {
  _cached_size_.Set(size);
}

void CTransportAuth_Authenticate_Response::Clear() {
// @@protoc_insertion_point(message_clear_start:CTransportAuth_Authenticate_Response)
  ::PROTOBUF_NAMESPACE_ID::uint32 cached_has_bits = 0;
  // Prevent compiler warnings about cached_has_bits being unused
  (void) cached_has_bits;

  _internal_metadata_.Clear<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>();
}

const char* CTransportAuth_Authenticate_Response::_InternalParse(const char* ptr, ::PROTOBUF_NAMESPACE_ID::internal::ParseContext* ctx) {
#define CHK_(x) if (PROTOBUF_PREDICT_FALSE(!(x))) goto failure
  while (!ctx->Done(&ptr)) {
    ::PROTOBUF_NAMESPACE_ID::uint32 tag;
    ptr = ::PROTOBUF_NAMESPACE_ID::internal::ReadTag(ptr, &tag);
    CHK_(ptr);
        if ((tag & 7) == 4 || tag == 0) {
          ctx->SetLastTag(tag);
          goto success;
        }
        ptr = UnknownFieldParse(tag,
            _internal_metadata_.mutable_unknown_fields<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(),
            ptr, ctx);
        CHK_(ptr != nullptr);
        continue;
  }  // while
success:
  return ptr;
failure:
  ptr = nullptr;
  goto success;
#undef CHK_
}

::PROTOBUF_NAMESPACE_ID::uint8* CTransportAuth_Authenticate_Response::_InternalSerialize(
    ::PROTOBUF_NAMESPACE_ID::uint8* target, ::PROTOBUF_NAMESPACE_ID::io::EpsCopyOutputStream* stream) const {
  // @@protoc_insertion_point(serialize_to_array_start:CTransportAuth_Authenticate_Response)
  ::PROTOBUF_NAMESPACE_ID::uint32 cached_has_bits = 0;
  (void) cached_has_bits;

  if (PROTOBUF_PREDICT_FALSE(_internal_metadata_.have_unknown_fields())) {
    target = ::PROTOBUF_NAMESPACE_ID::internal::WireFormat::InternalSerializeUnknownFieldsToArray(
        _internal_metadata_.unknown_fields<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(::PROTOBUF_NAMESPACE_ID::UnknownFieldSet::default_instance), target, stream);
  }
  // @@protoc_insertion_point(serialize_to_array_end:CTransportAuth_Authenticate_Response)
  return target;
}

size_t CTransportAuth_Authenticate_Response::ByteSizeLong() const {
// @@protoc_insertion_point(message_byte_size_start:CTransportAuth_Authenticate_Response)
  size_t total_size = 0;

  ::PROTOBUF_NAMESPACE_ID::uint32 cached_has_bits = 0;
  // Prevent compiler warnings about cached_has_bits being unused
  (void) cached_has_bits;

  if (PROTOBUF_PREDICT_FALSE(_internal_metadata_.have_unknown_fields())) {
    return ::PROTOBUF_NAMESPACE_ID::internal::ComputeUnknownFieldsSize(
        _internal_metadata_, total_size, &_cached_size_);
  }
  int cached_size = ::PROTOBUF_NAMESPACE_ID::internal::ToCachedSize(total_size);
  SetCachedSize(cached_size);
  return total_size;
}

void CTransportAuth_Authenticate_Response::MergeFrom(const ::PROTOBUF_NAMESPACE_ID::Message& from) {
// @@protoc_insertion_point(generalized_merge_from_start:CTransportAuth_Authenticate_Response)
  GOOGLE_DCHECK_NE(&from, this);
  const CTransportAuth_Authenticate_Response* source =
      ::PROTOBUF_NAMESPACE_ID::DynamicCastToGenerated<CTransportAuth_Authenticate_Response>(
          &from);
  if (source == nullptr) {
  // @@protoc_insertion_point(generalized_merge_from_cast_fail:CTransportAuth_Authenticate_Response)
    ::PROTOBUF_NAMESPACE_ID::internal::ReflectionOps::Merge(from, this);
  } else {
  // @@protoc_insertion_point(generalized_merge_from_cast_success:CTransportAuth_Authenticate_Response)
    MergeFrom(*source);
  }
}

void CTransportAuth_Authenticate_Response::MergeFrom(const CTransportAuth_Authenticate_Response& from) {
// @@protoc_insertion_point(class_specific_merge_from_start:CTransportAuth_Authenticate_Response)
  GOOGLE_DCHECK_NE(&from, this);
  _internal_metadata_.MergeFrom<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(from._internal_metadata_);
  ::PROTOBUF_NAMESPACE_ID::uint32 cached_has_bits = 0;
  (void) cached_has_bits;

}

void CTransportAuth_Authenticate_Response::CopyFrom(const ::PROTOBUF_NAMESPACE_ID::Message& from) {
// @@protoc_insertion_point(generalized_copy_from_start:CTransportAuth_Authenticate_Response)
  if (&from == this) return;
  Clear();
  MergeFrom(from);
}

void CTransportAuth_Authenticate_Response::CopyFrom(const CTransportAuth_Authenticate_Response& from) {
// @@protoc_insertion_point(class_specific_copy_from_start:CTransportAuth_Authenticate_Response)
  if (&from == this) return;
  Clear();
  MergeFrom(from);
}

bool CTransportAuth_Authenticate_Response::IsInitialized() const {
  return true;
}

void CTransportAuth_Authenticate_Response::InternalSwap(CTransportAuth_Authenticate_Response* other) {
  using std::swap;
  _internal_metadata_.Swap<::PROTOBUF_NAMESPACE_ID::UnknownFieldSet>(&other->_internal_metadata_);
}

::PROTOBUF_NAMESPACE_ID::Metadata CTransportAuth_Authenticate_Response::GetMetadata() const {
  return GetMetadataStatic();
}


// ===================================================================

TransportAuth::~TransportAuth() {}

const ::PROTOBUF_NAMESPACE_ID::ServiceDescriptor* TransportAuth::descriptor() {
  ::PROTOBUF_NAMESPACE_ID::internal::AssignDescriptors(&descriptor_table_webuimessages_5ftransport_2eproto);
  return file_level_service_descriptors_webuimessages_5ftransport_2eproto[0];
}

const ::PROTOBUF_NAMESPACE_ID::ServiceDescriptor* TransportAuth::GetDescriptor() {
  return descriptor();
}

void TransportAuth::Authenticate(::PROTOBUF_NAMESPACE_ID::RpcController* controller,
                         const ::CTransportAuth_Authenticate_Request*,
                         ::CTransportAuth_Authenticate_Response*,
                         ::google::protobuf::Closure* done) {
  controller->SetFailed("Method Authenticate() not implemented.");
  done->Run();
}

void TransportAuth::CallMethod(const ::PROTOBUF_NAMESPACE_ID::MethodDescriptor* method,
                             ::PROTOBUF_NAMESPACE_ID::RpcController* controller,
                             const ::PROTOBUF_NAMESPACE_ID::Message* request,
                             ::PROTOBUF_NAMESPACE_ID::Message* response,
                             ::google::protobuf::Closure* done) {
  GOOGLE_DCHECK_EQ(method->service(), file_level_service_descriptors_webuimessages_5ftransport_2eproto[0]);
  switch(method->index()) {
    case 0:
      Authenticate(controller,
             ::PROTOBUF_NAMESPACE_ID::internal::DownCast<const ::CTransportAuth_Authenticate_Request*>(
                 request),
             ::PROTOBUF_NAMESPACE_ID::internal::DownCast<::CTransportAuth_Authenticate_Response*>(
                 response),
             done);
      break;
    default:
      GOOGLE_LOG(FATAL) << "Bad method index; this should never happen.";
      break;
  }
}

const ::PROTOBUF_NAMESPACE_ID::Message& TransportAuth::GetRequestPrototype(
    const ::PROTOBUF_NAMESPACE_ID::MethodDescriptor* method) const {
  GOOGLE_DCHECK_EQ(method->service(), descriptor());
  switch(method->index()) {
    case 0:
      return ::CTransportAuth_Authenticate_Request::default_instance();
    default:
      GOOGLE_LOG(FATAL) << "Bad method index; this should never happen.";
      return *::PROTOBUF_NAMESPACE_ID::MessageFactory::generated_factory()
          ->GetPrototype(method->input_type());
  }
}

const ::PROTOBUF_NAMESPACE_ID::Message& TransportAuth::GetResponsePrototype(
    const ::PROTOBUF_NAMESPACE_ID::MethodDescriptor* method) const {
  GOOGLE_DCHECK_EQ(method->service(), descriptor());
  switch(method->index()) {
    case 0:
      return ::CTransportAuth_Authenticate_Response::default_instance();
    default:
      GOOGLE_LOG(FATAL) << "Bad method index; this should never happen.";
      return *::PROTOBUF_NAMESPACE_ID::MessageFactory::generated_factory()
          ->GetPrototype(method->output_type());
  }
}

TransportAuth_Stub::TransportAuth_Stub(::PROTOBUF_NAMESPACE_ID::RpcChannel* channel)
  : channel_(channel), owns_channel_(false) {}
TransportAuth_Stub::TransportAuth_Stub(
    ::PROTOBUF_NAMESPACE_ID::RpcChannel* channel,
    ::PROTOBUF_NAMESPACE_ID::Service::ChannelOwnership ownership)
  : channel_(channel),
    owns_channel_(ownership == ::PROTOBUF_NAMESPACE_ID::Service::STUB_OWNS_CHANNEL) {}
TransportAuth_Stub::~TransportAuth_Stub() {
  if (owns_channel_) delete channel_;
}

void TransportAuth_Stub::Authenticate(::PROTOBUF_NAMESPACE_ID::RpcController* controller,
                              const ::CTransportAuth_Authenticate_Request* request,
                              ::CTransportAuth_Authenticate_Response* response,
                              ::google::protobuf::Closure* done) {
  channel_->CallMethod(descriptor()->method(0),
                       controller, request, response, done);
}

// @@protoc_insertion_point(namespace_scope)
PROTOBUF_NAMESPACE_OPEN
template<> PROTOBUF_NOINLINE ::CTransportAuth_Authenticate_Request* Arena::CreateMaybeMessage< ::CTransportAuth_Authenticate_Request >(Arena* arena) {
  return Arena::CreateMessageInternal< ::CTransportAuth_Authenticate_Request >(arena);
}
template<> PROTOBUF_NOINLINE ::CTransportAuth_Authenticate_Response* Arena::CreateMaybeMessage< ::CTransportAuth_Authenticate_Response >(Arena* arena) {
  return Arena::CreateMessageInternal< ::CTransportAuth_Authenticate_Response >(arena);
}
PROTOBUF_NAMESPACE_CLOSE

// @@protoc_insertion_point(global_scope)
#include <google/protobuf/port_undef.inc>