#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>
#include <stdint.h>



// System.Byte[]
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
// System.Delegate[]
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771;
// System.Action
struct Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07;
// System.AsyncCallback
struct AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C;
// System.Delegate
struct Delegate_t;
// System.DelegateData
struct DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E;
// Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck
struct EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D;
// Oculus.Platform.Models.Error
struct Error_t0A46640739F2057B84B1EE6489A55DDC224935A4;
// System.IAsyncResult
struct IAsyncResult_t7B9B5A0ECB35DCEC31B8A8122C37D687369253B5;
// Oculus.Platform.Message
struct Message_t5E5BB1D7C1870D878913D21BAA1AFD1EC65431D9;
// System.Reflection.MethodInfo
struct MethodInfo_t;
// UnityEngine.MonoBehaviour
struct MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71;
// Oculus.Platform.Request
struct Request_t0773858FF1AC67C0D8B43058CC7119DDD1202D3B;
// System.String
struct String_t;
// System.Void
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
// Oculus.Platform.Message/Callback
struct Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D;
// Oculus.Platform.Message/ExtraMessageTypesHandler
struct ExtraMessageTypesHandler_t1140ACF58BA319459C041E281C5BCC00FBD2D389;

IL2CPP_EXTERN_C RuntimeClass* Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Core_t272580A990CA827C27F3116C5420EB2F87FE290C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* RuntimeObject_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral21C17994972DD214893B82640E0D675E392FC185;
IL2CPP_EXTERN_C String_t* _stringLiteral2CD3F1ADDCE85E66879EDA556C862F4E45829F83;
IL2CPP_EXTERN_C String_t* _stringLiteral4EBD08B18CC99B6FD20FDD9215C61B731F93C76A;
IL2CPP_EXTERN_C String_t* _stringLiteralA4AFF14AE5157F3A41CA32B8CAA514AC80B16DA0;
IL2CPP_EXTERN_C String_t* _stringLiteralE188B7082D61178A7D9F7C1D16955F8CCC096ED4;
IL2CPP_EXTERN_C const RuntimeMethod* EntitlementCheck_EntitlementCheckCallback_m9D24743D51C5F99715DCFA28C2320AD8C746D8D0_RuntimeMethod_var;
struct Delegate_t_marshaled_com;
struct Delegate_t_marshaled_pinvoke;


IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// <PrivateImplementationDetails>
struct U3CPrivateImplementationDetailsU3E_t1828878FB092BA210A76238953E3118A4B8005CC  : public RuntimeObject
{
};

struct U3CPrivateImplementationDetailsU3E_t1828878FB092BA210A76238953E3118A4B8005CC_StaticFields
{
	// System.Int32 <PrivateImplementationDetails>::532EAABD9574880DBF76B9B8CC00832C20A6EC113D682299550D7A6E0F345E25
	int32_t ___532EAABD9574880DBF76B9B8CC00832C20A6EC113D682299550D7A6E0F345E25_0;
};
struct Il2CppArrayBounds;

// Oculus.Platform.Message
struct Message_t5E5BB1D7C1870D878913D21BAA1AFD1EC65431D9  : public RuntimeObject
{
	// Oculus.Platform.Message/MessageType Oculus.Platform.Message::type
	uint32_t ___type_0;
	// System.UInt64 Oculus.Platform.Message::requestID
	uint64_t ___requestID_1;
	// Oculus.Platform.Models.Error Oculus.Platform.Message::error
	Error_t0A46640739F2057B84B1EE6489A55DDC224935A4* ___error_2;
};

struct Message_t5E5BB1D7C1870D878913D21BAA1AFD1EC65431D9_StaticFields
{
	// Oculus.Platform.Message/ExtraMessageTypesHandler Oculus.Platform.Message::<HandleExtraMessageTypes>k__BackingField
	ExtraMessageTypesHandler_t1140ACF58BA319459C041E281C5BCC00FBD2D389* ___U3CHandleExtraMessageTypesU3Ek__BackingField_3;
};

// Oculus.Platform.Request
struct Request_t0773858FF1AC67C0D8B43058CC7119DDD1202D3B  : public RuntimeObject
{
	// Oculus.Platform.Message/Callback Oculus.Platform.Request::callback_
	Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D* ___callback__0;
	// System.UInt64 Oculus.Platform.Request::<RequestID>k__BackingField
	uint64_t ___U3CRequestIDU3Ek__BackingField_1;
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

// System.Int32
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	// System.Int32 System.Int32::m_value
	int32_t ___m_value_0;
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

// System.UInt32
struct UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B 
{
	// System.UInt32 System.UInt32::m_value
	uint32_t ___m_value_0;
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

// System.Delegate
struct Delegate_t  : public RuntimeObject
{
	// System.IntPtr System.Delegate::method_ptr
	Il2CppMethodPointer ___method_ptr_0;
	// System.IntPtr System.Delegate::invoke_impl
	intptr_t ___invoke_impl_1;
	// System.Object System.Delegate::m_target
	RuntimeObject* ___m_target_2;
	// System.IntPtr System.Delegate::method
	intptr_t ___method_3;
	// System.IntPtr System.Delegate::delegate_trampoline
	intptr_t ___delegate_trampoline_4;
	// System.IntPtr System.Delegate::extra_arg
	intptr_t ___extra_arg_5;
	// System.IntPtr System.Delegate::method_code
	intptr_t ___method_code_6;
	// System.IntPtr System.Delegate::interp_method
	intptr_t ___interp_method_7;
	// System.IntPtr System.Delegate::interp_invoke_impl
	intptr_t ___interp_invoke_impl_8;
	// System.Reflection.MethodInfo System.Delegate::method_info
	MethodInfo_t* ___method_info_9;
	// System.Reflection.MethodInfo System.Delegate::original_method_info
	MethodInfo_t* ___original_method_info_10;
	// System.DelegateData System.Delegate::data
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data_11;
	// System.Boolean System.Delegate::method_is_virtual
	bool ___method_is_virtual_12;
};
// Native definition for P/Invoke marshalling of System.Delegate
struct Delegate_t_marshaled_pinvoke
{
	intptr_t ___method_ptr_0;
	intptr_t ___invoke_impl_1;
	Il2CppIUnknown* ___m_target_2;
	intptr_t ___method_3;
	intptr_t ___delegate_trampoline_4;
	intptr_t ___extra_arg_5;
	intptr_t ___method_code_6;
	intptr_t ___interp_method_7;
	intptr_t ___interp_invoke_impl_8;
	MethodInfo_t* ___method_info_9;
	MethodInfo_t* ___original_method_info_10;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data_11;
	int32_t ___method_is_virtual_12;
};
// Native definition for COM marshalling of System.Delegate
struct Delegate_t_marshaled_com
{
	intptr_t ___method_ptr_0;
	intptr_t ___invoke_impl_1;
	Il2CppIUnknown* ___m_target_2;
	intptr_t ___method_3;
	intptr_t ___delegate_trampoline_4;
	intptr_t ___extra_arg_5;
	intptr_t ___method_code_6;
	intptr_t ___interp_method_7;
	intptr_t ___interp_invoke_impl_8;
	MethodInfo_t* ___method_info_9;
	MethodInfo_t* ___original_method_info_10;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data_11;
	int32_t ___method_is_virtual_12;
};

// UnityEngine.Object
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C  : public RuntimeObject
{
	// System.IntPtr UnityEngine.Object::m_CachedPtr
	intptr_t ___m_CachedPtr_0;
};

struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_StaticFields
{
	// System.Int32 UnityEngine.Object::OffsetOfInstanceIDInCPlusPlusObject
	int32_t ___OffsetOfInstanceIDInCPlusPlusObject_1;
};
// Native definition for P/Invoke marshalling of UnityEngine.Object
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_pinvoke
{
	intptr_t ___m_CachedPtr_0;
};
// Native definition for COM marshalling of UnityEngine.Object
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_com
{
	intptr_t ___m_CachedPtr_0;
};

// UnityEngine.Component
struct Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};

// System.MulticastDelegate
struct MulticastDelegate_t  : public Delegate_t
{
	// System.Delegate[] System.MulticastDelegate::delegates
	DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771* ___delegates_13;
};
// Native definition for P/Invoke marshalling of System.MulticastDelegate
struct MulticastDelegate_t_marshaled_pinvoke : public Delegate_t_marshaled_pinvoke
{
	Delegate_t_marshaled_pinvoke** ___delegates_13;
};
// Native definition for COM marshalling of System.MulticastDelegate
struct MulticastDelegate_t_marshaled_com : public Delegate_t_marshaled_com
{
	Delegate_t_marshaled_com** ___delegates_13;
};

// System.Action
struct Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07  : public MulticastDelegate_t
{
};

// UnityEngine.Behaviour
struct Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA  : public Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3
{
};

// Oculus.Platform.Message/Callback
struct Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D  : public MulticastDelegate_t
{
};

// UnityEngine.MonoBehaviour
struct MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71  : public Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA
{
};

// Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck
struct EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D  : public MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71
{
	// System.Boolean Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::exitAppOnFailure
	bool ___exitAppOnFailure_4;
};

struct EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields
{
	// System.Action Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::UserFailedEntitlementCheck
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* ___UserFailedEntitlementCheck_5;
	// System.Action Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::UserPassedEntitlementCheck
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* ___UserPassedEntitlementCheck_6;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif



// System.Delegate System.Delegate::Combine(System.Delegate,System.Delegate)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Delegate_t* Delegate_Combine_m8B9D24CED35033C7FC56501DFE650F5CB7FF012C (Delegate_t* ___a0, Delegate_t* ___b1, const RuntimeMethod* method) ;
// System.Delegate System.Delegate::Remove(System.Delegate,System.Delegate)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Delegate_t* Delegate_Remove_m40506877934EC1AD4ADAE57F5E97AF0BC0F96116 (Delegate_t* ___source0, Delegate_t* ___value1, const RuntimeMethod* method) ;
// System.Boolean Oculus.Platform.Core::IsInitialized()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Core_IsInitialized_m2A9AF05BAD7A54490ED0EE266829C5A36FB4EAA8 (const RuntimeMethod* method) ;
// System.Void Oculus.Platform.Core::Initialize(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Core_Initialize_m75C5B6CB99DCAE4BBCFF6855489A27D3F4472009 (String_t* ___appId0, const RuntimeMethod* method) ;
// Oculus.Platform.Request Oculus.Platform.Entitlements::IsUserEntitledToApplication()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Request_t0773858FF1AC67C0D8B43058CC7119DDD1202D3B* Entitlements_IsUserEntitledToApplication_m0991406029D4228953B4F31614C263AF1E15A1D2 (const RuntimeMethod* method) ;
// System.Void Oculus.Platform.Message/Callback::.ctor(System.Object,System.IntPtr)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Callback__ctor_mD171F5D506678F07015C8FDDC8BB3CC3B2059E92 (Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D* __this, RuntimeObject* ___object0, intptr_t ___method1, const RuntimeMethod* method) ;
// Oculus.Platform.Request Oculus.Platform.Request::OnComplete(Oculus.Platform.Message/Callback)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Request_t0773858FF1AC67C0D8B43058CC7119DDD1202D3B* Request_OnComplete_mFF740AAA53CD7EC649138E513189CD533A602BBE (Request_t0773858FF1AC67C0D8B43058CC7119DDD1202D3B* __this, Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D* ___callback0, const RuntimeMethod* method) ;
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::HandleEntitlementCheckResult(System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck_HandleEntitlementCheckResult_mDC6CB501975F2597EDA8BDE3373C90CDAA9B5919 (EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D* __this, bool ___result0, const RuntimeMethod* method) ;
// System.Boolean Oculus.Platform.Message::get_IsError()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Message_get_IsError_m969FA3045AEAD9BDC34AA96BB25DD7083E8790C4 (Message_t5E5BB1D7C1870D878913D21BAA1AFD1EC65431D9* __this, const RuntimeMethod* method) ;
// System.Void UnityEngine.Debug::Log(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Debug_Log_m86567BCF22BBE7809747817453CACA0E41E68219 (RuntimeObject* ___message0, const RuntimeMethod* method) ;
// System.Void System.Action::Invoke()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Action_Invoke_m7126A54DACA72B845424072887B5F3A51FC3808E_inline (Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* __this, const RuntimeMethod* method) ;
// System.Void UnityEngine.Debug::LogError(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Debug_LogError_m059825802BB6AF7EA9693FEBEEB0D85F59A3E38E (RuntimeObject* ___message0, const RuntimeMethod* method) ;
// System.Void UnityEngine.Application::Quit()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Application_Quit_m965C6D4CA85A24DD95B347D22837074F19C58134 (const RuntimeMethod* method) ;
// System.Void UnityEngine.MonoBehaviour::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void MonoBehaviour__ctor_m592DB0105CA0BC97AA1C5F4AD27B12D68A3B7C1E (MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71* __this, const RuntimeMethod* method) ;
// System.Char System.String::get_Chars(System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar String_get_Chars_mC49DF0CD2D3BE7BE97B3AD9C995BE3094F8E36D3 (String_t* __this, int32_t ___index0, const RuntimeMethod* method) ;
// System.Int32 System.String::get_Length()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::add_UserFailedEntitlementCheck(System.Action)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck_add_UserFailedEntitlementCheck_m486CF12215B9576734EB70390D1004B4E6301997 (Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* ___value0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_0 = NULL;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_1 = NULL;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_2 = NULL;
	{
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_0 = ((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserFailedEntitlementCheck_5;
		V_0 = L_0;
	}

IL_0006:
	{
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_1 = V_0;
		V_1 = L_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_2 = V_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_3 = ___value0;
		Delegate_t* L_4;
		L_4 = Delegate_Combine_m8B9D24CED35033C7FC56501DFE650F5CB7FF012C(L_2, L_3, NULL);
		V_2 = ((Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)CastclassSealed((RuntimeObject*)L_4, Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var));
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_5 = V_2;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_6 = V_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_7;
		L_7 = InterlockedCompareExchangeImpl<Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*>((&((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserFailedEntitlementCheck_5), L_5, L_6);
		V_0 = L_7;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_8 = V_0;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_9 = V_1;
		if ((!(((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_8) == ((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_9))))
		{
			goto IL_0006;
		}
	}
	{
		return;
	}
}
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::remove_UserFailedEntitlementCheck(System.Action)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck_remove_UserFailedEntitlementCheck_m85554E7525C7B8DA0D997922F1FA0A8CD7727E0E (Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* ___value0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_0 = NULL;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_1 = NULL;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_2 = NULL;
	{
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_0 = ((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserFailedEntitlementCheck_5;
		V_0 = L_0;
	}

IL_0006:
	{
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_1 = V_0;
		V_1 = L_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_2 = V_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_3 = ___value0;
		Delegate_t* L_4;
		L_4 = Delegate_Remove_m40506877934EC1AD4ADAE57F5E97AF0BC0F96116(L_2, L_3, NULL);
		V_2 = ((Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)CastclassSealed((RuntimeObject*)L_4, Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var));
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_5 = V_2;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_6 = V_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_7;
		L_7 = InterlockedCompareExchangeImpl<Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*>((&((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserFailedEntitlementCheck_5), L_5, L_6);
		V_0 = L_7;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_8 = V_0;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_9 = V_1;
		if ((!(((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_8) == ((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_9))))
		{
			goto IL_0006;
		}
	}
	{
		return;
	}
}
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::add_UserPassedEntitlementCheck(System.Action)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck_add_UserPassedEntitlementCheck_mE29C8EC8ED477E37456744683F3D3FC2431C2487 (Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* ___value0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_0 = NULL;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_1 = NULL;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_2 = NULL;
	{
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_0 = ((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserPassedEntitlementCheck_6;
		V_0 = L_0;
	}

IL_0006:
	{
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_1 = V_0;
		V_1 = L_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_2 = V_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_3 = ___value0;
		Delegate_t* L_4;
		L_4 = Delegate_Combine_m8B9D24CED35033C7FC56501DFE650F5CB7FF012C(L_2, L_3, NULL);
		V_2 = ((Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)CastclassSealed((RuntimeObject*)L_4, Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var));
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_5 = V_2;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_6 = V_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_7;
		L_7 = InterlockedCompareExchangeImpl<Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*>((&((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserPassedEntitlementCheck_6), L_5, L_6);
		V_0 = L_7;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_8 = V_0;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_9 = V_1;
		if ((!(((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_8) == ((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_9))))
		{
			goto IL_0006;
		}
	}
	{
		return;
	}
}
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::remove_UserPassedEntitlementCheck(System.Action)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck_remove_UserPassedEntitlementCheck_m354F78AECAD64C5A13DA895923CC9426BB5F5DB4 (Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* ___value0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_0 = NULL;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_1 = NULL;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* V_2 = NULL;
	{
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_0 = ((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserPassedEntitlementCheck_6;
		V_0 = L_0;
	}

IL_0006:
	{
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_1 = V_0;
		V_1 = L_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_2 = V_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_3 = ___value0;
		Delegate_t* L_4;
		L_4 = Delegate_Remove_m40506877934EC1AD4ADAE57F5E97AF0BC0F96116(L_2, L_3, NULL);
		V_2 = ((Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)CastclassSealed((RuntimeObject*)L_4, Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07_il2cpp_TypeInfo_var));
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_5 = V_2;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_6 = V_1;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_7;
		L_7 = InterlockedCompareExchangeImpl<Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*>((&((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserPassedEntitlementCheck_6), L_5, L_6);
		V_0 = L_7;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_8 = V_0;
		Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_9 = V_1;
		if ((!(((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_8) == ((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_9))))
		{
			goto IL_0006;
		}
	}
	{
		return;
	}
}
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::Start()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck_Start_mF8533B057EF2F20C6C514FF0ECB047BD4CA3FDD4 (EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Core_t272580A990CA827C27F3116C5420EB2F87FE290C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&EntitlementCheck_EntitlementCheckCallback_m9D24743D51C5F99715DCFA28C2320AD8C746D8D0_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
	}
	try
	{// begin try (depth: 1)
		{
			// if (!Oculus.Platform.Core.IsInitialized())
			il2cpp_codegen_runtime_class_init_inline(Core_t272580A990CA827C27F3116C5420EB2F87FE290C_il2cpp_TypeInfo_var);
			bool L_0;
			L_0 = Core_IsInitialized_m2A9AF05BAD7A54490ED0EE266829C5A36FB4EAA8(NULL);
			V_0 = (bool)((((int32_t)L_0) == ((int32_t)0))? 1 : 0);
			bool L_1 = V_0;
			if (!L_1)
			{
				goto IL_0017_1;
			}
		}
		{
			// Oculus.Platform.Core.Initialize();
			il2cpp_codegen_runtime_class_init_inline(Core_t272580A990CA827C27F3116C5420EB2F87FE290C_il2cpp_TypeInfo_var);
			Core_Initialize_m75C5B6CB99DCAE4BBCFF6855489A27D3F4472009((String_t*)NULL, NULL);
		}

IL_0017_1:
		{
			// Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCheckCallback);
			Request_t0773858FF1AC67C0D8B43058CC7119DDD1202D3B* L_2;
			L_2 = Entitlements_IsUserEntitledToApplication_m0991406029D4228953B4F31614C263AF1E15A1D2(NULL);
			Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D* L_3 = (Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D*)il2cpp_codegen_object_new(Callback_t9228F6822067C9E1BBEE3F816F1D26C56CB2F30D_il2cpp_TypeInfo_var);
			NullCheck(L_3);
			Callback__ctor_mD171F5D506678F07015C8FDDC8BB3CC3B2059E92(L_3, __this, (intptr_t)((void*)EntitlementCheck_EntitlementCheckCallback_m9D24743D51C5F99715DCFA28C2320AD8C746D8D0_RuntimeMethod_var), NULL);
			NullCheck(L_2);
			Request_t0773858FF1AC67C0D8B43058CC7119DDD1202D3B* L_4;
			L_4 = Request_OnComplete_mFF740AAA53CD7EC649138E513189CD533A602BBE(L_2, L_3, NULL);
			goto IL_003e;
		}
	}// end try (depth: 1)
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0031;
		}
		throw e;
	}

CATCH_0031:
	{// begin catch(System.Object)
		// catch
		// HandleEntitlementCheckResult(false);
		EntitlementCheck_HandleEntitlementCheckResult_mDC6CB501975F2597EDA8BDE3373C90CDAA9B5919(__this, (bool)0, NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION();
		goto IL_003e;
	}// end catch (depth: 1)

IL_003e:
	{
		// }
		return;
	}
}
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::EntitlementCheckCallback(Oculus.Platform.Message)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck_EntitlementCheckCallback_m9D24743D51C5F99715DCFA28C2320AD8C746D8D0 (EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D* __this, Message_t5E5BB1D7C1870D878913D21BAA1AFD1EC65431D9* ___msg0, const RuntimeMethod* method) 
{
	{
		// HandleEntitlementCheckResult(msg.IsError == false);
		Message_t5E5BB1D7C1870D878913D21BAA1AFD1EC65431D9* L_0 = ___msg0;
		NullCheck(L_0);
		bool L_1;
		L_1 = Message_get_IsError_m969FA3045AEAD9BDC34AA96BB25DD7083E8790C4(L_0, NULL);
		EntitlementCheck_HandleEntitlementCheckResult_mDC6CB501975F2597EDA8BDE3373C90CDAA9B5919(__this, (bool)((((int32_t)L_1) == ((int32_t)0))? 1 : 0), NULL);
		// }
		return;
	}
}
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::HandleEntitlementCheckResult(System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck_HandleEntitlementCheckResult_mDC6CB501975F2597EDA8BDE3373C90CDAA9B5919 (EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D* __this, bool ___result0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral21C17994972DD214893B82640E0D675E392FC185);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral2CD3F1ADDCE85E66879EDA556C862F4E45829F83);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral4EBD08B18CC99B6FD20FDD9215C61B731F93C76A);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	bool V_1 = false;
	bool V_2 = false;
	bool V_3 = false;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		// if (result) // User passed entitlement check
		bool L_0 = ___result0;
		V_0 = L_0;
		bool L_1 = V_0;
		if (!L_1)
		{
			goto IL_0042;
		}
	}
	{
		// Debug.Log("Oculus user entitlement check successful.");
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_Log_m86567BCF22BBE7809747817453CACA0E41E68219(_stringLiteral4EBD08B18CC99B6FD20FDD9215C61B731F93C76A, NULL);
	}
	try
	{// begin try (depth: 1)
		{
			// if (UserPassedEntitlementCheck != null)
			Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_2 = ((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserPassedEntitlementCheck_6;
			V_1 = (bool)((!(((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_2) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
			bool L_3 = V_1;
			if (!L_3)
			{
				goto IL_002c_1;
			}
		}
		{
			// UserPassedEntitlementCheck();
			Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_4 = ((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserPassedEntitlementCheck_6;
			NullCheck(L_4);
			Action_Invoke_m7126A54DACA72B845424072887B5F3A51FC3808E_inline(L_4, NULL);
		}

IL_002c_1:
		{
			goto IL_003f;
		}
	}// end try (depth: 1)
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_002f;
		}
		throw e;
	}

CATCH_002f:
	{// begin catch(System.Object)
		// catch
		// Debug.LogError("Suppressed exception in app-provided UserPassedEntitlementCheck() event handler.");
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var)));
		Debug_LogError_m059825802BB6AF7EA9693FEBEEB0D85F59A3E38E(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralA4AFF14AE5157F3A41CA32B8CAA514AC80B16DA0)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION();
		goto IL_003f;
	}// end catch (depth: 1)

IL_003f:
	{
		goto IL_009d;
	}

IL_0042:
	{
	}
	try
	{// begin try (depth: 1)
		{
			// if (UserFailedEntitlementCheck != null)
			Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_5 = ((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserFailedEntitlementCheck_5;
			V_2 = (bool)((!(((RuntimeObject*)(Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07*)L_5) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
			bool L_6 = V_2;
			if (!L_6)
			{
				goto IL_005d_1;
			}
		}
		{
			// UserFailedEntitlementCheck();
			Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* L_7 = ((EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_StaticFields*)il2cpp_codegen_static_fields_for(EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D_il2cpp_TypeInfo_var))->___UserFailedEntitlementCheck_5;
			NullCheck(L_7);
			Action_Invoke_m7126A54DACA72B845424072887B5F3A51FC3808E_inline(L_7, NULL);
		}

IL_005d_1:
		{
			goto IL_0070;
		}
	}// end try (depth: 1)
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0060;
		}
		throw e;
	}

CATCH_0060:
	{// begin catch(System.Object)
		// catch
		// Debug.LogError("Suppressed exception in app-provided UserFailedEntitlementCheck() event handler.");
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var)));
		Debug_LogError_m059825802BB6AF7EA9693FEBEEB0D85F59A3E38E(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralE188B7082D61178A7D9F7C1D16955F8CCC096ED4)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION();
		goto IL_0070;
	}// end catch (depth: 1)

IL_0070:
	{
		// if (exitAppOnFailure)
		bool L_8 = __this->___exitAppOnFailure_4;
		V_3 = L_8;
		bool L_9 = V_3;
		if (!L_9)
		{
			goto IL_008f;
		}
	}
	{
		// Debug.LogError("Oculus user entitlement check failed. Exiting now.");
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_LogError_m059825802BB6AF7EA9693FEBEEB0D85F59A3E38E(_stringLiteral2CD3F1ADDCE85E66879EDA556C862F4E45829F83, NULL);
		// UnityEngine.Application.Quit();
		Application_Quit_m965C6D4CA85A24DD95B347D22837074F19C58134(NULL);
		goto IL_009c;
	}

IL_008f:
	{
		// Debug.LogError("Oculus user entitlement check failed.");
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_LogError_m059825802BB6AF7EA9693FEBEEB0D85F59A3E38E(_stringLiteral21C17994972DD214893B82640E0D675E392FC185, NULL);
	}

IL_009c:
	{
	}

IL_009d:
	{
		// }
		return;
	}
}
// System.Void Oculus.Platform.Samples.EntitlementCheck.EntitlementCheck::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EntitlementCheck__ctor_m489DC2C35D364E52A2B9FA5361A084C34C9D660D (EntitlementCheck_tB040EAF15EA94AEE91DC479969A1284415FF613D* __this, const RuntimeMethod* method) 
{
	{
		// public bool exitAppOnFailure = true;
		__this->___exitAppOnFailure_4 = (bool)1;
		MonoBehaviour__ctor_m592DB0105CA0BC97AA1C5F4AD27B12D68A3B7C1E(__this, NULL);
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
// System.UInt32 <PrivateImplementationDetails>::ComputeStringHash(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint32_t U3CPrivateImplementationDetailsU3E_ComputeStringHash_m7FF5BB319D6F5D1AEF0959FCE61C58701A89B1EB (String_t* ___s0, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		String_t* L_0 = ___s0;
		if (!L_0)
		{
			goto IL_002c;
		}
	}
	{
		V_0 = ((int32_t)-2128831035);
		V_1 = 0;
		goto IL_0021;
	}

IL_000d:
	{
		String_t* L_1 = ___s0;
		int32_t L_2 = V_1;
		NullCheck(L_1);
		Il2CppChar L_3;
		L_3 = String_get_Chars_mC49DF0CD2D3BE7BE97B3AD9C995BE3094F8E36D3(L_1, L_2, NULL);
		uint32_t L_4 = V_0;
		V_0 = ((int32_t)il2cpp_codegen_multiply(((int32_t)((int32_t)L_3^(int32_t)L_4)), ((int32_t)16777619)));
		int32_t L_5 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_0021:
	{
		int32_t L_6 = V_1;
		String_t* L_7 = ___s0;
		NullCheck(L_7);
		int32_t L_8;
		L_8 = String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline(L_7, NULL);
		if ((((int32_t)L_6) >= ((int32_t)L_8)))
		{
			goto IL_002c;
		}
	}
	{
		goto IL_000d;
	}

IL_002c:
	{
		uint32_t L_9 = V_0;
		return L_9;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Action_Invoke_m7126A54DACA72B845424072887B5F3A51FC3808E_inline (Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* __this, const RuntimeMethod* method) 
{
	typedef void (*FunctionPointerType) (RuntimeObject*, const RuntimeMethod*);
	((FunctionPointerType)__this->___invoke_impl_1)((Il2CppObject*)__this->___method_code_6, reinterpret_cast<RuntimeMethod*>(__this->___method_3));
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____stringLength_4;
		return L_0;
	}
}
