#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>
#include <stdint.h>


template <typename T1>
struct VirtualActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename T1, typename T2>
struct VirtualActionInvoker2
{
	typedef void (*Action)(void*, T1, T2, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1, T2 p2)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		((Action)invokeData.methodPtr)(obj, p1, p2, invokeData.method);
	}
};
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
template <typename R, typename T1>
struct VirtualFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
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

// System.Collections.Generic.Dictionary`2<System.Object,System.Object>
struct Dictionary_2_t14FE4A752A83D53771C584E4C8D14E01F2AFD7BA;
// System.Collections.Generic.Dictionary`2<System.String,System.Collections.Generic.List`1<System.String>>
struct Dictionary_2_t79BA378F246EFA4AD0AFFA017D788423CACA8638;
// System.Collections.Generic.Dictionary`2<System.String,Facebook.WitAi.Lib.WitResponseNode>
struct Dictionary_2_tC75B15D6BE3719CB8EEF14082A03D3744B788B4F;
// System.Collections.Generic.Dictionary`2<System.Text.RegularExpressions.Regex/CachedCodeEntryKey,System.Text.RegularExpressions.Regex/CachedCodeEntry>
struct Dictionary_2_t5B5B38BB06341F50E1C75FB53208A2A66CAE57F7;
// System.Collections.Generic.IEnumerable`1<System.Object>
struct IEnumerable_1_tF95C9E01A913DD50575531C8305932628663D9E9;
// System.Collections.Generic.IEnumerable`1<System.String>
struct IEnumerable_1_t349E66EC5F09B881A8E52EE40A1AB9EC60E08E44;
// System.Collections.Generic.IEqualityComparer`1<System.String>
struct IEqualityComparer_1_tAE94C8F24AD5B94D4EE85CA9FC59E3409D41CAF7;
// System.Collections.Generic.Dictionary`2/KeyCollection<System.String,System.Collections.Generic.List`1<System.String>>
struct KeyCollection_tAF09DE3ACAF1ABD7544A92CFC2787E520703246E;
// System.Collections.Generic.List`1<System.Object>
struct List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D;
// System.Collections.Generic.List`1<System.String>
struct List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD;
// System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitDynamicEntity>
struct List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB;
// System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitEntityKeyword>
struct List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887;
// System.Collections.Generic.List`1<Facebook.WitAi.Lib.WitResponseNode>
struct List_1_tC18B126FC489EF27D70BE515FE2369EB10D5FD4C;
// UnityEngine.Events.UnityAction`1<System.Object>
struct UnityAction_1_t9C30BCD020745BF400CBACF22C6F34ADBA2DDA6A;
// UnityEngine.Events.UnityAction`1<Facebook.WitAi.Lib.WitResponseNode>
struct UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8;
// UnityEngine.Events.UnityEvent`1<System.String[]>
struct UnityEvent_1_t477222AA4CE1BE0F15900D352218F0A842BF5EFD;
// UnityEngine.Events.UnityEvent`1<System.Object>
struct UnityEvent_1_t3CE03B42D5873C0C0E0692BEE72E1E6D5399F205;
// UnityEngine.Events.UnityEvent`1<System.String>
struct UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257;
// UnityEngine.Events.UnityEvent`1<Facebook.WitAi.Lib.WitResponseNode>
struct UnityEvent_1_t9B421777BDE1F0A6F551628B06DADBCD02D20366;
// System.Collections.Generic.Dictionary`2/ValueCollection<System.String,System.Collections.Generic.List`1<System.String>>
struct ValueCollection_tBA7D94F600201BDFFC90880B77DA2D76ED7EBB59;
// System.WeakReference`1<System.Text.RegularExpressions.RegexReplacement>
struct WeakReference_1_tDC6E83496181D1BAFA3B89CBC00BCD0B64450257;
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Int32>
struct WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07;
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Object>
struct WitEntityDataBase_1_tD992377A5996E2EC27D289A60634F3FAE327D362;
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Single>
struct WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3;
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.String>
struct WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73;
// System.Collections.Generic.Dictionary`2/Entry<System.String,System.Collections.Generic.List`1<System.String>>[]
struct EntryU5BU5D_t7D363C4258705E5C759DC3C7A84AD08784EEBE95;
// System.Int32[][]
struct Int32U5BU5DU5BU5D_t179D865D5B30EFCBC50F82C9774329C15943466E;
// Facebook.WitAi.CallbackHandlers.ConfidenceRange[]
struct ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC;
// System.Delegate[]
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771;
// Facebook.WitAi.CallbackHandlers.FormattedValueEvents[]
struct FormattedValueEventsU5BU5D_tAFBBA48044157DCAA323FD47280150A31580C188;
// System.Int32[]
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
// System.Object[]
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
// System.String[]
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
// System.Type[]
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
// Facebook.WitAi.CallbackHandlers.ValuePathMatcher[]
struct ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75;
// Facebook.WitAi.Data.Entities.WitDynamicEntity[]
struct WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D;
// Facebook.WitAi.Data.Entities.WitEntity[]
struct WitEntityU5BU5D_tC65502A56DE214BCA9ECD0258022C310876CDC65;
// Facebook.WitAi.Data.Entities.WitEntityKeyword[]
struct WitEntityKeywordU5BU5D_tEE7E30A869C03107C32A2F75DE4C9A22654C1BB8;
// Facebook.WitAi.Data.Entities.WitEntityRole[]
struct WitEntityRoleU5BU5D_t9BF80EDFBF052E416D7367E4384AEEE319BC7812;
// Facebook.WitAi.Data.Intents.WitIntent[]
struct WitIntentU5BU5D_tDB2609D617BB35FBBC73A93A1C8B54DF6B24ADAB;
// Facebook.WitAi.Data.Traits.WitTrait[]
struct WitTraitU5BU5D_t61DB38855B37C3123DBC4FC8B31E08E69BB8EC31;
// UnityEngine.Behaviour
struct Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA;
// System.Reflection.Binder
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
// System.Text.RegularExpressions.Capture
struct Capture_tE11B735186DAFEE5F7A3BF5A739E9CCCE99DC24A;
// System.Text.RegularExpressions.CaptureCollection
struct CaptureCollection_t38405272BD6A6DA77CD51487FD39624C6E95CC93;
// Facebook.WitAi.CallbackHandlers.ConfidenceRange
struct ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9;
// System.DelegateData
struct DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E;
// System.Text.RegularExpressions.ExclusiveReference
struct ExclusiveReference_t411F04D4CC440EB7399290027E1BBABEF4C28837;
// Facebook.WitAi.CallbackHandlers.FormattedValueEvents
struct FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4;
// System.Text.RegularExpressions.Group
struct Group_t26371E9136D6F43782C487B63C67C5FC4F472881;
// System.Text.RegularExpressions.GroupCollection
struct GroupCollection_tFFA1789730DD9EA122FBE77DC03BFEDCC3F2945E;
// System.Collections.Hashtable
struct Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D;
// UnityEngine.Events.InvokableCallList
struct InvokableCallList_t309E1C8C7CE885A0D2F98C84CEA77A8935688382;
// System.Text.RegularExpressions.Match
struct Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F;
// System.Reflection.MemberFilter
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
// System.Reflection.MethodInfo
struct MethodInfo_t;
// UnityEngine.MonoBehaviour
struct MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71;
// Facebook.WitAi.CallbackHandlers.MultiValueEvent
struct MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C;
// UnityEngine.Object
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C;
// Facebook.WitAi.CallbackHandlers.OutOfScopeUtteranceHandler
struct OutOfScopeUtteranceHandler_t6D1CE6F4B6A31C7CBD4E195FAADF88D3829984D1;
// UnityEngine.Events.PersistentCallGroup
struct PersistentCallGroup_tB826EDF15DC80F71BCBCD8E410FD959A04C33F25;
// System.Text.RegularExpressions.Regex
struct Regex_tE773142C2BE45C5D362B0F815AFF831707A51772;
// System.Text.RegularExpressions.RegexCode
struct RegexCode_tA23175D9DA02AD6A79B073E10EC5D225372ED6C7;
// System.Text.RegularExpressions.RegexRunnerFactory
struct RegexRunnerFactory_t72373B672C7D8785F63516DDD88834F286AF41E7;
// UnityEngine.ScriptableObject
struct ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A;
// Facebook.WitAi.CallbackHandlers.SimpleIntentHandler
struct SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1;
// Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler
struct SimpleStringEntityHandler_tB95E77E27B20E13042B687D717A45B509D5166DC;
// System.String
struct String_t;
// Facebook.WitAi.CallbackHandlers.StringEntityMatchEvent
struct StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7;
// Facebook.WitAi.Utilities.StringEvent
struct StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76;
// System.Type
struct Type_t;
// UnityEngine.Events.UnityEvent
struct UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977;
// Facebook.WitAi.CallbackHandlers.ValueEvent
struct ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74;
// Facebook.WitAi.CallbackHandlers.ValuePathMatcher
struct ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C;
// Facebook.WitAi.Events.VoiceEvents
struct VoiceEvents_tB9CCDD1C68F3A784600E7932935F705B17B8F1F5;
// Facebook.WitAi.VoiceService
struct VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99;
// System.Void
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
// Facebook.WitAi.Data.Configuration.WitApplication
struct WitApplication_t63668A99EE80B05F17BCCDDAA4BF50A84A9A1E3C;
// Facebook.WitAi.Events.WitByteDataEvent
struct WitByteDataEvent_t3E7D04458B66F77BFF599F5CBAF9F0ADD920502E;
// Facebook.WitAi.Data.Configuration.WitConfiguration
struct WitConfiguration_t3D1E7D46065A2742877307705778E1CBC33530DD;
// Facebook.WitAi.Configuration.WitConfigurationData
struct WitConfigurationData_tE8AEFEBCB9258DD2909EED5C1ACE770CBC099897;
// Facebook.WitAi.Data.Entities.WitDynamicEntities
struct WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054;
// Facebook.WitAi.Data.Entities.WitDynamicEntitiesData
struct WitDynamicEntitiesData_t89F1557016C7917FB0136B98FE1DC9C3CDDA2564;
// Facebook.WitAi.Data.Entities.WitDynamicEntity
struct WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB;
// Facebook.WitAi.Configuration.WitEndpointConfig
struct WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0;
// Facebook.WitAi.Data.Entities.WitEntity
struct WitEntity_t94A3BFC668CFCC30CC80133AEF7E9947F691F892;
// Facebook.WitAi.Data.Entities.WitEntityData
struct WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC;
// Facebook.WitAi.Data.Entities.WitEntityFloatData
struct WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B;
// Facebook.WitAi.Data.Entities.WitEntityIntData
struct WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4;
// Facebook.WitAi.Data.Entities.WitEntityKeyword
struct WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5;
// Facebook.WitAi.Data.Entities.WitEntityRole
struct WitEntityRole_t47AC15DDC8D9F37888EB6747B6ABC142F0589453;
// Facebook.WitAi.Events.WitErrorEvent
struct WitErrorEvent_tFF53D8544CC1D1E7FD8E98A1EC8AD7581F62EE34;
// Facebook.WitAi.Events.WitMicLevelChangedEvent
struct WitMicLevelChangedEvent_tA9B0047640D9DE7218C4699B6F6E0ABBCD966F1C;
// Facebook.WitAi.Events.WitRequestCreatedEvent
struct WitRequestCreatedEvent_t81EA9D8A3041CEADEBE670C485E17B1F9D5BEDF4;
// Facebook.WitAi.Lib.WitResponseArray
struct WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05;
// Facebook.WitAi.Lib.WitResponseClass
struct WitResponseClass_t739569309AE400E308EB8DD834327086751C855C;
// Facebook.WitAi.Lib.WitResponseData
struct WitResponseData_t78CC4B9FA619C3E5C20CC8CB4338164E4AB5F899;
// Facebook.WitAi.Events.WitResponseEvent
struct WitResponseEvent_t5DEE4CFA5D09DD8C6293E1D41EF6A9ACBDD2CFDD;
// Facebook.WitAi.CallbackHandlers.WitResponseHandler
struct WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B;
// Facebook.WitAi.CallbackHandlers.WitResponseMatcher
struct WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900;
// Facebook.WitAi.Lib.WitResponseNode
struct WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB;
// Facebook.WitAi.WitResponseReference
struct WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E;
// Facebook.WitAi.Data.Entities.WitSimpleDynamicEntity
struct WitSimpleDynamicEntity_t7462383F5C63BB5F2B1A153A6E2F4506651692DB;
// Facebook.WitAi.Events.WitTranscriptionEvent
struct WitTranscriptionEvent_t46F45C5FE99C833DFE345D2255587E22A1ABE32E;
// Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher
struct WitUtteranceMatcher_t1A2C2BA7A4F1DFF63A0266115636A1FEFD2166C4;
// Facebook.WitAi.Data.WitValue
struct WitValue_tD53CDFBDC3B12AB1495984258B6ACC38EEE55E32;
// System.Text.RegularExpressions.Regex/CachedCodeEntry
struct CachedCodeEntry_tE201C3AD65C234AD9ED7A78C95025824A7A9FF39;
// Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass12_0
struct U3CU3Ec__DisplayClass12_0_t74EEAADB5D1467BB6A6581295207DCC9B25C9A7A;
// Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass14_0
struct U3CU3Ec__DisplayClass14_0_t0E317FA09AA1ADB9E7810FCEBBDCF487BBBA7F75;
// Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass15_0
struct U3CU3Ec__DisplayClass15_0_t9278361DC2379675C4FBCB0FBF4950B7B760FEBC;

IL2CPP_EXTERN_C RuntimeClass* Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerable_1_tA414C8405AD2AB4B5AC23942210B16227AC96D03_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerator_1_tF1F4F17D3837E1B1E52ACBC529204A77CBA5264E_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* String_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitResponseClass_t739569309AE400E308EB8DD834327086751C855C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitResponseData_t78CC4B9FA619C3E5C20CC8CB4338164E4AB5F899_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral0116812C84CAD7B4AAD7CE2415EECA7B219CCE24;
IL2CPP_EXTERN_C String_t* _stringLiteral057E47B1A5317B280AD5B6CE27C8F11073D42316;
IL2CPP_EXTERN_C String_t* _stringLiteral0C3C6829C3CCF8020C6AC45B87963ADC095CD44A;
IL2CPP_EXTERN_C String_t* _stringLiteral0CF1C526BD3872B6F7B223B157F80669490DCBCF;
IL2CPP_EXTERN_C String_t* _stringLiteral22270319A52B6C9B9967E9291B9A79B116526038;
IL2CPP_EXTERN_C String_t* _stringLiteral29565B729B35A8E8D5DDB32B75BCCD98FDE1E555;
IL2CPP_EXTERN_C String_t* _stringLiteral334976BAB31C9D74A4F24AC5A19ADD9273522252;
IL2CPP_EXTERN_C String_t* _stringLiteral3709814F28060CE673AD4F414B802CCF4136017F;
IL2CPP_EXTERN_C String_t* _stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8;
IL2CPP_EXTERN_C String_t* _stringLiteral497E0727E6D4098F7DA86E306F0B961AA34D95FF;
IL2CPP_EXTERN_C String_t* _stringLiteral4D8D9C94AC5DA5FCED2EC8A64E10E714A2515C30;
IL2CPP_EXTERN_C String_t* _stringLiteralAD5D95ACB5EE6A85F7764DF4051FD214127B906C;
IL2CPP_EXTERN_C String_t* _stringLiteralB3F14BF976EFD974E34846B742502C802FABAE9D;
IL2CPP_EXTERN_C String_t* _stringLiteralBFC27499BD5186964D062DE5FE7C222AE471C53E;
IL2CPP_EXTERN_C String_t* _stringLiteralBFCC6EE94F1B7AA05A04750903E25F93A7188AE0;
IL2CPP_EXTERN_C String_t* _stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F;
IL2CPP_EXTERN_C String_t* _stringLiteralDA39A3EE5E6B4B0D3255BFEF95601890AFD80709;
IL2CPP_EXTERN_C String_t* _stringLiteralF3E84B722399601AD7E281754E917478AA9AD48D;
IL2CPP_EXTERN_C String_t* _stringLiteralFD60316EE3ADB7B16A998DF8AE0D68C293F6622E;
IL2CPP_EXTERN_C const RuntimeMethod* Dictionary_2_GetEnumerator_m7B0E05167CA47611244E25AD67EBCEBDF5DB5EDE_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerable_ToList_TisString_t_mA7BFFF205C0EB2F8CE0436E85FC70A2449EDD7C5_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_Dispose_m25FAB013E20621ED9156A5D8602E37E6979CCDCB_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_Dispose_m7DE9243F0F854CE156AFB007FA010C1AB4321A44_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_MoveNext_m8D1B5472B8F40695AD00E362DF9B0CCE4F919D7C_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_MoveNext_mA71548765692C241BEFB57E80CBC1563829B1656_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_get_Current_mB41E65E33E936ACBD32C200633257CBD3261A9E2_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_get_Current_mD56264A4D181F6240745145CE039E2D7750313AF_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* KeyValuePair_2_get_Key_m852B0BE1941AFE5DA3A393534C0020AEAB59D0A6_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* KeyValuePair_2_get_Value_mD55616E290F6591FA95053D1C8D2DDA2E49A058C_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_AddRange_m157DD7AD4D25423F82A21E533BC4686C83770D5E_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Add_m4AFBFA0F69A9E9C42FDF4CBBED43E6C26A6EDE28_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_GetEnumerator_m4CCCD59E97F72FE069AAFD946983DDD5D8591FD1_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1__ctor_m38543D708664CDA034C6B6D276D1EA261BEE2796_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Object_FindObjectOfType_TisVoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99_mFF7470D00931D2219BE16DF57F4FE4992A642B31_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* UnityEvent_1_AddListener_mAAD4F26F685871E030131375B05060E33216E0C3_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* UnityEvent_1_Invoke_mDABA5BBDA189937099CB491730CD6AC57D7A5F94_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* UnityEvent_1_RemoveListener_m696D0B92F65BCDA59D7C1EEA56EF4B017BFE607B_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* UnityEvent_1__ctor_m0F7D790DACD6F3D18E865D01FE4427603C1B0CC6_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* UnityEvent_1__ctor_mA1CAC73F82FF0E8BC4870AEBF137B7F72614957D_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WitEntityDataBase_1_FromEntityWitResponseNode_m44BCC95882F476B59D585A3BB4B76287A13C528F_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WitEntityDataBase_1_FromEntityWitResponseNode_m63B33E2731C95F8A7209705B9BDBACFDA0FC8557_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WitEntityDataBase_1_FromEntityWitResponseNode_mEBB6417BC069B64576611835830E1CCE7D824FCF_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WitEntityDataBase_1__ctor_m954C8C7C12B6AE8DC251B99574E791D559661402_RuntimeMethod_var;
struct Delegate_t_marshaled_com;
struct Delegate_t_marshaled_pinvoke;

struct ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC;
struct FormattedValueEventsU5BU5D_tAFBBA48044157DCAA323FD47280150A31580C188;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75;
struct WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// System.Collections.Generic.Dictionary`2<System.String,System.Collections.Generic.List`1<System.String>>
struct Dictionary_2_t79BA378F246EFA4AD0AFFA017D788423CACA8638  : public RuntimeObject
{
	// System.Int32[] System.Collections.Generic.Dictionary`2::_buckets
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____buckets_0;
	// System.Collections.Generic.Dictionary`2/Entry<TKey,TValue>[] System.Collections.Generic.Dictionary`2::_entries
	EntryU5BU5D_t7D363C4258705E5C759DC3C7A84AD08784EEBE95* ____entries_1;
	// System.Int32 System.Collections.Generic.Dictionary`2::_count
	int32_t ____count_2;
	// System.Int32 System.Collections.Generic.Dictionary`2::_freeList
	int32_t ____freeList_3;
	// System.Int32 System.Collections.Generic.Dictionary`2::_freeCount
	int32_t ____freeCount_4;
	// System.Int32 System.Collections.Generic.Dictionary`2::_version
	int32_t ____version_5;
	// System.Collections.Generic.IEqualityComparer`1<TKey> System.Collections.Generic.Dictionary`2::_comparer
	RuntimeObject* ____comparer_6;
	// System.Collections.Generic.Dictionary`2/KeyCollection<TKey,TValue> System.Collections.Generic.Dictionary`2::_keys
	KeyCollection_tAF09DE3ACAF1ABD7544A92CFC2787E520703246E* ____keys_7;
	// System.Collections.Generic.Dictionary`2/ValueCollection<TKey,TValue> System.Collections.Generic.Dictionary`2::_values
	ValueCollection_tBA7D94F600201BDFFC90880B77DA2D76ED7EBB59* ____values_8;
	// System.Object System.Collections.Generic.Dictionary`2::_syncRoot
	RuntimeObject* ____syncRoot_9;
};

// System.Collections.Generic.List`1<System.Object>
struct List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D  : public RuntimeObject
{
	// T[] System.Collections.Generic.List`1::_items
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ____items_1;
	// System.Int32 System.Collections.Generic.List`1::_size
	int32_t ____size_2;
	// System.Int32 System.Collections.Generic.List`1::_version
	int32_t ____version_3;
	// System.Object System.Collections.Generic.List`1::_syncRoot
	RuntimeObject* ____syncRoot_4;
};

struct List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D_StaticFields
{
	// T[] System.Collections.Generic.List`1::s_emptyArray
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___s_emptyArray_5;
};

// System.Collections.Generic.List`1<System.String>
struct List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD  : public RuntimeObject
{
	// T[] System.Collections.Generic.List`1::_items
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ____items_1;
	// System.Int32 System.Collections.Generic.List`1::_size
	int32_t ____size_2;
	// System.Int32 System.Collections.Generic.List`1::_version
	int32_t ____version_3;
	// System.Object System.Collections.Generic.List`1::_syncRoot
	RuntimeObject* ____syncRoot_4;
};

struct List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_StaticFields
{
	// T[] System.Collections.Generic.List`1::s_emptyArray
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___s_emptyArray_5;
};

// System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitDynamicEntity>
struct List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB  : public RuntimeObject
{
	// T[] System.Collections.Generic.List`1::_items
	WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D* ____items_1;
	// System.Int32 System.Collections.Generic.List`1::_size
	int32_t ____size_2;
	// System.Int32 System.Collections.Generic.List`1::_version
	int32_t ____version_3;
	// System.Object System.Collections.Generic.List`1::_syncRoot
	RuntimeObject* ____syncRoot_4;
};

struct List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB_StaticFields
{
	// T[] System.Collections.Generic.List`1::s_emptyArray
	WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D* ___s_emptyArray_5;
};

// System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitEntityKeyword>
struct List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887  : public RuntimeObject
{
	// T[] System.Collections.Generic.List`1::_items
	WitEntityKeywordU5BU5D_tEE7E30A869C03107C32A2F75DE4C9A22654C1BB8* ____items_1;
	// System.Int32 System.Collections.Generic.List`1::_size
	int32_t ____size_2;
	// System.Int32 System.Collections.Generic.List`1::_version
	int32_t ____version_3;
	// System.Object System.Collections.Generic.List`1::_syncRoot
	RuntimeObject* ____syncRoot_4;
};

struct List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_StaticFields
{
	// T[] System.Collections.Generic.List`1::s_emptyArray
	WitEntityKeywordU5BU5D_tEE7E30A869C03107C32A2F75DE4C9A22654C1BB8* ___s_emptyArray_5;
};

// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Int32>
struct WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07  : public RuntimeObject
{
	// Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Data.Entities.WitEntityDataBase`1::responseNode
	WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___responseNode_0;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::id
	String_t* ___id_1;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::name
	String_t* ___name_2;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::role
	String_t* ___role_3;
	// System.Int32 Facebook.WitAi.Data.Entities.WitEntityDataBase`1::start
	int32_t ___start_4;
	// System.Int32 Facebook.WitAi.Data.Entities.WitEntityDataBase`1::end
	int32_t ___end_5;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::type
	String_t* ___type_6;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::body
	String_t* ___body_7;
	// T Facebook.WitAi.Data.Entities.WitEntityDataBase`1::value
	int32_t ___value_8;
	// System.Single Facebook.WitAi.Data.Entities.WitEntityDataBase`1::confidence
	float ___confidence_9;
	// System.Boolean Facebook.WitAi.Data.Entities.WitEntityDataBase`1::hasData
	bool ___hasData_10;
	// Facebook.WitAi.Lib.WitResponseArray Facebook.WitAi.Data.Entities.WitEntityDataBase`1::entities
	WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* ___entities_11;
};

// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Single>
struct WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3  : public RuntimeObject
{
	// Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Data.Entities.WitEntityDataBase`1::responseNode
	WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___responseNode_0;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::id
	String_t* ___id_1;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::name
	String_t* ___name_2;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::role
	String_t* ___role_3;
	// System.Int32 Facebook.WitAi.Data.Entities.WitEntityDataBase`1::start
	int32_t ___start_4;
	// System.Int32 Facebook.WitAi.Data.Entities.WitEntityDataBase`1::end
	int32_t ___end_5;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::type
	String_t* ___type_6;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::body
	String_t* ___body_7;
	// T Facebook.WitAi.Data.Entities.WitEntityDataBase`1::value
	float ___value_8;
	// System.Single Facebook.WitAi.Data.Entities.WitEntityDataBase`1::confidence
	float ___confidence_9;
	// System.Boolean Facebook.WitAi.Data.Entities.WitEntityDataBase`1::hasData
	bool ___hasData_10;
	// Facebook.WitAi.Lib.WitResponseArray Facebook.WitAi.Data.Entities.WitEntityDataBase`1::entities
	WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* ___entities_11;
};

// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.String>
struct WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73  : public RuntimeObject
{
	// Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Data.Entities.WitEntityDataBase`1::responseNode
	WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___responseNode_0;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::id
	String_t* ___id_1;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::name
	String_t* ___name_2;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::role
	String_t* ___role_3;
	// System.Int32 Facebook.WitAi.Data.Entities.WitEntityDataBase`1::start
	int32_t ___start_4;
	// System.Int32 Facebook.WitAi.Data.Entities.WitEntityDataBase`1::end
	int32_t ___end_5;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::type
	String_t* ___type_6;
	// System.String Facebook.WitAi.Data.Entities.WitEntityDataBase`1::body
	String_t* ___body_7;
	// T Facebook.WitAi.Data.Entities.WitEntityDataBase`1::value
	String_t* ___value_8;
	// System.Single Facebook.WitAi.Data.Entities.WitEntityDataBase`1::confidence
	float ___confidence_9;
	// System.Boolean Facebook.WitAi.Data.Entities.WitEntityDataBase`1::hasData
	bool ___hasData_10;
	// Facebook.WitAi.Lib.WitResponseArray Facebook.WitAi.Data.Entities.WitEntityDataBase`1::entities
	WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* ___entities_11;
};
struct Il2CppArrayBounds;

// System.Text.RegularExpressions.Capture
struct Capture_tE11B735186DAFEE5F7A3BF5A739E9CCCE99DC24A  : public RuntimeObject
{
	// System.Int32 System.Text.RegularExpressions.Capture::<Index>k__BackingField
	int32_t ___U3CIndexU3Ek__BackingField_0;
	// System.Int32 System.Text.RegularExpressions.Capture::<Length>k__BackingField
	int32_t ___U3CLengthU3Ek__BackingField_1;
	// System.String System.Text.RegularExpressions.Capture::<Text>k__BackingField
	String_t* ___U3CTextU3Ek__BackingField_2;
};

// Facebook.WitAi.CallbackHandlers.ConfidenceRange
struct ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9  : public RuntimeObject
{
	// System.Single Facebook.WitAi.CallbackHandlers.ConfidenceRange::minConfidence
	float ___minConfidence_0;
	// System.Single Facebook.WitAi.CallbackHandlers.ConfidenceRange::maxConfidence
	float ___maxConfidence_1;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.CallbackHandlers.ConfidenceRange::onWithinConfidenceRange
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___onWithinConfidenceRange_2;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.CallbackHandlers.ConfidenceRange::onOutsideConfidenceRange
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___onOutsideConfidenceRange_3;
};

// Facebook.WitAi.CallbackHandlers.FormattedValueEvents
struct FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4  : public RuntimeObject
{
	// System.String Facebook.WitAi.CallbackHandlers.FormattedValueEvents::format
	String_t* ___format_0;
	// Facebook.WitAi.CallbackHandlers.ValueEvent Facebook.WitAi.CallbackHandlers.FormattedValueEvents::onFormattedValueEvent
	ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74* ___onFormattedValueEvent_1;
};

// System.Reflection.MemberInfo
struct MemberInfo_t  : public RuntimeObject
{
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

// UnityEngine.Events.UnityEventBase
struct UnityEventBase_t4968A4C72559F35C0923E4BD9C042C3A842E1DB8  : public RuntimeObject
{
	// UnityEngine.Events.InvokableCallList UnityEngine.Events.UnityEventBase::m_Calls
	InvokableCallList_t309E1C8C7CE885A0D2F98C84CEA77A8935688382* ___m_Calls_0;
	// UnityEngine.Events.PersistentCallGroup UnityEngine.Events.UnityEventBase::m_PersistentCalls
	PersistentCallGroup_tB826EDF15DC80F71BCBCD8E410FD959A04C33F25* ___m_PersistentCalls_1;
	// System.Boolean UnityEngine.Events.UnityEventBase::m_CallsDirty
	bool ___m_CallsDirty_2;
};

// Facebook.WitAi.CallbackHandlers.ValuePathMatcher
struct ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C  : public RuntimeObject
{
	// System.String Facebook.WitAi.CallbackHandlers.ValuePathMatcher::path
	String_t* ___path_0;
	// Facebook.WitAi.Data.WitValue Facebook.WitAi.CallbackHandlers.ValuePathMatcher::witValueReference
	WitValue_tD53CDFBDC3B12AB1495984258B6ACC38EEE55E32* ___witValueReference_1;
	// System.Boolean Facebook.WitAi.CallbackHandlers.ValuePathMatcher::contentRequired
	bool ___contentRequired_2;
	// Facebook.WitAi.CallbackHandlers.MatchMethod Facebook.WitAi.CallbackHandlers.ValuePathMatcher::matchMethod
	int32_t ___matchMethod_3;
	// Facebook.WitAi.CallbackHandlers.ComparisonMethod Facebook.WitAi.CallbackHandlers.ValuePathMatcher::comparisonMethod
	int32_t ___comparisonMethod_4;
	// System.String Facebook.WitAi.CallbackHandlers.ValuePathMatcher::matchValue
	String_t* ___matchValue_5;
	// System.Double Facebook.WitAi.CallbackHandlers.ValuePathMatcher::floatingPointComparisonTolerance
	double ___floatingPointComparisonTolerance_6;
	// Facebook.WitAi.CallbackHandlers.ConfidenceRange[] Facebook.WitAi.CallbackHandlers.ValuePathMatcher::confidenceRanges
	ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC* ___confidenceRanges_7;
	// Facebook.WitAi.WitResponseReference Facebook.WitAi.CallbackHandlers.ValuePathMatcher::pathReference
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* ___pathReference_8;
	// Facebook.WitAi.WitResponseReference Facebook.WitAi.CallbackHandlers.ValuePathMatcher::confidencePathReference
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* ___confidencePathReference_9;
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

// Facebook.WitAi.Events.VoiceEvents
struct VoiceEvents_tB9CCDD1C68F3A784600E7932935F705B17B8F1F5  : public RuntimeObject
{
	// Facebook.WitAi.Events.WitResponseEvent Facebook.WitAi.Events.VoiceEvents::OnResponse
	WitResponseEvent_t5DEE4CFA5D09DD8C6293E1D41EF6A9ACBDD2CFDD* ___OnResponse_0;
	// Facebook.WitAi.Events.WitErrorEvent Facebook.WitAi.Events.VoiceEvents::OnError
	WitErrorEvent_tFF53D8544CC1D1E7FD8E98A1EC8AD7581F62EE34* ___OnError_1;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnAborting
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnAborting_2;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnAborted
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnAborted_3;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnRequestCompleted
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnRequestCompleted_4;
	// Facebook.WitAi.Events.WitMicLevelChangedEvent Facebook.WitAi.Events.VoiceEvents::OnMicLevelChanged
	WitMicLevelChangedEvent_tA9B0047640D9DE7218C4699B6F6E0ABBCD966F1C* ___OnMicLevelChanged_5;
	// Facebook.WitAi.Events.WitRequestCreatedEvent Facebook.WitAi.Events.VoiceEvents::OnRequestCreated
	WitRequestCreatedEvent_t81EA9D8A3041CEADEBE670C485E17B1F9D5BEDF4* ___OnRequestCreated_6;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnStartListening
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnStartListening_7;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnStoppedListening
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnStoppedListening_8;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnStoppedListeningDueToInactivity
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnStoppedListeningDueToInactivity_9;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnStoppedListeningDueToTimeout
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnStoppedListeningDueToTimeout_10;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnStoppedListeningDueToDeactivation
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnStoppedListeningDueToDeactivation_11;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnMicDataSent
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnMicDataSent_12;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.Events.VoiceEvents::OnMinimumWakeThresholdHit
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___OnMinimumWakeThresholdHit_13;
	// Facebook.WitAi.Events.WitTranscriptionEvent Facebook.WitAi.Events.VoiceEvents::OnPartialTranscription
	WitTranscriptionEvent_t46F45C5FE99C833DFE345D2255587E22A1ABE32E* ___OnPartialTranscription_14;
	// Facebook.WitAi.Events.WitTranscriptionEvent Facebook.WitAi.Events.VoiceEvents::OnFullTranscription
	WitTranscriptionEvent_t46F45C5FE99C833DFE345D2255587E22A1ABE32E* ___OnFullTranscription_15;
	// Facebook.WitAi.Events.WitByteDataEvent Facebook.WitAi.Events.VoiceEvents::OnByteDataReady
	WitByteDataEvent_t3E7D04458B66F77BFF599F5CBAF9F0ADD920502E* ___OnByteDataReady_16;
	// Facebook.WitAi.Events.WitByteDataEvent Facebook.WitAi.Events.VoiceEvents::OnByteDataSent
	WitByteDataEvent_t3E7D04458B66F77BFF599F5CBAF9F0ADD920502E* ___OnByteDataSent_17;
};

// Facebook.WitAi.Configuration.WitConfigurationData
struct WitConfigurationData_tE8AEFEBCB9258DD2909EED5C1ACE770CBC099897  : public RuntimeObject
{
	// Facebook.WitAi.Data.Configuration.WitConfiguration Facebook.WitAi.Configuration.WitConfigurationData::witConfiguration
	WitConfiguration_t3D1E7D46065A2742877307705778E1CBC33530DD* ___witConfiguration_0;
};

// Facebook.WitAi.Data.Entities.WitDynamicEntities
struct WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054  : public RuntimeObject
{
	// System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitDynamicEntity> Facebook.WitAi.Data.Entities.WitDynamicEntities::entities
	List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB* ___entities_0;
};

// Facebook.WitAi.Data.Entities.WitDynamicEntity
struct WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB  : public RuntimeObject
{
	// System.String Facebook.WitAi.Data.Entities.WitDynamicEntity::entity
	String_t* ___entity_0;
	// System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitEntityKeyword> Facebook.WitAi.Data.Entities.WitDynamicEntity::keywords
	List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* ___keywords_1;
};

// Facebook.WitAi.Configuration.WitEndpointConfig
struct WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0  : public RuntimeObject
{
	// System.String Facebook.WitAi.Configuration.WitEndpointConfig::uriScheme
	String_t* ___uriScheme_1;
	// System.String Facebook.WitAi.Configuration.WitEndpointConfig::authority
	String_t* ___authority_2;
	// System.Int32 Facebook.WitAi.Configuration.WitEndpointConfig::port
	int32_t ___port_3;
	// System.String Facebook.WitAi.Configuration.WitEndpointConfig::witApiVersion
	String_t* ___witApiVersion_4;
	// System.String Facebook.WitAi.Configuration.WitEndpointConfig::speech
	String_t* ___speech_5;
	// System.String Facebook.WitAi.Configuration.WitEndpointConfig::message
	String_t* ___message_6;
};

struct WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0_StaticFields
{
	// Facebook.WitAi.Configuration.WitEndpointConfig Facebook.WitAi.Configuration.WitEndpointConfig::defaultEndpointConfig
	WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0* ___defaultEndpointConfig_0;
};

// Facebook.WitAi.Data.Entities.WitEntityKeyword
struct WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5  : public RuntimeObject
{
	// System.String Facebook.WitAi.Data.Entities.WitEntityKeyword::keyword
	String_t* ___keyword_0;
	// System.Collections.Generic.List`1<System.String> Facebook.WitAi.Data.Entities.WitEntityKeyword::synonyms
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* ___synonyms_1;
};

// Facebook.WitAi.Data.Entities.WitEntityRole
struct WitEntityRole_t47AC15DDC8D9F37888EB6747B6ABC142F0589453  : public RuntimeObject
{
	// System.String Facebook.WitAi.Data.Entities.WitEntityRole::id
	String_t* ___id_0;
	// System.String Facebook.WitAi.Data.Entities.WitEntityRole::name
	String_t* ___name_1;
};

// Facebook.WitAi.Lib.WitResponseNode
struct WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB  : public RuntimeObject
{
};

// Facebook.WitAi.WitResponseReference
struct WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E  : public RuntimeObject
{
	// Facebook.WitAi.WitResponseReference Facebook.WitAi.WitResponseReference::child
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* ___child_0;
	// System.String Facebook.WitAi.WitResponseReference::path
	String_t* ___path_1;
};

// Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass12_0
struct U3CU3Ec__DisplayClass12_0_t74EEAADB5D1467BB6A6581295207DCC9B25C9A7A  : public RuntimeObject
{
	// Facebook.WitAi.Data.Entities.WitDynamicEntity Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass12_0::dynamicEntity
	WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* ___dynamicEntity_0;
};

// Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass14_0
struct U3CU3Ec__DisplayClass14_0_t0E317FA09AA1ADB9E7810FCEBBDCF487BBBA7F75  : public RuntimeObject
{
	// System.String Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass14_0::entityName
	String_t* ___entityName_0;
};

// Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass15_0
struct U3CU3Ec__DisplayClass15_0_t9278361DC2379675C4FBCB0FBF4950B7B760FEBC  : public RuntimeObject
{
	// System.String Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass15_0::entityName
	String_t* ___entityName_0;
};

// Facebook.WitAi.Data.Entities.WitEntity/Fields
struct Fields_t7542C9E3FE2EF7822C73CE979E8967A8C04DE05B  : public RuntimeObject
{
};

// System.Collections.Generic.List`1/Enumerator<System.Object>
struct Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A 
{
	// System.Collections.Generic.List`1<T> System.Collections.Generic.List`1/Enumerator::_list
	List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* ____list_0;
	// System.Int32 System.Collections.Generic.List`1/Enumerator::_index
	int32_t ____index_1;
	// System.Int32 System.Collections.Generic.List`1/Enumerator::_version
	int32_t ____version_2;
	// T System.Collections.Generic.List`1/Enumerator::_current
	RuntimeObject* ____current_3;
};

// System.Collections.Generic.List`1/Enumerator<System.String>
struct Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1 
{
	// System.Collections.Generic.List`1<T> System.Collections.Generic.List`1/Enumerator::_list
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* ____list_0;
	// System.Int32 System.Collections.Generic.List`1/Enumerator::_index
	int32_t ____index_1;
	// System.Int32 System.Collections.Generic.List`1/Enumerator::_version
	int32_t ____version_2;
	// T System.Collections.Generic.List`1/Enumerator::_current
	String_t* ____current_3;
};

// System.Collections.Generic.List`1/Enumerator<Facebook.WitAi.Data.Entities.WitEntityKeyword>
struct Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A 
{
	// System.Collections.Generic.List`1<T> System.Collections.Generic.List`1/Enumerator::_list
	List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* ____list_0;
	// System.Int32 System.Collections.Generic.List`1/Enumerator::_index
	int32_t ____index_1;
	// System.Int32 System.Collections.Generic.List`1/Enumerator::_version
	int32_t ____version_2;
	// T System.Collections.Generic.List`1/Enumerator::_current
	WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* ____current_3;
};

// System.Collections.Generic.KeyValuePair`2<System.Object,System.Object>
struct KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230 
{
	// TKey System.Collections.Generic.KeyValuePair`2::key
	RuntimeObject* ___key_0;
	// TValue System.Collections.Generic.KeyValuePair`2::value
	RuntimeObject* ___value_1;
};

// System.Collections.Generic.KeyValuePair`2<System.String,System.Collections.Generic.List`1<System.String>>
struct KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89 
{
	// TKey System.Collections.Generic.KeyValuePair`2::key
	String_t* ___key_0;
	// TValue System.Collections.Generic.KeyValuePair`2::value
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* ___value_1;
};

// UnityEngine.Events.UnityEvent`1<System.String[]>
struct UnityEvent_1_t477222AA4CE1BE0F15900D352218F0A842BF5EFD  : public UnityEventBase_t4968A4C72559F35C0923E4BD9C042C3A842E1DB8
{
	// System.Object[] UnityEngine.Events.UnityEvent`1::m_InvokeArray
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___m_InvokeArray_3;
};

// UnityEngine.Events.UnityEvent`1<System.String>
struct UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257  : public UnityEventBase_t4968A4C72559F35C0923E4BD9C042C3A842E1DB8
{
	// System.Object[] UnityEngine.Events.UnityEvent`1::m_InvokeArray
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___m_InvokeArray_3;
};

// UnityEngine.Events.UnityEvent`1<Facebook.WitAi.Lib.WitResponseNode>
struct UnityEvent_1_t9B421777BDE1F0A6F551628B06DADBCD02D20366  : public UnityEventBase_t4968A4C72559F35C0923E4BD9C042C3A842E1DB8
{
	// System.Object[] UnityEngine.Events.UnityEvent`1::m_InvokeArray
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___m_InvokeArray_3;
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

// System.Double
struct Double_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F 
{
	// System.Double System.Double::m_value
	double ___m_value_0;
};

// System.Text.RegularExpressions.Group
struct Group_t26371E9136D6F43782C487B63C67C5FC4F472881  : public Capture_tE11B735186DAFEE5F7A3BF5A739E9CCCE99DC24A
{
	// System.Int32[] System.Text.RegularExpressions.Group::_caps
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____caps_4;
	// System.Int32 System.Text.RegularExpressions.Group::_capcount
	int32_t ____capcount_5;
	// System.Text.RegularExpressions.CaptureCollection System.Text.RegularExpressions.Group::_capcoll
	CaptureCollection_t38405272BD6A6DA77CD51487FD39624C6E95CC93* ____capcoll_6;
	// System.String System.Text.RegularExpressions.Group::<Name>k__BackingField
	String_t* ___U3CNameU3Ek__BackingField_7;
};

struct Group_t26371E9136D6F43782C487B63C67C5FC4F472881_StaticFields
{
	// System.Text.RegularExpressions.Group System.Text.RegularExpressions.Group::s_emptyGroup
	Group_t26371E9136D6F43782C487B63C67C5FC4F472881* ___s_emptyGroup_3;
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

// UnityEngine.Mathf
struct Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682 
{
	union
	{
		struct
		{
		};
		uint8_t Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682__padding[1];
	};
};

struct Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_StaticFields
{
	// System.Single UnityEngine.Mathf::Epsilon
	float ___Epsilon_0;
};

// System.Single
struct Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C 
{
	// System.Single System.Single::m_value
	float ___m_value_0;
};

// System.TimeSpan
struct TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A 
{
	// System.Int64 System.TimeSpan::_ticks
	int64_t ____ticks_22;
};

struct TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_StaticFields
{
	// System.TimeSpan System.TimeSpan::Zero
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___Zero_19;
	// System.TimeSpan System.TimeSpan::MaxValue
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___MaxValue_20;
	// System.TimeSpan System.TimeSpan::MinValue
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___MinValue_21;
};

// UnityEngine.Events.UnityEvent
struct UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977  : public UnityEventBase_t4968A4C72559F35C0923E4BD9C042C3A842E1DB8
{
	// System.Object[] UnityEngine.Events.UnityEvent::m_InvokeArray
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___m_InvokeArray_3;
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

// Facebook.WitAi.Data.Configuration.WitApplication
struct WitApplication_t63668A99EE80B05F17BCCDDAA4BF50A84A9A1E3C  : public WitConfigurationData_tE8AEFEBCB9258DD2909EED5C1ACE770CBC099897
{
	// System.String Facebook.WitAi.Data.Configuration.WitApplication::name
	String_t* ___name_1;
	// System.String Facebook.WitAi.Data.Configuration.WitApplication::id
	String_t* ___id_2;
	// System.String Facebook.WitAi.Data.Configuration.WitApplication::lang
	String_t* ___lang_3;
	// System.Boolean Facebook.WitAi.Data.Configuration.WitApplication::isPrivate
	bool ___isPrivate_4;
	// System.String Facebook.WitAi.Data.Configuration.WitApplication::createdAt
	String_t* ___createdAt_5;
};

// Facebook.WitAi.Data.Entities.WitEntity
struct WitEntity_t94A3BFC668CFCC30CC80133AEF7E9947F691F892  : public WitConfigurationData_tE8AEFEBCB9258DD2909EED5C1ACE770CBC099897
{
	// System.String Facebook.WitAi.Data.Entities.WitEntity::id
	String_t* ___id_1;
	// System.String Facebook.WitAi.Data.Entities.WitEntity::name
	String_t* ___name_2;
	// System.String[] Facebook.WitAi.Data.Entities.WitEntity::lookups
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___lookups_3;
	// Facebook.WitAi.Data.Entities.WitEntityRole[] Facebook.WitAi.Data.Entities.WitEntity::roles
	WitEntityRoleU5BU5D_t9BF80EDFBF052E416D7367E4384AEEE319BC7812* ___roles_4;
	// Facebook.WitAi.Data.Entities.WitEntityKeyword[] Facebook.WitAi.Data.Entities.WitEntity::keywords
	WitEntityKeywordU5BU5D_tEE7E30A869C03107C32A2F75DE4C9A22654C1BB8* ___keywords_5;
};

// Facebook.WitAi.Data.Entities.WitEntityData
struct WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC  : public WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73
{
};

// Facebook.WitAi.Data.Entities.WitEntityFloatData
struct WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B  : public WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3
{
};

// Facebook.WitAi.Data.Entities.WitEntityIntData
struct WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4  : public WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07
{
};

// Facebook.WitAi.Lib.WitResponseArray
struct WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05  : public WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB
{
	// System.Collections.Generic.List`1<Facebook.WitAi.Lib.WitResponseNode> Facebook.WitAi.Lib.WitResponseArray::m_List
	List_1_tC18B126FC489EF27D70BE515FE2369EB10D5FD4C* ___m_List_0;
};

// Facebook.WitAi.Lib.WitResponseClass
struct WitResponseClass_t739569309AE400E308EB8DD834327086751C855C  : public WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB
{
	// System.Collections.Generic.Dictionary`2<System.String,Facebook.WitAi.Lib.WitResponseNode> Facebook.WitAi.Lib.WitResponseClass::m_Dict
	Dictionary_2_tC75B15D6BE3719CB8EEF14082A03D3744B788B4F* ___m_Dict_0;
};

// Facebook.WitAi.Lib.WitResponseData
struct WitResponseData_t78CC4B9FA619C3E5C20CC8CB4338164E4AB5F899  : public WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB
{
	// System.String Facebook.WitAi.Lib.WitResponseData::m_Data
	String_t* ___m_Data_0;
};

// System.Collections.Generic.Dictionary`2/Enumerator<System.Object,System.Object>
struct Enumerator_tEA93FE2B778D098F590CA168BEFC4CD85D73A6B9 
{
	// System.Collections.Generic.Dictionary`2<TKey,TValue> System.Collections.Generic.Dictionary`2/Enumerator::_dictionary
	Dictionary_2_t14FE4A752A83D53771C584E4C8D14E01F2AFD7BA* ____dictionary_0;
	// System.Int32 System.Collections.Generic.Dictionary`2/Enumerator::_version
	int32_t ____version_1;
	// System.Int32 System.Collections.Generic.Dictionary`2/Enumerator::_index
	int32_t ____index_2;
	// System.Collections.Generic.KeyValuePair`2<TKey,TValue> System.Collections.Generic.Dictionary`2/Enumerator::_current
	KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230 ____current_3;
	// System.Int32 System.Collections.Generic.Dictionary`2/Enumerator::_getEnumeratorRetType
	int32_t ____getEnumeratorRetType_4;
};

// System.Collections.Generic.Dictionary`2/Enumerator<System.String,System.Collections.Generic.List`1<System.String>>
struct Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250 
{
	// System.Collections.Generic.Dictionary`2<TKey,TValue> System.Collections.Generic.Dictionary`2/Enumerator::_dictionary
	Dictionary_2_t79BA378F246EFA4AD0AFFA017D788423CACA8638* ____dictionary_0;
	// System.Int32 System.Collections.Generic.Dictionary`2/Enumerator::_version
	int32_t ____version_1;
	// System.Int32 System.Collections.Generic.Dictionary`2/Enumerator::_index
	int32_t ____index_2;
	// System.Collections.Generic.KeyValuePair`2<TKey,TValue> System.Collections.Generic.Dictionary`2/Enumerator::_current
	KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89 ____current_3;
	// System.Int32 System.Collections.Generic.Dictionary`2/Enumerator::_getEnumeratorRetType
	int32_t ____getEnumeratorRetType_4;
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

// System.Text.RegularExpressions.Match
struct Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F  : public Group_t26371E9136D6F43782C487B63C67C5FC4F472881
{
	// System.Text.RegularExpressions.GroupCollection System.Text.RegularExpressions.Match::_groupcoll
	GroupCollection_tFFA1789730DD9EA122FBE77DC03BFEDCC3F2945E* ____groupcoll_8;
	// System.Text.RegularExpressions.Regex System.Text.RegularExpressions.Match::_regex
	Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* ____regex_9;
	// System.Int32 System.Text.RegularExpressions.Match::_textbeg
	int32_t ____textbeg_10;
	// System.Int32 System.Text.RegularExpressions.Match::_textpos
	int32_t ____textpos_11;
	// System.Int32 System.Text.RegularExpressions.Match::_textend
	int32_t ____textend_12;
	// System.Int32 System.Text.RegularExpressions.Match::_textstart
	int32_t ____textstart_13;
	// System.Int32[][] System.Text.RegularExpressions.Match::_matches
	Int32U5BU5DU5BU5D_t179D865D5B30EFCBC50F82C9774329C15943466E* ____matches_14;
	// System.Int32[] System.Text.RegularExpressions.Match::_matchcount
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____matchcount_15;
	// System.Boolean System.Text.RegularExpressions.Match::_balancing
	bool ____balancing_16;
};

struct Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F_StaticFields
{
	// System.Text.RegularExpressions.Match System.Text.RegularExpressions.Match::<Empty>k__BackingField
	Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F* ___U3CEmptyU3Ek__BackingField_17;
};

// Facebook.WitAi.CallbackHandlers.MultiValueEvent
struct MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C  : public UnityEvent_1_t477222AA4CE1BE0F15900D352218F0A842BF5EFD
{
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

// System.Text.RegularExpressions.Regex
struct Regex_tE773142C2BE45C5D362B0F815AFF831707A51772  : public RuntimeObject
{
	// System.TimeSpan System.Text.RegularExpressions.Regex::internalMatchTimeout
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___internalMatchTimeout_10;
	// System.String System.Text.RegularExpressions.Regex::pattern
	String_t* ___pattern_12;
	// System.Text.RegularExpressions.RegexOptions System.Text.RegularExpressions.Regex::roptions
	int32_t ___roptions_13;
	// System.Text.RegularExpressions.RegexRunnerFactory System.Text.RegularExpressions.Regex::factory
	RegexRunnerFactory_t72373B672C7D8785F63516DDD88834F286AF41E7* ___factory_14;
	// System.Collections.Hashtable System.Text.RegularExpressions.Regex::caps
	Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D* ___caps_15;
	// System.Collections.Hashtable System.Text.RegularExpressions.Regex::capnames
	Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D* ___capnames_16;
	// System.String[] System.Text.RegularExpressions.Regex::capslist
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___capslist_17;
	// System.Int32 System.Text.RegularExpressions.Regex::capsize
	int32_t ___capsize_18;
	// System.Text.RegularExpressions.ExclusiveReference System.Text.RegularExpressions.Regex::_runnerref
	ExclusiveReference_t411F04D4CC440EB7399290027E1BBABEF4C28837* ____runnerref_19;
	// System.WeakReference`1<System.Text.RegularExpressions.RegexReplacement> System.Text.RegularExpressions.Regex::_replref
	WeakReference_1_tDC6E83496181D1BAFA3B89CBC00BCD0B64450257* ____replref_20;
	// System.Text.RegularExpressions.RegexCode System.Text.RegularExpressions.Regex::_code
	RegexCode_tA23175D9DA02AD6A79B073E10EC5D225372ED6C7* ____code_21;
	// System.Boolean System.Text.RegularExpressions.Regex::_refsInitialized
	bool ____refsInitialized_22;
};

struct Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_StaticFields
{
	// System.Int32 System.Text.RegularExpressions.Regex::s_cacheSize
	int32_t ___s_cacheSize_1;
	// System.Collections.Generic.Dictionary`2<System.Text.RegularExpressions.Regex/CachedCodeEntryKey,System.Text.RegularExpressions.Regex/CachedCodeEntry> System.Text.RegularExpressions.Regex::s_cache
	Dictionary_2_t5B5B38BB06341F50E1C75FB53208A2A66CAE57F7* ___s_cache_2;
	// System.Int32 System.Text.RegularExpressions.Regex::s_cacheCount
	int32_t ___s_cacheCount_3;
	// System.Text.RegularExpressions.Regex/CachedCodeEntry System.Text.RegularExpressions.Regex::s_cacheFirst
	CachedCodeEntry_tE201C3AD65C234AD9ED7A78C95025824A7A9FF39* ___s_cacheFirst_4;
	// System.Text.RegularExpressions.Regex/CachedCodeEntry System.Text.RegularExpressions.Regex::s_cacheLast
	CachedCodeEntry_tE201C3AD65C234AD9ED7A78C95025824A7A9FF39* ___s_cacheLast_5;
	// System.TimeSpan System.Text.RegularExpressions.Regex::s_maximumMatchTimeout
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___s_maximumMatchTimeout_6;
	// System.TimeSpan System.Text.RegularExpressions.Regex::s_defaultMatchTimeout
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___s_defaultMatchTimeout_8;
	// System.TimeSpan System.Text.RegularExpressions.Regex::InfiniteMatchTimeout
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___InfiniteMatchTimeout_9;
};

// System.RuntimeTypeHandle
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	// System.IntPtr System.RuntimeTypeHandle::value
	intptr_t ___value_0;
};

// Facebook.WitAi.CallbackHandlers.StringEntityMatchEvent
struct StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7  : public UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257
{
};

// Facebook.WitAi.Utilities.StringEvent
struct StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76  : public UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257
{
};

// Facebook.WitAi.CallbackHandlers.ValueEvent
struct ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74  : public UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257
{
};

// Facebook.WitAi.Events.WitResponseEvent
struct WitResponseEvent_t5DEE4CFA5D09DD8C6293E1D41EF6A9ACBDD2CFDD  : public UnityEvent_1_t9B421777BDE1F0A6F551628B06DADBCD02D20366
{
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

// UnityEngine.ScriptableObject
struct ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
// Native definition for P/Invoke marshalling of UnityEngine.ScriptableObject
struct ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A_marshaled_pinvoke : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_pinvoke
{
};
// Native definition for COM marshalling of UnityEngine.ScriptableObject
struct ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A_marshaled_com : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_com
{
};

// System.Type
struct Type_t  : public MemberInfo_t
{
	// System.RuntimeTypeHandle System.Type::_impl
	RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ____impl_8;
};

struct Type_t_StaticFields
{
	// System.Reflection.Binder modreq(System.Runtime.CompilerServices.IsVolatile) System.Type::s_defaultBinder
	Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235* ___s_defaultBinder_0;
	// System.Char System.Type::Delimiter
	Il2CppChar ___Delimiter_1;
	// System.Type[] System.Type::EmptyTypes
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___EmptyTypes_2;
	// System.Object System.Type::Missing
	RuntimeObject* ___Missing_3;
	// System.Reflection.MemberFilter System.Type::FilterAttribute
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterAttribute_4;
	// System.Reflection.MemberFilter System.Type::FilterName
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterName_5;
	// System.Reflection.MemberFilter System.Type::FilterNameIgnoreCase
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterNameIgnoreCase_6;
};

// UnityEngine.Events.UnityAction`1<Facebook.WitAi.Lib.WitResponseNode>
struct UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8  : public MulticastDelegate_t
{
};

// UnityEngine.Behaviour
struct Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA  : public Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3
{
};

// Facebook.WitAi.Data.Configuration.WitConfiguration
struct WitConfiguration_t3D1E7D46065A2742877307705778E1CBC33530DD  : public ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A
{
	// Facebook.WitAi.Data.Configuration.WitApplication Facebook.WitAi.Data.Configuration.WitConfiguration::application
	WitApplication_t63668A99EE80B05F17BCCDDAA4BF50A84A9A1E3C* ___application_4;
	// System.String Facebook.WitAi.Data.Configuration.WitConfiguration::configId
	String_t* ___configId_5;
	// System.String Facebook.WitAi.Data.Configuration.WitConfiguration::clientAccessToken
	String_t* ___clientAccessToken_6;
	// System.Int32 Facebook.WitAi.Data.Configuration.WitConfiguration::timeoutMS
	int32_t ___timeoutMS_7;
	// Facebook.WitAi.Configuration.WitEndpointConfig Facebook.WitAi.Data.Configuration.WitConfiguration::endpointConfiguration
	WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0* ___endpointConfiguration_8;
	// Facebook.WitAi.Data.Entities.WitEntity[] Facebook.WitAi.Data.Configuration.WitConfiguration::entities
	WitEntityU5BU5D_tC65502A56DE214BCA9ECD0258022C310876CDC65* ___entities_9;
	// Facebook.WitAi.Data.Intents.WitIntent[] Facebook.WitAi.Data.Configuration.WitConfiguration::intents
	WitIntentU5BU5D_tDB2609D617BB35FBBC73A93A1C8B54DF6B24ADAB* ___intents_10;
	// Facebook.WitAi.Data.Traits.WitTrait[] Facebook.WitAi.Data.Configuration.WitConfiguration::traits
	WitTraitU5BU5D_t61DB38855B37C3123DBC4FC8B31E08E69BB8EC31* ___traits_11;
	// System.Boolean Facebook.WitAi.Data.Configuration.WitConfiguration::isDemoOnly
	bool ___isDemoOnly_12;
};

// Facebook.WitAi.Data.Entities.WitDynamicEntitiesData
struct WitDynamicEntitiesData_t89F1557016C7917FB0136B98FE1DC9C3CDDA2564  : public ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A
{
	// Facebook.WitAi.Data.Entities.WitDynamicEntities Facebook.WitAi.Data.Entities.WitDynamicEntitiesData::entities
	WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* ___entities_4;
};

// Facebook.WitAi.Data.WitValue
struct WitValue_tD53CDFBDC3B12AB1495984258B6ACC38EEE55E32  : public ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A
{
	// System.String Facebook.WitAi.Data.WitValue::path
	String_t* ___path_4;
	// Facebook.WitAi.WitResponseReference Facebook.WitAi.Data.WitValue::reference
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* ___reference_5;
};

// UnityEngine.MonoBehaviour
struct MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71  : public Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA
{
};

// Facebook.WitAi.VoiceService
struct VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99  : public MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71
{
	// Facebook.WitAi.Events.VoiceEvents Facebook.WitAi.VoiceService::events
	VoiceEvents_tB9CCDD1C68F3A784600E7932935F705B17B8F1F5* ___events_4;
};

// Facebook.WitAi.CallbackHandlers.WitResponseHandler
struct WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B  : public MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71
{
	// Facebook.WitAi.VoiceService Facebook.WitAi.CallbackHandlers.WitResponseHandler::wit
	VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* ___wit_4;
};

// Facebook.WitAi.Data.Entities.WitSimpleDynamicEntity
struct WitSimpleDynamicEntity_t7462383F5C63BB5F2B1A153A6E2F4506651692DB  : public MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71
{
	// System.String Facebook.WitAi.Data.Entities.WitSimpleDynamicEntity::entityName
	String_t* ___entityName_4;
	// System.String[] Facebook.WitAi.Data.Entities.WitSimpleDynamicEntity::keywords
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___keywords_5;
};

// Facebook.WitAi.CallbackHandlers.OutOfScopeUtteranceHandler
struct OutOfScopeUtteranceHandler_t6D1CE6F4B6A31C7CBD4E195FAADF88D3829984D1  : public WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B
{
	// UnityEngine.Events.UnityEvent Facebook.WitAi.CallbackHandlers.OutOfScopeUtteranceHandler::onOutOfDomain
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___onOutOfDomain_5;
};

// Facebook.WitAi.CallbackHandlers.SimpleIntentHandler
struct SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1  : public WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B
{
	// System.String Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::intent
	String_t* ___intent_5;
	// System.Single Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::confidence
	float ___confidence_6;
	// UnityEngine.Events.UnityEvent Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::onIntentTriggered
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* ___onIntentTriggered_7;
	// System.Boolean Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::allowConfidenceOverlap
	bool ___allowConfidenceOverlap_8;
	// Facebook.WitAi.CallbackHandlers.ConfidenceRange[] Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::confidenceRanges
	ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC* ___confidenceRanges_9;
};

// Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler
struct SimpleStringEntityHandler_tB95E77E27B20E13042B687D717A45B509D5166DC  : public WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B
{
	// System.String Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler::intent
	String_t* ___intent_5;
	// System.String Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler::entity
	String_t* ___entity_6;
	// System.Single Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler::confidence
	float ___confidence_7;
	// System.String Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler::format
	String_t* ___format_8;
	// Facebook.WitAi.CallbackHandlers.StringEntityMatchEvent Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler::onIntentEntityTriggered
	StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7* ___onIntentEntityTriggered_9;
};

// Facebook.WitAi.CallbackHandlers.WitResponseMatcher
struct WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900  : public WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B
{
	// System.String Facebook.WitAi.CallbackHandlers.WitResponseMatcher::intent
	String_t* ___intent_5;
	// System.Single Facebook.WitAi.CallbackHandlers.WitResponseMatcher::confidenceThreshold
	float ___confidenceThreshold_6;
	// Facebook.WitAi.CallbackHandlers.ValuePathMatcher[] Facebook.WitAi.CallbackHandlers.WitResponseMatcher::valueMatchers
	ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75* ___valueMatchers_7;
	// Facebook.WitAi.CallbackHandlers.FormattedValueEvents[] Facebook.WitAi.CallbackHandlers.WitResponseMatcher::formattedValueEvents
	FormattedValueEventsU5BU5D_tAFBBA48044157DCAA323FD47280150A31580C188* ___formattedValueEvents_8;
	// Facebook.WitAi.CallbackHandlers.MultiValueEvent Facebook.WitAi.CallbackHandlers.WitResponseMatcher::onMultiValueEvent
	MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C* ___onMultiValueEvent_9;
};

struct WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_StaticFields
{
	// System.Text.RegularExpressions.Regex Facebook.WitAi.CallbackHandlers.WitResponseMatcher::valueRegex
	Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* ___valueRegex_10;
};

// Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher
struct WitUtteranceMatcher_t1A2C2BA7A4F1DFF63A0266115636A1FEFD2166C4  : public WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B
{
	// System.String Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher::searchText
	String_t* ___searchText_5;
	// System.Boolean Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher::exactMatch
	bool ___exactMatch_6;
	// System.Boolean Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher::useRegex
	bool ___useRegex_7;
	// Facebook.WitAi.Utilities.StringEvent Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher::onUtteranceMatched
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* ___onUtteranceMatched_8;
	// System.Text.RegularExpressions.Regex Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher::regex
	Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* ___regex_9;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
// System.String[]
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248  : public RuntimeArray
{
	ALIGN_FIELD (8) String_t* m_Items[1];

	inline String_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline String_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, String_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline String_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline String_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, String_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
// Facebook.WitAi.Data.Entities.WitDynamicEntity[]
struct WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D  : public RuntimeArray
{
	ALIGN_FIELD (8) WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* m_Items[1];

	inline WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
// Facebook.WitAi.CallbackHandlers.ConfidenceRange[]
struct ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC  : public RuntimeArray
{
	ALIGN_FIELD (8) ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* m_Items[1];

	inline ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
// Facebook.WitAi.CallbackHandlers.FormattedValueEvents[]
struct FormattedValueEventsU5BU5D_tAFBBA48044157DCAA323FD47280150A31580C188  : public RuntimeArray
{
	ALIGN_FIELD (8) FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* m_Items[1];

	inline FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
// Facebook.WitAi.CallbackHandlers.ValuePathMatcher[]
struct ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75  : public RuntimeArray
{
	ALIGN_FIELD (8) ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* m_Items[1];

	inline ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
// System.Object[]
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918  : public RuntimeArray
{
	ALIGN_FIELD (8) RuntimeObject* m_Items[1];

	inline RuntimeObject* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, RuntimeObject* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline RuntimeObject* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, RuntimeObject* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};


// System.Void System.Collections.Generic.List`1<System.Object>::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void List_1__ctor_m7F078BB342729BDF11327FD89D7872265328F690_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.Object>::Add(T)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___item0, const RuntimeMethod* method) ;
// System.Collections.Generic.Dictionary`2/Enumerator<TKey,TValue> System.Collections.Generic.Dictionary`2<System.Object,System.Object>::GetEnumerator()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_tEA93FE2B778D098F590CA168BEFC4CD85D73A6B9 Dictionary_2_GetEnumerator_m52AB12790B0B9B46B1DFB1F861C9DBEAB07C1FDA_gshared (Dictionary_2_t14FE4A752A83D53771C584E4C8D14E01F2AFD7BA* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.Dictionary`2/Enumerator<System.Object,System.Object>::Dispose()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator_Dispose_mEA5E01B81EB943B7003D87CEC1B6040524F0402C_gshared (Enumerator_tEA93FE2B778D098F590CA168BEFC4CD85D73A6B9* __this, const RuntimeMethod* method) ;
// System.Collections.Generic.KeyValuePair`2<TKey,TValue> System.Collections.Generic.Dictionary`2/Enumerator<System.Object,System.Object>::get_Current()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230 Enumerator_get_Current_mE3475384B761E1C7971D3639BD09117FE8363422_gshared_inline (Enumerator_tEA93FE2B778D098F590CA168BEFC4CD85D73A6B9* __this, const RuntimeMethod* method) ;
// TKey System.Collections.Generic.KeyValuePair`2<System.Object,System.Object>::get_Key()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* KeyValuePair_2_get_Key_mBD8EA7557C27E6956F2AF29DA3F7499B2F51A282_gshared_inline (KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230* __this, const RuntimeMethod* method) ;
// TValue System.Collections.Generic.KeyValuePair`2<System.Object,System.Object>::get_Value()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* KeyValuePair_2_get_Value_mC6BD8075F9C9DDEF7B4D731E5C38EC19103988E7_gshared_inline (KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230* __this, const RuntimeMethod* method) ;
// System.Boolean System.Collections.Generic.Dictionary`2/Enumerator<System.Object,System.Object>::MoveNext()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Enumerator_MoveNext_mCD4950A75FFADD54AF354D48C6C0DB0B5A22A5F4_gshared (Enumerator_tEA93FE2B778D098F590CA168BEFC4CD85D73A6B9* __this, const RuntimeMethod* method) ;
// System.Collections.Generic.List`1/Enumerator<T> System.Collections.Generic.List`1<System.Object>::GetEnumerator()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A List_1_GetEnumerator_mD8294A7FA2BEB1929487127D476F8EC1CDC23BFC_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1/Enumerator<System.Object>::Dispose()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator_Dispose_mD9DC3E3C3697830A4823047AB29A77DBBB5ED419_gshared (Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A* __this, const RuntimeMethod* method) ;
// T System.Collections.Generic.List`1/Enumerator<System.Object>::get_Current()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* Enumerator_get_Current_m6330F15D18EE4F547C05DF9BF83C5EB710376027_gshared_inline (Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A* __this, const RuntimeMethod* method) ;
// System.Boolean System.Collections.Generic.List`1/Enumerator<System.Object>::MoveNext()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Enumerator_MoveNext_mE921CC8F29FBBDE7CC3209A0ED0D921D58D00BCB_gshared (Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A* __this, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Object>::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityDataBase_1__ctor_mBE1172303D1D1230A11FF1B8EC38110688115B78_gshared (WitEntityDataBase_1_tD992377A5996E2EC27D289A60634F3FAE327D362* __this, const RuntimeMethod* method) ;
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<T> Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Object>::FromEntityWitResponseNode(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitEntityDataBase_1_tD992377A5996E2EC27D289A60634F3FAE327D362* WitEntityDataBase_1_FromEntityWitResponseNode_mA2434A6859F2D25D71070D839414E7E640D2DF2F_gshared (WitEntityDataBase_1_tD992377A5996E2EC27D289A60634F3FAE327D362* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Single>::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893_gshared (WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3* __this, const RuntimeMethod* method) ;
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<T> Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Single>::FromEntityWitResponseNode(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3* WitEntityDataBase_1_FromEntityWitResponseNode_mEBB6417BC069B64576611835830E1CCE7D824FCF_gshared (WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Int32>::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8_gshared (WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07* __this, const RuntimeMethod* method) ;
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<T> Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Int32>::FromEntityWitResponseNode(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07* WitEntityDataBase_1_FromEntityWitResponseNode_m63B33E2731C95F8A7209705B9BDBACFDA0FC8557_gshared (WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.Object>::AddRange(System.Collections.Generic.IEnumerable`1<T>)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void List_1_AddRange_m1F76B300133150E6046C5FED00E88B5DE0A02E17_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___collection0, const RuntimeMethod* method) ;
// System.Collections.Generic.List`1<TSource> System.Linq.Enumerable::ToList<System.Object>(System.Collections.Generic.IEnumerable`1<TSource>)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* Enumerable_ToList_TisRuntimeObject_mBDB9895C2D14F2A92043507996018A329BD32A64_gshared (RuntimeObject* ___source0, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent`1<System.Object>::Invoke(T0)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityEvent_1_Invoke_m6CDC8B0639CE8935E2E13D10B2C8E500968130B6_gshared (UnityEvent_1_t3CE03B42D5873C0C0E0692BEE72E1E6D5399F205* __this, RuntimeObject* ___arg00, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent`1<System.Object>::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityEvent_1__ctor_m8D77F4F05F69D0E52E8A445322811EEC25987525_gshared (UnityEvent_1_t3CE03B42D5873C0C0E0692BEE72E1E6D5399F205* __this, const RuntimeMethod* method) ;
// T UnityEngine.Object::FindObjectOfType<System.Object>()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Object_FindObjectOfType_TisRuntimeObject_m9990A7304DF02BA1ED160587D1C2F6DAE89BB343_gshared (const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityAction`1<System.Object>::.ctor(System.Object,System.IntPtr)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityAction_1__ctor_m0C2FC6B483B474AE9596A43EBA7FF6E85503A92A_gshared (UnityAction_1_t9C30BCD020745BF400CBACF22C6F34ADBA2DDA6A* __this, RuntimeObject* ___object0, intptr_t ___method1, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent`1<System.Object>::AddListener(UnityEngine.Events.UnityAction`1<T0>)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityEvent_1_AddListener_m055233246714700E4BDAA62635BC0AA49E8165CC_gshared (UnityEvent_1_t3CE03B42D5873C0C0E0692BEE72E1E6D5399F205* __this, UnityAction_1_t9C30BCD020745BF400CBACF22C6F34ADBA2DDA6A* ___call0, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent`1<System.Object>::RemoveListener(UnityEngine.Events.UnityAction`1<T0>)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityEvent_1_RemoveListener_m904FA6BDD0D33FDF8650EF816FF5C131867E693E_gshared (UnityEvent_1_t3CE03B42D5873C0C0E0692BEE72E1E6D5399F205* __this, UnityAction_1_t9C30BCD020745BF400CBACF22C6F34ADBA2DDA6A* ___call0, const RuntimeMethod* method) ;
// T[] System.Collections.Generic.List`1<System.Object>::ToArray()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* List_1_ToArray_mD7E4F8E7C11C3C67CB5739FCC0A6E86106A6291F_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.Object>::AddWithResize(T)
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___item0, const RuntimeMethod* method) ;

// System.Void System.Object::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
// System.Boolean System.String::op_Equality(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0 (String_t* ___a0, String_t* ___b1, const RuntimeMethod* method) ;
// System.Void UnityEngine.ScriptableObject::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ScriptableObject__ctor_mD037FDB0B487295EA47F79A4DB1BF1846C9087FF (ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitEntityKeyword>::.ctor()
inline void List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* __this, const RuntimeMethod* method)
{
	((  void (*) (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887*, const RuntimeMethod*))List_1__ctor_m7F078BB342729BDF11327FD89D7872265328F690_gshared)(__this, method);
}
// System.Void System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitEntityKeyword>::Add(T)
inline void List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_inline (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* __this, WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* ___item0, const RuntimeMethod* method)
{
	((  void (*) (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887*, WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5*, const RuntimeMethod*))List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline)(__this, ___item0, method);
}
// System.Void Facebook.WitAi.Data.Entities.WitEntityKeyword::.ctor(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityKeyword__ctor_m0F0DC376B974BD93B174E7A4D6AC604DE0BEB8FB (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* __this, String_t* ___keyword0, const RuntimeMethod* method) ;
// System.Collections.Generic.Dictionary`2/Enumerator<TKey,TValue> System.Collections.Generic.Dictionary`2<System.String,System.Collections.Generic.List`1<System.String>>::GetEnumerator()
inline Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250 Dictionary_2_GetEnumerator_m7B0E05167CA47611244E25AD67EBCEBDF5DB5EDE (Dictionary_2_t79BA378F246EFA4AD0AFFA017D788423CACA8638* __this, const RuntimeMethod* method)
{
	return ((  Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250 (*) (Dictionary_2_t79BA378F246EFA4AD0AFFA017D788423CACA8638*, const RuntimeMethod*))Dictionary_2_GetEnumerator_m52AB12790B0B9B46B1DFB1F861C9DBEAB07C1FDA_gshared)(__this, method);
}
// System.Void System.Collections.Generic.Dictionary`2/Enumerator<System.String,System.Collections.Generic.List`1<System.String>>::Dispose()
inline void Enumerator_Dispose_m7DE9243F0F854CE156AFB007FA010C1AB4321A44 (Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250* __this, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250*, const RuntimeMethod*))Enumerator_Dispose_mEA5E01B81EB943B7003D87CEC1B6040524F0402C_gshared)(__this, method);
}
// System.Collections.Generic.KeyValuePair`2<TKey,TValue> System.Collections.Generic.Dictionary`2/Enumerator<System.String,System.Collections.Generic.List`1<System.String>>::get_Current()
inline KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89 Enumerator_get_Current_mB41E65E33E936ACBD32C200633257CBD3261A9E2_inline (Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250* __this, const RuntimeMethod* method)
{
	return ((  KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89 (*) (Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250*, const RuntimeMethod*))Enumerator_get_Current_mE3475384B761E1C7971D3639BD09117FE8363422_gshared_inline)(__this, method);
}
// System.Void Facebook.WitAi.Data.Entities.WitEntityKeyword::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityKeyword__ctor_m428EA8D89870BEBE0C2CCFF4041C82EB3171DD6C (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* __this, const RuntimeMethod* method) ;
// TKey System.Collections.Generic.KeyValuePair`2<System.String,System.Collections.Generic.List`1<System.String>>::get_Key()
inline String_t* KeyValuePair_2_get_Key_m852B0BE1941AFE5DA3A393534C0020AEAB59D0A6_inline (KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89* __this, const RuntimeMethod* method)
{
	return ((  String_t* (*) (KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89*, const RuntimeMethod*))KeyValuePair_2_get_Key_mBD8EA7557C27E6956F2AF29DA3F7499B2F51A282_gshared_inline)(__this, method);
}
// TValue System.Collections.Generic.KeyValuePair`2<System.String,System.Collections.Generic.List`1<System.String>>::get_Value()
inline List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* KeyValuePair_2_get_Value_mD55616E290F6591FA95053D1C8D2DDA2E49A058C_inline (KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89* __this, const RuntimeMethod* method)
{
	return ((  List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* (*) (KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89*, const RuntimeMethod*))KeyValuePair_2_get_Value_mC6BD8075F9C9DDEF7B4D731E5C38EC19103988E7_gshared_inline)(__this, method);
}
// System.Boolean System.Collections.Generic.Dictionary`2/Enumerator<System.String,System.Collections.Generic.List`1<System.String>>::MoveNext()
inline bool Enumerator_MoveNext_mA71548765692C241BEFB57E80CBC1563829B1656 (Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250*, const RuntimeMethod*))Enumerator_MoveNext_mCD4950A75FFADD54AF354D48C6C0DB0B5A22A5F4_gshared)(__this, method);
}
// System.Void Facebook.WitAi.Lib.WitResponseArray::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseArray__ctor_m6D22459257B33D9C45FF0EA9619A9AA49124606D (WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* __this, const RuntimeMethod* method) ;
// System.Collections.Generic.List`1/Enumerator<T> System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitEntityKeyword>::GetEnumerator()
inline Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A List_1_GetEnumerator_m4CCCD59E97F72FE069AAFD946983DDD5D8591FD1 (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* __this, const RuntimeMethod* method)
{
	return ((  Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A (*) (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887*, const RuntimeMethod*))List_1_GetEnumerator_mD8294A7FA2BEB1929487127D476F8EC1CDC23BFC_gshared)(__this, method);
}
// System.Void System.Collections.Generic.List`1/Enumerator<Facebook.WitAi.Data.Entities.WitEntityKeyword>::Dispose()
inline void Enumerator_Dispose_m25FAB013E20621ED9156A5D8602E37E6979CCDCB (Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A* __this, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A*, const RuntimeMethod*))Enumerator_Dispose_mD9DC3E3C3697830A4823047AB29A77DBBB5ED419_gshared)(__this, method);
}
// T System.Collections.Generic.List`1/Enumerator<Facebook.WitAi.Data.Entities.WitEntityKeyword>::get_Current()
inline WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* Enumerator_get_Current_mD56264A4D181F6240745145CE039E2D7750313AF_inline (Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A* __this, const RuntimeMethod* method)
{
	return ((  WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* (*) (Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A*, const RuntimeMethod*))Enumerator_get_Current_m6330F15D18EE4F547C05DF9BF83C5EB710376027_gshared_inline)(__this, method);
}
// Facebook.WitAi.Lib.WitResponseClass Facebook.WitAi.Data.Entities.WitEntityKeyword::get_AsJson()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* WitEntityKeyword_get_AsJson_mF26B952D2770B770116A8D862D1DAA6853653DC6 (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* __this, const RuntimeMethod* method) ;
// System.Boolean System.Collections.Generic.List`1/Enumerator<Facebook.WitAi.Data.Entities.WitEntityKeyword>::MoveNext()
inline bool Enumerator_MoveNext_m8D1B5472B8F40695AD00E362DF9B0CCE4F919D7C (Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A*, const RuntimeMethod*))Enumerator_MoveNext_mE921CC8F29FBBDE7CC3209A0ED0D921D58D00BCB_gshared)(__this, method);
}
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntities::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitDynamicEntities__ctor_mFACF5C5BC88B719671AEC348696B8ABDDE7FD800 (WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitDynamicEntity>::.ctor()
inline void List_1__ctor_m38543D708664CDA034C6B6D276D1EA261BEE2796 (List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB* __this, const RuntimeMethod* method)
{
	((  void (*) (List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB*, const RuntimeMethod*))List_1__ctor_m7F078BB342729BDF11327FD89D7872265328F690_gshared)(__this, method);
}
// System.Void System.Collections.Generic.List`1<Facebook.WitAi.Data.Entities.WitDynamicEntity>::Add(T)
inline void List_1_Add_m4AFBFA0F69A9E9C42FDF4CBBED43E6C26A6EDE28_inline (List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB* __this, WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* ___item0, const RuntimeMethod* method)
{
	((  void (*) (List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB*, WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB*, const RuntimeMethod*))List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline)(__this, ___item0, method);
}
// System.Void Facebook.WitAi.Configuration.WitConfigurationData::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitConfigurationData__ctor_m8D4068A08641205F93EF9642BA9106B0EFD37975 (WitConfigurationData_tE8AEFEBCB9258DD2909EED5C1ACE770CBC099897* __this, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.String>::.ctor()
inline void WitEntityDataBase_1__ctor_m954C8C7C12B6AE8DC251B99574E791D559661402 (WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73* __this, const RuntimeMethod* method)
{
	((  void (*) (WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73*, const RuntimeMethod*))WitEntityDataBase_1__ctor_mBE1172303D1D1230A11FF1B8EC38110688115B78_gshared)(__this, method);
}
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<T> Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.String>::FromEntityWitResponseNode(Facebook.WitAi.Lib.WitResponseNode)
inline WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73* WitEntityDataBase_1_FromEntityWitResponseNode_m44BCC95882F476B59D585A3BB4B76287A13C528F (WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method)
{
	return ((  WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73* (*) (WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73*, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, const RuntimeMethod*))WitEntityDataBase_1_FromEntityWitResponseNode_mA2434A6859F2D25D71070D839414E7E640D2DF2F_gshared)(__this, ___node0, method);
}
// System.String Facebook.WitAi.Lib.WitResponseNode::op_Implicit(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* WitResponseNode_op_Implicit_mBEC61604C409E23E9D4BEBA57CE513B294B68AB6 (WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___d0, const RuntimeMethod* method) ;
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityData::op_Inequality(Facebook.WitAi.Data.Entities.WitEntityData,System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityData_op_Inequality_mC5EADA586EF7358C1C5C3E530F13FCC8B030B39F (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* ___data0, RuntimeObject* ___value1, const RuntimeMethod* method) ;
// System.Boolean System.String::IsNullOrEmpty(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A (String_t* ___value0, const RuntimeMethod* method) ;
// System.Boolean System.Object::Equals(System.Object,System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Object_Equals_mF52C7AEB4AA9F136C3EA31AE3C1FD200B831B3D1 (RuntimeObject* ___objA0, RuntimeObject* ___objB1, const RuntimeMethod* method) ;
// System.Boolean System.Object::Equals(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Object_Equals_m07105C4585D3FE204F2A80D58523D001DC43F63B (RuntimeObject* __this, RuntimeObject* ___obj0, const RuntimeMethod* method) ;
// System.Int32 System.Object::GetHashCode()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Object_GetHashCode_m372C5A7AB16CAC13307C11C4256D706CE57E090C (RuntimeObject* __this, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Single>::.ctor()
inline void WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893 (WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3* __this, const RuntimeMethod* method)
{
	((  void (*) (WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*, const RuntimeMethod*))WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893_gshared)(__this, method);
}
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<T> Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Single>::FromEntityWitResponseNode(Facebook.WitAi.Lib.WitResponseNode)
inline WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3* WitEntityDataBase_1_FromEntityWitResponseNode_mEBB6417BC069B64576611835830E1CCE7D824FCF (WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method)
{
	return ((  WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3* (*) (WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, const RuntimeMethod*))WitEntityDataBase_1_FromEntityWitResponseNode_mEBB6417BC069B64576611835830E1CCE7D824FCF_gshared)(__this, ___node0, method);
}
// System.Boolean UnityEngine.Mathf::Approximately(System.Single,System.Single)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool Mathf_Approximately_m1C8DD0BB6A2D22A7DCF09AD7F8EE9ABD12D3F620_inline (float ___a0, float ___b1, const RuntimeMethod* method) ;
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Equality(Facebook.WitAi.Data.Entities.WitEntityFloatData,System.Single)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Equality_mDA7E5EB195526CAF4EEC92686EA6DA1FF2131A05 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data0, float ___value1, const RuntimeMethod* method) ;
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Equality(Facebook.WitAi.Data.Entities.WitEntityFloatData,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Equality_mF7C0A4BE6ED6E60663B359F19BD910329487820A (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data0, int32_t ___value1, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Int32>::.ctor()
inline void WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8 (WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07* __this, const RuntimeMethod* method)
{
	((  void (*) (WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07*, const RuntimeMethod*))WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8_gshared)(__this, method);
}
// Facebook.WitAi.Data.Entities.WitEntityDataBase`1<T> Facebook.WitAi.Data.Entities.WitEntityDataBase`1<System.Int32>::FromEntityWitResponseNode(Facebook.WitAi.Lib.WitResponseNode)
inline WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07* WitEntityDataBase_1_FromEntityWitResponseNode_m63B33E2731C95F8A7209705B9BDBACFDA0FC8557 (WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method)
{
	return ((  WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07* (*) (WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07*, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, const RuntimeMethod*))WitEntityDataBase_1_FromEntityWitResponseNode_m63B33E2731C95F8A7209705B9BDBACFDA0FC8557_gshared)(__this, ___node0, method);
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityIntData::op_Equality(Facebook.WitAi.Data.Entities.WitEntityIntData,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityIntData_op_Equality_mD4249AEC5F17CFB0127B7998EC36C5D1E3CA2D15 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* ___data0, int32_t ___value1, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.String>::.ctor()
inline void List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* __this, const RuntimeMethod* method)
{
	((  void (*) (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*, const RuntimeMethod*))List_1__ctor_m7F078BB342729BDF11327FD89D7872265328F690_gshared)(__this, method);
}
// System.Void System.Collections.Generic.List`1<System.String>::AddRange(System.Collections.Generic.IEnumerable`1<T>)
inline void List_1_AddRange_m157DD7AD4D25423F82A21E533BC4686C83770D5E (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* __this, RuntimeObject* ___collection0, const RuntimeMethod* method)
{
	((  void (*) (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*, RuntimeObject*, const RuntimeMethod*))List_1_AddRange_m1F76B300133150E6046C5FED00E88B5DE0A02E17_gshared)(__this, ___collection0, method);
}
// System.Collections.Generic.List`1/Enumerator<T> System.Collections.Generic.List`1<System.String>::GetEnumerator()
inline Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1 List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* __this, const RuntimeMethod* method)
{
	return ((  Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1 (*) (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*, const RuntimeMethod*))List_1_GetEnumerator_mD8294A7FA2BEB1929487127D476F8EC1CDC23BFC_gshared)(__this, method);
}
// System.Void System.Collections.Generic.List`1/Enumerator<System.String>::Dispose()
inline void Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7 (Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1* __this, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1*, const RuntimeMethod*))Enumerator_Dispose_mD9DC3E3C3697830A4823047AB29A77DBBB5ED419_gshared)(__this, method);
}
// T System.Collections.Generic.List`1/Enumerator<System.String>::get_Current()
inline String_t* Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_inline (Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1* __this, const RuntimeMethod* method)
{
	return ((  String_t* (*) (Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1*, const RuntimeMethod*))Enumerator_get_Current_m6330F15D18EE4F547C05DF9BF83C5EB710376027_gshared_inline)(__this, method);
}
// Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::op_Implicit(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* WitResponseNode_op_Implicit_m7392348272E97DB110F9139AC4838B8FB3EF2B88 (String_t* ___s0, const RuntimeMethod* method) ;
// System.Boolean System.Collections.Generic.List`1/Enumerator<System.String>::MoveNext()
inline bool Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED (Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1*, const RuntimeMethod*))Enumerator_MoveNext_mE921CC8F29FBBDE7CC3209A0ED0D921D58D00BCB_gshared)(__this, method);
}
// System.Void Facebook.WitAi.Lib.WitResponseClass::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseClass__ctor_m9CBC789798C208F99B1EE6384D791A39AF77FC79 (WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* __this, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Lib.WitResponseData::.ctor(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseData__ctor_m3A5FEF3343F12DA87251CD2C8F7BDEEABB3DCA68 (WitResponseData_t78CC4B9FA619C3E5C20CC8CB4338164E4AB5F899* __this, String_t* ___aData0, const RuntimeMethod* method) ;
// System.Collections.Generic.List`1<TSource> System.Linq.Enumerable::ToList<System.String>(System.Collections.Generic.IEnumerable`1<TSource>)
inline List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* Enumerable_ToList_TisString_t_mA7BFFF205C0EB2F8CE0436E85FC70A2449EDD7C5 (RuntimeObject* ___source0, const RuntimeMethod* method)
{
	return ((  List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* (*) (RuntimeObject*, const RuntimeMethod*))Enumerable_ToList_TisRuntimeObject_mBDB9895C2D14F2A92043507996018A329BD32A64_gshared)(___source0, method);
}
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntity::.ctor(System.String,System.String[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitDynamicEntity__ctor_mC6FE01FD85FED5466AE52CAF3200BFAF93FA7C04 (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* __this, String_t* ___entity0, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___keywords1, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntities::.ctor(Facebook.WitAi.Data.Entities.WitDynamicEntity[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitDynamicEntities__ctor_m0F22F28030E3D54891FE26BA0654012D37BCFB4E (WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* __this, WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D* ___entity0, const RuntimeMethod* method) ;
// System.Void UnityEngine.MonoBehaviour::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void MonoBehaviour__ctor_m592DB0105CA0BC97AA1C5F4AD27B12D68A3B7C1E (MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71* __this, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Configuration.WitEndpointConfig::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEndpointConfig__ctor_m45851DBF1D527584556B4989BC8429E8CB543C90 (WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0* __this, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityEvent__ctor_m03D3E5121B9A6100351984D0CE3050B909CD3235 (UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* __this, const RuntimeMethod* method) ;
// System.Boolean Facebook.WitAi.Lib.WitResponseNode::op_Equality(Facebook.WitAi.Lib.WitResponseNode,System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseNode_op_Equality_m9F1A77CB5E0EAFEFA74CEDEC43A8DA3C3568033B (WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___a0, RuntimeObject* ___b1, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent::Invoke()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityEvent_Invoke_mFBF80D59B03C30C5FE6A06F897D954ACADE061D2 (UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* __this, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseHandler::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseHandler__ctor_m90BE72DD4323937145932F727817A8EA2216A683 (WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B* __this, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::CheckInsideRange(System.Single)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SimpleIntentHandler_CheckInsideRange_mD6CBC4679B71419827151FEB27E89A03BD30BEE2 (SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1* __this, float ___resultConfidence0, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::CheckOutsideRange(System.Single)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SimpleIntentHandler_CheckOutsideRange_m4729456664D2CCDDB0F2AFB4516DE3F5154DEC87 (SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1* __this, float ___resultConfidence0, const RuntimeMethod* method) ;
// Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.WitResultUtilities::GetFirstIntent(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* WitResultUtilities_GetFirstIntent_mE9748A7275970EABF61BC4920BFB445857066F58 (WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___witResponse0, const RuntimeMethod* method) ;
// System.String Facebook.WitAi.WitResultUtilities::GetFirstEntityValue(Facebook.WitAi.Lib.WitResponseNode,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* WitResultUtilities_GetFirstEntityValue_m66FD91C71028FF980D19497D71F800E535EB85FC (WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___witResponse0, String_t* ___name1, const RuntimeMethod* method) ;
// System.String System.String::Replace(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Replace_mABDB7003A1D0AEDCAE9FF85E3DFFFBA752D2A166 (String_t* __this, String_t* ___oldValue0, String_t* ___newValue1, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent`1<System.String>::Invoke(T0)
inline void UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15 (UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257* __this, String_t* ___arg00, const RuntimeMethod* method)
{
	((  void (*) (UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257*, String_t*, const RuntimeMethod*))UnityEvent_1_Invoke_m6CDC8B0639CE8935E2E13D10B2C8E500968130B6_gshared)(__this, ___arg00, method);
}
// System.Void Facebook.WitAi.CallbackHandlers.StringEntityMatchEvent::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringEntityMatchEvent__ctor_mADFDA95EA9466ACCE74C701C74BC6C01C1BDEA82 (StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7* __this, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent`1<System.String>::.ctor()
inline void UnityEvent_1__ctor_m0F7D790DACD6F3D18E865D01FE4427603C1B0CC6 (UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257* __this, const RuntimeMethod* method)
{
	((  void (*) (UnityEvent_1_tC9859540CF1468306CAB6D758C0A0D95DBCEC257*, const RuntimeMethod*))UnityEvent_1__ctor_m8D77F4F05F69D0E52E8A445322811EEC25987525_gshared)(__this, method);
}
// System.Boolean UnityEngine.Object::op_Implicit(UnityEngine.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Object_op_Implicit_m18E1885C296CC868AC918101523697CFE6413C79 (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___exists0, const RuntimeMethod* method) ;
// T UnityEngine.Object::FindObjectOfType<Facebook.WitAi.VoiceService>()
inline VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* Object_FindObjectOfType_TisVoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99_mFF7470D00931D2219BE16DF57F4FE4992A642B31 (const RuntimeMethod* method)
{
	return ((  VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* (*) (const RuntimeMethod*))Object_FindObjectOfType_TisRuntimeObject_m9990A7304DF02BA1ED160587D1C2F6DAE89BB343_gshared)(method);
}
// System.Type System.Object::GetType()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3 (RuntimeObject* __this, const RuntimeMethod* method) ;
// System.String UnityEngine.Object::get_name()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392 (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* __this, const RuntimeMethod* method) ;
// System.String System.String::Concat(System.String,System.String,System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_mF8B69BE42B5C5ABCAD3C176FBBE3010E0815D65D (String_t* ___str00, String_t* ___str11, String_t* ___str22, String_t* ___str33, const RuntimeMethod* method) ;
// System.Void UnityEngine.Debug::LogError(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Debug_LogError_m059825802BB6AF7EA9693FEBEEB0D85F59A3E38E (RuntimeObject* ___message0, const RuntimeMethod* method) ;
// System.Void UnityEngine.Behaviour::set_enabled(System.Boolean)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Behaviour_set_enabled_mF1DCFE60EB09E0529FE9476CA804A3AA2D72B16A (Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA* __this, bool ___value0, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityAction`1<Facebook.WitAi.Lib.WitResponseNode>::.ctor(System.Object,System.IntPtr)
inline void UnityAction_1__ctor_mF4F621DB3C8F308CF78411309EDFD1334CF9CE75 (UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8* __this, RuntimeObject* ___object0, intptr_t ___method1, const RuntimeMethod* method)
{
	((  void (*) (UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8*, RuntimeObject*, intptr_t, const RuntimeMethod*))UnityAction_1__ctor_m0C2FC6B483B474AE9596A43EBA7FF6E85503A92A_gshared)(__this, ___object0, ___method1, method);
}
// System.Void UnityEngine.Events.UnityEvent`1<Facebook.WitAi.Lib.WitResponseNode>::AddListener(UnityEngine.Events.UnityAction`1<T0>)
inline void UnityEvent_1_AddListener_mAAD4F26F685871E030131375B05060E33216E0C3 (UnityEvent_1_t9B421777BDE1F0A6F551628B06DADBCD02D20366* __this, UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8* ___call0, const RuntimeMethod* method)
{
	((  void (*) (UnityEvent_1_t9B421777BDE1F0A6F551628B06DADBCD02D20366*, UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8*, const RuntimeMethod*))UnityEvent_1_AddListener_m055233246714700E4BDAA62635BC0AA49E8165CC_gshared)(__this, ___call0, method);
}
// System.Void UnityEngine.Events.UnityEvent`1<Facebook.WitAi.Lib.WitResponseNode>::RemoveListener(UnityEngine.Events.UnityAction`1<T0>)
inline void UnityEvent_1_RemoveListener_m696D0B92F65BCDA59D7C1EEA56EF4B017BFE607B (UnityEvent_1_t9B421777BDE1F0A6F551628B06DADBCD02D20366* __this, UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8* ___call0, const RuntimeMethod* method)
{
	((  void (*) (UnityEvent_1_t9B421777BDE1F0A6F551628B06DADBCD02D20366*, UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8*, const RuntimeMethod*))UnityEvent_1_RemoveListener_m904FA6BDD0D33FDF8650EF816FF5C131867E693E_gshared)(__this, ___call0, method);
}
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::IntentMatches(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_IntentMatches_mC627F7AF4545BCC223D06C2674367446F6E98A37 (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) ;
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::ValueMatches(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_ValueMatches_mFFD7A8502BD3EA08F7AA931E417D1C9393481512 (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) ;
// Facebook.WitAi.WitResponseReference Facebook.WitAi.CallbackHandlers.ValuePathMatcher::get_Reference()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* ValuePathMatcher_get_Reference_m2A32CA50F5C03A5EF44B4FFDBC17F8BB562E5787 (ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* __this, const RuntimeMethod* method) ;
// System.String System.Text.RegularExpressions.Regex::Replace(System.String,System.String,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Regex_Replace_m7CFA4979545DFCE842FC4DEFBAFD25F505559492 (Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* __this, String_t* ___input0, String_t* ___replacement1, int32_t ___count2, const RuntimeMethod* method) ;
// System.String System.Int32::ToString()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Int32_ToString_m030E01C24E294D6762FB0B6F37CB541581F55CA5 (int32_t* __this, const RuntimeMethod* method) ;
// System.String System.String::Concat(System.String,System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_m9B13B47FCB3DF61144D9647DDA05F527377251B0 (String_t* ___str00, String_t* ___str11, String_t* ___str22, const RuntimeMethod* method) ;
// System.Boolean System.String::Contains(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_Contains_m6D77B121FADA7CA5F397C0FABB65DA62DF03B6C3 (String_t* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.String>::Add(T)
inline void List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_inline (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* __this, String_t* ___item0, const RuntimeMethod* method)
{
	((  void (*) (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*, String_t*, const RuntimeMethod*))List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline)(__this, ___item0, method);
}
// T[] System.Collections.Generic.List`1<System.String>::ToArray()
inline StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* __this, const RuntimeMethod* method)
{
	return ((  StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* (*) (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*, const RuntimeMethod*))List_1_ToArray_mD7E4F8E7C11C3C67CB5739FCC0A6E86106A6291F_gshared)(__this, method);
}
// System.Void UnityEngine.Events.UnityEvent`1<System.String[]>::Invoke(T0)
inline void UnityEvent_1_Invoke_mDABA5BBDA189937099CB491730CD6AC57D7A5F94 (UnityEvent_1_t477222AA4CE1BE0F15900D352218F0A842BF5EFD* __this, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___arg00, const RuntimeMethod* method)
{
	((  void (*) (UnityEvent_1_t477222AA4CE1BE0F15900D352218F0A842BF5EFD*, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*, const RuntimeMethod*))UnityEvent_1_Invoke_m6CDC8B0639CE8935E2E13D10B2C8E500968130B6_gshared)(__this, ___arg00, method);
}
// System.Text.RegularExpressions.Match System.Text.RegularExpressions.Regex::Match(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F* Regex_Match_m49DD7357B4C9A9BCBCF686DAB3B5598B1BC688D7 (String_t* ___input0, String_t* ___pattern1, const RuntimeMethod* method) ;
// System.Boolean System.Text.RegularExpressions.Group::get_Success()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Group_get_Success_m4E0238EE4B1E7F927E2AF13E2E5901BCA92BE62F (Group_t26371E9136D6F43782C487B63C67C5FC4F472881* __this, const RuntimeMethod* method) ;
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::CompareInt(System.String,Facebook.WitAi.CallbackHandlers.ValuePathMatcher)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_CompareInt_mBFEA1319F29D734BB9902412C9B3BD757E5B5FFB (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, String_t* ___value0, ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* ___matcher1, const RuntimeMethod* method) ;
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::CompareFloat(System.String,Facebook.WitAi.CallbackHandlers.ValuePathMatcher)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_CompareFloat_m39D94664E1B155931817F0C2434F2093F037CF5D (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, String_t* ___value0, ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* ___matcher1, const RuntimeMethod* method) ;
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::CompareDouble(System.String,Facebook.WitAi.CallbackHandlers.ValuePathMatcher)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_CompareDouble_m01E1A016568F0A2A109C5E4556505AD7188ABC85 (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, String_t* ___value0, ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* ___matcher1, const RuntimeMethod* method) ;
// System.Boolean System.Double::TryParse(System.String,System.Double&)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Double_TryParse_m6939FA2B8DCF60C46E0B859746DD9622450E7DD9 (String_t* ___s0, double* ___result1, const RuntimeMethod* method) ;
// System.Double System.Double::Parse(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double Double_Parse_mBED785C952A63E8D714E429A4A704BCC4D92931B (String_t* ___s0, const RuntimeMethod* method) ;
// System.Boolean System.Single::TryParse(System.String,System.Single&)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Single_TryParse_mF23E88B4B12DDC9E82179BB2483A714005BF006F (String_t* ___s0, float* ___result1, const RuntimeMethod* method) ;
// System.Single System.Single::Parse(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float Single_Parse_m349A7F699C77834259812ABE86909825C1F93CF4 (String_t* ___s0, const RuntimeMethod* method) ;
// System.Boolean System.Int32::TryParse(System.String,System.Int32&)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Int32_TryParse_mFC6BFCB86964E2BCA4052155B10983837A695EA4 (String_t* ___s0, int32_t* ___result1, const RuntimeMethod* method) ;
// System.Int32 System.Int32::Parse(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Int32_Parse_m59B9CC9D5E5B6C99C14251E57FB43BE6AB658767 (String_t* ___s0, const RuntimeMethod* method) ;
// System.String System.Single::ToString(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Single_ToString_m3F2C4433B6ADFA5ED8E3F14ED19CD23014E5179D (float* __this, String_t* ___format0, const RuntimeMethod* method) ;
// System.String System.String::Concat(System.String[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_m6B0734B65813C8EA093D78E5C2D16534EB6FE8C0 (StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___values0, const RuntimeMethod* method) ;
// System.Void UnityEngine.Debug::Log(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Debug_Log_m86567BCF22BBE7809747817453CACA0E41E68219 (RuntimeObject* ___message0, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.CallbackHandlers.MultiValueEvent::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void MultiValueEvent__ctor_m24721BEA88A8DC6A7438F16DF9C7F4B141BE0FF0 (MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C* __this, const RuntimeMethod* method) ;
// System.String System.Text.RegularExpressions.Regex::Escape(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Regex_Escape_m2E4E071ABAFAE1BF55932725F28F4194CD56D7DE (String_t* ___str0, const RuntimeMethod* method) ;
// System.Void System.Text.RegularExpressions.Regex::.ctor(System.String,System.Text.RegularExpressions.RegexOptions)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Regex__ctor_mE3996C71B04A4A6845745D01C93B1D27423D0621 (Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* __this, String_t* ___pattern0, int32_t ___options1, const RuntimeMethod* method) ;
// System.Void UnityEngine.Events.UnityEvent`1<System.String[]>::.ctor()
inline void UnityEvent_1__ctor_mA1CAC73F82FF0E8BC4870AEBF137B7F72614957D (UnityEvent_1_t477222AA4CE1BE0F15900D352218F0A842BF5EFD* __this, const RuntimeMethod* method)
{
	((  void (*) (UnityEvent_1_t477222AA4CE1BE0F15900D352218F0A842BF5EFD*, const RuntimeMethod*))UnityEvent_1__ctor_m8D77F4F05F69D0E52E8A445322811EEC25987525_gshared)(__this, method);
}
// System.Void Facebook.WitAi.CallbackHandlers.ValueEvent::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ValueEvent__ctor_mD1D914C07A91A8D88BEEB83833BCDC9818261377 (ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74* __this, const RuntimeMethod* method) ;
// System.Int32 System.String::LastIndexOf(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t String_LastIndexOf_m8923DBD89F2B3E5A34190B038B48F402E0C17E40 (String_t* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.String System.String::Substring(System.Int32,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Substring_mB1D94F47935D22E130FF2C01DBB6A4135FBB76CE (String_t* __this, int32_t ___startIndex0, int32_t ___length1, const RuntimeMethod* method) ;
// System.String System.String::Concat(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_mAF2CE02CC0CB7460753D0A1A91CCF2B1E9804C5D (String_t* ___str00, String_t* ___str11, const RuntimeMethod* method) ;
// Facebook.WitAi.WitResponseReference Facebook.WitAi.WitResultUtilities::GetWitResponseReference(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* WitResultUtilities_GetWitResponseReference_m7E0E209887806033E842A60BEDDECC03FAB77AAD (String_t* ___path0, const RuntimeMethod* method) ;
// Facebook.WitAi.WitResponseReference Facebook.WitAi.Data.WitValue::get_Reference()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* WitValue_get_Reference_m983BF433C074A617B269D6B1C6ACF9D74EE7958E (WitValue_tD53CDFBDC3B12AB1495984258B6ACC38EEE55E32* __this, const RuntimeMethod* method) ;
// System.Boolean System.String::op_Inequality(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_op_Inequality_m0FBE5AC4931D312E5B347BAA603755676E6DA2FE (String_t* ___a0, String_t* ___b1, const RuntimeMethod* method) ;
// System.Text.RegularExpressions.Match System.Text.RegularExpressions.Regex::Match(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F* Regex_Match_m58565ECF23ACCD2CA77D6F10A6A182B03CF0FF84 (Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* __this, String_t* ___input0, const RuntimeMethod* method) ;
// System.String System.Text.RegularExpressions.Capture::get_Value()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Capture_get_Value_m1AB4193C2FC4B0D08AA34FECF10D03876D848BDC (Capture_tE11B735186DAFEE5F7A3BF5A739E9CCCE99DC24A* __this, const RuntimeMethod* method) ;
// System.String System.String::ToLower()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_ToLower_m6191ABA3DC514ED47C10BDA23FD0DDCEAE7ACFBD (String_t* __this, const RuntimeMethod* method) ;
// System.Void Facebook.WitAi.Utilities.StringEvent::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringEvent__ctor_mF3F63CB79A236D91609C9B7EBF293D7E6935006A (StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* __this, const RuntimeMethod* method) ;
// System.Single UnityEngine.Mathf::Max(System.Single,System.Single)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Mathf_Max_mA9DCA91E87D6D27034F56ABA52606A9090406016_inline (float ___a0, float ___b1, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.Object>::AddWithResize(T)
inline void List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4 (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___item0, const RuntimeMethod* method)
{
	((  void (*) (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D*, RuntimeObject*, const RuntimeMethod*))List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4_gshared)(__this, ___item0, method);
}
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass12_0::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CU3Ec__DisplayClass12_0__ctor_m6F9B764F8D21DDB847D9FAF1FF2699340D73F99E (U3CU3Ec__DisplayClass12_0_t74EEAADB5D1467BB6A6581295207DCC9B25C9A7A* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass12_0::<Add>b__0(Facebook.WitAi.Data.Entities.WitDynamicEntity)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool U3CU3Ec__DisplayClass12_0_U3CAddU3Eb__0_m8F655F9490AD0EF34E869F0F56FC5CF4D820456E (U3CU3Ec__DisplayClass12_0_t74EEAADB5D1467BB6A6581295207DCC9B25C9A7A* __this, WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* ___e0, const RuntimeMethod* method) 
{
	{
		// int index = entities.FindIndex(e => e.entity == dynamicEntity.entity);
		WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* L_0 = ___e0;
		NullCheck(L_0);
		String_t* L_1 = L_0->___entity_0;
		WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* L_2 = __this->___dynamicEntity_0;
		NullCheck(L_2);
		String_t* L_3 = L_2->___entity_0;
		bool L_4;
		L_4 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_1, L_3, NULL);
		return L_4;
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
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass14_0::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CU3Ec__DisplayClass14_0__ctor_mD68EB8CF35DBEEDB63A76D2D48212966C1695ECB (U3CU3Ec__DisplayClass14_0_t0E317FA09AA1ADB9E7810FCEBBDCF487BBBA7F75* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass14_0::<AddKeyword>b__0(Facebook.WitAi.Data.Entities.WitDynamicEntity)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool U3CU3Ec__DisplayClass14_0_U3CAddKeywordU3Eb__0_m6DDA6D25E9C2C761669D21ECF1CDB28F9F1ECF44 (U3CU3Ec__DisplayClass14_0_t0E317FA09AA1ADB9E7810FCEBBDCF487BBBA7F75* __this, WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* ___e0, const RuntimeMethod* method) 
{
	{
		// var entity = entities.Find(e => entityName == e.entity);
		String_t* L_0 = __this->___entityName_0;
		WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* L_1 = ___e0;
		NullCheck(L_1);
		String_t* L_2 = L_1->___entity_0;
		bool L_3;
		L_3 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_0, L_2, NULL);
		return L_3;
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
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass15_0::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CU3Ec__DisplayClass15_0__ctor_m8D0E22D4F38C1C78E58C2188BFE7862B882F10EA (U3CU3Ec__DisplayClass15_0_t9278361DC2379675C4FBCB0FBF4950B7B760FEBC* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitDynamicEntities/<>c__DisplayClass15_0::<RemoveKeyword>b__0(Facebook.WitAi.Data.Entities.WitDynamicEntity)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool U3CU3Ec__DisplayClass15_0_U3CRemoveKeywordU3Eb__0_mF3D19B89844D6E8BFA1115CAA60A5BDD5EF1F8DB (U3CU3Ec__DisplayClass15_0_t9278361DC2379675C4FBCB0FBF4950B7B760FEBC* __this, WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* ___e0, const RuntimeMethod* method) 
{
	{
		// int index = entities.FindIndex(e => e.entity == entityName);
		WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* L_0 = ___e0;
		NullCheck(L_0);
		String_t* L_1 = L_0->___entity_0;
		String_t* L_2 = __this->___entityName_0;
		bool L_3;
		L_3 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_1, L_2, NULL);
		return L_3;
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
// Facebook.WitAi.Data.Entities.WitDynamicEntities Facebook.WitAi.Data.Entities.WitDynamicEntitiesData::GetDynamicEntities()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* WitDynamicEntitiesData_GetDynamicEntities_m963886EB4A912A5E18691EC6FAA96BAEC080060A (WitDynamicEntitiesData_t89F1557016C7917FB0136B98FE1DC9C3CDDA2564* __this, const RuntimeMethod* method) 
{
	WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* V_0 = NULL;
	{
		// return entities;
		WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* L_0 = __this->___entities_4;
		V_0 = L_0;
		goto IL_000a;
	}

IL_000a:
	{
		// }
		WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* L_1 = V_0;
		return L_1;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntitiesData::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitDynamicEntitiesData__ctor_mF46974C7974D2D9E0711BF71A55B3186F472CA55 (WitDynamicEntitiesData_t89F1557016C7917FB0136B98FE1DC9C3CDDA2564* __this, const RuntimeMethod* method) 
{
	{
		ScriptableObject__ctor_mD037FDB0B487295EA47F79A4DB1BF1846C9087FF(__this, NULL);
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
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntity::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitDynamicEntity__ctor_m78CC87086AFBC34CA487A411C4ABF4A76AEA6DF1 (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public List<WitEntityKeyword> keywords = new List<WitEntityKeyword>();
		List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* L_0 = (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887*)il2cpp_codegen_object_new(List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D(L_0, List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var);
		__this->___keywords_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___keywords_1), (void*)L_0);
		// public WitDynamicEntity()
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// }
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntity::.ctor(System.String,Facebook.WitAi.Data.Entities.WitEntityKeyword)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitDynamicEntity__ctor_m43379C3B399A1D467F875EB6B22FF3CCF326B8F3 (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* __this, String_t* ___entity0, WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* ___keyword1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public List<WitEntityKeyword> keywords = new List<WitEntityKeyword>();
		List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* L_0 = (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887*)il2cpp_codegen_object_new(List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D(L_0, List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var);
		__this->___keywords_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___keywords_1), (void*)L_0);
		// public WitDynamicEntity(string entity, WitEntityKeyword keyword)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.entity = entity;
		String_t* L_1 = ___entity0;
		__this->___entity_0 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___entity_0), (void*)L_1);
		// this.keywords.Add(keyword);
		List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* L_2 = __this->___keywords_1;
		WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_3 = ___keyword1;
		NullCheck(L_2);
		List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_inline(L_2, L_3, List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_RuntimeMethod_var);
		// }
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntity::.ctor(System.String,System.String[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitDynamicEntity__ctor_mC6FE01FD85FED5466AE52CAF3200BFAF93FA7C04 (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* __this, String_t* ___entity0, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___keywords1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* V_0 = NULL;
	int32_t V_1 = 0;
	String_t* V_2 = NULL;
	{
		// public List<WitEntityKeyword> keywords = new List<WitEntityKeyword>();
		List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* L_0 = (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887*)il2cpp_codegen_object_new(List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D(L_0, List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var);
		__this->___keywords_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___keywords_1), (void*)L_0);
		// public WitDynamicEntity(string entity, params string[] keywords)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.entity = entity;
		String_t* L_1 = ___entity0;
		__this->___entity_0 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___entity_0), (void*)L_1);
		// foreach (var keyword in keywords)
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_2 = ___keywords1;
		V_0 = L_2;
		V_1 = 0;
		goto IL_003d;
	}

IL_0021:
	{
		// foreach (var keyword in keywords)
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_3 = V_0;
		int32_t L_4 = V_1;
		NullCheck(L_3);
		int32_t L_5 = L_4;
		String_t* L_6 = (L_3)->GetAt(static_cast<il2cpp_array_size_t>(L_5));
		V_2 = L_6;
		// this.keywords.Add(new WitEntityKeyword(keyword));
		List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* L_7 = __this->___keywords_1;
		String_t* L_8 = V_2;
		WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_9 = (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5*)il2cpp_codegen_object_new(WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5_il2cpp_TypeInfo_var);
		NullCheck(L_9);
		WitEntityKeyword__ctor_m0F0DC376B974BD93B174E7A4D6AC604DE0BEB8FB(L_9, L_8, NULL);
		NullCheck(L_7);
		List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_inline(L_7, L_9, List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_RuntimeMethod_var);
		int32_t L_10 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_10, 1));
	}

IL_003d:
	{
		// foreach (var keyword in keywords)
		int32_t L_11 = V_1;
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_12 = V_0;
		NullCheck(L_12);
		if ((((int32_t)L_11) < ((int32_t)((int32_t)(((RuntimeArray*)L_12)->max_length)))))
		{
			goto IL_0021;
		}
	}
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitDynamicEntity::.ctor(System.String,System.Collections.Generic.Dictionary`2<System.String,System.Collections.Generic.List`1<System.String>>)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitDynamicEntity__ctor_mAA99CCFEDD61F3E2F8175F707C10E63B5E9A946D (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* __this, String_t* ___entity0, Dictionary_2_t79BA378F246EFA4AD0AFFA017D788423CACA8638* ___keywordsToSynonyms1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Dictionary_2_GetEnumerator_m7B0E05167CA47611244E25AD67EBCEBDF5DB5EDE_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_Dispose_m7DE9243F0F854CE156AFB007FA010C1AB4321A44_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_MoveNext_mA71548765692C241BEFB57E80CBC1563829B1656_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_get_Current_mB41E65E33E936ACBD32C200633257CBD3261A9E2_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&KeyValuePair_2_get_Key_m852B0BE1941AFE5DA3A393534C0020AEAB59D0A6_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&KeyValuePair_2_get_Value_mD55616E290F6591FA95053D1C8D2DDA2E49A058C_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250 V_0;
	memset((&V_0), 0, sizeof(V_0));
	KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		// public List<WitEntityKeyword> keywords = new List<WitEntityKeyword>();
		List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* L_0 = (List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887*)il2cpp_codegen_object_new(List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D(L_0, List_1__ctor_mDCB43822A4DFE2F6B5BE818B82DC23AE063EE56D_RuntimeMethod_var);
		__this->___keywords_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___keywords_1), (void*)L_0);
		// public WitDynamicEntity(string entity, Dictionary<string, List<string>> keywordsToSynonyms)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.entity = entity;
		String_t* L_1 = ___entity0;
		__this->___entity_0 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___entity_0), (void*)L_1);
		// foreach (var synonym in keywordsToSynonyms)
		Dictionary_2_t79BA378F246EFA4AD0AFFA017D788423CACA8638* L_2 = ___keywordsToSynonyms1;
		NullCheck(L_2);
		Enumerator_tF61ACA1FDE72603A35B49225874CAC6CA43B2250 L_3;
		L_3 = Dictionary_2_GetEnumerator_m7B0E05167CA47611244E25AD67EBCEBDF5DB5EDE(L_2, Dictionary_2_GetEnumerator_m7B0E05167CA47611244E25AD67EBCEBDF5DB5EDE_RuntimeMethod_var);
		V_0 = L_3;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0064:
			{// begin finally (depth: 1)
				Enumerator_Dispose_m7DE9243F0F854CE156AFB007FA010C1AB4321A44((&V_0), Enumerator_Dispose_m7DE9243F0F854CE156AFB007FA010C1AB4321A44_RuntimeMethod_var);
				return;
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_0059_1;
			}

IL_0024_1:
			{
				// foreach (var synonym in keywordsToSynonyms)
				KeyValuePair_2_tF2D5A036C624EBE9BEAB8512F34B28AB195C5D89 L_4;
				L_4 = Enumerator_get_Current_mB41E65E33E936ACBD32C200633257CBD3261A9E2_inline((&V_0), Enumerator_get_Current_mB41E65E33E936ACBD32C200633257CBD3261A9E2_RuntimeMethod_var);
				V_1 = L_4;
				// keywords.Add(new WitEntityKeyword()
				// {
				//     keyword = synonym.Key,
				//     synonyms = synonym.Value
				// });
				List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* L_5 = __this->___keywords_1;
				WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_6 = (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5*)il2cpp_codegen_object_new(WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5_il2cpp_TypeInfo_var);
				NullCheck(L_6);
				WitEntityKeyword__ctor_m428EA8D89870BEBE0C2CCFF4041C82EB3171DD6C(L_6, NULL);
				WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_7 = L_6;
				String_t* L_8;
				L_8 = KeyValuePair_2_get_Key_m852B0BE1941AFE5DA3A393534C0020AEAB59D0A6_inline((&V_1), KeyValuePair_2_get_Key_m852B0BE1941AFE5DA3A393534C0020AEAB59D0A6_RuntimeMethod_var);
				NullCheck(L_7);
				L_7->___keyword_0 = L_8;
				Il2CppCodeGenWriteBarrier((void**)(&L_7->___keyword_0), (void*)L_8);
				WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_9 = L_7;
				List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_10;
				L_10 = KeyValuePair_2_get_Value_mD55616E290F6591FA95053D1C8D2DDA2E49A058C_inline((&V_1), KeyValuePair_2_get_Value_mD55616E290F6591FA95053D1C8D2DDA2E49A058C_RuntimeMethod_var);
				NullCheck(L_9);
				L_9->___synonyms_1 = L_10;
				Il2CppCodeGenWriteBarrier((void**)(&L_9->___synonyms_1), (void*)L_10);
				NullCheck(L_5);
				List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_inline(L_5, L_9, List_1_Add_m57806926C3F1B7EEF0B008479ECA5BE9027A179D_RuntimeMethod_var);
			}

IL_0059_1:
			{
				// foreach (var synonym in keywordsToSynonyms)
				bool L_11;
				L_11 = Enumerator_MoveNext_mA71548765692C241BEFB57E80CBC1563829B1656((&V_0), Enumerator_MoveNext_mA71548765692C241BEFB57E80CBC1563829B1656_RuntimeMethod_var);
				if (L_11)
				{
					goto IL_0024_1;
				}
			}
			{
				goto IL_0073;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0073:
	{
		// }
		return;
	}
}
// Facebook.WitAi.Lib.WitResponseArray Facebook.WitAi.Data.Entities.WitDynamicEntity::get_AsJson()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* WitDynamicEntity_get_AsJson_m3E898F87E1494557AC5A76A5DCFC8B1B1F91729A (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_Dispose_m25FAB013E20621ED9156A5D8602E37E6979CCDCB_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_MoveNext_m8D1B5472B8F40695AD00E362DF9B0CCE4F919D7C_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_get_Current_mD56264A4D181F6240745145CE039E2D7750313AF_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_GetEnumerator_m4CCCD59E97F72FE069AAFD946983DDD5D8591FD1_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* V_0 = NULL;
	Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A V_1;
	memset((&V_1), 0, sizeof(V_1));
	WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* V_2 = NULL;
	WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* V_3 = NULL;
	{
		// WitResponseArray synonymArray = new WitResponseArray();
		WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* L_0 = (WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05*)il2cpp_codegen_object_new(WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		WitResponseArray__ctor_m6D22459257B33D9C45FF0EA9619A9AA49124606D(L_0, NULL);
		V_0 = L_0;
		// foreach (var keyword in keywords)
		List_1_tC8924E5DCB1302F87A924D4A2EEBC2434968F887* L_1 = __this->___keywords_1;
		NullCheck(L_1);
		Enumerator_tA5C2AD228C0A9F224D92979908D12B57CEF6E96A L_2;
		L_2 = List_1_GetEnumerator_m4CCCD59E97F72FE069AAFD946983DDD5D8591FD1(L_1, List_1_GetEnumerator_m4CCCD59E97F72FE069AAFD946983DDD5D8591FD1_RuntimeMethod_var);
		V_1 = L_2;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0038:
			{// begin finally (depth: 1)
				Enumerator_Dispose_m25FAB013E20621ED9156A5D8602E37E6979CCDCB((&V_1), Enumerator_Dispose_m25FAB013E20621ED9156A5D8602E37E6979CCDCB_RuntimeMethod_var);
				return;
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_002d_1;
			}

IL_0016_1:
			{
				// foreach (var keyword in keywords)
				WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_3;
				L_3 = Enumerator_get_Current_mD56264A4D181F6240745145CE039E2D7750313AF_inline((&V_1), Enumerator_get_Current_mD56264A4D181F6240745145CE039E2D7750313AF_RuntimeMethod_var);
				V_2 = L_3;
				// synonymArray.Add(keyword.AsJson);
				WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* L_4 = V_0;
				WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_5 = V_2;
				NullCheck(L_5);
				WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* L_6;
				L_6 = WitEntityKeyword_get_AsJson_mF26B952D2770B770116A8D862D1DAA6853653DC6(L_5, NULL);
				NullCheck(L_4);
				VirtualActionInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(12 /* System.Void Facebook.WitAi.Lib.WitResponseNode::Add(Facebook.WitAi.Lib.WitResponseNode) */, L_4, L_6);
			}

IL_002d_1:
			{
				// foreach (var keyword in keywords)
				bool L_7;
				L_7 = Enumerator_MoveNext_m8D1B5472B8F40695AD00E362DF9B0CCE4F919D7C((&V_1), Enumerator_MoveNext_m8D1B5472B8F40695AD00E362DF9B0CCE4F919D7C_RuntimeMethod_var);
				if (L_7)
				{
					goto IL_0016_1;
				}
			}
			{
				goto IL_0047;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0047:
	{
		// return synonymArray;
		WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* L_8 = V_0;
		V_3 = L_8;
		goto IL_004b;
	}

IL_004b:
	{
		// }
		WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* L_9 = V_3;
		return L_9;
	}
}
// Facebook.WitAi.Data.Entities.WitDynamicEntities Facebook.WitAi.Data.Entities.WitDynamicEntity::GetDynamicEntities()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* WitDynamicEntity_GetDynamicEntities_mCA3E4210C970FE269FAC81D59B72DE56FF8EFCCB (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_m4AFBFA0F69A9E9C42FDF4CBBED43E6C26A6EDE28_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_m38543D708664CDA034C6B6D276D1EA261BEE2796_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* V_0 = NULL;
	{
		// return new WitDynamicEntities()
		// {
		//     entities = new List<WitDynamicEntity>
		//     {
		//         this
		//     }
		// };
		WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* L_0 = (WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054*)il2cpp_codegen_object_new(WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		WitDynamicEntities__ctor_mFACF5C5BC88B719671AEC348696B8ABDDE7FD800(L_0, NULL);
		WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* L_1 = L_0;
		List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB* L_2 = (List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB*)il2cpp_codegen_object_new(List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB_il2cpp_TypeInfo_var);
		NullCheck(L_2);
		List_1__ctor_m38543D708664CDA034C6B6D276D1EA261BEE2796(L_2, List_1__ctor_m38543D708664CDA034C6B6D276D1EA261BEE2796_RuntimeMethod_var);
		List_1_t38B7BD41064635B832F9DDAA569AB0626FC926BB* L_3 = L_2;
		NullCheck(L_3);
		List_1_Add_m4AFBFA0F69A9E9C42FDF4CBBED43E6C26A6EDE28_inline(L_3, __this, List_1_Add_m4AFBFA0F69A9E9C42FDF4CBBED43E6C26A6EDE28_RuntimeMethod_var);
		NullCheck(L_1);
		L_1->___entities_0 = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&L_1->___entities_0), (void*)L_3);
		V_0 = L_1;
		goto IL_001c;
	}

IL_001c:
	{
		// }
		WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* L_4 = V_0;
		return L_4;
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
// System.Void Facebook.WitAi.Data.Entities.WitEntity::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntity__ctor_mC46AC661CB8DCA81813BD37A2D41034B09584954 (WitEntity_t94A3BFC668CFCC30CC80133AEF7E9947F691F892* __this, const RuntimeMethod* method) 
{
	{
		WitConfigurationData__ctor_m8D4068A08641205F93EF9642BA9106B0EFD37975(__this, NULL);
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
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// System.Void Facebook.WitAi.Data.Entities.WitEntityData::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityData__ctor_m5998AD1EABFF15C05ADA683D7149E7FC47D0CDE2 (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1__ctor_m954C8C7C12B6AE8DC251B99574E791D559661402_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public WitEntityData() {}
		WitEntityDataBase_1__ctor_m954C8C7C12B6AE8DC251B99574E791D559661402(__this, WitEntityDataBase_1__ctor_m954C8C7C12B6AE8DC251B99574E791D559661402_RuntimeMethod_var);
		// public WitEntityData() {}
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitEntityData::.ctor(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityData__ctor_mADBBBECBD117EB5A0B3D52075F74312E4F8644B1 (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1_FromEntityWitResponseNode_m44BCC95882F476B59D585A3BB4B76287A13C528F_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1__ctor_m954C8C7C12B6AE8DC251B99574E791D559661402_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public WitEntityData(WitResponseNode node)
		WitEntityDataBase_1__ctor_m954C8C7C12B6AE8DC251B99574E791D559661402(__this, WitEntityDataBase_1__ctor_m954C8C7C12B6AE8DC251B99574E791D559661402_RuntimeMethod_var);
		// FromEntityWitResponseNode(node);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___node0;
		WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73* L_1;
		L_1 = WitEntityDataBase_1_FromEntityWitResponseNode_m44BCC95882F476B59D585A3BB4B76287A13C528F(__this, L_0, WitEntityDataBase_1_FromEntityWitResponseNode_m44BCC95882F476B59D585A3BB4B76287A13C528F_RuntimeMethod_var);
		// }
		return;
	}
}
// System.String Facebook.WitAi.Data.Entities.WitEntityData::OnParseValue(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* WitEntityData_OnParseValue_mC190378E8BF7A6C2A1EEB39DCD6F76F72B61EB7A (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	{
		// return node[WitEntity.Fields.VALUE];
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___node0;
		NullCheck(L_0);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_1;
		L_1 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_0, _stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8);
		String_t* L_2;
		L_2 = WitResponseNode_op_Implicit_mBEC61604C409E23E9D4BEBA57CE513B294B68AB6(L_1, NULL);
		V_0 = L_2;
		goto IL_0014;
	}

IL_0014:
	{
		// }
		String_t* L_3 = V_0;
		return L_3;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityData::op_Implicit(Facebook.WitAi.Data.Entities.WitEntityData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityData_op_Implicit_mEA87D541E9AD27DC55D8F5847D6F2342C05F6A14 (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* ___data0, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// public static implicit operator bool(WitEntityData data) => null != data && !string.IsNullOrEmpty(data.value);
		WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* L_0 = ___data0;
		bool L_1;
		L_1 = WitEntityData_op_Inequality_mC5EADA586EF7358C1C5C3E530F13FCC8B030B39F((WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC*)NULL, L_0, NULL);
		if (!L_1)
		{
			goto IL_0019;
		}
	}
	{
		WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* L_2 = ___data0;
		NullCheck(L_2);
		String_t* L_3 = ((WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73*)L_2)->___value_8;
		bool L_4;
		L_4 = String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A(L_3, NULL);
		G_B3_0 = ((((int32_t)L_4) == ((int32_t)0))? 1 : 0);
		goto IL_001a;
	}

IL_0019:
	{
		G_B3_0 = 0;
	}

IL_001a:
	{
		return (bool)G_B3_0;
	}
}
// System.String Facebook.WitAi.Data.Entities.WitEntityData::op_Implicit(Facebook.WitAi.Data.Entities.WitEntityData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* WitEntityData_op_Implicit_m07EC2F851A12F5653924BE29C1C17881768301DA (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* ___data0, const RuntimeMethod* method) 
{
	{
		// public static implicit operator string(WitEntityData data) => data.value;
		WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* L_0 = ___data0;
		NullCheck(L_0);
		String_t* L_1 = ((WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73*)L_0)->___value_8;
		return L_1;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityData::op_Equality(Facebook.WitAi.Data.Entities.WitEntityData,System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityData_op_Equality_m2738EF611A8B6196FE5DC382C37EC960B4899B83 (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* ___data0, RuntimeObject* ___value1, const RuntimeMethod* method) 
{
	String_t* G_B3_0 = NULL;
	{
		// public static bool operator ==(WitEntityData data, object value) => Equals(data?.value, value);
		WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* L_0 = ___data0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		G_B3_0 = ((String_t*)(NULL));
		goto IL_000c;
	}

IL_0006:
	{
		WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* L_1 = ___data0;
		NullCheck(L_1);
		String_t* L_2 = ((WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73*)L_1)->___value_8;
		G_B3_0 = L_2;
	}

IL_000c:
	{
		RuntimeObject* L_3 = ___value1;
		bool L_4;
		L_4 = Object_Equals_mF52C7AEB4AA9F136C3EA31AE3C1FD200B831B3D1(G_B3_0, L_3, NULL);
		return L_4;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityData::op_Inequality(Facebook.WitAi.Data.Entities.WitEntityData,System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityData_op_Inequality_mC5EADA586EF7358C1C5C3E530F13FCC8B030B39F (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* ___data0, RuntimeObject* ___value1, const RuntimeMethod* method) 
{
	String_t* G_B3_0 = NULL;
	{
		// public static bool operator !=(WitEntityData data, object value) => !Equals(data?.value, value);
		WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* L_0 = ___data0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		G_B3_0 = ((String_t*)(NULL));
		goto IL_000c;
	}

IL_0006:
	{
		WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* L_1 = ___data0;
		NullCheck(L_1);
		String_t* L_2 = ((WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73*)L_1)->___value_8;
		G_B3_0 = L_2;
	}

IL_000c:
	{
		RuntimeObject* L_3 = ___value1;
		bool L_4;
		L_4 = Object_Equals_mF52C7AEB4AA9F136C3EA31AE3C1FD200B831B3D1(G_B3_0, L_3, NULL);
		return (bool)((((int32_t)L_4) == ((int32_t)0))? 1 : 0);
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityData::Equals(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityData_Equals_m935C3A0D10C7C7EE3A6D6F9C74945062335EA700 (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* __this, RuntimeObject* ___obj0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&String_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	bool V_1 = false;
	bool V_2 = false;
	{
		// if (obj is string s) return s == value;
		RuntimeObject* L_0 = ___obj0;
		V_0 = ((String_t*)IsInstSealed((RuntimeObject*)L_0, String_t_il2cpp_TypeInfo_var));
		String_t* L_1 = V_0;
		V_1 = (bool)((!(((RuntimeObject*)(String_t*)L_1) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_001f;
		}
	}
	{
		// if (obj is string s) return s == value;
		String_t* L_3 = V_0;
		String_t* L_4 = ((WitEntityDataBase_1_t15096AB26C7BE580FC9D4916EBFB66A4B9CF4C73*)__this)->___value_8;
		bool L_5;
		L_5 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_3, L_4, NULL);
		V_2 = L_5;
		goto IL_0029;
	}

IL_001f:
	{
		// return base.Equals(obj);
		RuntimeObject* L_6 = ___obj0;
		bool L_7;
		L_7 = Object_Equals_m07105C4585D3FE204F2A80D58523D001DC43F63B(__this, L_6, NULL);
		V_2 = L_7;
		goto IL_0029;
	}

IL_0029:
	{
		// }
		bool L_8 = V_2;
		return L_8;
	}
}
// System.Int32 Facebook.WitAi.Data.Entities.WitEntityData::GetHashCode()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t WitEntityData_GetHashCode_m2860E04002015CE351667D5A29BBAAC393210643 (WitEntityData_tDDA8E2C904F3DF1FEC36CE153524F0C0F0E6E9BC* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		// return base.GetHashCode();
		int32_t L_0;
		L_0 = Object_GetHashCode_m372C5A7AB16CAC13307C11C4256D706CE57E090C(__this, NULL);
		V_0 = L_0;
		goto IL_000a;
	}

IL_000a:
	{
		// }
		int32_t L_1 = V_0;
		return L_1;
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
// System.Void Facebook.WitAi.Data.Entities.WitEntityFloatData::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityFloatData__ctor_m0270E5FE62E52476EE4A92BDE6449641FFE6305A (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public WitEntityFloatData() {}
		WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893(__this, WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893_RuntimeMethod_var);
		// public WitEntityFloatData() {}
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitEntityFloatData::.ctor(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityFloatData__ctor_mB4D2ADFE66739B2B36266743E16983049CC32E37 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1_FromEntityWitResponseNode_mEBB6417BC069B64576611835830E1CCE7D824FCF_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public WitEntityFloatData(WitResponseNode node)
		WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893(__this, WitEntityDataBase_1__ctor_m0BDA5D9ABBD4DF5591D8961245233F9652851893_RuntimeMethod_var);
		// FromEntityWitResponseNode(node);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___node0;
		WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3* L_1;
		L_1 = WitEntityDataBase_1_FromEntityWitResponseNode_mEBB6417BC069B64576611835830E1CCE7D824FCF(__this, L_0, WitEntityDataBase_1_FromEntityWitResponseNode_mEBB6417BC069B64576611835830E1CCE7D824FCF_RuntimeMethod_var);
		// }
		return;
	}
}
// System.Single Facebook.WitAi.Data.Entities.WitEntityFloatData::OnParseValue(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float WitEntityFloatData_OnParseValue_mAE207BA914EF6B834C82CF34CB0F64F0E0E39A80 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8);
		s_Il2CppMethodInitialized = true;
	}
	float V_0 = 0.0f;
	{
		// return node[WitEntity.Fields.VALUE].AsFloat;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___node0;
		NullCheck(L_0);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_1;
		L_1 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_0, _stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8);
		NullCheck(L_1);
		float L_2;
		L_2 = VirtualFuncInvoker0< float >::Invoke(20 /* System.Single Facebook.WitAi.Lib.WitResponseNode::get_AsFloat() */, L_1);
		V_0 = L_2;
		goto IL_0014;
	}

IL_0014:
	{
		// }
		float L_3 = V_0;
		return L_3;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Implicit(Facebook.WitAi.Data.Entities.WitEntityFloatData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Implicit_m304E6BD085994AC98DCD71A016E78ED4B87C6AF1 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data0, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// null != data && data.hasData;
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data0;
		if (!L_0)
		{
			goto IL_000b;
		}
	}
	{
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_1 = ___data0;
		NullCheck(L_1);
		bool L_2 = ((WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*)L_1)->___hasData_10;
		G_B3_0 = ((int32_t)(L_2));
		goto IL_000c;
	}

IL_000b:
	{
		G_B3_0 = 0;
	}

IL_000c:
	{
		return (bool)G_B3_0;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::Approximately(System.Single)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_Approximately_m59DC1C8A148025CC49F5D023150D6C53E1ED73A2 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* __this, float ___v0, const RuntimeMethod* method) 
{
	{
		// public bool Approximately(float v) => Mathf.Approximately(value, v);
		float L_0 = ((WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*)__this)->___value_8;
		float L_1 = ___v0;
		bool L_2;
		L_2 = Mathf_Approximately_m1C8DD0BB6A2D22A7DCF09AD7F8EE9ABD12D3F620_inline(L_0, L_1, NULL);
		return L_2;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Equality(Facebook.WitAi.Data.Entities.WitEntityFloatData,System.Single)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Equality_mDA7E5EB195526CAF4EEC92686EA6DA1FF2131A05 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data0, float ___value1, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// public static bool operator ==(WitEntityFloatData data, float value) => data?.value == value;
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		G_B3_0 = 0;
		goto IL_000f;
	}

IL_0006:
	{
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_1 = ___data0;
		NullCheck(L_1);
		float L_2 = ((WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*)L_1)->___value_8;
		float L_3 = ___value1;
		G_B3_0 = ((((float)L_2) == ((float)L_3))? 1 : 0);
	}

IL_000f:
	{
		return (bool)G_B3_0;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Inequality(Facebook.WitAi.Data.Entities.WitEntityFloatData,System.Single)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Inequality_mF19D3632EFB1A24936E64C9EFF68D5335A12A909 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data0, float ___value1, const RuntimeMethod* method) 
{
	{
		// public static bool operator !=(WitEntityFloatData data, float value) => !(data == value);
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data0;
		float L_1 = ___value1;
		bool L_2;
		L_2 = WitEntityFloatData_op_Equality_mDA7E5EB195526CAF4EEC92686EA6DA1FF2131A05(L_0, L_1, NULL);
		return (bool)((((int32_t)L_2) == ((int32_t)0))? 1 : 0);
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Equality(Facebook.WitAi.Data.Entities.WitEntityFloatData,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Equality_mF7C0A4BE6ED6E60663B359F19BD910329487820A (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data0, int32_t ___value1, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// public static bool operator ==(WitEntityFloatData data, int value) => data?.value == value;
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		G_B3_0 = 0;
		goto IL_0010;
	}

IL_0006:
	{
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_1 = ___data0;
		NullCheck(L_1);
		float L_2 = ((WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*)L_1)->___value_8;
		int32_t L_3 = ___value1;
		G_B3_0 = ((((float)L_2) == ((float)((float)L_3)))? 1 : 0);
	}

IL_0010:
	{
		return (bool)G_B3_0;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Inequality(Facebook.WitAi.Data.Entities.WitEntityFloatData,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Inequality_m268A4D68F4AD0A2116C6074877562B8D584E0853 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data0, int32_t ___value1, const RuntimeMethod* method) 
{
	{
		// public static bool operator !=(WitEntityFloatData data, int value) => !(data == value);
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data0;
		int32_t L_1 = ___value1;
		bool L_2;
		L_2 = WitEntityFloatData_op_Equality_mF7C0A4BE6ED6E60663B359F19BD910329487820A(L_0, L_1, NULL);
		return (bool)((((int32_t)L_2) == ((int32_t)0))? 1 : 0);
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Equality(System.Single,Facebook.WitAi.Data.Entities.WitEntityFloatData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Equality_mE99A494390DFBF5B30917182C250E6714136EDDE (float ___value0, WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data1, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// public static bool operator ==(float value, WitEntityFloatData data) => data?.value == value;
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data1;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		G_B3_0 = 0;
		goto IL_000f;
	}

IL_0006:
	{
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_1 = ___data1;
		NullCheck(L_1);
		float L_2 = ((WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*)L_1)->___value_8;
		float L_3 = ___value0;
		G_B3_0 = ((((float)L_2) == ((float)L_3))? 1 : 0);
	}

IL_000f:
	{
		return (bool)G_B3_0;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Inequality(System.Single,Facebook.WitAi.Data.Entities.WitEntityFloatData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Inequality_m274AE8355CC402B2D464546F2939539FF6A051D5 (float ___value0, WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data1, const RuntimeMethod* method) 
{
	{
		// public static bool operator !=(float value, WitEntityFloatData data) => !(data == value);
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data1;
		float L_1 = ___value0;
		bool L_2;
		L_2 = WitEntityFloatData_op_Equality_mDA7E5EB195526CAF4EEC92686EA6DA1FF2131A05(L_0, L_1, NULL);
		return (bool)((((int32_t)L_2) == ((int32_t)0))? 1 : 0);
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Equality(System.Int32,Facebook.WitAi.Data.Entities.WitEntityFloatData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Equality_m0A4B26344FAF95EA1BBD8E3F963BB46BFBC86FD0 (int32_t ___value0, WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data1, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// public static bool operator ==(int value, WitEntityFloatData data) => data?.value == value;
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data1;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		G_B3_0 = 0;
		goto IL_0010;
	}

IL_0006:
	{
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_1 = ___data1;
		NullCheck(L_1);
		float L_2 = ((WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*)L_1)->___value_8;
		int32_t L_3 = ___value0;
		G_B3_0 = ((((float)L_2) == ((float)((float)L_3)))? 1 : 0);
	}

IL_0010:
	{
		return (bool)G_B3_0;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::op_Inequality(System.Int32,Facebook.WitAi.Data.Entities.WitEntityFloatData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_op_Inequality_mEB0B1CE5C6EBE590E4B0AA8DE1B9A921D0B3A4C7 (int32_t ___value0, WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* ___data1, const RuntimeMethod* method) 
{
	{
		// public static bool operator !=(int value, WitEntityFloatData data) => !(data == value);
		WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* L_0 = ___data1;
		int32_t L_1 = ___value0;
		bool L_2;
		L_2 = WitEntityFloatData_op_Equality_mF7C0A4BE6ED6E60663B359F19BD910329487820A(L_0, L_1, NULL);
		return (bool)((((int32_t)L_2) == ((int32_t)0))? 1 : 0);
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityFloatData::Equals(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityFloatData_Equals_mD4D42AE1CC27CAE2E520980E4252818B4F88C9F5 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* __this, RuntimeObject* ___obj0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	float V_0 = 0.0f;
	bool V_1 = false;
	bool V_2 = false;
	int32_t G_B3_0 = 0;
	{
		// if (obj is float f) return f == value;
		RuntimeObject* L_0 = ___obj0;
		if (!((RuntimeObject*)IsInstSealed((RuntimeObject*)L_0, Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_il2cpp_TypeInfo_var)))
		{
			goto IL_0013;
		}
	}
	{
		RuntimeObject* L_1 = ___obj0;
		V_0 = ((*(float*)((float*)(float*)UnBox(L_1, Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_il2cpp_TypeInfo_var))));
		G_B3_0 = 1;
		goto IL_0014;
	}

IL_0013:
	{
		G_B3_0 = 0;
	}

IL_0014:
	{
		V_1 = (bool)G_B3_0;
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_0024;
		}
	}
	{
		// if (obj is float f) return f == value;
		float L_3 = V_0;
		float L_4 = ((WitEntityDataBase_1_tABE913B8DEE0D7741D01250BCF54CB222D019FA3*)__this)->___value_8;
		V_2 = (bool)((((float)L_3) == ((float)L_4))? 1 : 0);
		goto IL_002e;
	}

IL_0024:
	{
		// return base.Equals(obj);
		RuntimeObject* L_5 = ___obj0;
		bool L_6;
		L_6 = Object_Equals_m07105C4585D3FE204F2A80D58523D001DC43F63B(__this, L_5, NULL);
		V_2 = L_6;
		goto IL_002e;
	}

IL_002e:
	{
		// }
		bool L_7 = V_2;
		return L_7;
	}
}
// System.Int32 Facebook.WitAi.Data.Entities.WitEntityFloatData::GetHashCode()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t WitEntityFloatData_GetHashCode_mE2A9EA1F9B2E9393D4FB3050EAF7ED8567BB30E3 (WitEntityFloatData_tC929892407D54A42C285CB034B85DE6E7A23C18B* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		// return base.GetHashCode();
		int32_t L_0;
		L_0 = Object_GetHashCode_m372C5A7AB16CAC13307C11C4256D706CE57E090C(__this, NULL);
		V_0 = L_0;
		goto IL_000a;
	}

IL_000a:
	{
		// }
		int32_t L_1 = V_0;
		return L_1;
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
// System.Void Facebook.WitAi.Data.Entities.WitEntityIntData::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityIntData__ctor_m1727D274EFBC1F612C0538955E0D37E93ED24F72 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public WitEntityIntData() {}
		WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8(__this, WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8_RuntimeMethod_var);
		// public WitEntityIntData() {}
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitEntityIntData::.ctor(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityIntData__ctor_m8093C8F8CBA82F47526000A9045A2CD782CB3110 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1_FromEntityWitResponseNode_m63B33E2731C95F8A7209705B9BDBACFDA0FC8557_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public WitEntityIntData(WitResponseNode node)
		WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8(__this, WitEntityDataBase_1__ctor_m55E914060BE2E600ACB8CC71FC9C80AB2CB992B8_RuntimeMethod_var);
		// FromEntityWitResponseNode(node);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___node0;
		WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07* L_1;
		L_1 = WitEntityDataBase_1_FromEntityWitResponseNode_m63B33E2731C95F8A7209705B9BDBACFDA0FC8557(__this, L_0, WitEntityDataBase_1_FromEntityWitResponseNode_m63B33E2731C95F8A7209705B9BDBACFDA0FC8557_RuntimeMethod_var);
		// }
		return;
	}
}
// System.Int32 Facebook.WitAi.Data.Entities.WitEntityIntData::OnParseValue(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t WitEntityIntData_OnParseValue_m4182B28934A6D17EBF719F684C0073B60D8D0E58 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___node0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;
	{
		// return node[WitEntity.Fields.VALUE].AsInt;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___node0;
		NullCheck(L_0);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_1;
		L_1 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_0, _stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8);
		NullCheck(L_1);
		int32_t L_2;
		L_2 = VirtualFuncInvoker0< int32_t >::Invoke(18 /* System.Int32 Facebook.WitAi.Lib.WitResponseNode::get_AsInt() */, L_1);
		V_0 = L_2;
		goto IL_0014;
	}

IL_0014:
	{
		// }
		int32_t L_3 = V_0;
		return L_3;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityIntData::op_Implicit(Facebook.WitAi.Data.Entities.WitEntityIntData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityIntData_op_Implicit_m83E4CF74F9154A8B544F0A88466CC50686E2FA13 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* ___data0, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// null != data && data.hasData;
		WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* L_0 = ___data0;
		if (!L_0)
		{
			goto IL_000b;
		}
	}
	{
		WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* L_1 = ___data0;
		NullCheck(L_1);
		bool L_2 = ((WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07*)L_1)->___hasData_10;
		G_B3_0 = ((int32_t)(L_2));
		goto IL_000c;
	}

IL_000b:
	{
		G_B3_0 = 0;
	}

IL_000c:
	{
		return (bool)G_B3_0;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityIntData::op_Equality(Facebook.WitAi.Data.Entities.WitEntityIntData,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityIntData_op_Equality_mD4249AEC5F17CFB0127B7998EC36C5D1E3CA2D15 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* ___data0, int32_t ___value1, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// public static bool operator ==(WitEntityIntData data, int value) => data?.value == value;
		WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* L_0 = ___data0;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		G_B3_0 = 0;
		goto IL_000f;
	}

IL_0006:
	{
		WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* L_1 = ___data0;
		NullCheck(L_1);
		int32_t L_2 = ((WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07*)L_1)->___value_8;
		int32_t L_3 = ___value1;
		G_B3_0 = ((((int32_t)L_2) == ((int32_t)L_3))? 1 : 0);
	}

IL_000f:
	{
		return (bool)G_B3_0;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityIntData::op_Inequality(Facebook.WitAi.Data.Entities.WitEntityIntData,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityIntData_op_Inequality_mFB50CF67F6050BE26860B0A57274470985F74BD0 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* ___data0, int32_t ___value1, const RuntimeMethod* method) 
{
	{
		// public static bool operator !=(WitEntityIntData data, int value) => !(data == value);
		WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* L_0 = ___data0;
		int32_t L_1 = ___value1;
		bool L_2;
		L_2 = WitEntityIntData_op_Equality_mD4249AEC5F17CFB0127B7998EC36C5D1E3CA2D15(L_0, L_1, NULL);
		return (bool)((((int32_t)L_2) == ((int32_t)0))? 1 : 0);
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityIntData::op_Equality(System.Int32,Facebook.WitAi.Data.Entities.WitEntityIntData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityIntData_op_Equality_m6822202A1306823C5DCF027D93494EE4EB16D450 (int32_t ___value0, WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* ___data1, const RuntimeMethod* method) 
{
	int32_t G_B3_0 = 0;
	{
		// public static bool operator ==(int value, WitEntityIntData data) => data?.value == value;
		WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* L_0 = ___data1;
		if (L_0)
		{
			goto IL_0006;
		}
	}
	{
		G_B3_0 = 0;
		goto IL_000f;
	}

IL_0006:
	{
		WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* L_1 = ___data1;
		NullCheck(L_1);
		int32_t L_2 = ((WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07*)L_1)->___value_8;
		int32_t L_3 = ___value0;
		G_B3_0 = ((((int32_t)L_2) == ((int32_t)L_3))? 1 : 0);
	}

IL_000f:
	{
		return (bool)G_B3_0;
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityIntData::op_Inequality(System.Int32,Facebook.WitAi.Data.Entities.WitEntityIntData)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityIntData_op_Inequality_m04EA980E93E5A251E9A5E60E791140D207F52FC0 (int32_t ___value0, WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* ___data1, const RuntimeMethod* method) 
{
	{
		// public static bool operator !=(int value, WitEntityIntData data) => !(data == value);
		WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* L_0 = ___data1;
		int32_t L_1 = ___value0;
		bool L_2;
		L_2 = WitEntityIntData_op_Equality_mD4249AEC5F17CFB0127B7998EC36C5D1E3CA2D15(L_0, L_1, NULL);
		return (bool)((((int32_t)L_2) == ((int32_t)0))? 1 : 0);
	}
}
// System.Boolean Facebook.WitAi.Data.Entities.WitEntityIntData::Equals(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitEntityIntData_Equals_m1A97A4D57A4735D912A5C00AC184AB693C0FB744 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* __this, RuntimeObject* ___obj0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;
	bool V_1 = false;
	bool V_2 = false;
	int32_t G_B3_0 = 0;
	{
		// if (obj is int i) return i == value;
		RuntimeObject* L_0 = ___obj0;
		if (!((RuntimeObject*)IsInstSealed((RuntimeObject*)L_0, Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)))
		{
			goto IL_0013;
		}
	}
	{
		RuntimeObject* L_1 = ___obj0;
		V_0 = ((*(int32_t*)((int32_t*)(int32_t*)UnBox(L_1, Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var))));
		G_B3_0 = 1;
		goto IL_0014;
	}

IL_0013:
	{
		G_B3_0 = 0;
	}

IL_0014:
	{
		V_1 = (bool)G_B3_0;
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_0024;
		}
	}
	{
		// if (obj is int i) return i == value;
		int32_t L_3 = V_0;
		int32_t L_4 = ((WitEntityDataBase_1_t6A3C61732A7C2581DF05134AB74E1DFB59E05C07*)__this)->___value_8;
		V_2 = (bool)((((int32_t)L_3) == ((int32_t)L_4))? 1 : 0);
		goto IL_002e;
	}

IL_0024:
	{
		// return base.Equals(obj);
		RuntimeObject* L_5 = ___obj0;
		bool L_6;
		L_6 = Object_Equals_m07105C4585D3FE204F2A80D58523D001DC43F63B(__this, L_5, NULL);
		V_2 = L_6;
		goto IL_002e;
	}

IL_002e:
	{
		// }
		bool L_7 = V_2;
		return L_7;
	}
}
// System.Int32 Facebook.WitAi.Data.Entities.WitEntityIntData::GetHashCode()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t WitEntityIntData_GetHashCode_mAE8EA34E626ACA7996F7AE07C59FD887BC5CCB25 (WitEntityIntData_t80FC1C7E72820445CF6D31576A246C23947A0FE4* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		// return base.GetHashCode();
		int32_t L_0;
		L_0 = Object_GetHashCode_m372C5A7AB16CAC13307C11C4256D706CE57E090C(__this, NULL);
		V_0 = L_0;
		goto IL_000a;
	}

IL_000a:
	{
		// }
		int32_t L_1 = V_0;
		return L_1;
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
// System.Void Facebook.WitAi.Data.Entities.WitEntityKeyword::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityKeyword__ctor_m428EA8D89870BEBE0C2CCFF4041C82EB3171DD6C (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public List<string> synonyms = new List<string>();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_0 = (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*)il2cpp_codegen_object_new(List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E(L_0, List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		__this->___synonyms_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___synonyms_1), (void*)L_0);
		// public WitEntityKeyword() {}
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// public WitEntityKeyword() {}
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitEntityKeyword::.ctor(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityKeyword__ctor_m0F0DC376B974BD93B174E7A4D6AC604DE0BEB8FB (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* __this, String_t* ___keyword0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public List<string> synonyms = new List<string>();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_0 = (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*)il2cpp_codegen_object_new(List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E(L_0, List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		__this->___synonyms_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___synonyms_1), (void*)L_0);
		// public WitEntityKeyword(string keyword)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.keyword = keyword;
		String_t* L_1 = ___keyword0;
		__this->___keyword_0 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___keyword_0), (void*)L_1);
		// }
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitEntityKeyword::.ctor(System.String,System.String[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityKeyword__ctor_mD6293B504C9C078C96E3EF5970A6ACA35009424D (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* __this, String_t* ___keyword0, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___synonyms1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_AddRange_m157DD7AD4D25423F82A21E533BC4686C83770D5E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public List<string> synonyms = new List<string>();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_0 = (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*)il2cpp_codegen_object_new(List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E(L_0, List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		__this->___synonyms_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___synonyms_1), (void*)L_0);
		// public WitEntityKeyword(string keyword, params string[] synonyms)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.keyword = keyword;
		String_t* L_1 = ___keyword0;
		__this->___keyword_0 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___keyword_0), (void*)L_1);
		// this.synonyms.AddRange(synonyms);
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_2 = __this->___synonyms_1;
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_3 = ___synonyms1;
		NullCheck(L_2);
		List_1_AddRange_m157DD7AD4D25423F82A21E533BC4686C83770D5E(L_2, (RuntimeObject*)L_3, List_1_AddRange_m157DD7AD4D25423F82A21E533BC4686C83770D5E_RuntimeMethod_var);
		// }
		return;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitEntityKeyword::.ctor(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityKeyword__ctor_m5C0F5286D37C56D010889F58E4B199CE810F5748 (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* __this, String_t* ___keyword0, RuntimeObject* ___synonyms1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_AddRange_m157DD7AD4D25423F82A21E533BC4686C83770D5E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public List<string> synonyms = new List<string>();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_0 = (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*)il2cpp_codegen_object_new(List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E(L_0, List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		__this->___synonyms_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___synonyms_1), (void*)L_0);
		// public WitEntityKeyword(string keyword, IEnumerable<string> synonyms)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.keyword = keyword;
		String_t* L_1 = ___keyword0;
		__this->___keyword_0 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___keyword_0), (void*)L_1);
		// this.synonyms.AddRange(synonyms);
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_2 = __this->___synonyms_1;
		RuntimeObject* L_3 = ___synonyms1;
		NullCheck(L_2);
		List_1_AddRange_m157DD7AD4D25423F82A21E533BC4686C83770D5E(L_2, L_3, List_1_AddRange_m157DD7AD4D25423F82A21E533BC4686C83770D5E_RuntimeMethod_var);
		// }
		return;
	}
}
// Facebook.WitAi.Lib.WitResponseClass Facebook.WitAi.Data.Entities.WitEntityKeyword::get_AsJson()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* WitEntityKeyword_get_AsJson_mF26B952D2770B770116A8D862D1DAA6853653DC6 (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitResponseClass_t739569309AE400E308EB8DD834327086751C855C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitResponseData_t78CC4B9FA619C3E5C20CC8CB4338164E4AB5F899_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0116812C84CAD7B4AAD7CE2415EECA7B219CCE24);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral3709814F28060CE673AD4F414B802CCF4136017F);
		s_Il2CppMethodInitialized = true;
	}
	WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* V_0 = NULL;
	Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1 V_1;
	memset((&V_1), 0, sizeof(V_1));
	String_t* V_2 = NULL;
	WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* V_3 = NULL;
	{
		// var synonymArray = new WitResponseArray();
		WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* L_0 = (WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05*)il2cpp_codegen_object_new(WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		WitResponseArray__ctor_m6D22459257B33D9C45FF0EA9619A9AA49124606D(L_0, NULL);
		V_0 = L_0;
		// foreach (var synonym in synonyms)
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_1 = __this->___synonyms_1;
		NullCheck(L_1);
		Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1 L_2;
		L_2 = List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D(L_1, List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D_RuntimeMethod_var);
		V_1 = L_2;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0038:
			{// begin finally (depth: 1)
				Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7((&V_1), Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7_RuntimeMethod_var);
				return;
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_002d_1;
			}

IL_0016_1:
			{
				// foreach (var synonym in synonyms)
				String_t* L_3;
				L_3 = Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_inline((&V_1), Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_RuntimeMethod_var);
				V_2 = L_3;
				// synonymArray.Add(synonym);
				WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* L_4 = V_0;
				String_t* L_5 = V_2;
				WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_6;
				L_6 = WitResponseNode_op_Implicit_m7392348272E97DB110F9139AC4838B8FB3EF2B88(L_5, NULL);
				NullCheck(L_4);
				VirtualActionInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(12 /* System.Void Facebook.WitAi.Lib.WitResponseNode::Add(Facebook.WitAi.Lib.WitResponseNode) */, L_4, L_6);
			}

IL_002d_1:
			{
				// foreach (var synonym in synonyms)
				bool L_7;
				L_7 = Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED((&V_1), Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED_RuntimeMethod_var);
				if (L_7)
				{
					goto IL_0016_1;
				}
			}
			{
				goto IL_0047;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0047:
	{
		// return new WitResponseClass
		// {
		//     {"keyword", new WitResponseData(keyword)},
		//     {"synonyms", synonymArray}
		// };
		WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* L_8 = (WitResponseClass_t739569309AE400E308EB8DD834327086751C855C*)il2cpp_codegen_object_new(WitResponseClass_t739569309AE400E308EB8DD834327086751C855C_il2cpp_TypeInfo_var);
		NullCheck(L_8);
		WitResponseClass__ctor_m9CBC789798C208F99B1EE6384D791A39AF77FC79(L_8, NULL);
		WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* L_9 = L_8;
		String_t* L_10 = __this->___keyword_0;
		WitResponseData_t78CC4B9FA619C3E5C20CC8CB4338164E4AB5F899* L_11 = (WitResponseData_t78CC4B9FA619C3E5C20CC8CB4338164E4AB5F899*)il2cpp_codegen_object_new(WitResponseData_t78CC4B9FA619C3E5C20CC8CB4338164E4AB5F899_il2cpp_TypeInfo_var);
		NullCheck(L_11);
		WitResponseData__ctor_m3A5FEF3343F12DA87251CD2C8F7BDEEABB3DCA68(L_11, L_10, NULL);
		NullCheck(L_9);
		VirtualActionInvoker2< String_t*, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(4 /* System.Void Facebook.WitAi.Lib.WitResponseNode::Add(System.String,Facebook.WitAi.Lib.WitResponseNode) */, L_9, _stringLiteral0116812C84CAD7B4AAD7CE2415EECA7B219CCE24, L_11);
		WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* L_12 = L_9;
		WitResponseArray_tB302DB8BD3C7CC1CF016D86CC5606920A2235E05* L_13 = V_0;
		NullCheck(L_12);
		VirtualActionInvoker2< String_t*, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(4 /* System.Void Facebook.WitAi.Lib.WitResponseNode::Add(System.String,Facebook.WitAi.Lib.WitResponseNode) */, L_12, _stringLiteral3709814F28060CE673AD4F414B802CCF4136017F, L_13);
		V_3 = L_12;
		goto IL_0073;
	}

IL_0073:
	{
		// }
		WitResponseClass_t739569309AE400E308EB8DD834327086751C855C* L_14 = V_3;
		return L_14;
	}
}
// Facebook.WitAi.Data.Entities.WitEntityKeyword Facebook.WitAi.Data.Entities.WitEntityKeyword::FromJson(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* WitEntityKeyword_FromJson_m807F411F4436B436AAA0225C0A18D512587BF021 (WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___keywordNode0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerable_ToList_TisString_t_mA7BFFF205C0EB2F8CE0436E85FC70A2449EDD7C5_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0116812C84CAD7B4AAD7CE2415EECA7B219CCE24);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral3709814F28060CE673AD4F414B802CCF4136017F);
		s_Il2CppMethodInitialized = true;
	}
	WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* V_0 = NULL;
	{
		// return new WitEntityKeyword()
		// {
		//     keyword = keywordNode["keyword"],
		//     synonyms = keywordNode["synonyms"].AsStringArray.ToList()
		// };
		WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_0 = (WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5*)il2cpp_codegen_object_new(WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		WitEntityKeyword__ctor_m428EA8D89870BEBE0C2CCFF4041C82EB3171DD6C(L_0, NULL);
		WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_1 = L_0;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_2 = ___keywordNode0;
		NullCheck(L_2);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_3;
		L_3 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_2, _stringLiteral0116812C84CAD7B4AAD7CE2415EECA7B219CCE24);
		String_t* L_4;
		L_4 = WitResponseNode_op_Implicit_mBEC61604C409E23E9D4BEBA57CE513B294B68AB6(L_3, NULL);
		NullCheck(L_1);
		L_1->___keyword_0 = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&L_1->___keyword_0), (void*)L_4);
		WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_5 = L_1;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_6 = ___keywordNode0;
		NullCheck(L_6);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_7;
		L_7 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_6, _stringLiteral3709814F28060CE673AD4F414B802CCF4136017F);
		NullCheck(L_7);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_8;
		L_8 = VirtualFuncInvoker0< StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* >::Invoke(27 /* System.String[] Facebook.WitAi.Lib.WitResponseNode::get_AsStringArray() */, L_7);
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_9;
		L_9 = Enumerable_ToList_TisString_t_mA7BFFF205C0EB2F8CE0436E85FC70A2449EDD7C5((RuntimeObject*)L_8, Enumerable_ToList_TisString_t_mA7BFFF205C0EB2F8CE0436E85FC70A2449EDD7C5_RuntimeMethod_var);
		NullCheck(L_5);
		L_5->___synonyms_1 = L_9;
		Il2CppCodeGenWriteBarrier((void**)(&L_5->___synonyms_1), (void*)L_9);
		V_0 = L_5;
		goto IL_003a;
	}

IL_003a:
	{
		// }
		WitEntityKeyword_t167817E0271B423A77D207B9E66FCEA879613EC5* L_10 = V_0;
		return L_10;
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
// System.Void Facebook.WitAi.Data.Entities.WitEntityRole::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitEntityRole__ctor_m6BD6172B88AA64B467E645BC4F956628563F2CB6 (WitEntityRole_t47AC15DDC8D9F37888EB6747B6ABC142F0589453* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
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
// Facebook.WitAi.Data.Entities.WitDynamicEntities Facebook.WitAi.Data.Entities.WitSimpleDynamicEntity::GetDynamicEntities()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* WitSimpleDynamicEntity_GetDynamicEntities_m196EED15D559780FDB9FBA3545AC60F39F2A023A (WitSimpleDynamicEntity_t7462383F5C63BB5F2B1A153A6E2F4506651692DB* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* V_0 = NULL;
	WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* V_1 = NULL;
	WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* V_2 = NULL;
	{
		// var entity = new WitDynamicEntity(entityName, keywords);
		String_t* L_0 = __this->___entityName_4;
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_1 = __this->___keywords_5;
		WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* L_2 = (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB*)il2cpp_codegen_object_new(WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB_il2cpp_TypeInfo_var);
		NullCheck(L_2);
		WitDynamicEntity__ctor_mC6FE01FD85FED5466AE52CAF3200BFAF93FA7C04(L_2, L_0, L_1, NULL);
		V_0 = L_2;
		// var entities = new WitDynamicEntities(entity);
		WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D* L_3 = (WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D*)(WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D*)SZArrayNew(WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D_il2cpp_TypeInfo_var, (uint32_t)1);
		WitDynamicEntityU5BU5D_tC9CA8D9163A2C005902FA1DA1998AA249D84DB5D* L_4 = L_3;
		WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB* L_5 = V_0;
		NullCheck(L_4);
		ArrayElementTypeCheck (L_4, L_5);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(0), (WitDynamicEntity_t235D64BB173F2AB83FEC57D244015C1E987996BB*)L_5);
		WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* L_6 = (WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054*)il2cpp_codegen_object_new(WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054_il2cpp_TypeInfo_var);
		NullCheck(L_6);
		WitDynamicEntities__ctor_m0F22F28030E3D54891FE26BA0654012D37BCFB4E(L_6, L_4, NULL);
		V_1 = L_6;
		// return entities;
		WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* L_7 = V_1;
		V_2 = L_7;
		goto IL_0027;
	}

IL_0027:
	{
		// }
		WitDynamicEntities_t7357ADD27A2311DC0EE89FE25DA7C404AB7C5054* L_8 = V_2;
		return L_8;
	}
}
// System.Void Facebook.WitAi.Data.Entities.WitSimpleDynamicEntity::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitSimpleDynamicEntity__ctor_m610895979786A570B54372CF02D23E42F10AA8A9 (WitSimpleDynamicEntity_t7462383F5C63BB5F2B1A153A6E2F4506651692DB* __this, const RuntimeMethod* method) 
{
	{
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
// System.Void Facebook.WitAi.Data.Configuration.WitApplication::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitApplication__ctor_m0CF01821BE8F62E6DD45AFFE5CFB686AF675EE69 (WitApplication_t63668A99EE80B05F17BCCDDAA4BF50A84A9A1E3C* __this, const RuntimeMethod* method) 
{
	{
		WitConfigurationData__ctor_m8D4068A08641205F93EF9642BA9106B0EFD37975(__this, NULL);
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
// Facebook.WitAi.Data.Configuration.WitApplication Facebook.WitAi.Data.Configuration.WitConfiguration::get_Application()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitApplication_t63668A99EE80B05F17BCCDDAA4BF50A84A9A1E3C* WitConfiguration_get_Application_mA50FA92E97AF67B91F2AD185CAE88DBC326264D3 (WitConfiguration_t3D1E7D46065A2742877307705778E1CBC33530DD* __this, const RuntimeMethod* method) 
{
	{
		// public WitApplication Application => application;
		WitApplication_t63668A99EE80B05F17BCCDDAA4BF50A84A9A1E3C* L_0 = __this->___application_4;
		return L_0;
	}
}
// System.Void Facebook.WitAi.Data.Configuration.WitConfiguration::OnEnable()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitConfiguration_OnEnable_m8E6E61DEB7BBE676AB1739CC4F0EC07FBF549F8B (WitConfiguration_t3D1E7D46065A2742877307705778E1CBC33530DD* __this, const RuntimeMethod* method) 
{
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.Data.Configuration.WitConfiguration::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitConfiguration__ctor_mBE875CBE608DDA97B19FA93392A6FC7720EC9C0A (WitConfiguration_t3D1E7D46065A2742877307705778E1CBC33530DD* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// [SerializeField] public int timeoutMS = 10000;
		__this->___timeoutMS_7 = ((int32_t)10000);
		// [SerializeField] public WitEndpointConfig endpointConfiguration = new WitEndpointConfig();
		WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0* L_0 = (WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0*)il2cpp_codegen_object_new(WitEndpointConfig_t736B6D17F8267F55239E85D5423598671E17E9B0_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		WitEndpointConfig__ctor_m45851DBF1D527584556B4989BC8429E8CB543C90(L_0, NULL);
		__this->___endpointConfiguration_8 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___endpointConfiguration_8), (void*)L_0);
		ScriptableObject__ctor_mD037FDB0B487295EA47F79A4DB1BF1846C9087FF(__this, NULL);
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
// System.Void Facebook.WitAi.CallbackHandlers.ConfidenceRange::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ConfidenceRange__ctor_m2FF548FCC0369D786CC81130B64B9B993356BC77 (ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public UnityEvent onWithinConfidenceRange = new UnityEvent();
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_0 = (UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977*)il2cpp_codegen_object_new(UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		UnityEvent__ctor_m03D3E5121B9A6100351984D0CE3050B909CD3235(L_0, NULL);
		__this->___onWithinConfidenceRange_2 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___onWithinConfidenceRange_2), (void*)L_0);
		// public UnityEvent onOutsideConfidenceRange = new UnityEvent();
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_1 = (UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977*)il2cpp_codegen_object_new(UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977_il2cpp_TypeInfo_var);
		NullCheck(L_1);
		UnityEvent__ctor_m03D3E5121B9A6100351984D0CE3050B909CD3235(L_1, NULL);
		__this->___onOutsideConfidenceRange_3 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___onOutsideConfidenceRange_3), (void*)L_1);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
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
// System.Void Facebook.WitAi.CallbackHandlers.OutOfScopeUtteranceHandler::OnHandleResponse(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OutOfScopeUtteranceHandler_OnHandleResponse_m1BD01A19CD40A76D0635124133352510C1001DB1 (OutOfScopeUtteranceHandler_t6D1CE6F4B6A31C7CBD4E195FAADF88D3829984D1* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralAD5D95ACB5EE6A85F7764DF4051FD214127B906C);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	bool V_1 = false;
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* G_B5_0 = NULL;
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* G_B4_0 = NULL;
	{
		// if (null == response) return;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___response0;
		bool L_1;
		L_1 = WitResponseNode_op_Equality_m9F1A77CB5E0EAFEFA74CEDEC43A8DA3C3568033B((WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*)NULL, L_0, NULL);
		V_0 = L_1;
		bool L_2 = V_0;
		if (!L_2)
		{
			goto IL_000e;
		}
	}
	{
		// if (null == response) return;
		goto IL_0039;
	}

IL_000e:
	{
		// if (response["intents"].Count == 0)
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_3 = ___response0;
		NullCheck(L_3);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_4;
		L_4 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_3, _stringLiteralAD5D95ACB5EE6A85F7764DF4051FD214127B906C);
		NullCheck(L_4);
		int32_t L_5;
		L_5 = VirtualFuncInvoker0< int32_t >::Invoke(11 /* System.Int32 Facebook.WitAi.Lib.WitResponseNode::get_Count() */, L_4);
		V_1 = (bool)((((int32_t)L_5) == ((int32_t)0))? 1 : 0);
		bool L_6 = V_1;
		if (!L_6)
		{
			goto IL_0039;
		}
	}
	{
		// onOutOfDomain?.Invoke();
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_7 = __this->___onOutOfDomain_5;
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_8 = L_7;
		G_B4_0 = L_8;
		if (L_8)
		{
			G_B5_0 = L_8;
			goto IL_0032;
		}
	}
	{
		goto IL_0038;
	}

IL_0032:
	{
		NullCheck(G_B5_0);
		UnityEvent_Invoke_mFBF80D59B03C30C5FE6A06F897D954ACADE061D2(G_B5_0, NULL);
	}

IL_0038:
	{
	}

IL_0039:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.OutOfScopeUtteranceHandler::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OutOfScopeUtteranceHandler__ctor_mD275FB82065BCC666651C9226F369E73CC2E717B (OutOfScopeUtteranceHandler_t6D1CE6F4B6A31C7CBD4E195FAADF88D3829984D1* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// [SerializeField] private UnityEvent onOutOfDomain = new UnityEvent();
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_0 = (UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977*)il2cpp_codegen_object_new(UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		UnityEvent__ctor_m03D3E5121B9A6100351984D0CE3050B909CD3235(L_0, NULL);
		__this->___onOutOfDomain_5 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___onOutOfDomain_5), (void*)L_0);
		WitResponseHandler__ctor_m90BE72DD4323937145932F727817A8EA2216A683(__this, NULL);
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
// UnityEngine.Events.UnityEvent Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::get_OnIntentTriggered()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* SimpleIntentHandler_get_OnIntentTriggered_mFF092F219C7BE54D19B14AE844243BF393594472 (SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1* __this, const RuntimeMethod* method) 
{
	{
		// public UnityEvent OnIntentTriggered => onIntentTriggered;
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_0 = __this->___onIntentTriggered_7;
		return L_0;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::OnHandleResponse(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SimpleIntentHandler_OnHandleResponse_m35F99ADEDCC082697445FA797A52BCCCF10640BE (SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerable_1_tA414C8405AD2AB4B5AC23942210B16227AC96D03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerator_1_tF1F4F17D3837E1B1E52ACBC529204A77CBA5264E_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral497E0727E6D4098F7DA86E306F0B961AA34D95FF);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralAD5D95ACB5EE6A85F7764DF4051FD214127B906C);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	bool V_1 = false;
	RuntimeObject* V_2 = NULL;
	WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* V_3 = NULL;
	float V_4 = 0.0f;
	bool V_5 = false;
	bool V_6 = false;
	bool V_7 = false;
	RuntimeObject* G_B7_0 = NULL;
	WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* G_B6_0 = NULL;
	WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* G_B5_0 = NULL;
	{
		// if (null == response) return;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___response0;
		bool L_1;
		L_1 = WitResponseNode_op_Equality_m9F1A77CB5E0EAFEFA74CEDEC43A8DA3C3568033B((WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*)NULL, L_0, NULL);
		V_1 = L_1;
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_0011;
		}
	}
	{
		// if (null == response) return;
		goto IL_00e5;
	}

IL_0011:
	{
		// bool matched = false;
		V_0 = (bool)0;
		// foreach (var intentNode in response?["intents"]?.Childs)
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_3 = ___response0;
		if (L_3)
		{
			goto IL_001a;
		}
	}
	{
		G_B7_0 = ((RuntimeObject*)(NULL));
		goto IL_0031;
	}

IL_001a:
	{
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_4 = ___response0;
		NullCheck(L_4);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_5;
		L_5 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_4, _stringLiteralAD5D95ACB5EE6A85F7764DF4051FD214127B906C);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_6 = L_5;
		G_B5_0 = L_6;
		if (L_6)
		{
			G_B6_0 = L_6;
			goto IL_002c;
		}
	}
	{
		G_B7_0 = ((RuntimeObject*)(NULL));
		goto IL_0031;
	}

IL_002c:
	{
		NullCheck(G_B6_0);
		RuntimeObject* L_7;
		L_7 = VirtualFuncInvoker0< RuntimeObject* >::Invoke(16 /* System.Collections.Generic.IEnumerable`1<Facebook.WitAi.Lib.WitResponseNode> Facebook.WitAi.Lib.WitResponseNode::get_Childs() */, G_B6_0);
		G_B7_0 = L_7;
	}

IL_0031:
	{
		NullCheck(G_B7_0);
		RuntimeObject* L_8;
		L_8 = InterfaceFuncInvoker0< RuntimeObject* >::Invoke(0 /* System.Collections.Generic.IEnumerator`1<T> System.Collections.Generic.IEnumerable`1<Facebook.WitAi.Lib.WitResponseNode>::GetEnumerator() */, IEnumerable_1_tA414C8405AD2AB4B5AC23942210B16227AC96D03_il2cpp_TypeInfo_var, G_B7_0);
		V_2 = L_8;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_00b6:
			{// begin finally (depth: 1)
				{
					RuntimeObject* L_9 = V_2;
					if (!L_9)
					{
						goto IL_00c0;
					}
				}
				{
					RuntimeObject* L_10 = V_2;
					NullCheck(L_10);
					InterfaceActionInvoker0::Invoke(0 /* System.Void System.IDisposable::Dispose() */, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_10);
				}

IL_00c0:
				{
					return;
				}
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_00ac_1;
			}

IL_0039_1:
			{
				// foreach (var intentNode in response?["intents"]?.Childs)
				RuntimeObject* L_11 = V_2;
				NullCheck(L_11);
				WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_12;
				L_12 = InterfaceFuncInvoker0< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(0 /* T System.Collections.Generic.IEnumerator`1<Facebook.WitAi.Lib.WitResponseNode>::get_Current() */, IEnumerator_1_tF1F4F17D3837E1B1E52ACBC529204A77CBA5264E_il2cpp_TypeInfo_var, L_11);
				V_3 = L_12;
				// var resultConfidence = intentNode["confidence"].AsFloat;
				WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_13 = V_3;
				NullCheck(L_13);
				WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_14;
				L_14 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_13, _stringLiteral497E0727E6D4098F7DA86E306F0B961AA34D95FF);
				NullCheck(L_14);
				float L_15;
				L_15 = VirtualFuncInvoker0< float >::Invoke(20 /* System.Single Facebook.WitAi.Lib.WitResponseNode::get_AsFloat() */, L_14);
				V_4 = L_15;
				// if (intent == intentNode["name"].Value)
				String_t* L_16 = __this->___intent_5;
				WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_17 = V_3;
				NullCheck(L_17);
				WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_18;
				L_18 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_17, _stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F);
				NullCheck(L_18);
				String_t* L_19;
				L_19 = VirtualFuncInvoker0< String_t* >::Invoke(9 /* System.String Facebook.WitAi.Lib.WitResponseNode::get_Value() */, L_18);
				bool L_20;
				L_20 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_16, L_19, NULL);
				V_5 = L_20;
				bool L_21 = V_5;
				if (!L_21)
				{
					goto IL_00ab_1;
				}
			}
			{
				// matched = true;
				V_0 = (bool)1;
				// if (resultConfidence >= confidence)
				float L_22 = V_4;
				float L_23 = __this->___confidence_6;
				V_6 = (bool)((((int32_t)((!(((float)L_22) >= ((float)L_23)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
				bool L_24 = V_6;
				if (!L_24)
				{
					goto IL_0098_1;
				}
			}
			{
				// onIntentTriggered.Invoke();
				UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_25 = __this->___onIntentTriggered_7;
				NullCheck(L_25);
				UnityEvent_Invoke_mFBF80D59B03C30C5FE6A06F897D954ACADE061D2(L_25, NULL);
			}

IL_0098_1:
			{
				// CheckInsideRange(resultConfidence);
				float L_26 = V_4;
				SimpleIntentHandler_CheckInsideRange_mD6CBC4679B71419827151FEB27E89A03BD30BEE2(__this, L_26, NULL);
				// CheckOutsideRange(resultConfidence);
				float L_27 = V_4;
				SimpleIntentHandler_CheckOutsideRange_m4729456664D2CCDDB0F2AFB4516DE3F5154DEC87(__this, L_27, NULL);
			}

IL_00ab_1:
			{
			}

IL_00ac_1:
			{
				// foreach (var intentNode in response?["intents"]?.Childs)
				RuntimeObject* L_28 = V_2;
				NullCheck(L_28);
				bool L_29;
				L_29 = InterfaceFuncInvoker0< bool >::Invoke(0 /* System.Boolean System.Collections.IEnumerator::MoveNext() */, IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var, L_28);
				if (L_29)
				{
					goto IL_0039_1;
				}
			}
			{
				goto IL_00c1;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_00c1:
	{
		// if(!matched)
		bool L_30 = V_0;
		V_7 = (bool)((((int32_t)L_30) == ((int32_t)0))? 1 : 0);
		bool L_31 = V_7;
		if (!L_31)
		{
			goto IL_00e5;
		}
	}
	{
		// CheckInsideRange(0);
		SimpleIntentHandler_CheckInsideRange_mD6CBC4679B71419827151FEB27E89A03BD30BEE2(__this, (0.0f), NULL);
		// CheckOutsideRange(0);
		SimpleIntentHandler_CheckOutsideRange_m4729456664D2CCDDB0F2AFB4516DE3F5154DEC87(__this, (0.0f), NULL);
	}

IL_00e5:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::CheckOutsideRange(System.Single)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SimpleIntentHandler_CheckOutsideRange_m4729456664D2CCDDB0F2AFB4516DE3F5154DEC87 (SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1* __this, float ___resultConfidence0, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* V_1 = NULL;
	bool V_2 = false;
	bool V_3 = false;
	bool V_4 = false;
	int32_t G_B4_0 = 0;
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* G_B7_0 = NULL;
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* G_B6_0 = NULL;
	int32_t G_B15_0 = 0;
	{
		// for (int i = 0; null != confidenceRanges && i < confidenceRanges.Length; i++)
		V_0 = 0;
		goto IL_0050;
	}

IL_0005:
	{
		// var range = confidenceRanges[i];
		ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC* L_0 = __this->___confidenceRanges_9;
		int32_t L_1 = V_0;
		NullCheck(L_0);
		int32_t L_2 = L_1;
		ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* L_3 = (L_0)->GetAt(static_cast<il2cpp_array_size_t>(L_2));
		V_1 = L_3;
		// if (resultConfidence < range.minConfidence ||
		//     resultConfidence > range.maxConfidence)
		float L_4 = ___resultConfidence0;
		ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* L_5 = V_1;
		NullCheck(L_5);
		float L_6 = L_5->___minConfidence_0;
		if ((((float)L_4) < ((float)L_6)))
		{
			goto IL_0023;
		}
	}
	{
		float L_7 = ___resultConfidence0;
		ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* L_8 = V_1;
		NullCheck(L_8);
		float L_9 = L_8->___maxConfidence_1;
		G_B4_0 = ((((float)L_7) > ((float)L_9))? 1 : 0);
		goto IL_0024;
	}

IL_0023:
	{
		G_B4_0 = 1;
	}

IL_0024:
	{
		V_2 = (bool)G_B4_0;
		bool L_10 = V_2;
		if (!L_10)
		{
			goto IL_004b;
		}
	}
	{
		// range.onOutsideConfidenceRange?.Invoke();
		ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* L_11 = V_1;
		NullCheck(L_11);
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_12 = L_11->___onOutsideConfidenceRange_3;
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_13 = L_12;
		G_B6_0 = L_13;
		if (L_13)
		{
			G_B7_0 = L_13;
			goto IL_0035;
		}
	}
	{
		goto IL_003b;
	}

IL_0035:
	{
		NullCheck(G_B7_0);
		UnityEvent_Invoke_mFBF80D59B03C30C5FE6A06F897D954ACADE061D2(G_B7_0, NULL);
	}

IL_003b:
	{
		// if (!allowConfidenceOverlap) break;
		bool L_14 = __this->___allowConfidenceOverlap_8;
		V_3 = (bool)((((int32_t)L_14) == ((int32_t)0))? 1 : 0);
		bool L_15 = V_3;
		if (!L_15)
		{
			goto IL_004a;
		}
	}
	{
		// if (!allowConfidenceOverlap) break;
		goto IL_006c;
	}

IL_004a:
	{
	}

IL_004b:
	{
		// for (int i = 0; null != confidenceRanges && i < confidenceRanges.Length; i++)
		int32_t L_16 = V_0;
		V_0 = ((int32_t)il2cpp_codegen_add(L_16, 1));
	}

IL_0050:
	{
		// for (int i = 0; null != confidenceRanges && i < confidenceRanges.Length; i++)
		ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC* L_17 = __this->___confidenceRanges_9;
		if (!L_17)
		{
			goto IL_0065;
		}
	}
	{
		int32_t L_18 = V_0;
		ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC* L_19 = __this->___confidenceRanges_9;
		NullCheck(L_19);
		G_B15_0 = ((((int32_t)L_18) < ((int32_t)((int32_t)(((RuntimeArray*)L_19)->max_length))))? 1 : 0);
		goto IL_0066;
	}

IL_0065:
	{
		G_B15_0 = 0;
	}

IL_0066:
	{
		V_4 = (bool)G_B15_0;
		bool L_20 = V_4;
		if (L_20)
		{
			goto IL_0005;
		}
	}

IL_006c:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::CheckInsideRange(System.Single)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SimpleIntentHandler_CheckInsideRange_mD6CBC4679B71419827151FEB27E89A03BD30BEE2 (SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1* __this, float ___resultConfidence0, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* V_1 = NULL;
	bool V_2 = false;
	bool V_3 = false;
	bool V_4 = false;
	int32_t G_B4_0 = 0;
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* G_B7_0 = NULL;
	UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* G_B6_0 = NULL;
	int32_t G_B15_0 = 0;
	{
		// for (int i = 0; null != confidenceRanges && i < confidenceRanges.Length; i++)
		V_0 = 0;
		goto IL_0053;
	}

IL_0005:
	{
		// var range = confidenceRanges[i];
		ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC* L_0 = __this->___confidenceRanges_9;
		int32_t L_1 = V_0;
		NullCheck(L_0);
		int32_t L_2 = L_1;
		ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* L_3 = (L_0)->GetAt(static_cast<il2cpp_array_size_t>(L_2));
		V_1 = L_3;
		// if (resultConfidence >= range.minConfidence &&
		//     resultConfidence <= range.maxConfidence)
		float L_4 = ___resultConfidence0;
		ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* L_5 = V_1;
		NullCheck(L_5);
		float L_6 = L_5->___minConfidence_0;
		if ((!(((float)L_4) >= ((float)L_6))))
		{
			goto IL_0026;
		}
	}
	{
		float L_7 = ___resultConfidence0;
		ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* L_8 = V_1;
		NullCheck(L_8);
		float L_9 = L_8->___maxConfidence_1;
		G_B4_0 = ((((int32_t)((!(((float)L_7) <= ((float)L_9)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_0027;
	}

IL_0026:
	{
		G_B4_0 = 0;
	}

IL_0027:
	{
		V_2 = (bool)G_B4_0;
		bool L_10 = V_2;
		if (!L_10)
		{
			goto IL_004e;
		}
	}
	{
		// range.onWithinConfidenceRange?.Invoke();
		ConfidenceRange_tE18A7D630A039B765127834BF07B91FE073588F9* L_11 = V_1;
		NullCheck(L_11);
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_12 = L_11->___onWithinConfidenceRange_2;
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_13 = L_12;
		G_B6_0 = L_13;
		if (L_13)
		{
			G_B7_0 = L_13;
			goto IL_0038;
		}
	}
	{
		goto IL_003e;
	}

IL_0038:
	{
		NullCheck(G_B7_0);
		UnityEvent_Invoke_mFBF80D59B03C30C5FE6A06F897D954ACADE061D2(G_B7_0, NULL);
	}

IL_003e:
	{
		// if (!allowConfidenceOverlap) break;
		bool L_14 = __this->___allowConfidenceOverlap_8;
		V_3 = (bool)((((int32_t)L_14) == ((int32_t)0))? 1 : 0);
		bool L_15 = V_3;
		if (!L_15)
		{
			goto IL_004d;
		}
	}
	{
		// if (!allowConfidenceOverlap) break;
		goto IL_006f;
	}

IL_004d:
	{
	}

IL_004e:
	{
		// for (int i = 0; null != confidenceRanges && i < confidenceRanges.Length; i++)
		int32_t L_16 = V_0;
		V_0 = ((int32_t)il2cpp_codegen_add(L_16, 1));
	}

IL_0053:
	{
		// for (int i = 0; null != confidenceRanges && i < confidenceRanges.Length; i++)
		ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC* L_17 = __this->___confidenceRanges_9;
		if (!L_17)
		{
			goto IL_0068;
		}
	}
	{
		int32_t L_18 = V_0;
		ConfidenceRangeU5BU5D_t57A91DF169DF248AE1622FCAA9F0BB8B6775C3EC* L_19 = __this->___confidenceRanges_9;
		NullCheck(L_19);
		G_B15_0 = ((((int32_t)L_18) < ((int32_t)((int32_t)(((RuntimeArray*)L_19)->max_length))))? 1 : 0);
		goto IL_0069;
	}

IL_0068:
	{
		G_B15_0 = 0;
	}

IL_0069:
	{
		V_4 = (bool)G_B15_0;
		bool L_20 = V_4;
		if (L_20)
		{
			goto IL_0005;
		}
	}

IL_006f:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.SimpleIntentHandler::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SimpleIntentHandler__ctor_mDC11B9B908AA713B3EE8E41329333BBFB09A6F8A (SimpleIntentHandler_tD80C11D1D51C8A1E12DC9170F02AC6463E5ABDE1* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// [SerializeField] public float confidence = .9f;
		__this->___confidence_6 = (0.899999976f);
		// [SerializeField] private UnityEvent onIntentTriggered = new UnityEvent();
		UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977* L_0 = (UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977*)il2cpp_codegen_object_new(UnityEvent_tDC2C3548799DBC91D1E3F3DE60083A66F4751977_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		UnityEvent__ctor_m03D3E5121B9A6100351984D0CE3050B909CD3235(L_0, NULL);
		__this->___onIntentTriggered_7 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___onIntentTriggered_7), (void*)L_0);
		WitResponseHandler__ctor_m90BE72DD4323937145932F727817A8EA2216A683(__this, NULL);
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
// Facebook.WitAi.CallbackHandlers.StringEntityMatchEvent Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler::get_OnIntentEntityTriggered()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7* SimpleStringEntityHandler_get_OnIntentEntityTriggered_m4069B433E4C1739163CABE3873E12EF70B9407E8 (SimpleStringEntityHandler_tB95E77E27B20E13042B687D717A45B509D5166DC* __this, const RuntimeMethod* method) 
{
	{
		// public StringEntityMatchEvent OnIntentEntityTriggered => onIntentEntityTriggered;
		StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7* L_0 = __this->___onIntentEntityTriggered_9;
		return L_0;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler::OnHandleResponse(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SimpleStringEntityHandler_OnHandleResponse_m86F1DA2CB7EDEBB2CCF62517A939F15EFD45C5E0 (SimpleStringEntityHandler_tB95E77E27B20E13042B687D717A45B509D5166DC* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral057E47B1A5317B280AD5B6CE27C8F11073D42316);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral497E0727E6D4098F7DA86E306F0B961AA34D95FF);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F);
		s_Il2CppMethodInitialized = true;
	}
	WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* V_0 = NULL;
	bool V_1 = false;
	String_t* V_2 = NULL;
	bool V_3 = false;
	int32_t G_B3_0 = 0;
	{
		// var intentNode = WitResultUtilities.GetFirstIntent(response);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___response0;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_1;
		L_1 = WitResultUtilities_GetFirstIntent_mE9748A7275970EABF61BC4920BFB445857066F58(L_0, NULL);
		V_0 = L_1;
		// if (intent == intentNode["name"].Value && intentNode["confidence"].AsFloat > confidence)
		String_t* L_2 = __this->___intent_5;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_3 = V_0;
		NullCheck(L_3);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_4;
		L_4 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_3, _stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F);
		NullCheck(L_4);
		String_t* L_5;
		L_5 = VirtualFuncInvoker0< String_t* >::Invoke(9 /* System.String Facebook.WitAi.Lib.WitResponseNode::get_Value() */, L_4);
		bool L_6;
		L_6 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_2, L_5, NULL);
		if (!L_6)
		{
			goto IL_003f;
		}
	}
	{
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_7 = V_0;
		NullCheck(L_7);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_8;
		L_8 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_7, _stringLiteral497E0727E6D4098F7DA86E306F0B961AA34D95FF);
		NullCheck(L_8);
		float L_9;
		L_9 = VirtualFuncInvoker0< float >::Invoke(20 /* System.Single Facebook.WitAi.Lib.WitResponseNode::get_AsFloat() */, L_8);
		float L_10 = __this->___confidence_7;
		G_B3_0 = ((((float)L_9) > ((float)L_10))? 1 : 0);
		goto IL_0040;
	}

IL_003f:
	{
		G_B3_0 = 0;
	}

IL_0040:
	{
		V_1 = (bool)G_B3_0;
		bool L_11 = V_1;
		if (!L_11)
		{
			goto IL_0095;
		}
	}
	{
		// var entityValue = WitResultUtilities.GetFirstEntityValue(response, entity);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_12 = ___response0;
		String_t* L_13 = __this->___entity_6;
		String_t* L_14;
		L_14 = WitResultUtilities_GetFirstEntityValue_m66FD91C71028FF980D19497D71F800E535EB85FC(L_12, L_13, NULL);
		V_2 = L_14;
		// if (!string.IsNullOrEmpty(format))
		String_t* L_15 = __this->___format_8;
		bool L_16;
		L_16 = String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A(L_15, NULL);
		V_3 = (bool)((((int32_t)L_16) == ((int32_t)0))? 1 : 0);
		bool L_17 = V_3;
		if (!L_17)
		{
			goto IL_0085;
		}
	}
	{
		// onIntentEntityTriggered.Invoke(format.Replace("{value}", entityValue));
		StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7* L_18 = __this->___onIntentEntityTriggered_9;
		String_t* L_19 = __this->___format_8;
		String_t* L_20 = V_2;
		NullCheck(L_19);
		String_t* L_21;
		L_21 = String_Replace_mABDB7003A1D0AEDCAE9FF85E3DFFFBA752D2A166(L_19, _stringLiteral057E47B1A5317B280AD5B6CE27C8F11073D42316, L_20, NULL);
		NullCheck(L_18);
		UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15(L_18, L_21, UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
		goto IL_0094;
	}

IL_0085:
	{
		// onIntentEntityTriggered.Invoke(entityValue);
		StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7* L_22 = __this->___onIntentEntityTriggered_9;
		String_t* L_23 = V_2;
		NullCheck(L_22);
		UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15(L_22, L_23, UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
	}

IL_0094:
	{
	}

IL_0095:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.SimpleStringEntityHandler::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SimpleStringEntityHandler__ctor_m2A8AD6A11EC7D31E0DA81457A263803A9C9D7C91 (SimpleStringEntityHandler_tB95E77E27B20E13042B687D717A45B509D5166DC* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// [Range(0, 1f)] [SerializeField] public float confidence = .9f;
		__this->___confidence_7 = (0.899999976f);
		// [SerializeField] private StringEntityMatchEvent onIntentEntityTriggered
		//     = new StringEntityMatchEvent();
		StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7* L_0 = (StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7*)il2cpp_codegen_object_new(StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringEntityMatchEvent__ctor_mADFDA95EA9466ACCE74C701C74BC6C01C1BDEA82(L_0, NULL);
		__this->___onIntentEntityTriggered_9 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___onIntentEntityTriggered_9), (void*)L_0);
		WitResponseHandler__ctor_m90BE72DD4323937145932F727817A8EA2216A683(__this, NULL);
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
// System.Void Facebook.WitAi.CallbackHandlers.StringEntityMatchEvent::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringEntityMatchEvent__ctor_mADFDA95EA9466ACCE74C701C74BC6C01C1BDEA82 (StringEntityMatchEvent_t1C2593F520E466A66C60F98D08EDBF2978C281D7* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1__ctor_m0F7D790DACD6F3D18E865D01FE4427603C1B0CC6_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		UnityEvent_1__ctor_m0F7D790DACD6F3D18E865D01FE4427603C1B0CC6(__this, UnityEvent_1__ctor_m0F7D790DACD6F3D18E865D01FE4427603C1B0CC6_RuntimeMethod_var);
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
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseHandler::OnValidate()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseHandler_OnValidate_m5993AE8531334DE9DF2F2A2E0BA537941D911AC4 (WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_FindObjectOfType_TisVoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99_mFF7470D00931D2219BE16DF57F4FE4992A642B31_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	{
		// if (!wit) wit = FindObjectOfType<VoiceService>();
		VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* L_0 = __this->___wit_4;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Implicit_m18E1885C296CC868AC918101523697CFE6413C79(L_0, NULL);
		V_0 = (bool)((((int32_t)L_1) == ((int32_t)0))? 1 : 0);
		bool L_2 = V_0;
		if (!L_2)
		{
			goto IL_001e;
		}
	}
	{
		// if (!wit) wit = FindObjectOfType<VoiceService>();
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* L_3;
		L_3 = Object_FindObjectOfType_TisVoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99_mFF7470D00931D2219BE16DF57F4FE4992A642B31(Object_FindObjectOfType_TisVoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99_mFF7470D00931D2219BE16DF57F4FE4992A642B31_RuntimeMethod_var);
		__this->___wit_4 = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___wit_4), (void*)L_3);
	}

IL_001e:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseHandler::OnEnable()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseHandler_OnEnable_m55F24E026839F33CBE76FC8B25FAC9AC3135250B (WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_FindObjectOfType_TisVoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99_mFF7470D00931D2219BE16DF57F4FE4992A642B31_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1_AddListener_mAAD4F26F685871E030131375B05060E33216E0C3_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0CF1C526BD3872B6F7B223B157F80669490DCBCF);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral29565B729B35A8E8D5DDB32B75BCCD98FDE1E555);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	bool V_1 = false;
	{
		// if (!wit) wit = FindObjectOfType<VoiceService>();
		VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* L_0 = __this->___wit_4;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Implicit_m18E1885C296CC868AC918101523697CFE6413C79(L_0, NULL);
		V_0 = (bool)((((int32_t)L_1) == ((int32_t)0))? 1 : 0);
		bool L_2 = V_0;
		if (!L_2)
		{
			goto IL_001e;
		}
	}
	{
		// if (!wit) wit = FindObjectOfType<VoiceService>();
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* L_3;
		L_3 = Object_FindObjectOfType_TisVoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99_mFF7470D00931D2219BE16DF57F4FE4992A642B31(Object_FindObjectOfType_TisVoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99_mFF7470D00931D2219BE16DF57F4FE4992A642B31_RuntimeMethod_var);
		__this->___wit_4 = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___wit_4), (void*)L_3);
	}

IL_001e:
	{
		// if (!wit)
		VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* L_4 = __this->___wit_4;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_5;
		L_5 = Object_op_Implicit_m18E1885C296CC868AC918101523697CFE6413C79(L_4, NULL);
		V_1 = (bool)((((int32_t)L_5) == ((int32_t)0))? 1 : 0);
		bool L_6 = V_1;
		if (!L_6)
		{
			goto IL_0062;
		}
	}
	{
		// Debug.LogError("Wit not found in scene. Disabling " + GetType().Name + " on " +
		//                name);
		Type_t* L_7;
		L_7 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3(__this, NULL);
		NullCheck(L_7);
		String_t* L_8;
		L_8 = VirtualFuncInvoker0< String_t* >::Invoke(7 /* System.String System.Reflection.MemberInfo::get_Name() */, L_7);
		String_t* L_9;
		L_9 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(__this, NULL);
		String_t* L_10;
		L_10 = String_Concat_mF8B69BE42B5C5ABCAD3C176FBBE3010E0815D65D(_stringLiteral29565B729B35A8E8D5DDB32B75BCCD98FDE1E555, L_8, _stringLiteral0CF1C526BD3872B6F7B223B157F80669490DCBCF, L_9, NULL);
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_LogError_m059825802BB6AF7EA9693FEBEEB0D85F59A3E38E(L_10, NULL);
		// enabled = false;
		Behaviour_set_enabled_mF1DCFE60EB09E0529FE9476CA804A3AA2D72B16A(__this, (bool)0, NULL);
		goto IL_0087;
	}

IL_0062:
	{
		// wit.events.OnResponse.AddListener(OnHandleResponse);
		VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* L_11 = __this->___wit_4;
		NullCheck(L_11);
		VoiceEvents_tB9CCDD1C68F3A784600E7932935F705B17B8F1F5* L_12 = L_11->___events_4;
		NullCheck(L_12);
		WitResponseEvent_t5DEE4CFA5D09DD8C6293E1D41EF6A9ACBDD2CFDD* L_13 = L_12->___OnResponse_0;
		UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8* L_14 = (UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8*)il2cpp_codegen_object_new(UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8_il2cpp_TypeInfo_var);
		NullCheck(L_14);
		UnityAction_1__ctor_mF4F621DB3C8F308CF78411309EDFD1334CF9CE75(L_14, __this, (intptr_t)((void*)GetVirtualMethodInfo(__this, 4)), NULL);
		NullCheck(L_13);
		UnityEvent_1_AddListener_mAAD4F26F685871E030131375B05060E33216E0C3(L_13, L_14, UnityEvent_1_AddListener_mAAD4F26F685871E030131375B05060E33216E0C3_RuntimeMethod_var);
	}

IL_0087:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseHandler::OnDisable()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseHandler_OnDisable_m2647C1AD81C5AB8B9BE30A6356302E4851947A29 (WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1_RemoveListener_m696D0B92F65BCDA59D7C1EEA56EF4B017BFE607B_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	{
		// if(wit) wit.events.OnResponse.RemoveListener(OnHandleResponse);
		VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* L_0 = __this->___wit_4;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Implicit_m18E1885C296CC868AC918101523697CFE6413C79(L_0, NULL);
		V_0 = L_1;
		bool L_2 = V_0;
		if (!L_2)
		{
			goto IL_0033;
		}
	}
	{
		// if(wit) wit.events.OnResponse.RemoveListener(OnHandleResponse);
		VoiceService_t632C41F7156160643165A0D2DAF4EEE6E5F73B99* L_3 = __this->___wit_4;
		NullCheck(L_3);
		VoiceEvents_tB9CCDD1C68F3A784600E7932935F705B17B8F1F5* L_4 = L_3->___events_4;
		NullCheck(L_4);
		WitResponseEvent_t5DEE4CFA5D09DD8C6293E1D41EF6A9ACBDD2CFDD* L_5 = L_4->___OnResponse_0;
		UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8* L_6 = (UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8*)il2cpp_codegen_object_new(UnityAction_1_t709797DCB7374983691CE23694848B9DDB4C4CB8_il2cpp_TypeInfo_var);
		NullCheck(L_6);
		UnityAction_1__ctor_mF4F621DB3C8F308CF78411309EDFD1334CF9CE75(L_6, __this, (intptr_t)((void*)GetVirtualMethodInfo(__this, 4)), NULL);
		NullCheck(L_5);
		UnityEvent_1_RemoveListener_m696D0B92F65BCDA59D7C1EEA56EF4B017BFE607B(L_5, L_6, UnityEvent_1_RemoveListener_m696D0B92F65BCDA59D7C1EEA56EF4B017BFE607B_RuntimeMethod_var);
	}

IL_0033:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseHandler::HandleResponse(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseHandler_HandleResponse_m1EF09793D58EB26C8F3264DB5A7D6FB8342E51E1 (WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) 
{
	{
		// public void HandleResponse(WitResponseNode response) => OnHandleResponse(response);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___response0;
		VirtualActionInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(4 /* System.Void Facebook.WitAi.CallbackHandlers.WitResponseHandler::OnHandleResponse(Facebook.WitAi.Lib.WitResponseNode) */, __this, L_0);
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseHandler::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseHandler__ctor_m90BE72DD4323937145932F727817A8EA2216A683 (WitResponseHandler_t282B996CD88AB9A6F40AC153FA83ACF0044CFD5B* __this, const RuntimeMethod* method) 
{
	{
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
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseMatcher::OnHandleResponse(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseMatcher_OnHandleResponse_mDC95CC8B6DA9B6987BEA2D5E98C5DF58635A0AF7 (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1_Invoke_mDABA5BBDA189937099CB491730CD6AC57D7A5F94_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0C3C6829C3CCF8020C6AC45B87963ADC095CD44A);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral4D8D9C94AC5DA5FCED2EC8A64E10E714A2515C30);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralDA39A3EE5E6B4B0D3255BFEF95601890AFD80709);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* V_1 = NULL;
	bool V_2 = false;
	int32_t V_3 = 0;
	FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* V_4 = NULL;
	String_t* V_5 = NULL;
	int32_t V_6 = 0;
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* V_7 = NULL;
	String_t* V_8 = NULL;
	bool V_9 = false;
	bool V_10 = false;
	bool V_11 = false;
	bool V_12 = false;
	bool V_13 = false;
	bool V_14 = false;
	int32_t V_15 = 0;
	String_t* V_16 = NULL;
	bool V_17 = false;
	ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74* G_B15_0 = NULL;
	ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74* G_B14_0 = NULL;
	{
		// if (IntentMatches(response))
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___response0;
		bool L_1;
		L_1 = WitResponseMatcher_IntentMatches_mC627F7AF4545BCC223D06C2674367446F6E98A37(__this, L_0, NULL);
		V_0 = L_1;
		bool L_2 = V_0;
		if (!L_2)
		{
			goto IL_019b;
		}
	}
	{
		// if (ValueMatches(response))
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_3 = ___response0;
		bool L_4;
		L_4 = WitResponseMatcher_ValueMatches_mFFD7A8502BD3EA08F7AA931E417D1C9393481512(__this, L_3, NULL);
		V_2 = L_4;
		bool L_5 = V_2;
		if (!L_5)
		{
			goto IL_0144;
		}
	}
	{
		// for (int j = 0; j < formattedValueEvents.Length; j++)
		V_3 = 0;
		goto IL_012f;
	}

IL_0026:
	{
		// var formatEvent = formattedValueEvents[j];
		FormattedValueEventsU5BU5D_tAFBBA48044157DCAA323FD47280150A31580C188* L_6 = __this->___formattedValueEvents_8;
		int32_t L_7 = V_3;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		V_4 = L_9;
		// var result = formatEvent.format;
		FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* L_10 = V_4;
		NullCheck(L_10);
		String_t* L_11 = L_10->___format_0;
		V_5 = L_11;
		// for (int i = 0; i < valueMatchers.Length; i++)
		V_6 = 0;
		goto IL_00ee;
	}

IL_0042:
	{
		// var reference = valueMatchers[i].Reference;
		ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75* L_12 = __this->___valueMatchers_7;
		int32_t L_13 = V_6;
		NullCheck(L_12);
		int32_t L_14 = L_13;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_15 = (L_12)->GetAt(static_cast<il2cpp_array_size_t>(L_14));
		NullCheck(L_15);
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_16;
		L_16 = ValuePathMatcher_get_Reference_m2A32CA50F5C03A5EF44B4FFDBC17F8BB562E5787(L_15, NULL);
		V_7 = L_16;
		// var value = reference.GetStringValue(response);
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_17 = V_7;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_18 = ___response0;
		NullCheck(L_17);
		String_t* L_19;
		L_19 = VirtualFuncInvoker1< String_t*, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(4 /* System.String Facebook.WitAi.WitResponseReference::GetStringValue(Facebook.WitAi.Lib.WitResponseNode) */, L_17, L_18);
		V_8 = L_19;
		// if (!string.IsNullOrEmpty(formatEvent.format))
		FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* L_20 = V_4;
		NullCheck(L_20);
		String_t* L_21 = L_20->___format_0;
		bool L_22;
		L_22 = String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A(L_21, NULL);
		V_9 = (bool)((((int32_t)L_22) == ((int32_t)0))? 1 : 0);
		bool L_23 = V_9;
		if (!L_23)
		{
			goto IL_00e7;
		}
	}
	{
		// if (!string.IsNullOrEmpty(value))
		String_t* L_24 = V_8;
		bool L_25;
		L_25 = String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A(L_24, NULL);
		V_10 = (bool)((((int32_t)L_25) == ((int32_t)0))? 1 : 0);
		bool L_26 = V_10;
		if (!L_26)
		{
			goto IL_00b9;
		}
	}
	{
		// result = valueRegex.Replace(result, value, 1);
		il2cpp_codegen_runtime_class_init_inline(WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_il2cpp_TypeInfo_var);
		Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* L_27 = ((WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_StaticFields*)il2cpp_codegen_static_fields_for(WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_il2cpp_TypeInfo_var))->___valueRegex_10;
		String_t* L_28 = V_5;
		String_t* L_29 = V_8;
		NullCheck(L_27);
		String_t* L_30;
		L_30 = Regex_Replace_m7CFA4979545DFCE842FC4DEFBAFD25F505559492(L_27, L_28, L_29, 1, NULL);
		V_5 = L_30;
		// result = result.Replace("{" + i + "}", value);
		String_t* L_31 = V_5;
		String_t* L_32;
		L_32 = Int32_ToString_m030E01C24E294D6762FB0B6F37CB541581F55CA5((&V_6), NULL);
		String_t* L_33;
		L_33 = String_Concat_m9B13B47FCB3DF61144D9647DDA05F527377251B0(_stringLiteral0C3C6829C3CCF8020C6AC45B87963ADC095CD44A, L_32, _stringLiteral4D8D9C94AC5DA5FCED2EC8A64E10E714A2515C30, NULL);
		String_t* L_34 = V_8;
		NullCheck(L_31);
		String_t* L_35;
		L_35 = String_Replace_mABDB7003A1D0AEDCAE9FF85E3DFFFBA752D2A166(L_31, L_33, L_34, NULL);
		V_5 = L_35;
		goto IL_00e6;
	}

IL_00b9:
	{
		// else if (result.Contains("{" + i + "}"))
		String_t* L_36 = V_5;
		String_t* L_37;
		L_37 = Int32_ToString_m030E01C24E294D6762FB0B6F37CB541581F55CA5((&V_6), NULL);
		String_t* L_38;
		L_38 = String_Concat_m9B13B47FCB3DF61144D9647DDA05F527377251B0(_stringLiteral0C3C6829C3CCF8020C6AC45B87963ADC095CD44A, L_37, _stringLiteral4D8D9C94AC5DA5FCED2EC8A64E10E714A2515C30, NULL);
		NullCheck(L_36);
		bool L_39;
		L_39 = String_Contains_m6D77B121FADA7CA5F397C0FABB65DA62DF03B6C3(L_36, L_38, NULL);
		V_11 = L_39;
		bool L_40 = V_11;
		if (!L_40)
		{
			goto IL_00e6;
		}
	}
	{
		// result = "";
		V_5 = _stringLiteralDA39A3EE5E6B4B0D3255BFEF95601890AFD80709;
		// break;
		goto IL_0103;
	}

IL_00e6:
	{
	}

IL_00e7:
	{
		// for (int i = 0; i < valueMatchers.Length; i++)
		int32_t L_41 = V_6;
		V_6 = ((int32_t)il2cpp_codegen_add(L_41, 1));
	}

IL_00ee:
	{
		// for (int i = 0; i < valueMatchers.Length; i++)
		int32_t L_42 = V_6;
		ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75* L_43 = __this->___valueMatchers_7;
		NullCheck(L_43);
		V_12 = (bool)((((int32_t)L_42) < ((int32_t)((int32_t)(((RuntimeArray*)L_43)->max_length))))? 1 : 0);
		bool L_44 = V_12;
		if (L_44)
		{
			goto IL_0042;
		}
	}

IL_0103:
	{
		// if (!string.IsNullOrEmpty(result))
		String_t* L_45 = V_5;
		bool L_46;
		L_46 = String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A(L_45, NULL);
		V_13 = (bool)((((int32_t)L_46) == ((int32_t)0))? 1 : 0);
		bool L_47 = V_13;
		if (!L_47)
		{
			goto IL_012a;
		}
	}
	{
		// formatEvent.onFormattedValueEvent?.Invoke(result);
		FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* L_48 = V_4;
		NullCheck(L_48);
		ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74* L_49 = L_48->___onFormattedValueEvent_1;
		ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74* L_50 = L_49;
		G_B14_0 = L_50;
		if (L_50)
		{
			G_B15_0 = L_50;
			goto IL_0121;
		}
	}
	{
		goto IL_0129;
	}

IL_0121:
	{
		String_t* L_51 = V_5;
		NullCheck(G_B15_0);
		UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15(G_B15_0, L_51, UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
	}

IL_0129:
	{
	}

IL_012a:
	{
		// for (int j = 0; j < formattedValueEvents.Length; j++)
		int32_t L_52 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_52, 1));
	}

IL_012f:
	{
		// for (int j = 0; j < formattedValueEvents.Length; j++)
		int32_t L_53 = V_3;
		FormattedValueEventsU5BU5D_tAFBBA48044157DCAA323FD47280150A31580C188* L_54 = __this->___formattedValueEvents_8;
		NullCheck(L_54);
		V_14 = (bool)((((int32_t)L_53) < ((int32_t)((int32_t)(((RuntimeArray*)L_54)->max_length))))? 1 : 0);
		bool L_55 = V_14;
		if (L_55)
		{
			goto IL_0026;
		}
	}
	{
	}

IL_0144:
	{
		// List<string> values = new List<string>();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_56 = (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*)il2cpp_codegen_object_new(List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		NullCheck(L_56);
		List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E(L_56, List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		V_1 = L_56;
		// for (int i = 0; i < valueMatchers.Length; i++)
		V_15 = 0;
		goto IL_0176;
	}

IL_014f:
	{
		// var value = valueMatchers[i].Reference.GetStringValue(response);
		ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75* L_57 = __this->___valueMatchers_7;
		int32_t L_58 = V_15;
		NullCheck(L_57);
		int32_t L_59 = L_58;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_60 = (L_57)->GetAt(static_cast<il2cpp_array_size_t>(L_59));
		NullCheck(L_60);
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_61;
		L_61 = ValuePathMatcher_get_Reference_m2A32CA50F5C03A5EF44B4FFDBC17F8BB562E5787(L_60, NULL);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_62 = ___response0;
		NullCheck(L_61);
		String_t* L_63;
		L_63 = VirtualFuncInvoker1< String_t*, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(4 /* System.String Facebook.WitAi.WitResponseReference::GetStringValue(Facebook.WitAi.Lib.WitResponseNode) */, L_61, L_62);
		V_16 = L_63;
		// values.Add(value);
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_64 = V_1;
		String_t* L_65 = V_16;
		NullCheck(L_64);
		List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_inline(L_64, L_65, List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_RuntimeMethod_var);
		// for (int i = 0; i < valueMatchers.Length; i++)
		int32_t L_66 = V_15;
		V_15 = ((int32_t)il2cpp_codegen_add(L_66, 1));
	}

IL_0176:
	{
		// for (int i = 0; i < valueMatchers.Length; i++)
		int32_t L_67 = V_15;
		ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75* L_68 = __this->___valueMatchers_7;
		NullCheck(L_68);
		V_17 = (bool)((((int32_t)L_67) < ((int32_t)((int32_t)(((RuntimeArray*)L_68)->max_length))))? 1 : 0);
		bool L_69 = V_17;
		if (L_69)
		{
			goto IL_014f;
		}
	}
	{
		// onMultiValueEvent.Invoke(values.ToArray());
		MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C* L_70 = __this->___onMultiValueEvent_9;
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_71 = V_1;
		NullCheck(L_71);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_72;
		L_72 = List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A(L_71, List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A_RuntimeMethod_var);
		NullCheck(L_70);
		UnityEvent_1_Invoke_mDABA5BBDA189937099CB491730CD6AC57D7A5F94(L_70, L_72, UnityEvent_1_Invoke_mDABA5BBDA189937099CB491730CD6AC57D7A5F94_RuntimeMethod_var);
	}

IL_019b:
	{
		// }
		return;
	}
}
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::ValueMatches(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_ValueMatches_mFFD7A8502BD3EA08F7AA931E417D1C9393481512 (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	int32_t V_1 = 0;
	ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* V_2 = NULL;
	String_t* V_3 = NULL;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	bool G_B3_0 = false;
	bool G_B2_0 = false;
	int32_t G_B4_0 = 0;
	bool G_B4_1 = false;
	{
		// bool matches = true;
		V_0 = (bool)1;
		// for (int i = 0; i < valueMatchers.Length && matches; i++)
		V_1 = 0;
		goto IL_00b6;
	}

IL_000a:
	{
		// var matcher = valueMatchers[i];
		ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75* L_0 = __this->___valueMatchers_7;
		int32_t L_1 = V_1;
		NullCheck(L_0);
		int32_t L_2 = L_1;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_3 = (L_0)->GetAt(static_cast<il2cpp_array_size_t>(L_2));
		V_2 = L_3;
		// var value = matcher.Reference.GetStringValue(response);
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_4 = V_2;
		NullCheck(L_4);
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_5;
		L_5 = ValuePathMatcher_get_Reference_m2A32CA50F5C03A5EF44B4FFDBC17F8BB562E5787(L_4, NULL);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_6 = ___response0;
		NullCheck(L_5);
		String_t* L_7;
		L_7 = VirtualFuncInvoker1< String_t*, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* >::Invoke(4 /* System.String Facebook.WitAi.WitResponseReference::GetStringValue(Facebook.WitAi.Lib.WitResponseNode) */, L_5, L_6);
		V_3 = L_7;
		// matches &= !matcher.contentRequired || !string.IsNullOrEmpty(value);
		bool L_8 = V_0;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_9 = V_2;
		NullCheck(L_9);
		bool L_10 = L_9->___contentRequired_2;
		G_B2_0 = L_8;
		if (!L_10)
		{
			G_B3_0 = L_8;
			goto IL_0035;
		}
	}
	{
		String_t* L_11 = V_3;
		bool L_12;
		L_12 = String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A(L_11, NULL);
		G_B4_0 = ((((int32_t)L_12) == ((int32_t)0))? 1 : 0);
		G_B4_1 = G_B2_0;
		goto IL_0036;
	}

IL_0035:
	{
		G_B4_0 = 1;
		G_B4_1 = G_B3_0;
	}

IL_0036:
	{
		V_0 = (bool)((int32_t)((int32_t)G_B4_1&G_B4_0));
		// switch (matcher.matchMethod)
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_13 = V_2;
		NullCheck(L_13);
		int32_t L_14 = L_13->___matchMethod_3;
		V_5 = L_14;
		int32_t L_15 = V_5;
		V_4 = L_15;
		int32_t L_16 = V_4;
		switch (((int32_t)il2cpp_codegen_subtract((int32_t)L_16, 1)))
		{
			case 0:
			{
				goto IL_0079;
			}
			case 1:
			{
				goto IL_0063;
			}
			case 2:
			{
				goto IL_008a;
			}
			case 3:
			{
				goto IL_0097;
			}
			case 4:
			{
				goto IL_00a4;
			}
		}
	}
	{
		goto IL_00b1;
	}

IL_0063:
	{
		// matches &= Regex.Match(value, matcher.matchValue).Success;
		bool L_17 = V_0;
		String_t* L_18 = V_3;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_19 = V_2;
		NullCheck(L_19);
		String_t* L_20 = L_19->___matchValue_5;
		il2cpp_codegen_runtime_class_init_inline(Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_il2cpp_TypeInfo_var);
		Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F* L_21;
		L_21 = Regex_Match_m49DD7357B4C9A9BCBCF686DAB3B5598B1BC688D7(L_18, L_20, NULL);
		NullCheck(L_21);
		bool L_22;
		L_22 = Group_get_Success_m4E0238EE4B1E7F927E2AF13E2E5901BCA92BE62F(L_21, NULL);
		V_0 = (bool)((int32_t)((int32_t)L_17&(int32_t)L_22));
		// break;
		goto IL_00b1;
	}

IL_0079:
	{
		// matches &= value == matcher.matchValue;
		bool L_23 = V_0;
		String_t* L_24 = V_3;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_25 = V_2;
		NullCheck(L_25);
		String_t* L_26 = L_25->___matchValue_5;
		bool L_27;
		L_27 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_24, L_26, NULL);
		V_0 = (bool)((int32_t)((int32_t)L_23&(int32_t)L_27));
		// break;
		goto IL_00b1;
	}

IL_008a:
	{
		// matches &= CompareInt(value, matcher);
		bool L_28 = V_0;
		String_t* L_29 = V_3;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_30 = V_2;
		bool L_31;
		L_31 = WitResponseMatcher_CompareInt_mBFEA1319F29D734BB9902412C9B3BD757E5B5FFB(__this, L_29, L_30, NULL);
		V_0 = (bool)((int32_t)((int32_t)L_28&(int32_t)L_31));
		// break;
		goto IL_00b1;
	}

IL_0097:
	{
		// matches &= CompareFloat(value, matcher);
		bool L_32 = V_0;
		String_t* L_33 = V_3;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_34 = V_2;
		bool L_35;
		L_35 = WitResponseMatcher_CompareFloat_m39D94664E1B155931817F0C2434F2093F037CF5D(__this, L_33, L_34, NULL);
		V_0 = (bool)((int32_t)((int32_t)L_32&(int32_t)L_35));
		// break;
		goto IL_00b1;
	}

IL_00a4:
	{
		// matches &= CompareDouble(value, matcher);
		bool L_36 = V_0;
		String_t* L_37 = V_3;
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_38 = V_2;
		bool L_39;
		L_39 = WitResponseMatcher_CompareDouble_m01E1A016568F0A2A109C5E4556505AD7188ABC85(__this, L_37, L_38, NULL);
		V_0 = (bool)((int32_t)((int32_t)L_36&(int32_t)L_39));
		// break;
		goto IL_00b1;
	}

IL_00b1:
	{
		// for (int i = 0; i < valueMatchers.Length && matches; i++)
		int32_t L_40 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_40, 1));
	}

IL_00b6:
	{
		// for (int i = 0; i < valueMatchers.Length && matches; i++)
		int32_t L_41 = V_1;
		ValuePathMatcherU5BU5D_t61A91ED1635E83400DCE1862E79AC70C514D3B75* L_42 = __this->___valueMatchers_7;
		NullCheck(L_42);
		bool L_43 = V_0;
		V_6 = (bool)((int32_t)(((((int32_t)L_41) < ((int32_t)((int32_t)(((RuntimeArray*)L_42)->max_length))))? 1 : 0)&(int32_t)L_43));
		bool L_44 = V_6;
		if (L_44)
		{
			goto IL_000a;
		}
	}
	{
		// return matches;
		bool L_45 = V_0;
		V_7 = L_45;
		goto IL_00d1;
	}

IL_00d1:
	{
		// }
		bool L_46 = V_7;
		return L_46;
	}
}
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::CompareDouble(System.String,Facebook.WitAi.CallbackHandlers.ValuePathMatcher)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_CompareDouble_m01E1A016568F0A2A109C5E4556505AD7188ABC85 (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, String_t* ___value0, ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* ___matcher1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	double V_0 = 0.0;
	double V_1 = 0.0;
	bool V_2 = false;
	bool V_3 = false;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	{
		// if (!double.TryParse(value, out double dValue)) return false;
		String_t* L_0 = ___value0;
		bool L_1;
		L_1 = Double_TryParse_m6939FA2B8DCF60C46E0B859746DD9622450E7DD9(L_0, (&V_0), NULL);
		V_2 = (bool)((((int32_t)L_1) == ((int32_t)0))? 1 : 0);
		bool L_2 = V_2;
		if (!L_2)
		{
			goto IL_0017;
		}
	}
	{
		// if (!double.TryParse(value, out double dValue)) return false;
		V_3 = (bool)0;
		goto IL_009c;
	}

IL_0017:
	{
		// double dMatchValue = double.Parse(matcher.matchValue);
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_3 = ___matcher1;
		NullCheck(L_3);
		String_t* L_4 = L_3->___matchValue_5;
		double L_5;
		L_5 = Double_Parse_mBED785C952A63E8D714E429A4A704BCC4D92931B(L_4, NULL);
		V_1 = L_5;
		// switch (matcher.comparisonMethod)
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_6 = ___matcher1;
		NullCheck(L_6);
		int32_t L_7 = L_6->___comparisonMethod_4;
		V_5 = L_7;
		int32_t L_8 = V_5;
		V_4 = L_8;
		int32_t L_9 = V_4;
		switch (L_9)
		{
			case 0:
			{
				goto IL_0050;
			}
			case 1:
			{
				goto IL_0063;
			}
			case 2:
			{
				goto IL_0076;
			}
			case 3:
			{
				goto IL_0084;
			}
			case 4:
			{
				goto IL_007d;
			}
			case 5:
			{
				goto IL_008e;
			}
		}
	}
	{
		goto IL_0098;
	}

IL_0050:
	{
		// return Math.Abs(dValue - dMatchValue) < matcher.floatingPointComparisonTolerance;
		double L_10 = V_0;
		double L_11 = V_1;
		il2cpp_codegen_runtime_class_init_inline(Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var);
		double L_12;
		L_12 = fabs(((double)il2cpp_codegen_subtract(L_10, L_11)));
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_13 = ___matcher1;
		NullCheck(L_13);
		double L_14 = L_13->___floatingPointComparisonTolerance_6;
		V_3 = (bool)((((double)L_12) < ((double)L_14))? 1 : 0);
		goto IL_009c;
	}

IL_0063:
	{
		// return Math.Abs(dValue - dMatchValue) > matcher.floatingPointComparisonTolerance;
		double L_15 = V_0;
		double L_16 = V_1;
		il2cpp_codegen_runtime_class_init_inline(Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var);
		double L_17;
		L_17 = fabs(((double)il2cpp_codegen_subtract(L_15, L_16)));
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_18 = ___matcher1;
		NullCheck(L_18);
		double L_19 = L_18->___floatingPointComparisonTolerance_6;
		V_3 = (bool)((((double)L_17) > ((double)L_19))? 1 : 0);
		goto IL_009c;
	}

IL_0076:
	{
		// return dValue > dMatchValue;
		double L_20 = V_0;
		double L_21 = V_1;
		V_3 = (bool)((((double)L_20) > ((double)L_21))? 1 : 0);
		goto IL_009c;
	}

IL_007d:
	{
		// return dValue < dMatchValue;
		double L_22 = V_0;
		double L_23 = V_1;
		V_3 = (bool)((((double)L_22) < ((double)L_23))? 1 : 0);
		goto IL_009c;
	}

IL_0084:
	{
		// return dValue >= dMatchValue;
		double L_24 = V_0;
		double L_25 = V_1;
		V_3 = (bool)((((int32_t)((!(((double)L_24) >= ((double)L_25)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_009c;
	}

IL_008e:
	{
		// return dValue <= dMatchValue;
		double L_26 = V_0;
		double L_27 = V_1;
		V_3 = (bool)((((int32_t)((!(((double)L_26) <= ((double)L_27)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_009c;
	}

IL_0098:
	{
		// return false;
		V_3 = (bool)0;
		goto IL_009c;
	}

IL_009c:
	{
		// }
		bool L_28 = V_3;
		return L_28;
	}
}
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::CompareFloat(System.String,Facebook.WitAi.CallbackHandlers.ValuePathMatcher)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_CompareFloat_m39D94664E1B155931817F0C2434F2093F037CF5D (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, String_t* ___value0, ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* ___matcher1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	float V_0 = 0.0f;
	float V_1 = 0.0f;
	bool V_2 = false;
	bool V_3 = false;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	{
		// if (!float.TryParse(value, out float dValue)) return false;
		String_t* L_0 = ___value0;
		bool L_1;
		L_1 = Single_TryParse_mF23E88B4B12DDC9E82179BB2483A714005BF006F(L_0, (&V_0), NULL);
		V_2 = (bool)((((int32_t)L_1) == ((int32_t)0))? 1 : 0);
		bool L_2 = V_2;
		if (!L_2)
		{
			goto IL_0017;
		}
	}
	{
		// if (!float.TryParse(value, out float dValue)) return false;
		V_3 = (bool)0;
		goto IL_009e;
	}

IL_0017:
	{
		// float dMatchValue = float.Parse(matcher.matchValue);
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_3 = ___matcher1;
		NullCheck(L_3);
		String_t* L_4 = L_3->___matchValue_5;
		float L_5;
		L_5 = Single_Parse_m349A7F699C77834259812ABE86909825C1F93CF4(L_4, NULL);
		V_1 = L_5;
		// switch (matcher.comparisonMethod)
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_6 = ___matcher1;
		NullCheck(L_6);
		int32_t L_7 = L_6->___comparisonMethod_4;
		V_5 = L_7;
		int32_t L_8 = V_5;
		V_4 = L_8;
		int32_t L_9 = V_4;
		switch (L_9)
		{
			case 0:
			{
				goto IL_0050;
			}
			case 1:
			{
				goto IL_0064;
			}
			case 2:
			{
				goto IL_0078;
			}
			case 3:
			{
				goto IL_0086;
			}
			case 4:
			{
				goto IL_007f;
			}
			case 5:
			{
				goto IL_0090;
			}
		}
	}
	{
		goto IL_009a;
	}

IL_0050:
	{
		// return Math.Abs(dValue - dMatchValue) <
		//        matcher.floatingPointComparisonTolerance;
		float L_10 = V_0;
		float L_11 = V_1;
		il2cpp_codegen_runtime_class_init_inline(Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var);
		float L_12;
		L_12 = fabsf(((float)il2cpp_codegen_subtract(L_10, L_11)));
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_13 = ___matcher1;
		NullCheck(L_13);
		double L_14 = L_13->___floatingPointComparisonTolerance_6;
		V_3 = (bool)((((double)((double)L_12)) < ((double)L_14))? 1 : 0);
		goto IL_009e;
	}

IL_0064:
	{
		// return Math.Abs(dValue - dMatchValue) >
		//        matcher.floatingPointComparisonTolerance;
		float L_15 = V_0;
		float L_16 = V_1;
		il2cpp_codegen_runtime_class_init_inline(Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var);
		float L_17;
		L_17 = fabsf(((float)il2cpp_codegen_subtract(L_15, L_16)));
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_18 = ___matcher1;
		NullCheck(L_18);
		double L_19 = L_18->___floatingPointComparisonTolerance_6;
		V_3 = (bool)((((double)((double)L_17)) > ((double)L_19))? 1 : 0);
		goto IL_009e;
	}

IL_0078:
	{
		// return dValue > dMatchValue;
		float L_20 = V_0;
		float L_21 = V_1;
		V_3 = (bool)((((float)L_20) > ((float)L_21))? 1 : 0);
		goto IL_009e;
	}

IL_007f:
	{
		// return dValue < dMatchValue;
		float L_22 = V_0;
		float L_23 = V_1;
		V_3 = (bool)((((float)L_22) < ((float)L_23))? 1 : 0);
		goto IL_009e;
	}

IL_0086:
	{
		// return dValue >= dMatchValue;
		float L_24 = V_0;
		float L_25 = V_1;
		V_3 = (bool)((((int32_t)((!(((float)L_24) >= ((float)L_25)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_009e;
	}

IL_0090:
	{
		// return dValue <= dMatchValue;
		float L_26 = V_0;
		float L_27 = V_1;
		V_3 = (bool)((((int32_t)((!(((float)L_26) <= ((float)L_27)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_009e;
	}

IL_009a:
	{
		// return false;
		V_3 = (bool)0;
		goto IL_009e;
	}

IL_009e:
	{
		// }
		bool L_28 = V_3;
		return L_28;
	}
}
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::CompareInt(System.String,Facebook.WitAi.CallbackHandlers.ValuePathMatcher)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_CompareInt_mBFEA1319F29D734BB9902412C9B3BD757E5B5FFB (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, String_t* ___value0, ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* ___matcher1, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	bool V_2 = false;
	bool V_3 = false;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	{
		// if (!int.TryParse(value, out int dValue)) return false;
		String_t* L_0 = ___value0;
		bool L_1;
		L_1 = Int32_TryParse_mFC6BFCB86964E2BCA4052155B10983837A695EA4(L_0, (&V_0), NULL);
		V_2 = (bool)((((int32_t)L_1) == ((int32_t)0))? 1 : 0);
		bool L_2 = V_2;
		if (!L_2)
		{
			goto IL_0014;
		}
	}
	{
		// if (!int.TryParse(value, out int dValue)) return false;
		V_3 = (bool)0;
		goto IL_0084;
	}

IL_0014:
	{
		// int dMatchValue = int.Parse(matcher.matchValue);
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_3 = ___matcher1;
		NullCheck(L_3);
		String_t* L_4 = L_3->___matchValue_5;
		int32_t L_5;
		L_5 = Int32_Parse_m59B9CC9D5E5B6C99C14251E57FB43BE6AB658767(L_4, NULL);
		V_1 = L_5;
		// switch (matcher.comparisonMethod)
		ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* L_6 = ___matcher1;
		NullCheck(L_6);
		int32_t L_7 = L_6->___comparisonMethod_4;
		V_5 = L_7;
		int32_t L_8 = V_5;
		V_4 = L_8;
		int32_t L_9 = V_4;
		switch (L_9)
		{
			case 0:
			{
				goto IL_004d;
			}
			case 1:
			{
				goto IL_0054;
			}
			case 2:
			{
				goto IL_005e;
			}
			case 3:
			{
				goto IL_006c;
			}
			case 4:
			{
				goto IL_0065;
			}
			case 5:
			{
				goto IL_0076;
			}
		}
	}
	{
		goto IL_0080;
	}

IL_004d:
	{
		// return dValue == dMatchValue;
		int32_t L_10 = V_0;
		int32_t L_11 = V_1;
		V_3 = (bool)((((int32_t)L_10) == ((int32_t)L_11))? 1 : 0);
		goto IL_0084;
	}

IL_0054:
	{
		// return dValue != dMatchValue;
		int32_t L_12 = V_0;
		int32_t L_13 = V_1;
		V_3 = (bool)((((int32_t)((((int32_t)L_12) == ((int32_t)L_13))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_0084;
	}

IL_005e:
	{
		// return dValue > dMatchValue;
		int32_t L_14 = V_0;
		int32_t L_15 = V_1;
		V_3 = (bool)((((int32_t)L_14) > ((int32_t)L_15))? 1 : 0);
		goto IL_0084;
	}

IL_0065:
	{
		// return dValue < dMatchValue;
		int32_t L_16 = V_0;
		int32_t L_17 = V_1;
		V_3 = (bool)((((int32_t)L_16) < ((int32_t)L_17))? 1 : 0);
		goto IL_0084;
	}

IL_006c:
	{
		// return dValue >= dMatchValue;
		int32_t L_18 = V_0;
		int32_t L_19 = V_1;
		V_3 = (bool)((((int32_t)((((int32_t)L_18) < ((int32_t)L_19))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_0084;
	}

IL_0076:
	{
		// return dValue <= dMatchValue;
		int32_t L_20 = V_0;
		int32_t L_21 = V_1;
		V_3 = (bool)((((int32_t)((((int32_t)L_20) > ((int32_t)L_21))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_0084;
	}

IL_0080:
	{
		// return false;
		V_3 = (bool)0;
		goto IL_0084;
	}

IL_0084:
	{
		// }
		bool L_22 = V_3;
		return L_22;
	}
}
// System.Boolean Facebook.WitAi.CallbackHandlers.WitResponseMatcher::IntentMatches(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WitResponseMatcher_IntentMatches_mC627F7AF4545BCC223D06C2674367446F6E98A37 (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral22270319A52B6C9B9967E9291B9A79B116526038);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral334976BAB31C9D74A4F24AC5A19ADD9273522252);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral497E0727E6D4098F7DA86E306F0B961AA34D95FF);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB3F14BF976EFD974E34846B742502C802FABAE9D);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralFD60316EE3ADB7B16A998DF8AE0D68C293F6622E);
		s_Il2CppMethodInitialized = true;
	}
	WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* V_0 = NULL;
	bool V_1 = false;
	bool V_2 = false;
	bool V_3 = false;
	float V_4 = 0.0f;
	bool V_5 = false;
	{
		// var intentNode = response.GetFirstIntent();
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___response0;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_1;
		L_1 = WitResultUtilities_GetFirstIntent_mE9748A7275970EABF61BC4920BFB445857066F58(L_0, NULL);
		V_0 = L_1;
		// if (string.IsNullOrEmpty(intent))
		String_t* L_2 = __this->___intent_5;
		bool L_3;
		L_3 = String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A(L_2, NULL);
		V_1 = L_3;
		bool L_4 = V_1;
		if (!L_4)
		{
			goto IL_001f;
		}
	}
	{
		// return true;
		V_2 = (bool)1;
		goto IL_00c5;
	}

IL_001f:
	{
		// if (intent == intentNode["name"].Value)
		String_t* L_5 = __this->___intent_5;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_6 = V_0;
		NullCheck(L_6);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_7;
		L_7 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_6, _stringLiteralCE18B047107AA23D1AA9B2ED32D316148E02655F);
		NullCheck(L_7);
		String_t* L_8;
		L_8 = VirtualFuncInvoker0< String_t* >::Invoke(9 /* System.String Facebook.WitAi.Lib.WitResponseNode::get_Value() */, L_7);
		bool L_9;
		L_9 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_5, L_8, NULL);
		V_3 = L_9;
		bool L_10 = V_3;
		if (!L_10)
		{
			goto IL_00c1;
		}
	}
	{
		// var actualConfidence = intentNode["confidence"].AsFloat;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_11 = V_0;
		NullCheck(L_11);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_12;
		L_12 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_11, _stringLiteral497E0727E6D4098F7DA86E306F0B961AA34D95FF);
		NullCheck(L_12);
		float L_13;
		L_13 = VirtualFuncInvoker0< float >::Invoke(20 /* System.Single Facebook.WitAi.Lib.WitResponseNode::get_AsFloat() */, L_12);
		V_4 = L_13;
		// if (actualConfidence >= confidenceThreshold)
		float L_14 = V_4;
		float L_15 = __this->___confidenceThreshold_6;
		V_5 = (bool)((((int32_t)((!(((float)L_14) >= ((float)L_15)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_16 = V_5;
		if (!L_16)
		{
			goto IL_006c;
		}
	}
	{
		// return true;
		V_2 = (bool)1;
		goto IL_00c5;
	}

IL_006c:
	{
		// Debug.Log($"{intent} matched, but confidence ({actualConfidence.ToString("F")}) was below threshold ({confidenceThreshold.ToString("F")})");
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_17 = (StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*)(StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*)SZArrayNew(StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var, (uint32_t)6);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_18 = L_17;
		String_t* L_19 = __this->___intent_5;
		NullCheck(L_18);
		ArrayElementTypeCheck (L_18, L_19);
		(L_18)->SetAt(static_cast<il2cpp_array_size_t>(0), (String_t*)L_19);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_20 = L_18;
		NullCheck(L_20);
		ArrayElementTypeCheck (L_20, _stringLiteral334976BAB31C9D74A4F24AC5A19ADD9273522252);
		(L_20)->SetAt(static_cast<il2cpp_array_size_t>(1), (String_t*)_stringLiteral334976BAB31C9D74A4F24AC5A19ADD9273522252);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_21 = L_20;
		String_t* L_22;
		L_22 = Single_ToString_m3F2C4433B6ADFA5ED8E3F14ED19CD23014E5179D((&V_4), _stringLiteralFD60316EE3ADB7B16A998DF8AE0D68C293F6622E, NULL);
		NullCheck(L_21);
		ArrayElementTypeCheck (L_21, L_22);
		(L_21)->SetAt(static_cast<il2cpp_array_size_t>(2), (String_t*)L_22);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_23 = L_21;
		NullCheck(L_23);
		ArrayElementTypeCheck (L_23, _stringLiteral22270319A52B6C9B9967E9291B9A79B116526038);
		(L_23)->SetAt(static_cast<il2cpp_array_size_t>(3), (String_t*)_stringLiteral22270319A52B6C9B9967E9291B9A79B116526038);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_24 = L_23;
		float* L_25 = (&__this->___confidenceThreshold_6);
		String_t* L_26;
		L_26 = Single_ToString_m3F2C4433B6ADFA5ED8E3F14ED19CD23014E5179D(L_25, _stringLiteralFD60316EE3ADB7B16A998DF8AE0D68C293F6622E, NULL);
		NullCheck(L_24);
		ArrayElementTypeCheck (L_24, L_26);
		(L_24)->SetAt(static_cast<il2cpp_array_size_t>(4), (String_t*)L_26);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_27 = L_24;
		NullCheck(L_27);
		ArrayElementTypeCheck (L_27, _stringLiteralB3F14BF976EFD974E34846B742502C802FABAE9D);
		(L_27)->SetAt(static_cast<il2cpp_array_size_t>(5), (String_t*)_stringLiteralB3F14BF976EFD974E34846B742502C802FABAE9D);
		String_t* L_28;
		L_28 = String_Concat_m6B0734B65813C8EA093D78E5C2D16534EB6FE8C0(L_27, NULL);
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_Log_m86567BCF22BBE7809747817453CACA0E41E68219(L_28, NULL);
	}

IL_00c1:
	{
		// return false;
		V_2 = (bool)0;
		goto IL_00c5;
	}

IL_00c5:
	{
		// }
		bool L_29 = V_2;
		return L_29;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseMatcher::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseMatcher__ctor_mB81768CF47FEDAA8B731D83226298AB18A46E5D4 (WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// [Range(0, 1f), SerializeField] public float confidenceThreshold = .6f;
		__this->___confidenceThreshold_6 = (0.600000024f);
		// [SerializeField] private MultiValueEvent onMultiValueEvent = new MultiValueEvent();
		MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C* L_0 = (MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C*)il2cpp_codegen_object_new(MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		MultiValueEvent__ctor_m24721BEA88A8DC6A7438F16DF9C7F4B141BE0FF0(L_0, NULL);
		__this->___onMultiValueEvent_9 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___onMultiValueEvent_9), (void*)L_0);
		WitResponseHandler__ctor_m90BE72DD4323937145932F727817A8EA2216A683(__this, NULL);
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.WitResponseMatcher::.cctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitResponseMatcher__cctor_m6F8D78AB8BA2DFE600AE008EEADD8866F7D47D5E (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral057E47B1A5317B280AD5B6CE27C8F11073D42316);
		s_Il2CppMethodInitialized = true;
	}
	{
		// private static Regex valueRegex = new Regex(Regex.Escape("{value}"), RegexOptions.Compiled);
		il2cpp_codegen_runtime_class_init_inline(Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = Regex_Escape_m2E4E071ABAFAE1BF55932725F28F4194CD56D7DE(_stringLiteral057E47B1A5317B280AD5B6CE27C8F11073D42316, NULL);
		Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* L_1 = (Regex_tE773142C2BE45C5D362B0F815AFF831707A51772*)il2cpp_codegen_object_new(Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_il2cpp_TypeInfo_var);
		NullCheck(L_1);
		Regex__ctor_mE3996C71B04A4A6845745D01C93B1D27423D0621(L_1, L_0, 8, NULL);
		((WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_StaticFields*)il2cpp_codegen_static_fields_for(WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_il2cpp_TypeInfo_var))->___valueRegex_10 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&((WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_StaticFields*)il2cpp_codegen_static_fields_for(WitResponseMatcher_t06B27E6514774265FC0C7D70B2E142578DA64900_il2cpp_TypeInfo_var))->___valueRegex_10), (void*)L_1);
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
// System.Void Facebook.WitAi.CallbackHandlers.MultiValueEvent::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void MultiValueEvent__ctor_m24721BEA88A8DC6A7438F16DF9C7F4B141BE0FF0 (MultiValueEvent_t2599FF3CB92AE4AE37B392D655E9B9686DEC640C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1__ctor_mA1CAC73F82FF0E8BC4870AEBF137B7F72614957D_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		UnityEvent_1__ctor_mA1CAC73F82FF0E8BC4870AEBF137B7F72614957D(__this, UnityEvent_1__ctor_mA1CAC73F82FF0E8BC4870AEBF137B7F72614957D_RuntimeMethod_var);
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
// System.Void Facebook.WitAi.CallbackHandlers.ValueEvent::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ValueEvent__ctor_mD1D914C07A91A8D88BEEB83833BCDC9818261377 (ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1__ctor_m0F7D790DACD6F3D18E865D01FE4427603C1B0CC6_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		UnityEvent_1__ctor_m0F7D790DACD6F3D18E865D01FE4427603C1B0CC6(__this, UnityEvent_1__ctor_m0F7D790DACD6F3D18E865D01FE4427603C1B0CC6_RuntimeMethod_var);
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
// System.Void Facebook.WitAi.CallbackHandlers.FormattedValueEvents::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void FormattedValueEvents__ctor_m4DA3F46BEB2E9D13C232CB1BC9DEAC759746B57C (FormattedValueEvents_tB10203753DC8F2717F64091CEA942383D08D48D4* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public ValueEvent onFormattedValueEvent = new ValueEvent();
		ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74* L_0 = (ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74*)il2cpp_codegen_object_new(ValueEvent_t14ED8C8ABC819EAA8F0F19F7B1EA579C45993E74_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		ValueEvent__ctor_mD1D914C07A91A8D88BEEB83833BCDC9818261377(L_0, NULL);
		__this->___onFormattedValueEvent_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___onFormattedValueEvent_1), (void*)L_0);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
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
// Facebook.WitAi.WitResponseReference Facebook.WitAi.CallbackHandlers.ValuePathMatcher::get_ConfidenceReference()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* ValuePathMatcher_get_ConfidenceReference_m561926765A3249DAB510C86C42FC2692338F5F62 (ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBFC27499BD5186964D062DE5FE7C222AE471C53E);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralF3E84B722399601AD7E281754E917478AA9AD48D);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	bool V_1 = false;
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* V_2 = NULL;
	bool V_3 = false;
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* G_B4_0 = NULL;
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* G_B3_0 = NULL;
	String_t* G_B5_0 = NULL;
	{
		// if (null != confidencePathReference) return confidencePathReference;
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_0 = __this->___confidencePathReference_9;
		V_1 = (bool)((!(((RuntimeObject*)(WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E*)L_0) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
		bool L_1 = V_1;
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		// if (null != confidencePathReference) return confidencePathReference;
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_2 = __this->___confidencePathReference_9;
		V_2 = L_2;
		goto IL_006d;
	}

IL_0017:
	{
		// var confidencePath = Reference?.path;
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_3;
		L_3 = ValuePathMatcher_get_Reference_m2A32CA50F5C03A5EF44B4FFDBC17F8BB562E5787(__this, NULL);
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_4 = L_3;
		G_B3_0 = L_4;
		if (L_4)
		{
			G_B4_0 = L_4;
			goto IL_0024;
		}
	}
	{
		G_B5_0 = ((String_t*)(NULL));
		goto IL_0029;
	}

IL_0024:
	{
		NullCheck(G_B4_0);
		String_t* L_5 = G_B4_0->___path_1;
		G_B5_0 = L_5;
	}

IL_0029:
	{
		V_0 = G_B5_0;
		// if (!string.IsNullOrEmpty(confidencePath))
		String_t* L_6 = V_0;
		bool L_7;
		L_7 = String_IsNullOrEmpty_m54CF0907E7C4F3AFB2E796A13DC751ECBB8DB64A(L_6, NULL);
		V_3 = (bool)((((int32_t)L_7) == ((int32_t)0))? 1 : 0);
		bool L_8 = V_3;
		if (!L_8)
		{
			goto IL_0064;
		}
	}
	{
		// confidencePath = confidencePath.Substring(0, confidencePath.LastIndexOf("."));
		String_t* L_9 = V_0;
		String_t* L_10 = V_0;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = String_LastIndexOf_m8923DBD89F2B3E5A34190B038B48F402E0C17E40(L_10, _stringLiteralF3E84B722399601AD7E281754E917478AA9AD48D, NULL);
		NullCheck(L_9);
		String_t* L_12;
		L_12 = String_Substring_mB1D94F47935D22E130FF2C01DBB6A4135FBB76CE(L_9, 0, L_11, NULL);
		V_0 = L_12;
		// confidencePath += ".confidence";
		String_t* L_13 = V_0;
		String_t* L_14;
		L_14 = String_Concat_mAF2CE02CC0CB7460753D0A1A91CCF2B1E9804C5D(L_13, _stringLiteralBFC27499BD5186964D062DE5FE7C222AE471C53E, NULL);
		V_0 = L_14;
		// confidencePathReference = WitResultUtilities.GetWitResponseReference(confidencePath);
		String_t* L_15 = V_0;
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_16;
		L_16 = WitResultUtilities_GetWitResponseReference_m7E0E209887806033E842A60BEDDECC03FAB77AAD(L_15, NULL);
		__this->___confidencePathReference_9 = L_16;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___confidencePathReference_9), (void*)L_16);
	}

IL_0064:
	{
		// return confidencePathReference;
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_17 = __this->___confidencePathReference_9;
		V_2 = L_17;
		goto IL_006d;
	}

IL_006d:
	{
		// }
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_18 = V_2;
		return L_18;
	}
}
// Facebook.WitAi.WitResponseReference Facebook.WitAi.CallbackHandlers.ValuePathMatcher::get_Reference()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* ValuePathMatcher_get_Reference_m2A32CA50F5C03A5EF44B4FFDBC17F8BB562E5787 (ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* V_1 = NULL;
	bool V_2 = false;
	int32_t G_B5_0 = 0;
	{
		// if (witValueReference) return witValueReference.Reference;
		WitValue_tD53CDFBDC3B12AB1495984258B6ACC38EEE55E32* L_0 = __this->___witValueReference_1;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Implicit_m18E1885C296CC868AC918101523697CFE6413C79(L_0, NULL);
		V_0 = L_1;
		bool L_2 = V_0;
		if (!L_2)
		{
			goto IL_001e;
		}
	}
	{
		// if (witValueReference) return witValueReference.Reference;
		WitValue_tD53CDFBDC3B12AB1495984258B6ACC38EEE55E32* L_3 = __this->___witValueReference_1;
		NullCheck(L_3);
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_4;
		L_4 = WitValue_get_Reference_m983BF433C074A617B269D6B1C6ACF9D74EE7958E(L_3, NULL);
		V_1 = L_4;
		goto IL_005f;
	}

IL_001e:
	{
		// if (null == pathReference || pathReference.path != path)
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_5 = __this->___pathReference_8;
		if (!L_5)
		{
			goto IL_003e;
		}
	}
	{
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_6 = __this->___pathReference_8;
		NullCheck(L_6);
		String_t* L_7 = L_6->___path_1;
		String_t* L_8 = __this->___path_0;
		bool L_9;
		L_9 = String_op_Inequality_m0FBE5AC4931D312E5B347BAA603755676E6DA2FE(L_7, L_8, NULL);
		G_B5_0 = ((int32_t)(L_9));
		goto IL_003f;
	}

IL_003e:
	{
		G_B5_0 = 1;
	}

IL_003f:
	{
		V_2 = (bool)G_B5_0;
		bool L_10 = V_2;
		if (!L_10)
		{
			goto IL_0056;
		}
	}
	{
		// pathReference = WitResultUtilities.GetWitResponseReference(path);
		String_t* L_11 = __this->___path_0;
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_12;
		L_12 = WitResultUtilities_GetWitResponseReference_m7E0E209887806033E842A60BEDDECC03FAB77AAD(L_11, NULL);
		__this->___pathReference_8 = L_12;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___pathReference_8), (void*)L_12);
	}

IL_0056:
	{
		// return pathReference;
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_13 = __this->___pathReference_8;
		V_1 = L_13;
		goto IL_005f;
	}

IL_005f:
	{
		// }
		WitResponseReference_t719B8BDD7237E571229C3C224366142B8339ED3E* L_14 = V_1;
		return L_14;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.ValuePathMatcher::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ValuePathMatcher__ctor_m07EFED8A2D66952E0FB33D2D6C6C6AC2F4FE4FED (ValuePathMatcher_tC7072C801D083BF3CEBF96F7C6A4C419277AAE4C* __this, const RuntimeMethod* method) 
{
	{
		// public bool contentRequired = true;
		__this->___contentRequired_2 = (bool)1;
		// public double floatingPointComparisonTolerance = .0001f;
		__this->___floatingPointComparisonTolerance_6 = (9.9999997473787516E-05);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
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
// System.Void Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher::OnHandleResponse(Facebook.WitAi.Lib.WitResponseNode)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitUtteranceMatcher_OnHandleResponse_m83929902127384A727DDFE5F36ACD0BF434F423B (WitUtteranceMatcher_t1A2C2BA7A4F1DFF63A0266115636A1FEFD2166C4* __this, WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* ___response0, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBFCC6EE94F1B7AA05A04750903E25F93A7188AE0);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	bool V_1 = false;
	Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F* V_2 = NULL;
	bool V_3 = false;
	bool V_4 = false;
	bool V_5 = false;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B7_0 = 0;
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* G_B10_0 = NULL;
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* G_B9_0 = NULL;
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* G_B14_0 = NULL;
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* G_B13_0 = NULL;
	int32_t G_B21_0 = 0;
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* G_B24_0 = NULL;
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* G_B23_0 = NULL;
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* G_B29_0 = NULL;
	StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* G_B28_0 = NULL;
	{
		// var text = response["text"].Value;
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_0 = ___response0;
		NullCheck(L_0);
		WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB* L_1;
		L_1 = VirtualFuncInvoker1< WitResponseNode_tACC149B056FE33C54D55A5EAB653A77902617DBB*, String_t* >::Invoke(7 /* Facebook.WitAi.Lib.WitResponseNode Facebook.WitAi.Lib.WitResponseNode::get_Item(System.String) */, L_0, _stringLiteralBFCC6EE94F1B7AA05A04750903E25F93A7188AE0);
		NullCheck(L_1);
		String_t* L_2;
		L_2 = VirtualFuncInvoker0< String_t* >::Invoke(9 /* System.String Facebook.WitAi.Lib.WitResponseNode::get_Value() */, L_1);
		V_0 = L_2;
		// if (useRegex)
		bool L_3 = __this->___useRegex_7;
		V_1 = L_3;
		bool L_4 = V_1;
		if (!L_4)
		{
			goto IL_00a9;
		}
	}
	{
		// if (null == regex)
		Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* L_5 = __this->___regex_9;
		V_3 = (bool)((((RuntimeObject*)(Regex_tE773142C2BE45C5D362B0F815AFF831707A51772*)L_5) == ((RuntimeObject*)(RuntimeObject*)NULL))? 1 : 0);
		bool L_6 = V_3;
		if (!L_6)
		{
			goto IL_0042;
		}
	}
	{
		// regex = new Regex(searchText, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		String_t* L_7 = __this->___searchText_5;
		Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* L_8 = (Regex_tE773142C2BE45C5D362B0F815AFF831707A51772*)il2cpp_codegen_object_new(Regex_tE773142C2BE45C5D362B0F815AFF831707A51772_il2cpp_TypeInfo_var);
		NullCheck(L_8);
		Regex__ctor_mE3996C71B04A4A6845745D01C93B1D27423D0621(L_8, L_7, ((int32_t)9), NULL);
		__this->___regex_9 = L_8;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___regex_9), (void*)L_8);
	}

IL_0042:
	{
		// var match = regex.Match(text);
		Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* L_9 = __this->___regex_9;
		String_t* L_10 = V_0;
		NullCheck(L_9);
		Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F* L_11;
		L_11 = Regex_Match_m58565ECF23ACCD2CA77D6F10A6A182B03CF0FF84(L_9, L_10, NULL);
		V_2 = L_11;
		// if (match.Success)
		Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F* L_12 = V_2;
		NullCheck(L_12);
		bool L_13;
		L_13 = Group_get_Success_m4E0238EE4B1E7F927E2AF13E2E5901BCA92BE62F(L_12, NULL);
		V_4 = L_13;
		bool L_14 = V_4;
		if (!L_14)
		{
			goto IL_00a6;
		}
	}
	{
		// if (exactMatch && match.Value == text)
		bool L_15 = __this->___exactMatch_6;
		if (!L_15)
		{
			goto IL_0072;
		}
	}
	{
		Match_tFBEBCF225BD8EA17BCE6CE3FE5C1BD8E3074105F* L_16 = V_2;
		NullCheck(L_16);
		String_t* L_17;
		L_17 = Capture_get_Value_m1AB4193C2FC4B0D08AA34FECF10D03876D848BDC(L_16, NULL);
		String_t* L_18 = V_0;
		bool L_19;
		L_19 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_17, L_18, NULL);
		G_B7_0 = ((int32_t)(L_19));
		goto IL_0073;
	}

IL_0072:
	{
		G_B7_0 = 0;
	}

IL_0073:
	{
		V_5 = (bool)G_B7_0;
		bool L_20 = V_5;
		if (!L_20)
		{
			goto IL_0090;
		}
	}
	{
		// onUtteranceMatched?.Invoke(text);
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_21 = __this->___onUtteranceMatched_8;
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_22 = L_21;
		G_B9_0 = L_22;
		if (L_22)
		{
			G_B10_0 = L_22;
			goto IL_0086;
		}
	}
	{
		goto IL_008d;
	}

IL_0086:
	{
		String_t* L_23 = V_0;
		NullCheck(G_B10_0);
		UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15(G_B10_0, L_23, UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
	}

IL_008d:
	{
		goto IL_00a5;
	}

IL_0090:
	{
		// onUtteranceMatched?.Invoke(text);
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_24 = __this->___onUtteranceMatched_8;
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_25 = L_24;
		G_B13_0 = L_25;
		if (L_25)
		{
			G_B14_0 = L_25;
			goto IL_009d;
		}
	}
	{
		goto IL_00a4;
	}

IL_009d:
	{
		String_t* L_26 = V_0;
		NullCheck(G_B14_0);
		UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15(G_B14_0, L_26, UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
	}

IL_00a4:
	{
	}

IL_00a5:
	{
	}

IL_00a6:
	{
		goto IL_0118;
	}

IL_00a9:
	{
		// else if (exactMatch && text.ToLower() == searchText.ToLower())
		bool L_27 = __this->___exactMatch_6;
		if (!L_27)
		{
			goto IL_00c9;
		}
	}
	{
		String_t* L_28 = V_0;
		NullCheck(L_28);
		String_t* L_29;
		L_29 = String_ToLower_m6191ABA3DC514ED47C10BDA23FD0DDCEAE7ACFBD(L_28, NULL);
		String_t* L_30 = __this->___searchText_5;
		NullCheck(L_30);
		String_t* L_31;
		L_31 = String_ToLower_m6191ABA3DC514ED47C10BDA23FD0DDCEAE7ACFBD(L_30, NULL);
		bool L_32;
		L_32 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_29, L_31, NULL);
		G_B21_0 = ((int32_t)(L_32));
		goto IL_00ca;
	}

IL_00c9:
	{
		G_B21_0 = 0;
	}

IL_00ca:
	{
		V_6 = (bool)G_B21_0;
		bool L_33 = V_6;
		if (!L_33)
		{
			goto IL_00e7;
		}
	}
	{
		// onUtteranceMatched?.Invoke(text);
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_34 = __this->___onUtteranceMatched_8;
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_35 = L_34;
		G_B23_0 = L_35;
		if (L_35)
		{
			G_B24_0 = L_35;
			goto IL_00dd;
		}
	}
	{
		goto IL_00e4;
	}

IL_00dd:
	{
		String_t* L_36 = V_0;
		NullCheck(G_B24_0);
		UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15(G_B24_0, L_36, UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
	}

IL_00e4:
	{
		goto IL_0118;
	}

IL_00e7:
	{
		// else if (text.ToLower().Contains(searchText.ToLower()))
		String_t* L_37 = V_0;
		NullCheck(L_37);
		String_t* L_38;
		L_38 = String_ToLower_m6191ABA3DC514ED47C10BDA23FD0DDCEAE7ACFBD(L_37, NULL);
		String_t* L_39 = __this->___searchText_5;
		NullCheck(L_39);
		String_t* L_40;
		L_40 = String_ToLower_m6191ABA3DC514ED47C10BDA23FD0DDCEAE7ACFBD(L_39, NULL);
		NullCheck(L_38);
		bool L_41;
		L_41 = String_Contains_m6D77B121FADA7CA5F397C0FABB65DA62DF03B6C3(L_38, L_40, NULL);
		V_7 = L_41;
		bool L_42 = V_7;
		if (!L_42)
		{
			goto IL_0118;
		}
	}
	{
		// onUtteranceMatched?.Invoke(text);
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_43 = __this->___onUtteranceMatched_8;
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_44 = L_43;
		G_B28_0 = L_44;
		if (L_44)
		{
			G_B29_0 = L_44;
			goto IL_0110;
		}
	}
	{
		goto IL_0117;
	}

IL_0110:
	{
		String_t* L_45 = V_0;
		NullCheck(G_B29_0);
		UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15(G_B29_0, L_45, UnityEvent_1_Invoke_mA633B48B5D287DA856FB954AC3E4012487E63C15_RuntimeMethod_var);
	}

IL_0117:
	{
	}

IL_0118:
	{
		// }
		return;
	}
}
// System.Void Facebook.WitAi.CallbackHandlers.WitUtteranceMatcher::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WitUtteranceMatcher__ctor_mD2508F7BF061E1643E07B446F990E5502AD260A6 (WitUtteranceMatcher_t1A2C2BA7A4F1DFF63A0266115636A1FEFD2166C4* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// [SerializeField] private bool exactMatch = true;
		__this->___exactMatch_6 = (bool)1;
		// [SerializeField] private StringEvent onUtteranceMatched = new StringEvent();
		StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76* L_0 = (StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76*)il2cpp_codegen_object_new(StringEvent_t900D996D498937BA8C57EB5A74AFC7E3C8652B76_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringEvent__ctor_mF3F63CB79A236D91609C9B7EBF293D7E6935006A(L_0, NULL);
		__this->___onUtteranceMatched_8 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___onUtteranceMatched_8), (void*)L_0);
		WitResponseHandler__ctor_m90BE72DD4323937145932F727817A8EA2216A683(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool Mathf_Approximately_m1C8DD0BB6A2D22A7DCF09AD7F8EE9ABD12D3F620_inline (float ___a0, float ___b1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	{
		float L_0 = ___b1;
		float L_1 = ___a0;
		float L_2;
		L_2 = fabsf(((float)il2cpp_codegen_subtract(L_0, L_1)));
		float L_3 = ___a0;
		float L_4;
		L_4 = fabsf(L_3);
		float L_5 = ___b1;
		float L_6;
		L_6 = fabsf(L_5);
		float L_7;
		L_7 = Mathf_Max_mA9DCA91E87D6D27034F56ABA52606A9090406016_inline(L_4, L_6, NULL);
		float L_8 = ((Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_StaticFields*)il2cpp_codegen_static_fields_for(Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_il2cpp_TypeInfo_var))->___Epsilon_0;
		float L_9;
		L_9 = Mathf_Max_mA9DCA91E87D6D27034F56ABA52606A9090406016_inline(((float)il2cpp_codegen_multiply((9.99999997E-07f), L_7)), ((float)il2cpp_codegen_multiply(L_8, (8.0f))), NULL);
		V_0 = (bool)((((float)L_2) < ((float)L_9))? 1 : 0);
		goto IL_0035;
	}

IL_0035:
	{
		bool L_10 = V_0;
		return L_10;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___item0, const RuntimeMethod* method) 
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = (int32_t)__this->____version_3;
		__this->____version_3 = ((int32_t)il2cpp_codegen_add(L_0, 1));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)__this->____items_1;
		V_0 = L_1;
		int32_t L_2 = (int32_t)__this->____size_2;
		V_1 = L_2;
		int32_t L_3 = V_1;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = V_0;
		NullCheck(L_4);
		if ((!(((uint32_t)L_3) < ((uint32_t)((int32_t)(((RuntimeArray*)L_4)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		int32_t L_5 = V_1;
		__this->____size_2 = ((int32_t)il2cpp_codegen_add(L_5, 1));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = V_0;
		int32_t L_7 = V_1;
		RuntimeObject* L_8 = ___item0;
		NullCheck(L_6);
		(L_6)->SetAt(static_cast<il2cpp_array_size_t>(L_7), (RuntimeObject*)L_8);
		return;
	}

IL_0034:
	{
		RuntimeObject* L_9 = ___item0;
		List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230 Enumerator_get_Current_mE3475384B761E1C7971D3639BD09117FE8363422_gshared_inline (Enumerator_tEA93FE2B778D098F590CA168BEFC4CD85D73A6B9* __this, const RuntimeMethod* method) 
{
	{
		KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230 L_0 = (KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230)__this->____current_3;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* KeyValuePair_2_get_Key_mBD8EA7557C27E6956F2AF29DA3F7499B2F51A282_gshared_inline (KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230* __this, const RuntimeMethod* method) 
{
	{
		RuntimeObject* L_0 = (RuntimeObject*)__this->___key_0;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* KeyValuePair_2_get_Value_mC6BD8075F9C9DDEF7B4D731E5C38EC19103988E7_gshared_inline (KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230* __this, const RuntimeMethod* method) 
{
	{
		RuntimeObject* L_0 = (RuntimeObject*)__this->___value_1;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* Enumerator_get_Current_m6330F15D18EE4F547C05DF9BF83C5EB710376027_gshared_inline (Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A* __this, const RuntimeMethod* method) 
{
	{
		RuntimeObject* L_0 = (RuntimeObject*)__this->____current_3;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Mathf_Max_mA9DCA91E87D6D27034F56ABA52606A9090406016_inline (float ___a0, float ___b1, const RuntimeMethod* method) 
{
	float V_0 = 0.0f;
	float G_B3_0 = 0.0f;
	{
		float L_0 = ___a0;
		float L_1 = ___b1;
		if ((((float)L_0) > ((float)L_1)))
		{
			goto IL_0008;
		}
	}
	{
		float L_2 = ___b1;
		G_B3_0 = L_2;
		goto IL_0009;
	}

IL_0008:
	{
		float L_3 = ___a0;
		G_B3_0 = L_3;
	}

IL_0009:
	{
		V_0 = G_B3_0;
		goto IL_000c;
	}

IL_000c:
	{
		float L_4 = V_0;
		return L_4;
	}
}
