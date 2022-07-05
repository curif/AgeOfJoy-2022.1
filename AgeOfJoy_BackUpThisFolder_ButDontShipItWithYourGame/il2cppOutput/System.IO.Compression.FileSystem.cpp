#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>
#include <stdint.h>


template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};
struct InterfaceActionInvoker0
{
	typedef void (*Action)(void*, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		((Action)invokeData.methodPtr)(obj, invokeData.method);
	}
};
template <typename R>
struct InterfaceFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};

// System.Collections.Generic.Dictionary`2<System.Int32,System.Text.Encoding>
struct Dictionary_2_t87EDE08B2E48F793A22DE50D6B3CC2E7EBB2DB54;
// System.Collections.Generic.Dictionary`2<System.String,System.IO.Compression.ZipArchiveEntry>
struct Dictionary_2_tA1C10CF9D35962FF0289095A43DEE774B7F13C47;
// System.Collections.Generic.IEnumerator`1<System.Object>
struct IEnumerator_1_t43D2E4BA9246755F293DFA74F001FB1A70A648FD;
// System.Collections.Generic.IEnumerator`1<System.IO.Compression.ZipArchiveEntry>
struct IEnumerator_1_tEDCB8B779ED9EE7AB107A7258544B436ADF7E5DC;
// System.Collections.Generic.IList`1<System.IO.Compression.ZipArchiveEntry>
struct IList_1_t23F3847D3CFC14A502A761CA1A0BB8A437ADBBBA;
// System.Collections.Generic.List`1<System.IO.Compression.ZipArchiveEntry>
struct List_1_tA389B1109F43128C89C6358C6B56C819460CF5B3;
// System.Collections.Generic.List`1<System.IO.Compression.ZipGenericExtraField>
struct List_1_t0845A5AAFB6B816C6E2719A0588604CE3A080FDC;
// System.Collections.ObjectModel.ReadOnlyCollection`1<System.Object>
struct ReadOnlyCollection_1_t5397DF0DB61D1090E7BBC89395CECB8D020CED92;
// System.Collections.ObjectModel.ReadOnlyCollection`1<System.IO.Compression.ZipArchiveEntry>
struct ReadOnlyCollection_1_t15A54E961DBC027444DA89894B8AD689A38CE9AC;
// System.Byte[][]
struct ByteU5BU5DU5BU5D_t19A0C6D66F22DF673E9CDB37DEF566FE0EC947FA;
// System.Byte[]
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
// System.Char[]
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
// System.Int32[]
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
// System.IntPtr[]
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
// System.Diagnostics.StackTrace[]
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
// System.ArgumentNullException
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129;
// System.ArgumentOutOfRangeException
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F;
// System.IO.BinaryReader
struct BinaryReader_t9A6D85F0FE9AE4EBB5E8D66997DFD1D84939E158;
// System.Globalization.CodePageDataItem
struct CodePageDataItem_t52460FA30AE37F4F26ACB81055E58002262F19F2;
// System.Text.DecoderFallback
struct DecoderFallback_t7324102215E4ED41EC065C02EB501CB0BC23CD90;
// System.IO.DirectoryInfo
struct DirectoryInfo_tEAEEC018EB49B4A71907FFEAFE935FAA8F9C1FE2;
// System.Text.EncoderFallback
struct EncoderFallback_tD2C40CE114AA9D8E1F7196608B2D088548015293;
// System.Text.Encoding
struct Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095;
// System.IO.FileStream
struct FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8;
// System.Collections.IDictionary
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
// System.IO.IOException
struct IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910;
// System.IO.MemoryStream
struct MemoryStream_tAAED1B42172E3390584E4194308AB878E786AAC2;
// Microsoft.Win32.SafeHandles.SafeFileHandle
struct SafeFileHandle_t033FA6AAAC65F4BB25F4CBA9A242A58C95CD406E;
// System.Runtime.Serialization.SafeSerializationManager
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
// System.Threading.SemaphoreSlim
struct SemaphoreSlim_t0D5CB5685D9BFA5BF95CEC6E7395490F933E8DB2;
// System.IO.Stream
struct Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE;
// System.String
struct String_t;
// System.Void
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
// System.IO.Compression.ZipArchive
struct ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41;
// System.IO.Compression.ZipArchiveEntry
struct ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4;
// System.IO.Stream/ReadWriteTask
struct ReadWriteTask_t0821BF49EE38596C7734E86E1A6A39D769BE2C05;

IL2CPP_EXTERN_C RuntimeClass* ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Exception_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerator_1_tEDCB8B779ED9EE7AB107A7258544B436ADF7E5DC_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* RuntimeObject_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral168DFB0223A253D8C177CD2D6A0DBA1B0ECEFB96;
IL2CPP_EXTERN_C String_t* _stringLiteral42C85608AB661F2121C84F54255CBAFC5728CF77;
IL2CPP_EXTERN_C String_t* _stringLiteral525277D2F376C0B1449BE7FBA9294AC2AE03DFD7;
IL2CPP_EXTERN_C String_t* _stringLiteral6052AC80E29B425758A2997B53AC96858AD5CF27;
IL2CPP_EXTERN_C String_t* _stringLiteral66F9618FDA792CAB23AF2D7FFB50AB2D3E393DC5;
IL2CPP_EXTERN_C String_t* _stringLiteral7E28E9DF3E4EBB1EFADEE524D7CE7A4F5B1DE1CA;
IL2CPP_EXTERN_C String_t* _stringLiteral977466E2B0BB387B2215E6C982AE462F2C9AB959;
IL2CPP_EXTERN_C String_t* _stringLiteralAF248E82BE9EBA1ADBF067429FAEE5A5B6E05A74;
IL2CPP_EXTERN_C String_t* _stringLiteralE42E8BB820D4F7550A0F04619F4E15FDC56943B9;
IL2CPP_EXTERN_C const RuntimeMethod* ReadOnlyCollection_1_GetEnumerator_mCC70E8DC19E3118E9FFF0B2AD6DABA4C3A1D8BDF_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ZipFileExtensions_ExtractToDirectory_m46D41E7457D7E3421DF84AAF8D63BA62837C3A15_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ZipFileExtensions_ExtractToFile_m2F93A06EC42F278D4730A19F853132ABE35BB923_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ZipFile_ExtractToDirectory_m8B5CB439A85E06B249A98A7EBA2448BB6CE34D10_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ZipFile_Open_m0CAF94BA8CAA42F3062A39A9F137121D3453A6AB_RuntimeMethod_var;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;


IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// <Module>
struct U3CModuleU3E_t5C9D30A33D5BCEBCEE3B70E895505EE5A827FE73 
{
};

// System.Collections.ObjectModel.ReadOnlyCollection`1<System.IO.Compression.ZipArchiveEntry>
struct ReadOnlyCollection_1_t15A54E961DBC027444DA89894B8AD689A38CE9AC  : public RuntimeObject
{
	// System.Collections.Generic.IList`1<T> System.Collections.ObjectModel.ReadOnlyCollection`1::list
	RuntimeObject* ___list_0;
	// System.Object System.Collections.ObjectModel.ReadOnlyCollection`1::_syncRoot
	RuntimeObject* ____syncRoot_1;
};
struct Il2CppArrayBounds;

// System.Text.Encoding
struct Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095  : public RuntimeObject
{
	// System.Int32 System.Text.Encoding::m_codePage
	int32_t ___m_codePage_9;
	// System.Globalization.CodePageDataItem System.Text.Encoding::dataItem
	CodePageDataItem_t52460FA30AE37F4F26ACB81055E58002262F19F2* ___dataItem_10;
	// System.Boolean System.Text.Encoding::m_deserializedFromEverett
	bool ___m_deserializedFromEverett_11;
	// System.Boolean System.Text.Encoding::m_isReadOnly
	bool ___m_isReadOnly_12;
	// System.Text.EncoderFallback System.Text.Encoding::encoderFallback
	EncoderFallback_tD2C40CE114AA9D8E1F7196608B2D088548015293* ___encoderFallback_13;
	// System.Text.DecoderFallback System.Text.Encoding::decoderFallback
	DecoderFallback_t7324102215E4ED41EC065C02EB501CB0BC23CD90* ___decoderFallback_14;
};

struct Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095_StaticFields
{
	// System.Text.Encoding modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::defaultEncoding
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___defaultEncoding_0;
	// System.Text.Encoding modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::unicodeEncoding
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___unicodeEncoding_1;
	// System.Text.Encoding modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::bigEndianUnicode
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___bigEndianUnicode_2;
	// System.Text.Encoding modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::utf7Encoding
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___utf7Encoding_3;
	// System.Text.Encoding modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::utf8Encoding
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___utf8Encoding_4;
	// System.Text.Encoding modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::utf32Encoding
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___utf32Encoding_5;
	// System.Text.Encoding modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::asciiEncoding
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___asciiEncoding_6;
	// System.Text.Encoding modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::latin1Encoding
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___latin1Encoding_7;
	// System.Collections.Generic.Dictionary`2<System.Int32,System.Text.Encoding> modreq(System.Runtime.CompilerServices.IsVolatile) System.Text.Encoding::encodings
	Dictionary_2_t87EDE08B2E48F793A22DE50D6B3CC2E7EBB2DB54* ___encodings_8;
	// System.Object System.Text.Encoding::s_InternalSyncObject
	RuntimeObject* ___s_InternalSyncObject_15;
};

// System.MarshalByRefObject
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE  : public RuntimeObject
{
	// System.Object System.MarshalByRefObject::_identity
	RuntimeObject* ____identity_0;
};
// Native definition for P/Invoke marshalling of System.MarshalByRefObject
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE_marshaled_pinvoke
{
	Il2CppIUnknown* ____identity_0;
};
// Native definition for COM marshalling of System.MarshalByRefObject
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE_marshaled_com
{
	Il2CppIUnknown* ____identity_0;
};

// System.IO.Path
struct Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC  : public RuntimeObject
{
};

struct Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_StaticFields
{
	// System.Char[] System.IO.Path::InvalidPathChars
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___InvalidPathChars_0;
	// System.Char System.IO.Path::AltDirectorySeparatorChar
	Il2CppChar ___AltDirectorySeparatorChar_1;
	// System.Char System.IO.Path::DirectorySeparatorChar
	Il2CppChar ___DirectorySeparatorChar_2;
	// System.Char System.IO.Path::PathSeparator
	Il2CppChar ___PathSeparator_3;
	// System.String System.IO.Path::DirectorySeparatorStr
	String_t* ___DirectorySeparatorStr_4;
	// System.Char System.IO.Path::VolumeSeparatorChar
	Il2CppChar ___VolumeSeparatorChar_5;
	// System.Char[] System.IO.Path::PathSeparatorChars
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___PathSeparatorChars_6;
	// System.Boolean System.IO.Path::dirEqualsVolume
	bool ___dirEqualsVolume_7;
	// System.Char[] System.IO.Path::trimEndCharsWindows
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___trimEndCharsWindows_8;
	// System.Char[] System.IO.Path::trimEndCharsUnix
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___trimEndCharsUnix_9;
};

// System.IO.PathInternal
struct PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0  : public RuntimeObject
{
};

struct PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_StaticFields
{
	// System.Boolean System.IO.PathInternal::s_isCaseSensitive
	bool ___s_isCaseSensitive_0;
};

// System.String
struct String_t  : public RuntimeObject
{
	// System.Int32 System.String::_stringLength
	int32_t ____stringLength_4;
	// System.Char System.String::_firstChar
	Il2CppChar ____firstChar_5;
};

struct String_t_StaticFields
{
	// System.String System.String::Empty
	String_t* ___Empty_6;
};

// System.ValueType
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
// Native definition for P/Invoke marshalling of System.ValueType
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
// Native definition for COM marshalling of System.ValueType
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};

// System.IO.Compression.ZipArchive
struct ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41  : public RuntimeObject
{
	// System.IO.Stream System.IO.Compression.ZipArchive::_archiveStream
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ____archiveStream_0;
	// System.IO.Compression.ZipArchiveEntry System.IO.Compression.ZipArchive::_archiveStreamOwner
	ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* ____archiveStreamOwner_1;
	// System.IO.BinaryReader System.IO.Compression.ZipArchive::_archiveReader
	BinaryReader_t9A6D85F0FE9AE4EBB5E8D66997DFD1D84939E158* ____archiveReader_2;
	// System.IO.Compression.ZipArchiveMode System.IO.Compression.ZipArchive::_mode
	int32_t ____mode_3;
	// System.Collections.Generic.List`1<System.IO.Compression.ZipArchiveEntry> System.IO.Compression.ZipArchive::_entries
	List_1_tA389B1109F43128C89C6358C6B56C819460CF5B3* ____entries_4;
	// System.Collections.ObjectModel.ReadOnlyCollection`1<System.IO.Compression.ZipArchiveEntry> System.IO.Compression.ZipArchive::_entriesCollection
	ReadOnlyCollection_1_t15A54E961DBC027444DA89894B8AD689A38CE9AC* ____entriesCollection_5;
	// System.Collections.Generic.Dictionary`2<System.String,System.IO.Compression.ZipArchiveEntry> System.IO.Compression.ZipArchive::_entriesDictionary
	Dictionary_2_tA1C10CF9D35962FF0289095A43DEE774B7F13C47* ____entriesDictionary_6;
	// System.Boolean System.IO.Compression.ZipArchive::_readEntries
	bool ____readEntries_7;
	// System.Boolean System.IO.Compression.ZipArchive::_leaveOpen
	bool ____leaveOpen_8;
	// System.Int64 System.IO.Compression.ZipArchive::_centralDirectoryStart
	int64_t ____centralDirectoryStart_9;
	// System.Boolean System.IO.Compression.ZipArchive::_isDisposed
	bool ____isDisposed_10;
	// System.UInt32 System.IO.Compression.ZipArchive::_numberOfThisDisk
	uint32_t ____numberOfThisDisk_11;
	// System.Int64 System.IO.Compression.ZipArchive::_expectedNumberOfEntries
	int64_t ____expectedNumberOfEntries_12;
	// System.IO.Stream System.IO.Compression.ZipArchive::_backingStream
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ____backingStream_13;
	// System.Byte[] System.IO.Compression.ZipArchive::_archiveComment
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ____archiveComment_14;
	// System.Text.Encoding System.IO.Compression.ZipArchive::_entryNameEncoding
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ____entryNameEncoding_15;
};

// System.IO.Compression.ZipFile
struct ZipFile_t18C9C6BB8ABF9F2AE85A0EB6FF918378A5745CC8  : public RuntimeObject
{
};

// System.IO.Compression.ZipFileExtensions
struct ZipFileExtensions_t06E75DEE7D8E222C1E0E563C2393652E251FD218  : public RuntimeObject
{
};

// System.Nullable`1<System.IO.Compression.CompressionLevel>
struct Nullable_1_tAC8899D7718BEF36A8590184EFBCA842A1BC9AB1 
{
	// System.Boolean System.Nullable`1::hasValue
	bool ___hasValue_0;
	// T System.Nullable`1::value
	int32_t ___value_1;
};

// System.Nullable`1<System.Int64>
struct Nullable_1_t365991B3904FDA7642A788423B28692FDC7CDB17 
{
	// System.Boolean System.Nullable`1::hasValue
	bool ___hasValue_0;
	// T System.Nullable`1::value
	int64_t ___value_1;
};

// System.Boolean
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	// System.Boolean System.Boolean::m_value
	bool ___m_value_0;
};

struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	// System.String System.Boolean::TrueString
	String_t* ___TrueString_5;
	// System.String System.Boolean::FalseString
	String_t* ___FalseString_6;
};

// System.Char
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17 
{
	// System.Char System.Char::m_value
	Il2CppChar ___m_value_0;
};

struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17_StaticFields
{
	// System.Byte[] System.Char::s_categoryForLatin1
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___s_categoryForLatin1_3;
};

// System.DateTime
struct DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D 
{
	// System.UInt64 System.DateTime::_dateData
	uint64_t ____dateData_46;
};

struct DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D_StaticFields
{
	// System.Int32[] System.DateTime::s_daysToMonth365
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___s_daysToMonth365_30;
	// System.Int32[] System.DateTime::s_daysToMonth366
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___s_daysToMonth366_31;
	// System.DateTime System.DateTime::MinValue
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___MinValue_32;
	// System.DateTime System.DateTime::MaxValue
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___MaxValue_33;
	// System.DateTime System.DateTime::UnixEpoch
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___UnixEpoch_34;
};

// System.Guid
struct Guid_t 
{
	// System.Int32 System.Guid::_a
	int32_t ____a_1;
	// System.Int16 System.Guid::_b
	int16_t ____b_2;
	// System.Int16 System.Guid::_c
	int16_t ____c_3;
	// System.Byte System.Guid::_d
	uint8_t ____d_4;
	// System.Byte System.Guid::_e
	uint8_t ____e_5;
	// System.Byte System.Guid::_f
	uint8_t ____f_6;
	// System.Byte System.Guid::_g
	uint8_t ____g_7;
	// System.Byte System.Guid::_h
	uint8_t ____h_8;
	// System.Byte System.Guid::_i
	uint8_t ____i_9;
	// System.Byte System.Guid::_j
	uint8_t ____j_10;
	// System.Byte System.Guid::_k
	uint8_t ____k_11;
};

struct Guid_t_StaticFields
{
	// System.Guid System.Guid::Empty
	Guid_t ___Empty_0;
};

// System.Int32
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	// System.Int32 System.Int32::m_value
	int32_t ___m_value_0;
};

// System.Int64
struct Int64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3 
{
	// System.Int64 System.Int64::m_value
	int64_t ___m_value_0;
};

// System.IntPtr
struct IntPtr_t 
{
	// System.Void* System.IntPtr::m_value
	void* ___m_value_0;
};

struct IntPtr_t_StaticFields
{
	// System.IntPtr System.IntPtr::Zero
	intptr_t ___Zero_1;
};

// System.IO.Stream
struct Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE  : public MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE
{
	// System.IO.Stream/ReadWriteTask System.IO.Stream::_activeReadWriteTask
	ReadWriteTask_t0821BF49EE38596C7734E86E1A6A39D769BE2C05* ____activeReadWriteTask_3;
	// System.Threading.SemaphoreSlim System.IO.Stream::_asyncActiveSemaphore
	SemaphoreSlim_t0D5CB5685D9BFA5BF95CEC6E7395490F933E8DB2* ____asyncActiveSemaphore_4;
};

struct Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_StaticFields
{
	// System.IO.Stream System.IO.Stream::Null
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___Null_1;
};

// System.Void
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};

// Interop/Sys/FileStatus
struct FileStatus_tCB96EDE0D0F945F685B9BBED6DBF0731207458C2 
{
	// Interop/Sys/FileStatusFlags Interop/Sys/FileStatus::Flags
	int32_t ___Flags_0;
	// System.Int32 Interop/Sys/FileStatus::Mode
	int32_t ___Mode_1;
	// System.UInt32 Interop/Sys/FileStatus::Uid
	uint32_t ___Uid_2;
	// System.UInt32 Interop/Sys/FileStatus::Gid
	uint32_t ___Gid_3;
	// System.Int64 Interop/Sys/FileStatus::Size
	int64_t ___Size_4;
	// System.Int64 Interop/Sys/FileStatus::ATime
	int64_t ___ATime_5;
	// System.Int64 Interop/Sys/FileStatus::ATimeNsec
	int64_t ___ATimeNsec_6;
	// System.Int64 Interop/Sys/FileStatus::MTime
	int64_t ___MTime_7;
	// System.Int64 Interop/Sys/FileStatus::MTimeNsec
	int64_t ___MTimeNsec_8;
	// System.Int64 Interop/Sys/FileStatus::CTime
	int64_t ___CTime_9;
	// System.Int64 Interop/Sys/FileStatus::CTimeNsec
	int64_t ___CTimeNsec_10;
	// System.Int64 Interop/Sys/FileStatus::BirthTime
	int64_t ___BirthTime_11;
	// System.Int64 Interop/Sys/FileStatus::BirthTimeNsec
	int64_t ___BirthTimeNsec_12;
	// System.Int64 Interop/Sys/FileStatus::Dev
	int64_t ___Dev_13;
	// System.Int64 Interop/Sys/FileStatus::Ino
	int64_t ___Ino_14;
	// System.UInt32 Interop/Sys/FileStatus::UserFlags
	uint32_t ___UserFlags_15;
};

// System.DateTimeOffset
struct DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 
{
	// System.DateTime System.DateTimeOffset::_dateTime
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ____dateTime_3;
	// System.Int16 System.DateTimeOffset::_offsetMinutes
	int16_t ____offsetMinutes_4;
};

struct DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4_StaticFields
{
	// System.DateTimeOffset System.DateTimeOffset::MinValue
	DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 ___MinValue_0;
	// System.DateTimeOffset System.DateTimeOffset::MaxValue
	DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 ___MaxValue_1;
	// System.DateTimeOffset System.DateTimeOffset::UnixEpoch
	DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 ___UnixEpoch_2;
};

// System.Exception
struct Exception_t  : public RuntimeObject
{
	// System.String System.Exception::_className
	String_t* ____className_1;
	// System.String System.Exception::_message
	String_t* ____message_2;
	// System.Collections.IDictionary System.Exception::_data
	RuntimeObject* ____data_3;
	// System.Exception System.Exception::_innerException
	Exception_t* ____innerException_4;
	// System.String System.Exception::_helpURL
	String_t* ____helpURL_5;
	// System.Object System.Exception::_stackTrace
	RuntimeObject* ____stackTrace_6;
	// System.String System.Exception::_stackTraceString
	String_t* ____stackTraceString_7;
	// System.String System.Exception::_remoteStackTraceString
	String_t* ____remoteStackTraceString_8;
	// System.Int32 System.Exception::_remoteStackIndex
	int32_t ____remoteStackIndex_9;
	// System.Object System.Exception::_dynamicMethods
	RuntimeObject* ____dynamicMethods_10;
	// System.Int32 System.Exception::_HResult
	int32_t ____HResult_11;
	// System.String System.Exception::_source
	String_t* ____source_12;
	// System.Runtime.Serialization.SafeSerializationManager System.Exception::_safeSerializationManager
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager_13;
	// System.Diagnostics.StackTrace[] System.Exception::captured_traces
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces_14;
	// System.IntPtr[] System.Exception::native_trace_ips
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips_15;
	// System.Int32 System.Exception::caught_in_unmanaged
	int32_t ___caught_in_unmanaged_16;
};

struct Exception_t_StaticFields
{
	// System.Object System.Exception::s_EDILock
	RuntimeObject* ___s_EDILock_0;
};
// Native definition for P/Invoke marshalling of System.Exception
struct Exception_t_marshaled_pinvoke
{
	char* ____className_1;
	char* ____message_2;
	RuntimeObject* ____data_3;
	Exception_t_marshaled_pinvoke* ____innerException_4;
	char* ____helpURL_5;
	Il2CppIUnknown* ____stackTrace_6;
	char* ____stackTraceString_7;
	char* ____remoteStackTraceString_8;
	int32_t ____remoteStackIndex_9;
	Il2CppIUnknown* ____dynamicMethods_10;
	int32_t ____HResult_11;
	char* ____source_12;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager_13;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces_14;
	Il2CppSafeArray/*NONE*/* ___native_trace_ips_15;
	int32_t ___caught_in_unmanaged_16;
};
// Native definition for COM marshalling of System.Exception
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className_1;
	Il2CppChar* ____message_2;
	RuntimeObject* ____data_3;
	Exception_t_marshaled_com* ____innerException_4;
	Il2CppChar* ____helpURL_5;
	Il2CppIUnknown* ____stackTrace_6;
	Il2CppChar* ____stackTraceString_7;
	Il2CppChar* ____remoteStackTraceString_8;
	int32_t ____remoteStackIndex_9;
	Il2CppIUnknown* ____dynamicMethods_10;
	int32_t ____HResult_11;
	Il2CppChar* ____source_12;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager_13;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces_14;
	Il2CppSafeArray/*NONE*/* ___native_trace_ips_15;
	int32_t ___caught_in_unmanaged_16;
};

// System.IO.FileStatus
struct FileStatus_tABB5F252F1E597EC95E9041035DC424EF66712A5 
{
	// Interop/Sys/FileStatus System.IO.FileStatus::_fileStatus
	FileStatus_tCB96EDE0D0F945F685B9BBED6DBF0731207458C2 ____fileStatus_0;
	// System.Int32 System.IO.FileStatus::_fileStatusInitialized
	int32_t ____fileStatusInitialized_1;
	// System.Boolean System.IO.FileStatus::<InitiallyDirectory>k__BackingField
	bool ___U3CInitiallyDirectoryU3Ek__BackingField_2;
	// System.Boolean System.IO.FileStatus::_isDirectory
	bool ____isDirectory_3;
	// System.Boolean System.IO.FileStatus::_exists
	bool ____exists_4;
};
// Native definition for P/Invoke marshalling of System.IO.FileStatus
struct FileStatus_tABB5F252F1E597EC95E9041035DC424EF66712A5_marshaled_pinvoke
{
	FileStatus_tCB96EDE0D0F945F685B9BBED6DBF0731207458C2 ____fileStatus_0;
	int32_t ____fileStatusInitialized_1;
	int32_t ___U3CInitiallyDirectoryU3Ek__BackingField_2;
	int32_t ____isDirectory_3;
	int32_t ____exists_4;
};
// Native definition for COM marshalling of System.IO.FileStatus
struct FileStatus_tABB5F252F1E597EC95E9041035DC424EF66712A5_marshaled_com
{
	FileStatus_tCB96EDE0D0F945F685B9BBED6DBF0731207458C2 ____fileStatus_0;
	int32_t ____fileStatusInitialized_1;
	int32_t ___U3CInitiallyDirectoryU3Ek__BackingField_2;
	int32_t ____isDirectory_3;
	int32_t ____exists_4;
};

// System.IO.FileStream
struct FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8  : public Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE
{
	// System.Byte[] System.IO.FileStream::buf
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___buf_7;
	// System.String System.IO.FileStream::name
	String_t* ___name_8;
	// Microsoft.Win32.SafeHandles.SafeFileHandle System.IO.FileStream::safeHandle
	SafeFileHandle_t033FA6AAAC65F4BB25F4CBA9A242A58C95CD406E* ___safeHandle_9;
	// System.Boolean System.IO.FileStream::isExposed
	bool ___isExposed_10;
	// System.Int64 System.IO.FileStream::append_startpos
	int64_t ___append_startpos_11;
	// System.IO.FileAccess System.IO.FileStream::access
	int32_t ___access_12;
	// System.Boolean System.IO.FileStream::owner
	bool ___owner_13;
	// System.Boolean System.IO.FileStream::async
	bool ___async_14;
	// System.Boolean System.IO.FileStream::canseek
	bool ___canseek_15;
	// System.Boolean System.IO.FileStream::anonymous
	bool ___anonymous_16;
	// System.Boolean System.IO.FileStream::buf_dirty
	bool ___buf_dirty_17;
	// System.Int32 System.IO.FileStream::buf_size
	int32_t ___buf_size_18;
	// System.Int32 System.IO.FileStream::buf_length
	int32_t ___buf_length_19;
	// System.Int32 System.IO.FileStream::buf_offset
	int32_t ___buf_offset_20;
	// System.Int64 System.IO.FileStream::buf_start
	int64_t ___buf_start_21;
};

struct FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8_StaticFields
{
	// System.Byte[] System.IO.FileStream::buf_recycle
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___buf_recycle_5;
	// System.Object System.IO.FileStream::buf_recycle_lock
	RuntimeObject* ___buf_recycle_lock_6;
};

// System.IO.FileSystemInfo
struct FileSystemInfo_tE3063B9229F46B05A5F6D018C8C4CA510104E8E9  : public MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE
{
	// System.IO.FileStatus System.IO.FileSystemInfo::_fileStatus
	FileStatus_tABB5F252F1E597EC95E9041035DC424EF66712A5 ____fileStatus_1;
	// System.String System.IO.FileSystemInfo::FullPath
	String_t* ___FullPath_2;
	// System.String System.IO.FileSystemInfo::OriginalPath
	String_t* ___OriginalPath_3;
	// System.String System.IO.FileSystemInfo::_name
	String_t* ____name_4;
};

// System.SystemException
struct SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295  : public Exception_t
{
};

// System.IO.Compression.ZipArchiveEntry
struct ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4  : public RuntimeObject
{
	// System.IO.Compression.ZipArchive System.IO.Compression.ZipArchiveEntry::_archive
	ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* ____archive_0;
	// System.Boolean System.IO.Compression.ZipArchiveEntry::_originallyInArchive
	bool ____originallyInArchive_1;
	// System.Int32 System.IO.Compression.ZipArchiveEntry::_diskNumberStart
	int32_t ____diskNumberStart_2;
	// System.IO.Compression.ZipVersionMadeByPlatform System.IO.Compression.ZipArchiveEntry::_versionMadeByPlatform
	uint8_t ____versionMadeByPlatform_3;
	// System.IO.Compression.ZipVersionNeededValues System.IO.Compression.ZipArchiveEntry::_versionMadeBySpecification
	uint16_t ____versionMadeBySpecification_4;
	// System.IO.Compression.ZipVersionNeededValues System.IO.Compression.ZipArchiveEntry::_versionToExtract
	uint16_t ____versionToExtract_5;
	// System.IO.Compression.ZipArchiveEntry/BitFlagValues System.IO.Compression.ZipArchiveEntry::_generalPurposeBitFlag
	uint16_t ____generalPurposeBitFlag_6;
	// System.IO.Compression.ZipArchiveEntry/CompressionMethodValues System.IO.Compression.ZipArchiveEntry::_storedCompressionMethod
	uint16_t ____storedCompressionMethod_7;
	// System.DateTimeOffset System.IO.Compression.ZipArchiveEntry::_lastModified
	DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 ____lastModified_8;
	// System.Int64 System.IO.Compression.ZipArchiveEntry::_compressedSize
	int64_t ____compressedSize_9;
	// System.Int64 System.IO.Compression.ZipArchiveEntry::_uncompressedSize
	int64_t ____uncompressedSize_10;
	// System.Int64 System.IO.Compression.ZipArchiveEntry::_offsetOfLocalHeader
	int64_t ____offsetOfLocalHeader_11;
	// System.Nullable`1<System.Int64> System.IO.Compression.ZipArchiveEntry::_storedOffsetOfCompressedData
	Nullable_1_t365991B3904FDA7642A788423B28692FDC7CDB17 ____storedOffsetOfCompressedData_12;
	// System.UInt32 System.IO.Compression.ZipArchiveEntry::_crc32
	uint32_t ____crc32_13;
	// System.Byte[][] System.IO.Compression.ZipArchiveEntry::_compressedBytes
	ByteU5BU5DU5BU5D_t19A0C6D66F22DF673E9CDB37DEF566FE0EC947FA* ____compressedBytes_14;
	// System.IO.MemoryStream System.IO.Compression.ZipArchiveEntry::_storedUncompressedData
	MemoryStream_tAAED1B42172E3390584E4194308AB878E786AAC2* ____storedUncompressedData_15;
	// System.Boolean System.IO.Compression.ZipArchiveEntry::_currentlyOpenForWrite
	bool ____currentlyOpenForWrite_16;
	// System.Boolean System.IO.Compression.ZipArchiveEntry::_everOpenedForWrite
	bool ____everOpenedForWrite_17;
	// System.IO.Stream System.IO.Compression.ZipArchiveEntry::_outstandingWriteStream
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ____outstandingWriteStream_18;
	// System.UInt32 System.IO.Compression.ZipArchiveEntry::_externalFileAttr
	uint32_t ____externalFileAttr_19;
	// System.String System.IO.Compression.ZipArchiveEntry::_storedEntryName
	String_t* ____storedEntryName_20;
	// System.Byte[] System.IO.Compression.ZipArchiveEntry::_storedEntryNameBytes
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ____storedEntryNameBytes_21;
	// System.Collections.Generic.List`1<System.IO.Compression.ZipGenericExtraField> System.IO.Compression.ZipArchiveEntry::_cdUnknownExtraFields
	List_1_t0845A5AAFB6B816C6E2719A0588604CE3A080FDC* ____cdUnknownExtraFields_22;
	// System.Collections.Generic.List`1<System.IO.Compression.ZipGenericExtraField> System.IO.Compression.ZipArchiveEntry::_lhUnknownExtraFields
	List_1_t0845A5AAFB6B816C6E2719A0588604CE3A080FDC* ____lhUnknownExtraFields_23;
	// System.Byte[] System.IO.Compression.ZipArchiveEntry::_fileComment
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ____fileComment_24;
	// System.Nullable`1<System.IO.Compression.CompressionLevel> System.IO.Compression.ZipArchiveEntry::_compressionLevel
	Nullable_1_tAC8899D7718BEF36A8590184EFBCA842A1BC9AB1 ____compressionLevel_25;
};

struct ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4_StaticFields
{
	// System.Boolean System.IO.Compression.ZipArchiveEntry::s_allowLargeZipArchiveEntriesInUpdateMode
	bool ___s_allowLargeZipArchiveEntriesInUpdateMode_26;
	// System.IO.Compression.ZipVersionMadeByPlatform System.IO.Compression.ZipArchiveEntry::CurrentZipPlatform
	uint8_t ___CurrentZipPlatform_27;
};

// System.ArgumentException
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
	// System.String System.ArgumentException::_paramName
	String_t* ____paramName_18;
};

// System.IO.DirectoryInfo
struct DirectoryInfo_tEAEEC018EB49B4A71907FFEAFE935FAA8F9C1FE2  : public FileSystemInfo_tE3063B9229F46B05A5F6D018C8C4CA510104E8E9
{
};

// System.IO.IOException
struct IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};

// System.ArgumentNullException
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
};

// System.ArgumentOutOfRangeException
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
	// System.Object System.ArgumentOutOfRangeException::_actualValue
	RuntimeObject* ____actualValue_19;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif


// System.Collections.Generic.IEnumerator`1<T> System.Collections.ObjectModel.ReadOnlyCollection`1<System.Object>::GetEnumerator()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* ReadOnlyCollection_1_GetEnumerator_m487A0501D6F875A04F7D8F93F1CB3C813994AA64_gshared (ReadOnlyCollection_1_t5397DF0DB61D1090E7BBC89395CECB8D020CED92* __this, const RuntimeMethod* method) ;

// System.String System.IO.Path::GetTempPath()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Path_GetTempPath_mDA8E37E0E953CA9E70CD0953777615F2C2FFA3B3 (const RuntimeMethod* method) ;
// System.Guid System.Guid::NewGuid()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Guid_t Guid_NewGuid_m1827D92D71326C3F3C263F057F6E90F907617903 (const RuntimeMethod* method) ;
// System.String System.Guid::ToString(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Guid_ToString_mDAA91A4A993E3A7AD8339665E3F0CC35FE00E833 (Guid_t* __this, String_t* ___format0, const RuntimeMethod* method) ;
// System.String System.String::Concat(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_mAF2CE02CC0CB7460753D0A1A91CCF2B1E9804C5D (String_t* ___str00, String_t* ___str11, const RuntimeMethod* method) ;
// System.String System.IO.Path::Combine(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Path_Combine_m64754D4E08990CE1EBC41CDF197807EE4B115474 (String_t* ___path10, String_t* ___path21, const RuntimeMethod* method) ;
// System.Void System.IO.FileStream::.ctor(System.String,System.IO.FileMode,System.IO.FileAccess,System.IO.FileShare,System.Int32,System.IO.FileOptions)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void FileStream__ctor_mCF0C1E859853B23725D0048DEA0653A759A5E657 (FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* __this, String_t* ___path0, int32_t ___mode1, int32_t ___access2, int32_t ___share3, int32_t ___bufferSize4, int32_t ___options5, const RuntimeMethod* method) ;
// System.String System.String::ToLowerInvariant()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_ToLowerInvariant_mBE32C93DE27C5353FEA3FA654FC1DDBE3D0EB0F2 (String_t* __this, const RuntimeMethod* method) ;
// System.Boolean System.IO.File::Exists(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool File_Exists_mD7E7A84A6B9E9A9BADBDA7C46AAE0624EF106D85 (String_t* ___path0, const RuntimeMethod* method) ;
// System.Boolean System.IO.PathInternal::GetIsCaseSensitive()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool PathInternal_GetIsCaseSensitive_mFEAF79505BAF85C91DAAA32D13D34101DC3D73A5 (const RuntimeMethod* method) ;
// System.Void System.ArgumentOutOfRangeException::.ctor(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentOutOfRangeException__ctor_mBC1D5DEEA1BA41DE77228CB27D6BAFEB6DCCBF4A (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* __this, String_t* ___paramName0, const RuntimeMethod* method) ;
// System.Void System.IO.FileStream::.ctor(System.String,System.IO.FileMode,System.IO.FileAccess,System.IO.FileShare,System.Int32,System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void FileStream__ctor_mB51E4FD96A6B396795C835EFD7B0F0018A3A5029 (FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* __this, String_t* ___path0, int32_t ___mode1, int32_t ___access2, int32_t ___share3, int32_t ___bufferSize4, bool ___useAsync5, const RuntimeMethod* method) ;
// System.Void System.IO.Compression.ZipArchive::.ctor(System.IO.Stream,System.IO.Compression.ZipArchiveMode,System.Boolean,System.Text.Encoding)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipArchive__ctor_m2706DA413E897A83057237178CCC8E51C07230B5 (ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* __this, Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___stream0, int32_t ___mode1, bool ___leaveOpen2, Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___entryNameEncoding3, const RuntimeMethod* method) ;
// System.Void System.IO.Stream::Dispose()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stream_Dispose_mCDB42F32A17541CCA6D3A5906827A401570B07A8 (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* __this, const RuntimeMethod* method) ;
// System.Void System.IO.Compression.ZipFile::ExtractToDirectory(System.String,System.String,System.Text.Encoding)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFile_ExtractToDirectory_mEF39886577A8E2114AEE60C8617E830A1F1DAA53 (String_t* ___sourceArchiveFileName0, String_t* ___destinationDirectoryName1, Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___entryNameEncoding2, const RuntimeMethod* method) ;
// System.Void System.IO.Compression.ZipFile::ExtractToDirectory(System.String,System.String,System.Text.Encoding,System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFile_ExtractToDirectory_m8B5CB439A85E06B249A98A7EBA2448BB6CE34D10 (String_t* ___sourceArchiveFileName0, String_t* ___destinationDirectoryName1, Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___entryNameEncoding2, bool ___overwrite3, const RuntimeMethod* method) ;
// System.Void System.ArgumentNullException::.ctor(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* __this, String_t* ___paramName0, const RuntimeMethod* method) ;
// System.IO.Compression.ZipArchive System.IO.Compression.ZipFile::Open(System.String,System.IO.Compression.ZipArchiveMode,System.Text.Encoding)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* ZipFile_Open_m0CAF94BA8CAA42F3062A39A9F137121D3453A6AB (String_t* ___archiveFileName0, int32_t ___mode1, Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___entryNameEncoding2, const RuntimeMethod* method) ;
// System.Void System.IO.Compression.ZipFileExtensions::ExtractToDirectory(System.IO.Compression.ZipArchive,System.String,System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFileExtensions_ExtractToDirectory_m46D41E7457D7E3421DF84AAF8D63BA62837C3A15 (ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* ___source0, String_t* ___destinationDirectoryName1, bool ___overwrite2, const RuntimeMethod* method) ;
// System.IO.DirectoryInfo System.IO.Directory::CreateDirectory(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR DirectoryInfo_tEAEEC018EB49B4A71907FFEAFE935FAA8F9C1FE2* Directory_CreateDirectory_mD89FECDFB25BC52F866DC0B1BB8552334FB249D2 (String_t* ___path0, const RuntimeMethod* method) ;
// System.Boolean System.String::EndsWith(System.Char)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_EndsWith_m1345909BD17FAD2AE0F70BC1B5CFC2010CF226B0 (String_t* __this, Il2CppChar ___value0, const RuntimeMethod* method) ;
// System.String System.Char::ToString()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Char_ToString_m2A308731F9577C06AF3C0901234E2EAC8327410C (Il2CppChar* __this, const RuntimeMethod* method) ;
// System.Collections.ObjectModel.ReadOnlyCollection`1<System.IO.Compression.ZipArchiveEntry> System.IO.Compression.ZipArchive::get_Entries()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlyCollection_1_t15A54E961DBC027444DA89894B8AD689A38CE9AC* ZipArchive_get_Entries_m2BB23EB2041B6BBD85AF079BEBEA8DEEA0EEEF88 (ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* __this, const RuntimeMethod* method) ;
// System.Collections.Generic.IEnumerator`1<T> System.Collections.ObjectModel.ReadOnlyCollection`1<System.IO.Compression.ZipArchiveEntry>::GetEnumerator()
inline RuntimeObject* ReadOnlyCollection_1_GetEnumerator_mCC70E8DC19E3118E9FFF0B2AD6DABA4C3A1D8BDF (ReadOnlyCollection_1_t15A54E961DBC027444DA89894B8AD689A38CE9AC* __this, const RuntimeMethod* method)
{
	return ((  RuntimeObject* (*) (ReadOnlyCollection_1_t15A54E961DBC027444DA89894B8AD689A38CE9AC*, const RuntimeMethod*))ReadOnlyCollection_1_GetEnumerator_m487A0501D6F875A04F7D8F93F1CB3C813994AA64_gshared)(__this, method);
}
// System.String System.IO.Compression.ZipArchiveEntry::get_FullName()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* ZipArchiveEntry_get_FullName_mB226F80A14EA72D5C3D63C912AD483020CE81F2F_inline (ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* __this, const RuntimeMethod* method) ;
// System.String System.IO.Path::GetFullPath(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Path_GetFullPath_m17A1AD4E216D884E3DF3208BF44F4E40823BAA23 (String_t* ___path0, const RuntimeMethod* method) ;
// System.StringComparison System.IO.PathInternal::get_StringComparison()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t PathInternal_get_StringComparison_mE5AE0510D4EEA560F43DD0DEA2BF38D9EB998698 (const RuntimeMethod* method) ;
// System.Boolean System.String::StartsWith(System.String,System.StringComparison)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_StartsWith_mA2A4405B1B9F3653A6A9AA7F223F68D86A0C6264 (String_t* __this, String_t* ___value0, int32_t ___comparisonType1, const RuntimeMethod* method) ;
// System.Void System.IO.IOException::.ctor(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void IOException__ctor_mE0612A16064F93C7EBB468D6874777BD70CB50CA (IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910* __this, String_t* ___message0, const RuntimeMethod* method) ;
// System.String System.IO.Path::GetFileName(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Path_GetFileName_mEBC73E0C8D8C56214D1DA4BA8409C5B5F00457A5 (String_t* ___path0, const RuntimeMethod* method) ;
// System.Int32 System.String::get_Length()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) ;
// System.Int64 System.IO.Compression.ZipArchiveEntry::get_Length()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t ZipArchiveEntry_get_Length_m4660921EEC25DF03D255896508A7BD1EEC6C7192 (ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* __this, const RuntimeMethod* method) ;
// System.String System.IO.Path::GetDirectoryName(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Path_GetDirectoryName_mB9369289430566A15BB0A0CFCCBED3C6ECA7F30C (String_t* ___path0, const RuntimeMethod* method) ;
// System.Void System.IO.Compression.ZipFileExtensions::ExtractToFile(System.IO.Compression.ZipArchiveEntry,System.String,System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFileExtensions_ExtractToFile_m2F93A06EC42F278D4730A19F853132ABE35BB923 (ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* ___source0, String_t* ___destinationFileName1, bool ___overwrite2, const RuntimeMethod* method) ;
// System.IO.Stream System.IO.Compression.ZipArchiveEntry::Open()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ZipArchiveEntry_Open_m1D0EFB9AD33BA96AAF0C624EADA3E58CD6CC67FF (ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* __this, const RuntimeMethod* method) ;
// System.Void System.IO.Stream::CopyTo(System.IO.Stream)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stream_CopyTo_m61DC54FF3708C2B8AB5C5D63D300AA57ADA01999 (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* __this, Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___destination0, const RuntimeMethod* method) ;
// System.DateTimeOffset System.IO.Compression.ZipArchiveEntry::get_LastWriteTime()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 ZipArchiveEntry_get_LastWriteTime_m98EB193BEB589BE1739A89C6104830F11573AFE9_inline (ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* __this, const RuntimeMethod* method) ;
// System.DateTime System.DateTimeOffset::get_DateTime()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D DateTimeOffset_get_DateTime_mDF6DC57E7A5647D8B964D3FD5B6855E7D66EF324 (DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4* __this, const RuntimeMethod* method) ;
// System.Void System.IO.File::SetLastWriteTime(System.String,System.DateTime)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void File_SetLastWriteTime_m444C27684387AD8DC03E739B9CCAA4927C75FDBC (String_t* ___path0, DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___lastWriteTime1, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// System.StringComparison System.IO.PathInternal::get_StringComparison()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t PathInternal_get_StringComparison_mE5AE0510D4EEA560F43DD0DEA2BF38D9EB998698 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_il2cpp_TypeInfo_var);
		bool L_0 = ((PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_StaticFields*)il2cpp_codegen_static_fields_for(PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_il2cpp_TypeInfo_var))->___s_isCaseSensitive_0;
		if (L_0)
		{
			goto IL_0009;
		}
	}
	{
		return (int32_t)(5);
	}

IL_0009:
	{
		return (int32_t)(4);
	}
}
// System.Boolean System.IO.PathInternal::GetIsCaseSensitive()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool PathInternal_GetIsCaseSensitive_mFEAF79505BAF85C91DAAA32D13D34101DC3D73A5 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral525277D2F376C0B1449BE7FBA9294AC2AE03DFD7);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE42E8BB820D4F7550A0F04619F4E15FDC56943B9);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	Guid_t V_1;
	memset((&V_1), 0, sizeof(V_1));
	FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* V_2 = NULL;
	bool V_3 = false;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	try
	{// begin try (depth: 1)
		{
			il2cpp_codegen_runtime_class_init_inline(Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var);
			String_t* L_0;
			L_0 = Path_GetTempPath_mDA8E37E0E953CA9E70CD0953777615F2C2FFA3B3(NULL);
			Guid_t L_1;
			L_1 = Guid_NewGuid_m1827D92D71326C3F3C263F057F6E90F907617903(NULL);
			V_1 = L_1;
			String_t* L_2;
			L_2 = Guid_ToString_mDAA91A4A993E3A7AD8339665E3F0CC35FE00E833((&V_1), _stringLiteralE42E8BB820D4F7550A0F04619F4E15FDC56943B9, NULL);
			String_t* L_3;
			L_3 = String_Concat_mAF2CE02CC0CB7460753D0A1A91CCF2B1E9804C5D(_stringLiteral525277D2F376C0B1449BE7FBA9294AC2AE03DFD7, L_2, NULL);
			String_t* L_4;
			L_4 = Path_Combine_m64754D4E08990CE1EBC41CDF197807EE4B115474(L_0, L_3, NULL);
			V_0 = L_4;
			String_t* L_5 = V_0;
			FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* L_6 = (FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8*)il2cpp_codegen_object_new(FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8_il2cpp_TypeInfo_var);
			NullCheck(L_6);
			FileStream__ctor_mCF0C1E859853B23725D0048DEA0653A759A5E657(L_6, L_5, 1, 3, 0, ((int32_t)4096), ((int32_t)67108864), NULL);
			V_2 = L_6;
		}
		{
			auto __finallyBlock = il2cpp::utils::Finally([&]
			{

FINALLY_004c_1:
				{// begin finally (depth: 2)
					{
						FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* L_7 = V_2;
						if (!L_7)
						{
							goto IL_0055_1;
						}
					}
					{
						FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* L_8 = V_2;
						NullCheck(L_8);
						InterfaceActionInvoker0::Invoke(0 /* System.Void System.IDisposable::Dispose() */, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_8);
					}

IL_0055_1:
					{
						return;
					}
				}// end finally (depth: 2)
			});
			try
			{// begin try (depth: 2)
				String_t* L_9 = V_0;
				NullCheck(L_9);
				String_t* L_10;
				L_10 = String_ToLowerInvariant_mBE32C93DE27C5353FEA3FA654FC1DDBE3D0EB0F2(L_9, NULL);
				bool L_11;
				L_11 = File_Exists_mD7E7A84A6B9E9A9BADBDA7C46AAE0624EF106D85(L_10, NULL);
				V_3 = (bool)((((int32_t)L_11) == ((int32_t)0))? 1 : 0);
				goto IL_005b;
			}// end try (depth: 2)
			catch(Il2CppExceptionWrapper& e)
			{
				__finallyBlock.StoreException(e.ex);
			}
		}
	}// end try (depth: 1)
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0056;
		}
		throw e;
	}

CATCH_0056:
	{// begin catch(System.Exception)
		V_3 = (bool)0;
		IL2CPP_POP_ACTIVE_EXCEPTION();
		goto IL_005b;
	}// end catch (depth: 1)

IL_005b:
	{
		bool L_12 = V_3;
		return L_12;
	}
}
// System.Void System.IO.PathInternal::.cctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PathInternal__cctor_m0D6692669FBE19557D5F51872F22C4FF6B75BC97 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		bool L_0;
		L_0 = PathInternal_GetIsCaseSensitive_mFEAF79505BAF85C91DAAA32D13D34101DC3D73A5(NULL);
		((PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_StaticFields*)il2cpp_codegen_static_fields_for(PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_il2cpp_TypeInfo_var))->___s_isCaseSensitive_0 = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// System.IO.Compression.ZipArchive System.IO.Compression.ZipFile::Open(System.String,System.IO.Compression.ZipArchiveMode,System.Text.Encoding)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* ZipFile_Open_m0CAF94BA8CAA42F3062A39A9F137121D3453A6AB (String_t* ___archiveFileName0, int32_t ___mode1, Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___entryNameEncoding2, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	int32_t V_2 = 0;
	FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* V_3 = NULL;
	ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* V_4 = NULL;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		int32_t L_0 = ___mode1;
		switch (L_0)
		{
			case 0:
			{
				goto IL_0014;
			}
			case 1:
			{
				goto IL_001c;
			}
			case 2:
			{
				goto IL_0024;
			}
		}
	}
	{
		goto IL_002c;
	}

IL_0014:
	{
		V_0 = 3;
		V_1 = 1;
		V_2 = 1;
		goto IL_0037;
	}

IL_001c:
	{
		V_0 = 1;
		V_1 = 2;
		V_2 = 0;
		goto IL_0037;
	}

IL_0024:
	{
		V_0 = 4;
		V_1 = 3;
		V_2 = 0;
		goto IL_0037;
	}

IL_002c:
	{
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_1 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		NullCheck(L_1);
		ArgumentOutOfRangeException__ctor_mBC1D5DEEA1BA41DE77228CB27D6BAFEB6DCCBF4A(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralAF248E82BE9EBA1ADBF067429FAEE5A5B6E05A74)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ZipFile_Open_m0CAF94BA8CAA42F3062A39A9F137121D3453A6AB_RuntimeMethod_var)));
	}

IL_0037:
	{
		String_t* L_2 = ___archiveFileName0;
		int32_t L_3 = V_0;
		int32_t L_4 = V_1;
		int32_t L_5 = V_2;
		FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* L_6 = (FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8*)il2cpp_codegen_object_new(FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8_il2cpp_TypeInfo_var);
		NullCheck(L_6);
		FileStream__ctor_mB51E4FD96A6B396795C835EFD7B0F0018A3A5029(L_6, L_2, L_3, L_4, L_5, ((int32_t)4096), (bool)0, NULL);
		V_3 = L_6;
	}
	try
	{// begin try (depth: 1)
		FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* L_7 = V_3;
		int32_t L_8 = ___mode1;
		Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* L_9 = ___entryNameEncoding2;
		ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* L_10 = (ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41*)il2cpp_codegen_object_new(ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41_il2cpp_TypeInfo_var);
		NullCheck(L_10);
		ZipArchive__ctor_m2706DA413E897A83057237178CCC8E51C07230B5(L_10, L_7, L_8, (bool)0, L_9, NULL);
		V_4 = L_10;
		goto IL_005d;
	}// end try (depth: 1)
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0054;
		}
		throw e;
	}

CATCH_0054:
	{// begin catch(System.Object)
		FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* L_11 = V_3;
		NullCheck(L_11);
		Stream_Dispose_mCDB42F32A17541CCA6D3A5906827A401570B07A8(L_11, NULL);
		IL2CPP_RETHROW_MANAGED_EXCEPTION(IL2CPP_GET_ACTIVE_EXCEPTION(Exception_t*));
	}// end catch (depth: 1)

IL_005d:
	{
		ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* L_12 = V_4;
		return L_12;
	}
}
// System.Void System.IO.Compression.ZipFile::ExtractToDirectory(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFile_ExtractToDirectory_m7FD001DFB8DE5227003F07935E3934BAC4D989EB (String_t* ___sourceArchiveFileName0, String_t* ___destinationDirectoryName1, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___sourceArchiveFileName0;
		String_t* L_1 = ___destinationDirectoryName1;
		ZipFile_ExtractToDirectory_mEF39886577A8E2114AEE60C8617E830A1F1DAA53(L_0, L_1, (Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095*)NULL, NULL);
		return;
	}
}
// System.Void System.IO.Compression.ZipFile::ExtractToDirectory(System.String,System.String,System.Text.Encoding)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFile_ExtractToDirectory_mEF39886577A8E2114AEE60C8617E830A1F1DAA53 (String_t* ___sourceArchiveFileName0, String_t* ___destinationDirectoryName1, Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___entryNameEncoding2, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___sourceArchiveFileName0;
		String_t* L_1 = ___destinationDirectoryName1;
		Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* L_2 = ___entryNameEncoding2;
		ZipFile_ExtractToDirectory_m8B5CB439A85E06B249A98A7EBA2448BB6CE34D10(L_0, L_1, L_2, (bool)0, NULL);
		return;
	}
}
// System.Void System.IO.Compression.ZipFile::ExtractToDirectory(System.String,System.String,System.Text.Encoding,System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFile_ExtractToDirectory_m8B5CB439A85E06B249A98A7EBA2448BB6CE34D10 (String_t* ___sourceArchiveFileName0, String_t* ___destinationDirectoryName1, Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___entryNameEncoding2, bool ___overwrite3, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* V_0 = NULL;
	{
		String_t* L_0 = ___sourceArchiveFileName0;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		NullCheck(L_1);
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6052AC80E29B425758A2997B53AC96858AD5CF27)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ZipFile_ExtractToDirectory_m8B5CB439A85E06B249A98A7EBA2448BB6CE34D10_RuntimeMethod_var)));
	}

IL_000e:
	{
		String_t* L_2 = ___sourceArchiveFileName0;
		Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* L_3 = ___entryNameEncoding2;
		ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* L_4;
		L_4 = ZipFile_Open_m0CAF94BA8CAA42F3062A39A9F137121D3453A6AB(L_2, 0, L_3, NULL);
		V_0 = L_4;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0021:
			{// begin finally (depth: 1)
				{
					ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* L_5 = V_0;
					if (!L_5)
					{
						goto IL_002a;
					}
				}
				{
					ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* L_6 = V_0;
					NullCheck(L_6);
					InterfaceActionInvoker0::Invoke(0 /* System.Void System.IDisposable::Dispose() */, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_6);
				}

IL_002a:
				{
					return;
				}
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* L_7 = V_0;
			String_t* L_8 = ___destinationDirectoryName1;
			bool L_9 = ___overwrite3;
			ZipFileExtensions_ExtractToDirectory_m46D41E7457D7E3421DF84AAF8D63BA62837C3A15(L_7, L_8, L_9, NULL);
			goto IL_002b;
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_002b:
	{
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// System.Void System.IO.Compression.ZipFileExtensions::ExtractToDirectory(System.IO.Compression.ZipArchive,System.String,System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFileExtensions_ExtractToDirectory_m46D41E7457D7E3421DF84AAF8D63BA62837C3A15 (ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* ___source0, String_t* ___destinationDirectoryName1, bool ___overwrite2, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerator_1_tEDCB8B779ED9EE7AB107A7258544B436ADF7E5DC_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ReadOnlyCollection_1_GetEnumerator_mCC70E8DC19E3118E9FFF0B2AD6DABA4C3A1D8BDF_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	RuntimeObject* V_1 = NULL;
	ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* V_2 = NULL;
	String_t* V_3 = NULL;
	{
		ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* L_0 = ___source0;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		NullCheck(L_1);
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral66F9618FDA792CAB23AF2D7FFB50AB2D3E393DC5)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ZipFileExtensions_ExtractToDirectory_m46D41E7457D7E3421DF84AAF8D63BA62837C3A15_RuntimeMethod_var)));
	}

IL_000e:
	{
		String_t* L_2 = ___destinationDirectoryName1;
		if (L_2)
		{
			goto IL_001c;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_3 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		NullCheck(L_3);
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42C85608AB661F2121C84F54255CBAFC5728CF77)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_3, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ZipFileExtensions_ExtractToDirectory_m46D41E7457D7E3421DF84AAF8D63BA62837C3A15_RuntimeMethod_var)));
	}

IL_001c:
	{
		String_t* L_4 = ___destinationDirectoryName1;
		DirectoryInfo_tEAEEC018EB49B4A71907FFEAFE935FAA8F9C1FE2* L_5;
		L_5 = Directory_CreateDirectory_mD89FECDFB25BC52F866DC0B1BB8552334FB249D2(L_4, NULL);
		NullCheck(L_5);
		String_t* L_6;
		L_6 = VirtualFuncInvoker0< String_t* >::Invoke(8 /* System.String System.IO.FileSystemInfo::get_FullName() */, L_5);
		V_0 = L_6;
		String_t* L_7 = V_0;
		il2cpp_codegen_runtime_class_init_inline(Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var);
		Il2CppChar L_8 = ((Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_StaticFields*)il2cpp_codegen_static_fields_for(Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var))->___DirectorySeparatorChar_2;
		NullCheck(L_7);
		bool L_9;
		L_9 = String_EndsWith_m1345909BD17FAD2AE0F70BC1B5CFC2010CF226B0(L_7, L_8, NULL);
		if (L_9)
		{
			goto IL_0046;
		}
	}
	{
		String_t* L_10 = V_0;
		il2cpp_codegen_runtime_class_init_inline(Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var);
		String_t* L_11;
		L_11 = Char_ToString_m2A308731F9577C06AF3C0901234E2EAC8327410C((&((Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_StaticFields*)il2cpp_codegen_static_fields_for(Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var))->___DirectorySeparatorChar_2), NULL);
		String_t* L_12;
		L_12 = String_Concat_mAF2CE02CC0CB7460753D0A1A91CCF2B1E9804C5D(L_10, L_11, NULL);
		V_0 = L_12;
	}

IL_0046:
	{
		ZipArchive_t6469B8DB5F18FB4C7E24F625D0E53EA635D31C41* L_13 = ___source0;
		NullCheck(L_13);
		ReadOnlyCollection_1_t15A54E961DBC027444DA89894B8AD689A38CE9AC* L_14;
		L_14 = ZipArchive_get_Entries_m2BB23EB2041B6BBD85AF079BEBEA8DEEA0EEEF88(L_13, NULL);
		NullCheck(L_14);
		RuntimeObject* L_15;
		L_15 = ReadOnlyCollection_1_GetEnumerator_mCC70E8DC19E3118E9FFF0B2AD6DABA4C3A1D8BDF(L_14, ReadOnlyCollection_1_GetEnumerator_mCC70E8DC19E3118E9FFF0B2AD6DABA4C3A1D8BDF_RuntimeMethod_var);
		V_1 = L_15;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_00cd:
			{// begin finally (depth: 1)
				{
					RuntimeObject* L_16 = V_1;
					if (!L_16)
					{
						goto IL_00d6;
					}
				}
				{
					RuntimeObject* L_17 = V_1;
					NullCheck(L_17);
					InterfaceActionInvoker0::Invoke(0 /* System.Void System.IDisposable::Dispose() */, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_17);
				}

IL_00d6:
				{
					return;
				}
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_00c3_1;
			}

IL_0054_1:
			{
				RuntimeObject* L_18 = V_1;
				NullCheck(L_18);
				ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* L_19;
				L_19 = InterfaceFuncInvoker0< ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* >::Invoke(0 /* T System.Collections.Generic.IEnumerator`1<System.IO.Compression.ZipArchiveEntry>::get_Current() */, IEnumerator_1_tEDCB8B779ED9EE7AB107A7258544B436ADF7E5DC_il2cpp_TypeInfo_var, L_18);
				V_2 = L_19;
				String_t* L_20 = V_0;
				ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* L_21 = V_2;
				NullCheck(L_21);
				String_t* L_22;
				L_22 = ZipArchiveEntry_get_FullName_mB226F80A14EA72D5C3D63C912AD483020CE81F2F_inline(L_21, NULL);
				il2cpp_codegen_runtime_class_init_inline(Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var);
				String_t* L_23;
				L_23 = Path_Combine_m64754D4E08990CE1EBC41CDF197807EE4B115474(L_20, L_22, NULL);
				String_t* L_24;
				L_24 = Path_GetFullPath_m17A1AD4E216D884E3DF3208BF44F4E40823BAA23(L_23, NULL);
				V_3 = L_24;
				String_t* L_25 = V_3;
				String_t* L_26 = V_0;
				il2cpp_codegen_runtime_class_init_inline(PathInternal_tA1D52C336D12A4ECB731F464CEFCE25D42EEFFD0_il2cpp_TypeInfo_var);
				int32_t L_27;
				L_27 = PathInternal_get_StringComparison_mE5AE0510D4EEA560F43DD0DEA2BF38D9EB998698(NULL);
				NullCheck(L_25);
				bool L_28;
				L_28 = String_StartsWith_mA2A4405B1B9F3653A6A9AA7F223F68D86A0C6264(L_25, L_26, L_27, NULL);
				if (L_28)
				{
					goto IL_0086_1;
				}
			}
			{
				IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910* L_29 = (IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910_il2cpp_TypeInfo_var)));
				NullCheck(L_29);
				IOException__ctor_mE0612A16064F93C7EBB468D6874777BD70CB50CA(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7E28E9DF3E4EBB1EFADEE524D7CE7A4F5B1DE1CA)), NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ZipFileExtensions_ExtractToDirectory_m46D41E7457D7E3421DF84AAF8D63BA62837C3A15_RuntimeMethod_var)));
			}

IL_0086_1:
			{
				String_t* L_30 = V_3;
				il2cpp_codegen_runtime_class_init_inline(Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var);
				String_t* L_31;
				L_31 = Path_GetFileName_mEBC73E0C8D8C56214D1DA4BA8409C5B5F00457A5(L_30, NULL);
				NullCheck(L_31);
				int32_t L_32;
				L_32 = String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline(L_31, NULL);
				if (L_32)
				{
					goto IL_00af_1;
				}
			}
			{
				ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* L_33 = V_2;
				NullCheck(L_33);
				int64_t L_34;
				L_34 = ZipArchiveEntry_get_Length_m4660921EEC25DF03D255896508A7BD1EEC6C7192(L_33, NULL);
				if (!L_34)
				{
					goto IL_00a6_1;
				}
			}
			{
				IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910* L_35 = (IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&IOException_t5D599190B003D41D45D4839A9B6B9AB53A755910_il2cpp_TypeInfo_var)));
				NullCheck(L_35);
				IOException__ctor_mE0612A16064F93C7EBB468D6874777BD70CB50CA(L_35, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral977466E2B0BB387B2215E6C982AE462F2C9AB959)), NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_35, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ZipFileExtensions_ExtractToDirectory_m46D41E7457D7E3421DF84AAF8D63BA62837C3A15_RuntimeMethod_var)));
			}

IL_00a6_1:
			{
				String_t* L_36 = V_3;
				DirectoryInfo_tEAEEC018EB49B4A71907FFEAFE935FAA8F9C1FE2* L_37;
				L_37 = Directory_CreateDirectory_mD89FECDFB25BC52F866DC0B1BB8552334FB249D2(L_36, NULL);
				goto IL_00c3_1;
			}

IL_00af_1:
			{
				String_t* L_38 = V_3;
				il2cpp_codegen_runtime_class_init_inline(Path_t8A38A801D0219E8209C1B1D90D82D4D755D998BC_il2cpp_TypeInfo_var);
				String_t* L_39;
				L_39 = Path_GetDirectoryName_mB9369289430566A15BB0A0CFCCBED3C6ECA7F30C(L_38, NULL);
				DirectoryInfo_tEAEEC018EB49B4A71907FFEAFE935FAA8F9C1FE2* L_40;
				L_40 = Directory_CreateDirectory_mD89FECDFB25BC52F866DC0B1BB8552334FB249D2(L_39, NULL);
				ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* L_41 = V_2;
				String_t* L_42 = V_3;
				bool L_43 = ___overwrite2;
				ZipFileExtensions_ExtractToFile_m2F93A06EC42F278D4730A19F853132ABE35BB923(L_41, L_42, L_43, NULL);
			}

IL_00c3_1:
			{
				RuntimeObject* L_44 = V_1;
				NullCheck(L_44);
				bool L_45;
				L_45 = InterfaceFuncInvoker0< bool >::Invoke(0 /* System.Boolean System.Collections.IEnumerator::MoveNext() */, IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var, L_44);
				if (L_45)
				{
					goto IL_0054_1;
				}
			}
			{
				goto IL_00d7;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_00d7:
	{
		return;
	}
}
// System.Void System.IO.Compression.ZipFileExtensions::ExtractToFile(System.IO.Compression.ZipArchiveEntry,System.String,System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ZipFileExtensions_ExtractToFile_m2F93A06EC42F278D4730A19F853132ABE35BB923 (ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* ___source0, String_t* ___destinationFileName1, bool ___overwrite2, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* V_1 = NULL;
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* V_2 = NULL;
	DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 V_3;
	memset((&V_3), 0, sizeof(V_3));
	int32_t G_B7_0 = 0;
	{
		ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* L_0 = ___source0;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		NullCheck(L_1);
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral66F9618FDA792CAB23AF2D7FFB50AB2D3E393DC5)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ZipFileExtensions_ExtractToFile_m2F93A06EC42F278D4730A19F853132ABE35BB923_RuntimeMethod_var)));
	}

IL_000e:
	{
		String_t* L_2 = ___destinationFileName1;
		if (L_2)
		{
			goto IL_001c;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_3 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		NullCheck(L_3);
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral168DFB0223A253D8C177CD2D6A0DBA1B0ECEFB96)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_3, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ZipFileExtensions_ExtractToFile_m2F93A06EC42F278D4730A19F853132ABE35BB923_RuntimeMethod_var)));
	}

IL_001c:
	{
		bool L_4 = ___overwrite2;
		if (L_4)
		{
			goto IL_0022;
		}
	}
	{
		G_B7_0 = 1;
		goto IL_0023;
	}

IL_0022:
	{
		G_B7_0 = 2;
	}

IL_0023:
	{
		V_0 = G_B7_0;
		String_t* L_5 = ___destinationFileName1;
		int32_t L_6 = V_0;
		FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8* L_7 = (FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8*)il2cpp_codegen_object_new(FileStream_t07C7222EE10B75F352B89B76E60820160FF10AD8_il2cpp_TypeInfo_var);
		NullCheck(L_7);
		FileStream__ctor_mB51E4FD96A6B396795C835EFD7B0F0018A3A5029(L_7, L_5, L_6, 2, 0, ((int32_t)4096), (bool)0, NULL);
		V_1 = L_7;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_004e:
			{// begin finally (depth: 1)
				{
					Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_8 = V_1;
					if (!L_8)
					{
						goto IL_0057;
					}
				}
				{
					Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_9 = V_1;
					NullCheck(L_9);
					InterfaceActionInvoker0::Invoke(0 /* System.Void System.IDisposable::Dispose() */, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_9);
				}

IL_0057:
				{
					return;
				}
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* L_10 = ___source0;
				NullCheck(L_10);
				Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_11;
				L_11 = ZipArchiveEntry_Open_m1D0EFB9AD33BA96AAF0C624EADA3E58CD6CC67FF(L_10, NULL);
				V_2 = L_11;
			}
			{
				auto __finallyBlock = il2cpp::utils::Finally([&]
				{

FINALLY_0044_1:
					{// begin finally (depth: 2)
						{
							Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_12 = V_2;
							if (!L_12)
							{
								goto IL_004d_1;
							}
						}
						{
							Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_13 = V_2;
							NullCheck(L_13);
							InterfaceActionInvoker0::Invoke(0 /* System.Void System.IDisposable::Dispose() */, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_13);
						}

IL_004d_1:
						{
							return;
						}
					}// end finally (depth: 2)
				});
				try
				{// begin try (depth: 2)
					Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_14 = V_2;
					Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_15 = V_1;
					NullCheck(L_14);
					Stream_CopyTo_m61DC54FF3708C2B8AB5C5D63D300AA57ADA01999(L_14, L_15, NULL);
					goto IL_0058;
				}// end try (depth: 2)
				catch(Il2CppExceptionWrapper& e)
				{
					__finallyBlock.StoreException(e.ex);
				}
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0058:
	{
		String_t* L_16 = ___destinationFileName1;
		ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* L_17 = ___source0;
		NullCheck(L_17);
		DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 L_18;
		L_18 = ZipArchiveEntry_get_LastWriteTime_m98EB193BEB589BE1739A89C6104830F11573AFE9_inline(L_17, NULL);
		V_3 = L_18;
		DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D L_19;
		L_19 = DateTimeOffset_get_DateTime_mDF6DC57E7A5647D8B964D3FD5B6855E7D66EF324((&V_3), NULL);
		File_SetLastWriteTime_m444C27684387AD8DC03E739B9CCAA4927C75FDBC(L_16, L_19, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* ZipArchiveEntry_get_FullName_mB226F80A14EA72D5C3D63C912AD483020CE81F2F_inline (ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* __this, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = __this->____storedEntryName_20;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____stringLength_4;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 ZipArchiveEntry_get_LastWriteTime_m98EB193BEB589BE1739A89C6104830F11573AFE9_inline (ZipArchiveEntry_tEFD75A0570102F8A3DF70A038302146B46071DD4* __this, const RuntimeMethod* method) 
{
	{
		DateTimeOffset_t4EE701FE2F386D6F932FAC9B11E4B74A5B30F0A4 L_0 = __this->____lastModified_8;
		return L_0;
	}
}
