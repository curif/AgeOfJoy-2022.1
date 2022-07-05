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
template <typename T1>
struct InterfaceActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename T1, typename T2>
struct InterfaceActionInvoker2
{
	typedef void (*Action)(void*, T1, T2, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj, T1 p1, T2 p2)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		((Action)invokeData.methodPtr)(obj, p1, p2, invokeData.method);
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
template <typename R, typename T1>
struct InterfaceFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename R, typename T1>
struct GenericInterfaceFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (const RuntimeMethod* method, RuntimeObject* obj, T1 p1)
	{
		VirtualInvokeData invokeData;
		il2cpp_codegen_get_generic_interface_invoke_data(method, obj, &invokeData);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};

// YamlDotNet.Serialization.BuilderSkeleton`1<YamlDotNet.Serialization.DeserializerBuilder>
struct BuilderSkeleton_1_t0167F386CC0A444D4DF47B2B025B1682E6E14DDD;
// YamlDotNet.Serialization.BuilderSkeleton`1<System.Object>
struct BuilderSkeleton_1_t69E43168AB3C84E1D6925FF23D23D6E6DA192CA3;
// System.Collections.Generic.Dictionary`2<YamlDotNet.Core.TagName,System.Type>
struct Dictionary_2_t436B50E1AD3A85D9B9AD3F1AFCA3B8641980FB2C;
// System.Collections.Generic.Dictionary`2<System.Type,System.Type>
struct Dictionary_2_t8BF76F08F2E28AE3B97CD39EBC7A0FE57398B1B0;
// System.Collections.Generic.IDictionary`2<System.Type,YamlDotNet.Core.TagName>
struct IDictionary_2_t422C8FD351D54AF2CA25959269C2520BD6D19C75;
// System.Collections.Generic.IEnumerator`1<YamlDotNet.RepresentationModel.YamlNode>
struct IEnumerator_1_tDC519E97395FDC74301B7D8B9B4C483E428B6E7A;
// System.Collections.Generic.IList`1<YamlDotNet.RepresentationModel.YamlDocument>
struct IList_1_t81A6FC3C71AC30CAB39D6D83169B2BBD59B37452;
// System.Collections.Generic.IList`1<YamlDotNet.RepresentationModel.YamlNode>
struct IList_1_tCCF9CCEB2EB3674951014128166F1E53763C8D8A;
// YamlDotNet.Helpers.IOrderedDictionary`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode>
struct IOrderedDictionary_2_tBDD3E4C049F103F1452CBF0854FD6EA6DEBF16B2;
// YamlDotNet.Serialization.LazyComponentRegistrationList`2<System.Collections.Generic.IEnumerable`1<YamlDotNet.Serialization.IYamlTypeConverter>,YamlDotNet.Serialization.IObjectGraphVisitor`1<YamlDotNet.Serialization.Nothing>>
struct LazyComponentRegistrationList_2_tE60B4EC11F8FFACDDE2389C906E16685AABA83C8;
// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.EmissionPhaseObjectGraphVisitorArgs,YamlDotNet.Serialization.IObjectGraphVisitor`1<YamlDotNet.Core.IEmitter>>
struct LazyComponentRegistrationList_2_t0C760579C6F034AB9E0BCBE47CB694DA5C24029E;
// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.IEventEmitter,YamlDotNet.Serialization.IEventEmitter>
struct LazyComponentRegistrationList_2_tDCCA8478D940865F5FC0FA0ACFB8046A57E02885;
// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.ITypeInspector,YamlDotNet.Serialization.ITypeInspector>
struct LazyComponentRegistrationList_2_t3354A54608161A131C567E47EFD2E263083CC975;
// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.Nothing,YamlDotNet.Serialization.INodeDeserializer>
struct LazyComponentRegistrationList_2_tE9CA46E66B0E398FED7522F956FBD4162B8C8720;
// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.Nothing,YamlDotNet.Serialization.INodeTypeResolver>
struct LazyComponentRegistrationList_2_t7CEE2487307F9FF90C1394DFB280198B4BAA96ED;
// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.Nothing,YamlDotNet.Serialization.IYamlTypeConverter>
struct LazyComponentRegistrationList_2_t14C28DADDF80BFA958B92199E7AC69E20C82EC8C;
// System.Lazy`1<YamlDotNet.Serialization.IObjectFactory>
struct Lazy_1_t07C5FF4736F8E7539A5BA1A2B91D93F6E8D1755E;
// System.Collections.Generic.List`1<System.Object>
struct List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D;
// System.Collections.Generic.List`1<System.String>
struct List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD;
// System.Collections.Generic.List`1<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem>
struct List_1_t125631862BDE1BA29850B762D409E23C07F29B40;
// System.Collections.Generic.Stack`1<YamlDotNet.Core.ParserState>
struct Stack_1_t55587C03FC878FB03AC42FAF88BFD6A72F3D8588;
// System.Char[]
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
// System.Int32[]
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
// YamlDotNet.Samples.Item[]
struct ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A;
// System.Reflection.MethodInfo[]
struct MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265;
// System.Object[]
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
// System.String[]
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
// System.Type[]
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
// YamlDotNet.Samples.DeserializeObjectGraph/OrderItem[]
struct OrderItemU5BU5D_tF6D97C809F8BE77B722928B1250A56266CDFC455;
// YamlDotNet.Samples.Address
struct Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22;
// System.Attribute
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA;
// System.Reflection.Binder
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
// System.Reflection.ConstructorInfo
struct ConstructorInfo_t1B5967EE7E5554272F79F8880183C70AD240EEEB;
// YamlDotNet.Samples.ConvertYamlToJson
struct ConvertYamlToJson_tBB2C23EB2A1FA789E90C44BD4ABF69525E9B74C7;
// YamlDotNet.Samples.Customer
struct Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B;
// YamlDotNet.Samples.DeserializeObjectGraph
struct DeserializeObjectGraph_tFB1288C0A3F2618E3667101B7BA1F28A4F01CC81;
// YamlDotNet.Serialization.DeserializerBuilder
struct DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2;
// YamlDotNet.Samples.DeserializingMultipleDocuments
struct DeserializingMultipleDocuments_tEF21A0E880B34920A6FFE552F54864A3948E0CA1;
// YamlDotNet.Core.Events.DocumentStart
struct DocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F;
// YamlDotNet.Core.EmitterSettings
struct EmitterSettings_tB71AD7EAF2109566C18A63AB8CAC292C6F49BF89;
// YamlDotNet.Samples.Helpers.ExampleRunner
struct ExampleRunner_tF444DF9613423B29748C32062F07F3469F498BE1;
// YamlDotNet.Serialization.IDeserializer
struct IDeserializer_tBD7A4B4E3C42FE5F64A8A26C27801C7570507EE8;
// YamlDotNet.Serialization.INamingConvention
struct INamingConvention_t67C300CAD742123FD87FE0094CCB022C528E2D36;
// YamlDotNet.Core.IParser
struct IParser_tAC4E000DB7F8E2AF87AB7E29CF18A89CE86AAE17;
// YamlDotNet.Core.IScanner
struct IScanner_tFAA5349F84829346CC37E81FE991CF45A2A8B453;
// YamlDotNet.Serialization.ISerializer
struct ISerializer_tBA1C2B389CD506D5E3CB5DC013F5C94C126A1136;
// Xunit.Abstractions.ITestOutputHelper
struct ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898;
// YamlDotNet.Serialization.ITypeResolver
struct ITypeResolver_t2A898D90FD586074C36971AE2DBB68560FBE5A18;
// YamlDotNet.Samples.Item
struct Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274;
// YamlDotNet.Samples.LoadingAYamlStream
struct LoadingAYamlStream_tB3542F174EAAE1ADEE5AF9F9D0ED1378B9ED8A75;
// YamlDotNet.Core.Mark
struct Mark_t950DC067D3EC830050595AD3F189554215D04694;
// System.Reflection.MemberFilter
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
// System.Reflection.MemberInfo
struct MemberInfo_t;
// System.Reflection.MethodBase
struct MethodBase_t;
// System.Reflection.MethodInfo
struct MethodInfo_t;
// UnityEngine.MonoBehaviour
struct MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71;
// YamlDotNet.Serialization.ObjectGraphTraversalStrategyFactory
struct ObjectGraphTraversalStrategyFactory_t11833ECC8109450AD9720894A3DDB582865C5C55;
// YamlDotNet.Core.Parser
struct Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4;
// YamlDotNet.Core.Events.ParsingEvent
struct ParsingEvent_tE58420F975B5631C8D828FAEAF925C00B889570E;
// YamlDotNet.Samples.Receipt
struct Receipt_tE506B8843866A1445C321C463687A56F99457821;
// System.Text.RegularExpressions.Regex
struct Regex_tE773142C2BE45C5D362B0F815AFF831707A51772;
// YamlDotNet.Samples.Helpers.SampleAttribute
struct SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF;
// YamlDotNet.Samples.SerializeObjectGraph
struct SerializeObjectGraph_t6D234A16443587BC3F34E50A277696E6616C7073;
// YamlDotNet.Serialization.SerializerBuilder
struct SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C;
// YamlDotNet.Core.Events.StreamStart
struct StreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18;
// System.String
struct String_t;
// System.Text.StringBuilder
struct StringBuilder_t;
// System.IO.StringReader
struct StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8;
// YamlDotNet.Core.TagDirectiveCollection
struct TagDirectiveCollection_t481CA3EBA69A9C486F25C9E36B75CAD521CE91D3;
// System.IO.TextReader
struct TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7;
// YamlDotNet.Core.Tokens.Token
struct Token_tBF9A8215C30363F3FD515BB7813C50A69413BD38;
// System.Type
struct Type_t;
// YamlDotNet.Core.Tokens.VersionDirective
struct VersionDirective_tA2D5B7E5BAE8CC67A93A5F981EF228413EE95DC5;
// System.Void
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
// YamlDotNet.Serialization.YamlAttributeOverrides
struct YamlAttributeOverrides_tB6A9AC079221C925C4950C101A12F7DF467C56E3;
// YamlDotNet.RepresentationModel.YamlDocument
struct YamlDocument_tF61A99B79C0F9627DA9492381E0D86BF934CBFEE;
// YamlDotNet.RepresentationModel.YamlMappingNode
struct YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687;
// YamlDotNet.RepresentationModel.YamlNode
struct YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA;
// YamlDotNet.RepresentationModel.YamlScalarNode
struct YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648;
// YamlDotNet.RepresentationModel.YamlSequenceNode
struct YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC;
// YamlDotNet.RepresentationModel.YamlStream
struct YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30;
// YamlDotNet.Samples.DeserializeObjectGraph/Address
struct Address_tD85CB003A9782A8A69C513245758DEC28AA76024;
// YamlDotNet.Samples.DeserializeObjectGraph/Customer
struct Customer_t3A696A73AE9D0CD1502A51269581AAE969303040;
// YamlDotNet.Samples.DeserializeObjectGraph/Order
struct Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53;
// YamlDotNet.Samples.DeserializeObjectGraph/OrderItem
struct OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4;
// YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper
struct StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29;
// YamlDotNet.Core.Parser/EventQueue
struct EventQueue_tB7F62ACE416D5DD59C719667DB6451455FD5E6B1;

IL2CPP_EXTERN_C RuntimeClass* Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* CamelCaseNamingConvention_t12B61C131A233B37C7908239FD79D9CB20E7A9D7_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IDeserializer_tBD7A4B4E3C42FE5F64A8A26C27801C7570507EE8_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IDictionary_2_t27535D075559E1DDCA209D995D55F8D1942BFFDB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerable_1_t1697387F8171A07386B91936BF35A2B40EBB7508_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerator_1_tC3AB957AEB9EF9C51A822113B42129D5A59E7C97_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerator_1_tDC519E97395FDC74301B7D8B9B4C483E428B6E7A_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IList_1_t81A6FC3C71AC30CAB39D6D83169B2BBD59B37452_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ISerializer_tBA1C2B389CD506D5E3CB5DC013F5C94C126A1136_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Receipt_tE506B8843866A1445C321C463687A56F99457821_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringBuilder_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Type_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral0CDC424E2E41D059A5128706BAF6E4498A03D39B;
IL2CPP_EXTERN_C String_t* _stringLiteral17F4D0F233F7C806CBB7F1E4936F65607B1F1168;
IL2CPP_EXTERN_C String_t* _stringLiteral22222F4E849B4072CE91FDF62F9BE7096EF5A56D;
IL2CPP_EXTERN_C String_t* _stringLiteral32D59C370DFA7384C9A1965AED35526215B01B41;
IL2CPP_EXTERN_C String_t* _stringLiteral34564CCF416EA1A7EB32DF54E7A91E201275ADFD;
IL2CPP_EXTERN_C String_t* _stringLiteral35D9703651C0B5FE577BAA089212BEF91D370ADB;
IL2CPP_EXTERN_C String_t* _stringLiteral3EF25B754B6A9B1A80F4D678709B7226A5AAC355;
IL2CPP_EXTERN_C String_t* _stringLiteral3F86111F44D66C543B732847E04E3C2A5B38BB3D;
IL2CPP_EXTERN_C String_t* _stringLiteral47772B3950A6EC25FA3C56B90D0D638D84BA85C9;
IL2CPP_EXTERN_C String_t* _stringLiteral55300DE6DA6DDD8D71829680B0ECD972C284D15B;
IL2CPP_EXTERN_C String_t* _stringLiteral55FDBD1185DCD87EC6576504CCAF9DED9FFDA000;
IL2CPP_EXTERN_C String_t* _stringLiteral62FE8FF641FE619F53CF8F5D38A8A6BC7AED19E4;
IL2CPP_EXTERN_C String_t* _stringLiteral661F8132E7CA6894B80DC3DCD155CD99652BCE79;
IL2CPP_EXTERN_C String_t* _stringLiteral7069C7F1D4004126C126503625BAF9ED9D1B55A6;
IL2CPP_EXTERN_C String_t* _stringLiteral76341B1F170DDD858BE9AB81FFF2E8F58575E06C;
IL2CPP_EXTERN_C String_t* _stringLiteral76F486E2D05B1268235F8E6A197128F313D7B852;
IL2CPP_EXTERN_C String_t* _stringLiteral7854BB2558C26BF20C4E12AE79BBB1B7A7B78B79;
IL2CPP_EXTERN_C String_t* _stringLiteral836EE39547BFB34DD863BF3BD7388E1DCE1CD167;
IL2CPP_EXTERN_C String_t* _stringLiteral9D452B73AB02C81825A66E4A40E989C8C105BAAB;
IL2CPP_EXTERN_C String_t* _stringLiteralAF0FE9B404A1D2065495B62C369E335378810973;
IL2CPP_EXTERN_C String_t* _stringLiteralB4B7B6A99BC5C77B775F2D9887AF276A891D9D68;
IL2CPP_EXTERN_C String_t* _stringLiteralB7B50DE055D313D7C46CD4D2452069DA3871AB41;
IL2CPP_EXTERN_C String_t* _stringLiteralBE647EF55536A560FF890347C80BB77C55E99DC0;
IL2CPP_EXTERN_C String_t* _stringLiteralC9FEB70F083D88ECA4A1F7EF7AB0A7BD3AC0892F;
IL2CPP_EXTERN_C String_t* _stringLiteralCB600D75C8BA5A8A7860C5C3ECBDFB0AB9CFA695;
IL2CPP_EXTERN_C String_t* _stringLiteralD42C2EFEBF138F5432C644B4CF104F04D3987CAC;
IL2CPP_EXTERN_C String_t* _stringLiteralD65C2004FD674D31FC063C3BF88CF632907BE277;
IL2CPP_EXTERN_C String_t* _stringLiteralE3E725A290611C2F36B4B1DC53D0D529279F0923;
IL2CPP_EXTERN_C String_t* _stringLiteralE7A3075116E53C28B6D5986FA50919EFAD0B1D0B;
IL2CPP_EXTERN_C String_t* _stringLiteralEBBAC010CB9033B6FACD3A10B94A4919AF898822;
IL2CPP_EXTERN_C String_t* _stringLiteralEF4B3E7859FBAABF4652DE2F6675345692C335AA;
IL2CPP_EXTERN_C String_t* _stringLiteralF300D2310959AF105732D339376803869D9B2B91;
IL2CPP_EXTERN_C String_t* _stringLiteralFB7070F86EF8CC6601805BCBB939D6865BC62D1F;
IL2CPP_EXTERN_C const RuntimeMethod* Array_IndexOf_TisString_t_m9107AABCE77608D1D21B9ECB6DA42D0D4334AF32_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* BuilderSkeleton_1_WithNamingConvention_mC36449BE7D8903C0DE46705F4CB97C7B34D8BAB3_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_Dispose_mD0224D5784331A02E72D2EE1F00EC806D852D377_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_MoveNext_m9DCB4F92A17E8A60784FC19CEA6C2042F94EF358_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_get_Current_mECC16CA0C47DDF96EECC358EC2FCDD3527495F54_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ExampleRunner_GetAllTestDisplayNames_mA7F999F8B96722BA70EAD672B45027E1B2678708_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ExampleRunner_GetAllTestNames_mC20DA32B264F88714446AF4C380A97AF820AD963_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ExampleRunner_Start_m3143B4441D1B7C4035C46E351676E1D6B63F7C3A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* IDeserializer_Deserialize_TisList_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_mA7A32850F13BAB7E82BD96CB70EC0416B5241829_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* IDeserializer_Deserialize_TisOrder_t455515D55F55269AEBF8AD3EF7162547E7AA0A53_m01A0A51FE5225C8C07C0690FC0025BD500C0E56F_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* KeyValuePair_2_get_Key_mBB9F6BBA53ADA88314F65DD538C0FA5E7C50425C_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_GetEnumerator_m348416E1E4D61F1D9E285BB9043058B83707653F_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ParserExtensions_Accept_TisDocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F_m583C79A9FA702726EDF18F5F87C013A0F6353566_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ParserExtensions_Consume_TisStreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18_mA969137CC45206737455EBA404755F3A0023B62C_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeType* SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_0_0_0_var;
IL2CPP_EXTERN_C const RuntimeType* StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29_0_0_0_var;

struct ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A;
struct MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// <Module>
struct U3CModuleU3E_t09D12E325B511FAA80714EEAB1C0B4CC6A0F4711 
{
};

// YamlDotNet.Serialization.BuilderSkeleton`1<YamlDotNet.Serialization.DeserializerBuilder>
struct BuilderSkeleton_1_t0167F386CC0A444D4DF47B2B025B1682E6E14DDD  : public RuntimeObject
{
	// YamlDotNet.Serialization.INamingConvention YamlDotNet.Serialization.BuilderSkeleton`1::namingConvention
	RuntimeObject* ___namingConvention_0;
	// YamlDotNet.Serialization.ITypeResolver YamlDotNet.Serialization.BuilderSkeleton`1::typeResolver
	RuntimeObject* ___typeResolver_1;
	// YamlDotNet.Serialization.YamlAttributeOverrides YamlDotNet.Serialization.BuilderSkeleton`1::overrides
	YamlAttributeOverrides_tB6A9AC079221C925C4950C101A12F7DF467C56E3* ___overrides_2;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.Nothing,YamlDotNet.Serialization.IYamlTypeConverter> YamlDotNet.Serialization.BuilderSkeleton`1::typeConverterFactories
	LazyComponentRegistrationList_2_t14C28DADDF80BFA958B92199E7AC69E20C82EC8C* ___typeConverterFactories_3;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.ITypeInspector,YamlDotNet.Serialization.ITypeInspector> YamlDotNet.Serialization.BuilderSkeleton`1::typeInspectorFactories
	LazyComponentRegistrationList_2_t3354A54608161A131C567E47EFD2E263083CC975* ___typeInspectorFactories_4;
	// System.Boolean YamlDotNet.Serialization.BuilderSkeleton`1::ignoreFields
	bool ___ignoreFields_5;
	// System.Boolean YamlDotNet.Serialization.BuilderSkeleton`1::includeNonPublicProperties
	bool ___includeNonPublicProperties_6;
};

// YamlDotNet.Serialization.BuilderSkeleton`1<YamlDotNet.Serialization.SerializerBuilder>
struct BuilderSkeleton_1_t867721E1061324583662EE3E2A372E13C2D955DB  : public RuntimeObject
{
	// YamlDotNet.Serialization.INamingConvention YamlDotNet.Serialization.BuilderSkeleton`1::namingConvention
	RuntimeObject* ___namingConvention_0;
	// YamlDotNet.Serialization.ITypeResolver YamlDotNet.Serialization.BuilderSkeleton`1::typeResolver
	RuntimeObject* ___typeResolver_1;
	// YamlDotNet.Serialization.YamlAttributeOverrides YamlDotNet.Serialization.BuilderSkeleton`1::overrides
	YamlAttributeOverrides_tB6A9AC079221C925C4950C101A12F7DF467C56E3* ___overrides_2;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.Nothing,YamlDotNet.Serialization.IYamlTypeConverter> YamlDotNet.Serialization.BuilderSkeleton`1::typeConverterFactories
	LazyComponentRegistrationList_2_t14C28DADDF80BFA958B92199E7AC69E20C82EC8C* ___typeConverterFactories_3;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.ITypeInspector,YamlDotNet.Serialization.ITypeInspector> YamlDotNet.Serialization.BuilderSkeleton`1::typeInspectorFactories
	LazyComponentRegistrationList_2_t3354A54608161A131C567E47EFD2E263083CC975* ___typeInspectorFactories_4;
	// System.Boolean YamlDotNet.Serialization.BuilderSkeleton`1::ignoreFields
	bool ___ignoreFields_5;
	// System.Boolean YamlDotNet.Serialization.BuilderSkeleton`1::includeNonPublicProperties
	bool ___includeNonPublicProperties_6;
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

// System.Collections.Generic.List`1<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem>
struct List_1_t125631862BDE1BA29850B762D409E23C07F29B40  : public RuntimeObject
{
	// T[] System.Collections.Generic.List`1::_items
	OrderItemU5BU5D_tF6D97C809F8BE77B722928B1250A56266CDFC455* ____items_1;
	// System.Int32 System.Collections.Generic.List`1::_size
	int32_t ____size_2;
	// System.Int32 System.Collections.Generic.List`1::_version
	int32_t ____version_3;
	// System.Object System.Collections.Generic.List`1::_syncRoot
	RuntimeObject* ____syncRoot_4;
};

struct List_1_t125631862BDE1BA29850B762D409E23C07F29B40_StaticFields
{
	// T[] System.Collections.Generic.List`1::s_emptyArray
	OrderItemU5BU5D_tF6D97C809F8BE77B722928B1250A56266CDFC455* ___s_emptyArray_5;
};

// YamlDotNet.Samples.Address
struct Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22  : public RuntimeObject
{
	// System.String YamlDotNet.Samples.Address::<street>k__BackingField
	String_t* ___U3CstreetU3Ek__BackingField_0;
	// System.String YamlDotNet.Samples.Address::<city>k__BackingField
	String_t* ___U3CcityU3Ek__BackingField_1;
	// System.String YamlDotNet.Samples.Address::<state>k__BackingField
	String_t* ___U3CstateU3Ek__BackingField_2;
};
struct Il2CppArrayBounds;

// System.Reflection.Assembly
struct Assembly_t  : public RuntimeObject
{
};
// Native definition for P/Invoke marshalling of System.Reflection.Assembly
struct Assembly_t_marshaled_pinvoke
{
};
// Native definition for COM marshalling of System.Reflection.Assembly
struct Assembly_t_marshaled_com
{
};

// System.Attribute
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA  : public RuntimeObject
{
};

// YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention
struct CamelCaseNamingConvention_t12B61C131A233B37C7908239FD79D9CB20E7A9D7  : public RuntimeObject
{
};

struct CamelCaseNamingConvention_t12B61C131A233B37C7908239FD79D9CB20E7A9D7_StaticFields
{
	// YamlDotNet.Serialization.INamingConvention YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention::Instance
	RuntimeObject* ___Instance_0;
};

// YamlDotNet.Samples.ConvertYamlToJson
struct ConvertYamlToJson_tBB2C23EB2A1FA789E90C44BD4ABF69525E9B74C7  : public RuntimeObject
{
	// Xunit.Abstractions.ITestOutputHelper YamlDotNet.Samples.ConvertYamlToJson::output
	RuntimeObject* ___output_0;
};

// YamlDotNet.Samples.Customer
struct Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B  : public RuntimeObject
{
	// System.String YamlDotNet.Samples.Customer::<given>k__BackingField
	String_t* ___U3CgivenU3Ek__BackingField_0;
	// System.String YamlDotNet.Samples.Customer::<family>k__BackingField
	String_t* ___U3CfamilyU3Ek__BackingField_1;
};

// YamlDotNet.Samples.DeserializeObjectGraph
struct DeserializeObjectGraph_tFB1288C0A3F2618E3667101B7BA1F28A4F01CC81  : public RuntimeObject
{
	// Xunit.Abstractions.ITestOutputHelper YamlDotNet.Samples.DeserializeObjectGraph::output
	RuntimeObject* ___output_0;
};

// YamlDotNet.Samples.DeserializingMultipleDocuments
struct DeserializingMultipleDocuments_tEF21A0E880B34920A6FFE552F54864A3948E0CA1  : public RuntimeObject
{
	// Xunit.Abstractions.ITestOutputHelper YamlDotNet.Samples.DeserializingMultipleDocuments::output
	RuntimeObject* ___output_0;
};

// YamlDotNet.Samples.LoadingAYamlStream
struct LoadingAYamlStream_tB3542F174EAAE1ADEE5AF9F9D0ED1378B9ED8A75  : public RuntimeObject
{
	// Xunit.Abstractions.ITestOutputHelper YamlDotNet.Samples.LoadingAYamlStream::output
	RuntimeObject* ___output_0;
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

// System.Reflection.MemberInfo
struct MemberInfo_t  : public RuntimeObject
{
};

// YamlDotNet.Core.Parser
struct Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4  : public RuntimeObject
{
	// System.Collections.Generic.Stack`1<YamlDotNet.Core.ParserState> YamlDotNet.Core.Parser::states
	Stack_1_t55587C03FC878FB03AC42FAF88BFD6A72F3D8588* ___states_0;
	// YamlDotNet.Core.TagDirectiveCollection YamlDotNet.Core.Parser::tagDirectives
	TagDirectiveCollection_t481CA3EBA69A9C486F25C9E36B75CAD521CE91D3* ___tagDirectives_1;
	// YamlDotNet.Core.ParserState YamlDotNet.Core.Parser::state
	int32_t ___state_2;
	// YamlDotNet.Core.IScanner YamlDotNet.Core.Parser::scanner
	RuntimeObject* ___scanner_3;
	// YamlDotNet.Core.Tokens.Token YamlDotNet.Core.Parser::currentToken
	Token_tBF9A8215C30363F3FD515BB7813C50A69413BD38* ___currentToken_4;
	// YamlDotNet.Core.Tokens.VersionDirective YamlDotNet.Core.Parser::version
	VersionDirective_tA2D5B7E5BAE8CC67A93A5F981EF228413EE95DC5* ___version_5;
	// YamlDotNet.Core.Events.ParsingEvent YamlDotNet.Core.Parser::<Current>k__BackingField
	ParsingEvent_tE58420F975B5631C8D828FAEAF925C00B889570E* ___U3CCurrentU3Ek__BackingField_6;
	// YamlDotNet.Core.Parser/EventQueue YamlDotNet.Core.Parser::pendingEvents
	EventQueue_tB7F62ACE416D5DD59C719667DB6451455FD5E6B1* ___pendingEvents_7;
};

// YamlDotNet.Core.Events.ParsingEvent
struct ParsingEvent_tE58420F975B5631C8D828FAEAF925C00B889570E  : public RuntimeObject
{
	// YamlDotNet.Core.Mark YamlDotNet.Core.Events.ParsingEvent::<Start>k__BackingField
	Mark_t950DC067D3EC830050595AD3F189554215D04694* ___U3CStartU3Ek__BackingField_0;
	// YamlDotNet.Core.Mark YamlDotNet.Core.Events.ParsingEvent::<End>k__BackingField
	Mark_t950DC067D3EC830050595AD3F189554215D04694* ___U3CEndU3Ek__BackingField_1;
};

// YamlDotNet.Samples.SerializeObjectGraph
struct SerializeObjectGraph_t6D234A16443587BC3F34E50A277696E6616C7073  : public RuntimeObject
{
	// Xunit.Abstractions.ITestOutputHelper YamlDotNet.Samples.SerializeObjectGraph::output
	RuntimeObject* ___output_0;
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

// System.Text.StringBuilder
struct StringBuilder_t  : public RuntimeObject
{
	// System.Char[] System.Text.StringBuilder::m_ChunkChars
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___m_ChunkChars_0;
	// System.Text.StringBuilder System.Text.StringBuilder::m_ChunkPrevious
	StringBuilder_t* ___m_ChunkPrevious_1;
	// System.Int32 System.Text.StringBuilder::m_ChunkLength
	int32_t ___m_ChunkLength_2;
	// System.Int32 System.Text.StringBuilder::m_ChunkOffset
	int32_t ___m_ChunkOffset_3;
	// System.Int32 System.Text.StringBuilder::m_MaxCapacity
	int32_t ___m_MaxCapacity_4;
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

// YamlDotNet.RepresentationModel.YamlDocument
struct YamlDocument_tF61A99B79C0F9627DA9492381E0D86BF934CBFEE  : public RuntimeObject
{
	// YamlDotNet.RepresentationModel.YamlNode YamlDotNet.RepresentationModel.YamlDocument::<RootNode>k__BackingField
	YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* ___U3CRootNodeU3Ek__BackingField_0;
};

// YamlDotNet.RepresentationModel.YamlStream
struct YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30  : public RuntimeObject
{
	// System.Collections.Generic.IList`1<YamlDotNet.RepresentationModel.YamlDocument> YamlDotNet.RepresentationModel.YamlStream::documents
	RuntimeObject* ___documents_0;
};

// YamlDotNet.Samples.DeserializeObjectGraph/Address
struct Address_tD85CB003A9782A8A69C513245758DEC28AA76024  : public RuntimeObject
{
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::<Street>k__BackingField
	String_t* ___U3CStreetU3Ek__BackingField_0;
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::<City>k__BackingField
	String_t* ___U3CCityU3Ek__BackingField_1;
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::<State>k__BackingField
	String_t* ___U3CStateU3Ek__BackingField_2;
};

// YamlDotNet.Samples.DeserializeObjectGraph/Customer
struct Customer_t3A696A73AE9D0CD1502A51269581AAE969303040  : public RuntimeObject
{
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/Customer::<Given>k__BackingField
	String_t* ___U3CGivenU3Ek__BackingField_0;
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/Customer::<Family>k__BackingField
	String_t* ___U3CFamilyU3Ek__BackingField_1;
};

// YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper
struct StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29  : public RuntimeObject
{
	// System.Text.StringBuilder YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::output
	StringBuilder_t* ___output_0;
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

// System.Collections.Generic.List`1/Enumerator<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem>
struct Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2 
{
	// System.Collections.Generic.List`1<T> System.Collections.Generic.List`1/Enumerator::_list
	List_1_t125631862BDE1BA29850B762D409E23C07F29B40* ____list_0;
	// System.Int32 System.Collections.Generic.List`1/Enumerator::_index
	int32_t ____index_1;
	// System.Int32 System.Collections.Generic.List`1/Enumerator::_version
	int32_t ____version_2;
	// T System.Collections.Generic.List`1/Enumerator::_current
	OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* ____current_3;
};

// System.Collections.Generic.KeyValuePair`2<System.Object,System.Object>
struct KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230 
{
	// TKey System.Collections.Generic.KeyValuePair`2::key
	RuntimeObject* ___key_0;
	// TValue System.Collections.Generic.KeyValuePair`2::value
	RuntimeObject* ___value_1;
};

// System.Collections.Generic.KeyValuePair`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode>
struct KeyValuePair_2_t5C963D5768B7B0EF500458A15140E7DDB438E997 
{
	// TKey System.Collections.Generic.KeyValuePair`2::key
	YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* ___key_0;
	// TValue System.Collections.Generic.KeyValuePair`2::value
	YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* ___value_1;
};

// YamlDotNet.Core.AnchorName
struct AnchorName_t94EA697EB10B53ECF53C1E86750105E5BA43A67E 
{
	// System.String YamlDotNet.Core.AnchorName::value
	String_t* ___value_2;
};

struct AnchorName_t94EA697EB10B53ECF53C1E86750105E5BA43A67E_StaticFields
{
	// YamlDotNet.Core.AnchorName YamlDotNet.Core.AnchorName::Empty
	AnchorName_t94EA697EB10B53ECF53C1E86750105E5BA43A67E ___Empty_0;
	// System.Text.RegularExpressions.Regex YamlDotNet.Core.AnchorName::AnchorPattern
	Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* ___AnchorPattern_1;
};
// Native definition for P/Invoke marshalling of YamlDotNet.Core.AnchorName
struct AnchorName_t94EA697EB10B53ECF53C1E86750105E5BA43A67E_marshaled_pinvoke
{
	char* ___value_2;
};
// Native definition for COM marshalling of YamlDotNet.Core.AnchorName
struct AnchorName_t94EA697EB10B53ECF53C1E86750105E5BA43A67E_marshaled_com
{
	Il2CppChar* ___value_2;
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

// System.Byte
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3 
{
	// System.Byte System.Byte::m_value
	uint8_t ___m_value_0;
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

// System.Decimal
struct Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			// System.Int32 System.Decimal::flags
			int32_t ___flags_8;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___flags_8_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___hi_9_OffsetPadding[4];
			// System.Int32 System.Decimal::hi
			int32_t ___hi_9;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___hi_9_OffsetPadding_forAlignmentOnly[4];
			int32_t ___hi_9_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___lo_10_OffsetPadding[8];
			// System.Int32 System.Decimal::lo
			int32_t ___lo_10;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___lo_10_OffsetPadding_forAlignmentOnly[8];
			int32_t ___lo_10_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___mid_11_OffsetPadding[12];
			// System.Int32 System.Decimal::mid
			int32_t ___mid_11;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___mid_11_OffsetPadding_forAlignmentOnly[12];
			int32_t ___mid_11_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ulomidLE_12_OffsetPadding[8];
			// System.UInt64 System.Decimal::ulomidLE
			uint64_t ___ulomidLE_12;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ulomidLE_12_OffsetPadding_forAlignmentOnly[8];
			uint64_t ___ulomidLE_12_forAlignmentOnly;
		};
	};
};

struct Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F_StaticFields
{
	// System.Decimal System.Decimal::Zero
	Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___Zero_3;
	// System.Decimal System.Decimal::One
	Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___One_4;
	// System.Decimal System.Decimal::MinusOne
	Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___MinusOne_5;
	// System.Decimal System.Decimal::MaxValue
	Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___MaxValue_6;
	// System.Decimal System.Decimal::MinValue
	Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___MinValue_7;
};

// YamlDotNet.Serialization.DeserializerBuilder
struct DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2  : public BuilderSkeleton_1_t0167F386CC0A444D4DF47B2B025B1682E6E14DDD
{
	// System.Lazy`1<YamlDotNet.Serialization.IObjectFactory> YamlDotNet.Serialization.DeserializerBuilder::objectFactory
	Lazy_1_t07C5FF4736F8E7539A5BA1A2B91D93F6E8D1755E* ___objectFactory_7;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.Nothing,YamlDotNet.Serialization.INodeDeserializer> YamlDotNet.Serialization.DeserializerBuilder::nodeDeserializerFactories
	LazyComponentRegistrationList_2_tE9CA46E66B0E398FED7522F956FBD4162B8C8720* ___nodeDeserializerFactories_8;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.Nothing,YamlDotNet.Serialization.INodeTypeResolver> YamlDotNet.Serialization.DeserializerBuilder::nodeTypeResolverFactories
	LazyComponentRegistrationList_2_t7CEE2487307F9FF90C1394DFB280198B4BAA96ED* ___nodeTypeResolverFactories_9;
	// System.Collections.Generic.Dictionary`2<YamlDotNet.Core.TagName,System.Type> YamlDotNet.Serialization.DeserializerBuilder::tagMappings
	Dictionary_2_t436B50E1AD3A85D9B9AD3F1AFCA3B8641980FB2C* ___tagMappings_10;
	// System.Collections.Generic.Dictionary`2<System.Type,System.Type> YamlDotNet.Serialization.DeserializerBuilder::typeMappings
	Dictionary_2_t8BF76F08F2E28AE3B97CD39EBC7A0FE57398B1B0* ___typeMappings_11;
	// System.Boolean YamlDotNet.Serialization.DeserializerBuilder::ignoreUnmatched
	bool ___ignoreUnmatched_12;
};

// YamlDotNet.Core.Events.DocumentStart
struct DocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F  : public ParsingEvent_tE58420F975B5631C8D828FAEAF925C00B889570E
{
	// YamlDotNet.Core.TagDirectiveCollection YamlDotNet.Core.Events.DocumentStart::<Tags>k__BackingField
	TagDirectiveCollection_t481CA3EBA69A9C486F25C9E36B75CAD521CE91D3* ___U3CTagsU3Ek__BackingField_2;
	// YamlDotNet.Core.Tokens.VersionDirective YamlDotNet.Core.Events.DocumentStart::<Version>k__BackingField
	VersionDirective_tA2D5B7E5BAE8CC67A93A5F981EF228413EE95DC5* ___U3CVersionU3Ek__BackingField_3;
	// System.Boolean YamlDotNet.Core.Events.DocumentStart::<IsImplicit>k__BackingField
	bool ___U3CIsImplicitU3Ek__BackingField_4;
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

// System.Reflection.MethodBase
struct MethodBase_t  : public MemberInfo_t
{
};

// YamlDotNet.Samples.Helpers.SampleAttribute
struct SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	// System.String YamlDotNet.Samples.Helpers.SampleAttribute::<DisplayName>k__BackingField
	String_t* ___U3CDisplayNameU3Ek__BackingField_0;
	// System.String YamlDotNet.Samples.Helpers.SampleAttribute::<Description>k__BackingField
	String_t* ___U3CDescriptionU3Ek__BackingField_1;
};

// YamlDotNet.Serialization.SerializerBuilder
struct SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C  : public BuilderSkeleton_1_t867721E1061324583662EE3E2A372E13C2D955DB
{
	// YamlDotNet.Serialization.ObjectGraphTraversalStrategyFactory YamlDotNet.Serialization.SerializerBuilder::objectGraphTraversalStrategyFactory
	ObjectGraphTraversalStrategyFactory_t11833ECC8109450AD9720894A3DDB582865C5C55* ___objectGraphTraversalStrategyFactory_7;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<System.Collections.Generic.IEnumerable`1<YamlDotNet.Serialization.IYamlTypeConverter>,YamlDotNet.Serialization.IObjectGraphVisitor`1<YamlDotNet.Serialization.Nothing>> YamlDotNet.Serialization.SerializerBuilder::preProcessingPhaseObjectGraphVisitorFactories
	LazyComponentRegistrationList_2_tE60B4EC11F8FFACDDE2389C906E16685AABA83C8* ___preProcessingPhaseObjectGraphVisitorFactories_8;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.EmissionPhaseObjectGraphVisitorArgs,YamlDotNet.Serialization.IObjectGraphVisitor`1<YamlDotNet.Core.IEmitter>> YamlDotNet.Serialization.SerializerBuilder::emissionPhaseObjectGraphVisitorFactories
	LazyComponentRegistrationList_2_t0C760579C6F034AB9E0BCBE47CB694DA5C24029E* ___emissionPhaseObjectGraphVisitorFactories_9;
	// YamlDotNet.Serialization.LazyComponentRegistrationList`2<YamlDotNet.Serialization.IEventEmitter,YamlDotNet.Serialization.IEventEmitter> YamlDotNet.Serialization.SerializerBuilder::eventEmitterFactories
	LazyComponentRegistrationList_2_tDCCA8478D940865F5FC0FA0ACFB8046A57E02885* ___eventEmitterFactories_10;
	// System.Collections.Generic.IDictionary`2<System.Type,YamlDotNet.Core.TagName> YamlDotNet.Serialization.SerializerBuilder::tagMappings
	RuntimeObject* ___tagMappings_11;
	// System.Int32 YamlDotNet.Serialization.SerializerBuilder::maximumRecursion
	int32_t ___maximumRecursion_12;
	// YamlDotNet.Core.EmitterSettings YamlDotNet.Serialization.SerializerBuilder::emitterSettings
	EmitterSettings_tB71AD7EAF2109566C18A63AB8CAC292C6F49BF89* ___emitterSettings_13;
	// YamlDotNet.Serialization.DefaultValuesHandling YamlDotNet.Serialization.SerializerBuilder::defaultValuesHandlingConfiguration
	int32_t ___defaultValuesHandlingConfiguration_14;
};

// YamlDotNet.Core.Events.StreamStart
struct StreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18  : public ParsingEvent_tE58420F975B5631C8D828FAEAF925C00B889570E
{
};

// YamlDotNet.Core.TagName
struct TagName_t15CB29949E97FF28193B6F635B58928554CB5854 
{
	// System.String YamlDotNet.Core.TagName::value
	String_t* ___value_1;
};

struct TagName_t15CB29949E97FF28193B6F635B58928554CB5854_StaticFields
{
	// YamlDotNet.Core.TagName YamlDotNet.Core.TagName::Empty
	TagName_t15CB29949E97FF28193B6F635B58928554CB5854 ___Empty_0;
};
// Native definition for P/Invoke marshalling of YamlDotNet.Core.TagName
struct TagName_t15CB29949E97FF28193B6F635B58928554CB5854_marshaled_pinvoke
{
	char* ___value_1;
};
// Native definition for COM marshalling of YamlDotNet.Core.TagName
struct TagName_t15CB29949E97FF28193B6F635B58928554CB5854_marshaled_com
{
	Il2CppChar* ___value_1;
};

// System.IO.TextReader
struct TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7  : public MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE
{
};

struct TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7_StaticFields
{
	// System.IO.TextReader System.IO.TextReader::Null
	TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7* ___Null_1;
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

// System.Reflection.ConstructorInfo
struct ConstructorInfo_t1B5967EE7E5554272F79F8880183C70AD240EEEB  : public MethodBase_t
{
};

struct ConstructorInfo_t1B5967EE7E5554272F79F8880183C70AD240EEEB_StaticFields
{
	// System.String System.Reflection.ConstructorInfo::ConstructorName
	String_t* ___ConstructorName_0;
	// System.String System.Reflection.ConstructorInfo::TypeConstructorName
	String_t* ___TypeConstructorName_1;
};

// YamlDotNet.Samples.Item
struct Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274  : public RuntimeObject
{
	// System.String YamlDotNet.Samples.Item::<part_no>k__BackingField
	String_t* ___U3Cpart_noU3Ek__BackingField_0;
	// System.String YamlDotNet.Samples.Item::<descrip>k__BackingField
	String_t* ___U3CdescripU3Ek__BackingField_1;
	// System.Decimal YamlDotNet.Samples.Item::<price>k__BackingField
	Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___U3CpriceU3Ek__BackingField_2;
	// System.Int32 YamlDotNet.Samples.Item::<quantity>k__BackingField
	int32_t ___U3CquantityU3Ek__BackingField_3;
};

// System.Reflection.MethodInfo
struct MethodInfo_t  : public MethodBase_t
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

// YamlDotNet.Samples.Receipt
struct Receipt_tE506B8843866A1445C321C463687A56F99457821  : public RuntimeObject
{
	// System.String YamlDotNet.Samples.Receipt::<receipt>k__BackingField
	String_t* ___U3CreceiptU3Ek__BackingField_0;
	// System.DateTime YamlDotNet.Samples.Receipt::<date>k__BackingField
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___U3CdateU3Ek__BackingField_1;
	// YamlDotNet.Samples.Customer YamlDotNet.Samples.Receipt::<customer>k__BackingField
	Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* ___U3CcustomerU3Ek__BackingField_2;
	// YamlDotNet.Samples.Item[] YamlDotNet.Samples.Receipt::<items>k__BackingField
	ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* ___U3CitemsU3Ek__BackingField_3;
	// YamlDotNet.Samples.Address YamlDotNet.Samples.Receipt::<bill_to>k__BackingField
	Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* ___U3Cbill_toU3Ek__BackingField_4;
	// YamlDotNet.Samples.Address YamlDotNet.Samples.Receipt::<ship_to>k__BackingField
	Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* ___U3Cship_toU3Ek__BackingField_5;
	// System.String YamlDotNet.Samples.Receipt::<specialDelivery>k__BackingField
	String_t* ___U3CspecialDeliveryU3Ek__BackingField_6;
};

// System.RuntimeTypeHandle
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	// System.IntPtr System.RuntimeTypeHandle::value
	intptr_t ___value_0;
};

// System.IO.StringReader
struct StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8  : public TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7
{
	// System.String System.IO.StringReader::_s
	String_t* ____s_2;
	// System.Int32 System.IO.StringReader::_pos
	int32_t ____pos_3;
	// System.Int32 System.IO.StringReader::_length
	int32_t ____length_4;
};

// YamlDotNet.RepresentationModel.YamlNode
struct YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA  : public RuntimeObject
{
	// YamlDotNet.Core.AnchorName YamlDotNet.RepresentationModel.YamlNode::<Anchor>k__BackingField
	AnchorName_t94EA697EB10B53ECF53C1E86750105E5BA43A67E ___U3CAnchorU3Ek__BackingField_2;
	// YamlDotNet.Core.TagName YamlDotNet.RepresentationModel.YamlNode::<Tag>k__BackingField
	TagName_t15CB29949E97FF28193B6F635B58928554CB5854 ___U3CTagU3Ek__BackingField_3;
	// YamlDotNet.Core.Mark YamlDotNet.RepresentationModel.YamlNode::<Start>k__BackingField
	Mark_t950DC067D3EC830050595AD3F189554215D04694* ___U3CStartU3Ek__BackingField_4;
	// YamlDotNet.Core.Mark YamlDotNet.RepresentationModel.YamlNode::<End>k__BackingField
	Mark_t950DC067D3EC830050595AD3F189554215D04694* ___U3CEndU3Ek__BackingField_5;
};

// YamlDotNet.Samples.DeserializeObjectGraph/Order
struct Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53  : public RuntimeObject
{
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/Order::<Receipt>k__BackingField
	String_t* ___U3CReceiptU3Ek__BackingField_0;
	// System.DateTime YamlDotNet.Samples.DeserializeObjectGraph/Order::<Date>k__BackingField
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___U3CDateU3Ek__BackingField_1;
	// YamlDotNet.Samples.DeserializeObjectGraph/Customer YamlDotNet.Samples.DeserializeObjectGraph/Order::<Customer>k__BackingField
	Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* ___U3CCustomerU3Ek__BackingField_2;
	// System.Collections.Generic.List`1<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem> YamlDotNet.Samples.DeserializeObjectGraph/Order::<Items>k__BackingField
	List_1_t125631862BDE1BA29850B762D409E23C07F29B40* ___U3CItemsU3Ek__BackingField_3;
	// YamlDotNet.Samples.DeserializeObjectGraph/Address YamlDotNet.Samples.DeserializeObjectGraph/Order::<BillTo>k__BackingField
	Address_tD85CB003A9782A8A69C513245758DEC28AA76024* ___U3CBillToU3Ek__BackingField_4;
	// YamlDotNet.Samples.DeserializeObjectGraph/Address YamlDotNet.Samples.DeserializeObjectGraph/Order::<ShipTo>k__BackingField
	Address_tD85CB003A9782A8A69C513245758DEC28AA76024* ___U3CShipToU3Ek__BackingField_5;
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/Order::<SpecialDelivery>k__BackingField
	String_t* ___U3CSpecialDeliveryU3Ek__BackingField_6;
};

// YamlDotNet.Samples.DeserializeObjectGraph/OrderItem
struct OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4  : public RuntimeObject
{
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::<PartNo>k__BackingField
	String_t* ___U3CPartNoU3Ek__BackingField_0;
	// System.String YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::<Descrip>k__BackingField
	String_t* ___U3CDescripU3Ek__BackingField_1;
	// System.Decimal YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::<Price>k__BackingField
	Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___U3CPriceU3Ek__BackingField_2;
	// System.Int32 YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::<Quantity>k__BackingField
	int32_t ___U3CQuantityU3Ek__BackingField_3;
};

// UnityEngine.Component
struct Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
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

// YamlDotNet.RepresentationModel.YamlMappingNode
struct YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687  : public YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA
{
	// YamlDotNet.Helpers.IOrderedDictionary`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode> YamlDotNet.RepresentationModel.YamlMappingNode::children
	RuntimeObject* ___children_6;
	// YamlDotNet.Core.Events.MappingStyle YamlDotNet.RepresentationModel.YamlMappingNode::<Style>k__BackingField
	int32_t ___U3CStyleU3Ek__BackingField_7;
};

// YamlDotNet.RepresentationModel.YamlScalarNode
struct YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648  : public YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA
{
	// System.String YamlDotNet.RepresentationModel.YamlScalarNode::<Value>k__BackingField
	String_t* ___U3CValueU3Ek__BackingField_6;
	// YamlDotNet.Core.ScalarStyle YamlDotNet.RepresentationModel.YamlScalarNode::<Style>k__BackingField
	int32_t ___U3CStyleU3Ek__BackingField_7;
};

// YamlDotNet.RepresentationModel.YamlSequenceNode
struct YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC  : public YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA
{
	// System.Collections.Generic.IList`1<YamlDotNet.RepresentationModel.YamlNode> YamlDotNet.RepresentationModel.YamlSequenceNode::children
	RuntimeObject* ___children_6;
	// YamlDotNet.Core.Events.SequenceStyle YamlDotNet.RepresentationModel.YamlSequenceNode::<Style>k__BackingField
	int32_t ___U3CStyleU3Ek__BackingField_7;
};

// UnityEngine.Behaviour
struct Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA  : public Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3
{
};

// UnityEngine.MonoBehaviour
struct MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71  : public Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA
{
};

// YamlDotNet.Samples.Helpers.ExampleRunner
struct ExampleRunner_tF444DF9613423B29748C32062F07F3469F498BE1  : public MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71
{
	// YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper YamlDotNet.Samples.Helpers.ExampleRunner::helper
	StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* ___helper_4;
	// System.String[] YamlDotNet.Samples.Helpers.ExampleRunner::disabledTests
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___disabledTests_5;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
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
// YamlDotNet.Samples.Item[]
struct ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A  : public RuntimeArray
{
	ALIGN_FIELD (8) Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* m_Items[1];

	inline Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
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
// System.Type[]
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB  : public RuntimeArray
{
	ALIGN_FIELD (8) Type_t* m_Items[1];

	inline Type_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Type_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Type_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline Type_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Type_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Type_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
// System.Reflection.MethodInfo[]
struct MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265  : public RuntimeArray
{
	ALIGN_FIELD (8) MethodInfo_t* m_Items[1];

	inline MethodInfo_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline MethodInfo_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, MethodInfo_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline MethodInfo_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline MethodInfo_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, MethodInfo_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};


// TBuilder YamlDotNet.Serialization.BuilderSkeleton`1<System.Object>::WithNamingConvention(YamlDotNet.Serialization.INamingConvention)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* BuilderSkeleton_1_WithNamingConvention_mEED0A8A4CE9C656B69668A60273B519CBCC58655_gshared (BuilderSkeleton_1_t69E43168AB3C84E1D6925FF23D23D6E6DA192CA3* __this, RuntimeObject* ___namingConvention0, const RuntimeMethod* method) ;
// System.Collections.Generic.List`1/Enumerator<T> System.Collections.Generic.List`1<System.Object>::GetEnumerator()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A List_1_GetEnumerator_mD8294A7FA2BEB1929487127D476F8EC1CDC23BFC_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1/Enumerator<System.Object>::Dispose()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator_Dispose_mD9DC3E3C3697830A4823047AB29A77DBBB5ED419_gshared (Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A* __this, const RuntimeMethod* method) ;
// T System.Collections.Generic.List`1/Enumerator<System.Object>::get_Current()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* Enumerator_get_Current_m6330F15D18EE4F547C05DF9BF83C5EB710376027_gshared_inline (Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A* __this, const RuntimeMethod* method) ;
// System.Boolean System.Collections.Generic.List`1/Enumerator<System.Object>::MoveNext()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Enumerator_MoveNext_mE921CC8F29FBBDE7CC3209A0ED0D921D58D00BCB_gshared (Enumerator_t9473BAB568A27E2339D48C1F91319E0F6D244D7A* __this, const RuntimeMethod* method) ;
// T YamlDotNet.Core.ParserExtensions::Consume<System.Object>(YamlDotNet.Core.IParser)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* ParserExtensions_Consume_TisRuntimeObject_m5D0A272E682CD76AE3EB79103859D395B7518CCE_gshared (RuntimeObject* ___parser0, const RuntimeMethod* method) ;
// System.Boolean YamlDotNet.Core.ParserExtensions::Accept<System.Object>(YamlDotNet.Core.IParser,T&)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool ParserExtensions_Accept_TisRuntimeObject_mCB7F9193CF778133974EDA0B9D6E66AB8FFB22E0_gshared (RuntimeObject* ___parser0, RuntimeObject** ___event1, const RuntimeMethod* method) ;
// TKey System.Collections.Generic.KeyValuePair`2<System.Object,System.Object>::get_Key()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* KeyValuePair_2_get_Key_mBD8EA7557C27E6956F2AF29DA3F7499B2F51A282_gshared_inline (KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.Object>::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void List_1__ctor_m7F078BB342729BDF11327FD89D7872265328F690_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.Object>::Add(T)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___item0, const RuntimeMethod* method) ;
// T[] System.Collections.Generic.List`1<System.Object>::ToArray()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* List_1_ToArray_mD7E4F8E7C11C3C67CB5739FCC0A6E86106A6291F_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) ;
// System.Int32 System.Array::IndexOf<System.Object>(T[],T)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_IndexOf_TisRuntimeObject_m4202FD457BB995E8553D010D1E861B7BD2F60BB0_gshared (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___array0, RuntimeObject* ___value1, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.Object>::AddWithResize(T)
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___item0, const RuntimeMethod* method) ;

// System.Void System.Object::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
// System.Void System.IO.StringReader::.ctor(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringReader__ctor_m72556EC1062F49E05CF41B0825AC7FA2DB2A81C0 (StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* __this, String_t* ___s0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Serialization.DeserializerBuilder::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DeserializerBuilder__ctor_mF61FF59EE374A791EE891257388FBB95A1812C6F (DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2* __this, const RuntimeMethod* method) ;
// YamlDotNet.Serialization.IDeserializer YamlDotNet.Serialization.DeserializerBuilder::Build()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* DeserializerBuilder_Build_mD3E9BBFA306704E1844C1BBC2D41F72734374069 (DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Serialization.SerializerBuilder::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SerializerBuilder__ctor_mC3F1F264785836360661ADBAF3C3EA09D974EFF0 (SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C* __this, const RuntimeMethod* method) ;
// YamlDotNet.Serialization.SerializerBuilder YamlDotNet.Serialization.SerializerBuilder::JsonCompatible()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C* SerializerBuilder_JsonCompatible_m45540AA8D0A87ED0AB2AD1EDAF02D11A6C4F91B4 (SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C* __this, const RuntimeMethod* method) ;
// YamlDotNet.Serialization.ISerializer YamlDotNet.Serialization.SerializerBuilder::Build()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* SerializerBuilder_Build_m37AF80C3630297349F07BCF78CDFC8D3742DCE6E (SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C* __this, const RuntimeMethod* method) ;
// TBuilder YamlDotNet.Serialization.BuilderSkeleton`1<YamlDotNet.Serialization.DeserializerBuilder>::WithNamingConvention(YamlDotNet.Serialization.INamingConvention)
inline DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2* BuilderSkeleton_1_WithNamingConvention_mC36449BE7D8903C0DE46705F4CB97C7B34D8BAB3 (BuilderSkeleton_1_t0167F386CC0A444D4DF47B2B025B1682E6E14DDD* __this, RuntimeObject* ___namingConvention0, const RuntimeMethod* method)
{
	return ((  DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2* (*) (BuilderSkeleton_1_t0167F386CC0A444D4DF47B2B025B1682E6E14DDD*, RuntimeObject*, const RuntimeMethod*))BuilderSkeleton_1_WithNamingConvention_mEED0A8A4CE9C656B69668A60273B519CBCC58655_gshared)(__this, ___namingConvention0, method);
}
// System.Collections.Generic.List`1<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem> YamlDotNet.Samples.DeserializeObjectGraph/Order::get_Items()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR List_1_t125631862BDE1BA29850B762D409E23C07F29B40* Order_get_Items_m04CBA6598E62087D34F49A24CFD74B42679DAE98_inline (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) ;
// System.Collections.Generic.List`1/Enumerator<T> System.Collections.Generic.List`1<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem>::GetEnumerator()
inline Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2 List_1_GetEnumerator_m348416E1E4D61F1D9E285BB9043058B83707653F (List_1_t125631862BDE1BA29850B762D409E23C07F29B40* __this, const RuntimeMethod* method)
{
	return ((  Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2 (*) (List_1_t125631862BDE1BA29850B762D409E23C07F29B40*, const RuntimeMethod*))List_1_GetEnumerator_mD8294A7FA2BEB1929487127D476F8EC1CDC23BFC_gshared)(__this, method);
}
// System.Void System.Collections.Generic.List`1/Enumerator<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem>::Dispose()
inline void Enumerator_Dispose_mD0224D5784331A02E72D2EE1F00EC806D852D377 (Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2* __this, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2*, const RuntimeMethod*))Enumerator_Dispose_mD9DC3E3C3697830A4823047AB29A77DBBB5ED419_gshared)(__this, method);
}
// T System.Collections.Generic.List`1/Enumerator<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem>::get_Current()
inline OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* Enumerator_get_Current_mECC16CA0C47DDF96EECC358EC2FCDD3527495F54_inline (Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2* __this, const RuntimeMethod* method)
{
	return ((  OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* (*) (Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2*, const RuntimeMethod*))Enumerator_get_Current_m6330F15D18EE4F547C05DF9BF83C5EB710376027_gshared_inline)(__this, method);
}
// System.String YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::get_PartNo()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* OrderItem_get_PartNo_m27D22430CD8BED232A06E432F42E536DB38F3A61_inline (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) ;
// System.Int32 YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::get_Quantity()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t OrderItem_get_Quantity_mFF50B28BBA27E7FC844280AE7E024B8FF59E8164_inline (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) ;
// System.Decimal YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::get_Price()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F OrderItem_get_Price_mFD13B646F706F3E18735C3BAADB775C06AC88BFC_inline (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) ;
// System.String YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::get_Descrip()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* OrderItem_get_Descrip_mC37419903157E1BBC8BF76B362D32D3E04A0D620_inline (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) ;
// System.Boolean System.Collections.Generic.List`1/Enumerator<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem>::MoveNext()
inline bool Enumerator_MoveNext_m9DCB4F92A17E8A60784FC19CEA6C2042F94EF358 (Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2*, const RuntimeMethod*))Enumerator_MoveNext_mE921CC8F29FBBDE7CC3209A0ED0D921D58D00BCB_gshared)(__this, method);
}
// YamlDotNet.Samples.DeserializeObjectGraph/Address YamlDotNet.Samples.DeserializeObjectGraph/Order::get_ShipTo()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Address_tD85CB003A9782A8A69C513245758DEC28AA76024* Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) ;
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::get_Street()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* Address_get_Street_mE2821ED2C5BC33B0F9643B86B64C5204930B37A7_inline (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) ;
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::get_City()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* Address_get_City_m8BADE3101BEEDDFFF69CC7FDE49BCBBB25B476F4_inline (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) ;
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::get_State()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* Address_get_State_m265F188BA17A5D8CAC8A3B89A114FC326E994908_inline (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) ;
// YamlDotNet.Samples.DeserializeObjectGraph/Address YamlDotNet.Samples.DeserializeObjectGraph/Order::get_BillTo()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Address_tD85CB003A9782A8A69C513245758DEC28AA76024* Order_get_BillTo_m9263CDEAEC8DA36B2104C14B612CF8B3A2424EFC_inline (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) ;
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Order::get_SpecialDelivery()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* Order_get_SpecialDelivery_m29487155B8D9FD5468C08389480733E3E179D94B_inline (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Core.Parser::.ctor(System.IO.TextReader)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Parser__ctor_m1A5F0024DBAD7675418CF593BFB634C95DF6D18B (Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4* __this, TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7* ___input0, const RuntimeMethod* method) ;
// T YamlDotNet.Core.ParserExtensions::Consume<YamlDotNet.Core.Events.StreamStart>(YamlDotNet.Core.IParser)
inline StreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18* ParserExtensions_Consume_TisStreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18_mA969137CC45206737455EBA404755F3A0023B62C (RuntimeObject* ___parser0, const RuntimeMethod* method)
{
	return ((  StreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18* (*) (RuntimeObject*, const RuntimeMethod*))ParserExtensions_Consume_TisRuntimeObject_m5D0A272E682CD76AE3EB79103859D395B7518CCE_gshared)(___parser0, method);
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
// System.Boolean System.Collections.Generic.List`1/Enumerator<System.String>::MoveNext()
inline bool Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED (Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1*, const RuntimeMethod*))Enumerator_MoveNext_mE921CC8F29FBBDE7CC3209A0ED0D921D58D00BCB_gshared)(__this, method);
}
// System.Boolean YamlDotNet.Core.ParserExtensions::Accept<YamlDotNet.Core.Events.DocumentStart>(YamlDotNet.Core.IParser,T&)
inline bool ParserExtensions_Accept_TisDocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F_m583C79A9FA702726EDF18F5F87C013A0F6353566 (RuntimeObject* ___parser0, DocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F** ___event1, const RuntimeMethod* method)
{
	return ((  bool (*) (RuntimeObject*, DocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F**, const RuntimeMethod*))ParserExtensions_Accept_TisRuntimeObject_mCB7F9193CF778133974EDA0B9D6E66AB8FFB22E0_gshared)(___parser0, ___event1, method);
}
// System.Void YamlDotNet.RepresentationModel.YamlStream::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void YamlStream__ctor_mB02CD1E92A127D645E6DA520F903D67A2B9E121E (YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.RepresentationModel.YamlStream::Load(System.IO.TextReader)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void YamlStream_Load_m9D04AEC580EFE7E19FD5AA94054CE651F6346E0F (YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30* __this, TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7* ___input0, const RuntimeMethod* method) ;
// System.Collections.Generic.IList`1<YamlDotNet.RepresentationModel.YamlDocument> YamlDotNet.RepresentationModel.YamlStream::get_Documents()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* YamlStream_get_Documents_mA9FE379D07185E210603717BE7DF5E78CC691B93 (YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30* __this, const RuntimeMethod* method) ;
// YamlDotNet.RepresentationModel.YamlNode YamlDotNet.RepresentationModel.YamlDocument::get_RootNode()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* YamlDocument_get_RootNode_mFDA2A8A64BF409D91B71A3F0C1C96722AF279D30_inline (YamlDocument_tF61A99B79C0F9627DA9492381E0D86BF934CBFEE* __this, const RuntimeMethod* method) ;
// YamlDotNet.Helpers.IOrderedDictionary`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode> YamlDotNet.RepresentationModel.YamlMappingNode::get_Children()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* YamlMappingNode_get_Children_m422A37EDF7D14EDE2AA4B23D6CE7AA440C5C823D (YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687* __this, const RuntimeMethod* method) ;
// TKey System.Collections.Generic.KeyValuePair`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode>::get_Key()
inline YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* KeyValuePair_2_get_Key_mBB9F6BBA53ADA88314F65DD538C0FA5E7C50425C_inline (KeyValuePair_2_t5C963D5768B7B0EF500458A15140E7DDB438E997* __this, const RuntimeMethod* method)
{
	return ((  YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* (*) (KeyValuePair_2_t5C963D5768B7B0EF500458A15140E7DDB438E997*, const RuntimeMethod*))KeyValuePair_2_get_Key_mBD8EA7557C27E6956F2AF29DA3F7499B2F51A282_gshared_inline)(__this, method);
}
// System.String YamlDotNet.RepresentationModel.YamlScalarNode::get_Value()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* YamlScalarNode_get_Value_m615F96CDDF044477CF47679024333453EEF98711_inline (YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.RepresentationModel.YamlScalarNode::.ctor(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void YamlScalarNode__ctor_mCF0C9B856F9F2EBDEA48475C7D6864C36A96DAC4 (YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Collections.Generic.IEnumerator`1<YamlDotNet.RepresentationModel.YamlNode> YamlDotNet.RepresentationModel.YamlSequenceNode::GetEnumerator()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* YamlSequenceNode_GetEnumerator_m542190AA8E38FE2F1BF41B3473402A1BCCCA6ED5 (YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Address::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address__ctor_mCE4E9D8BE7BD99B32E4D0AFCB8B1E3B2A19EB4C3 (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Address::set_street(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Address_set_street_m82E05D93979FB39D928C9AB7831517EB2AB07A89_inline (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Address::set_city(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Address_set_city_mACF46014990F092127EE327EAF11315F91CF7068_inline (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Address::set_state(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Address_set_state_m1434642D6CA9506A144D13B027C05046DE39E0F4_inline (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Receipt::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt__ctor_mD915A41D50BADDE2D4D689EFBB82D1627068AAE7 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Receipt::set_receipt(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_receipt_m2CD41C0DDE1FC9E896EC613C5547907EE1E23295_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void System.DateTime::.ctor(System.Int32,System.Int32,System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DateTime__ctor_mA3BF7CE28807F0A02634FD43913FAAFD989CEE88 (DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D* __this, int32_t ___year0, int32_t ___month1, int32_t ___day2, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Receipt::set_date(System.DateTime)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_date_mDC90FDE684124E33C1EC1D615BCBA1BCE397F3DC_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Customer::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Customer__ctor_m711E3C5703FFABEA25C2D6283E60D7367CBE9F41 (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Customer::set_given(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Customer_set_given_m5F124768B94E969B24A8F47D6B023888D0DA16D3_inline (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Customer::set_family(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Customer_set_family_m8565386D9A7735C1A82A1A05A6A6D2147A7ADF12_inline (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Receipt::set_customer(YamlDotNet.Samples.Customer)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_customer_m4DB78B0D39289ECC0F1007C4FF166FB6BF377B89_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Item::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Item__ctor_mB35BD96F76794831AA646FC20FDDCEFDC86914F4 (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Item::set_part_no(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Item_set_part_no_m0CE760B9F26A06ABF2BC7AB2B1B6430E317423D4_inline (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Item::set_descrip(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Item_set_descrip_m0B38A9144A563C9FC6A4E850C448F6249C9B4C83_inline (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void System.Decimal::.ctor(System.Int32,System.Int32,System.Int32,System.Boolean,System.Byte)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Decimal__ctor_mC089D0AF6A28E017DE6F2F0966D8EBEBFE2DAAF7 (Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F* __this, int32_t ___lo0, int32_t ___mid1, int32_t ___hi2, bool ___isNegative3, uint8_t ___scale4, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Item::set_price(System.Decimal)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Item_set_price_mAC9D0E64ABBC779377E02DCD3DBD29964DCB7C33_inline (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Item::set_quantity(System.Int32)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Item_set_quantity_mA66BE7C18DA97A2F5F894510F17294506BCA3520_inline (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, int32_t ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Receipt::set_items(YamlDotNet.Samples.Item[])
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_items_m9D9FB2BE4E4C12C4F585B69EA91A8B43C593F1B3_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Receipt::set_bill_to(YamlDotNet.Samples.Address)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_bill_to_m2B8A801F8C633D488890CD4AEDD5EE1E14244A52_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Receipt::set_ship_to(YamlDotNet.Samples.Address)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_ship_to_mF15187ACF9688D1A226755C41B16861D53DE4BD2_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* ___value0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Receipt::set_specialDelivery(System.String)
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_specialDelivery_m23A683515B0AE248B3B3A159ACC04EAF330693C6_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Void System.Collections.Generic.List`1<System.String>::.ctor()
inline void List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* __this, const RuntimeMethod* method)
{
	((  void (*) (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*, const RuntimeMethod*))List_1__ctor_m7F078BB342729BDF11327FD89D7872265328F690_gshared)(__this, method);
}
// System.Boolean System.String::op_Equality(System.String,System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0 (String_t* ___a0, String_t* ___b1, const RuntimeMethod* method) ;
// System.Boolean System.Type::get_IsClass()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Type_get_IsClass_mACC1E0E79C9996ADE9973F81971B740132B64549 (Type_t* __this, const RuntimeMethod* method) ;
// System.Reflection.MethodInfo[] System.Type::GetMethods()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* Type_GetMethods_m5D4A53D1E667CF33173EEA37D0111FE92A572559 (Type_t* __this, const RuntimeMethod* method) ;
// System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Type_GetTypeFromHandle_m2570A2A5B32A5E9D9F0F38B37459DA18736C823E (RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ___handle0, const RuntimeMethod* method) ;
// System.Attribute System.Attribute::GetCustomAttribute(System.Reflection.MemberInfo,System.Type)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* Attribute_GetCustomAttribute_mF9CB9F03A29701923B68556A396459E8FBEAE6B0 (MemberInfo_t* ___element0, Type_t* ___attributeType1, const RuntimeMethod* method) ;
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
// System.String YamlDotNet.Samples.Helpers.SampleAttribute::get_DisplayName()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* SampleAttribute_get_DisplayName_m10A912CE310DFE82E23C1C51096136E724B2402E_inline (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, const RuntimeMethod* method) ;
// System.Int32 System.Array::IndexOf<System.String>(T[],T)
inline int32_t Array_IndexOf_TisString_t_m9107AABCE77608D1D21B9ECB6DA42D0D4334AF32 (StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___array0, String_t* ___value1, const RuntimeMethod* method)
{
	return ((  int32_t (*) (StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*, String_t*, const RuntimeMethod*))Array_IndexOf_TisRuntimeObject_m4202FD457BB995E8553D010D1E861B7BD2F60BB0_gshared)(___array0, ___value1, method);
}
// System.String YamlDotNet.Samples.Helpers.SampleAttribute::get_Description()
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* SampleAttribute_get_Description_m512B7F186A5F27E873A27EDE67129475F74E1F11_inline (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::WriteLine(System.String,System.Object[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringTestOutputHelper_WriteLine_mE3837EA67D411B8ABBF742B538C073EF5EDBADE1 (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, String_t* ___format0, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___args1, const RuntimeMethod* method) ;
// System.Reflection.ConstructorInfo System.Type::GetConstructor(System.Type[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConstructorInfo_t1B5967EE7E5554272F79F8880183C70AD240EEEB* Type_GetConstructor_m7F0E5E1A61477DE81B35AE780C21FA6830124554 (Type_t* __this, TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___types0, const RuntimeMethod* method) ;
// System.Object System.Reflection.ConstructorInfo::Invoke(System.Object[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* ConstructorInfo_Invoke_m15FDF2B682BD01CC934BE4D314EF2193103CECFE (ConstructorInfo_t1B5967EE7E5554272F79F8880183C70AD240EEEB* __this, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___parameters0, const RuntimeMethod* method) ;
// System.Object System.Reflection.MethodBase::Invoke(System.Object,System.Object[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* MethodBase_Invoke_mEEF3218648F111A8C338001A7804091A0747C826 (MethodBase_t* __this, RuntimeObject* ___obj0, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___parameters1, const RuntimeMethod* method) ;
// System.Void UnityEngine.Debug::Log(System.Object)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Debug_Log_m86567BCF22BBE7809747817453CACA0E41E68219 (RuntimeObject* ___message0, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::Clear()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringTestOutputHelper_Clear_mCA2A3B194BD449A39A02B21AAEA20484309C2815 (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, const RuntimeMethod* method) ;
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringTestOutputHelper__ctor_m230467E77F649FE332A6A42405D4125B3D3F2BFD (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, const RuntimeMethod* method) ;
// System.Void UnityEngine.MonoBehaviour::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void MonoBehaviour__ctor_m592DB0105CA0BC97AA1C5F4AD27B12D68A3B7C1E (MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71* __this, const RuntimeMethod* method) ;
// System.Text.StringBuilder System.Text.StringBuilder::AppendLine()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringBuilder_t* StringBuilder_AppendLine_m3BC704C4E6A8531027D8C9287D0AB2AA0188AC4E (StringBuilder_t* __this, const RuntimeMethod* method) ;
// System.Text.StringBuilder System.Text.StringBuilder::AppendLine(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringBuilder_t* StringBuilder_AppendLine_mF75744CE941C63E33188E22E936B71A24D3CBF88 (StringBuilder_t* __this, String_t* ___value0, const RuntimeMethod* method) ;
// System.Text.StringBuilder System.Text.StringBuilder::AppendFormat(System.String,System.Object[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringBuilder_t* StringBuilder_AppendFormat_m14CB447291E6149BCF32E5E37DA21514BAD9C151 (StringBuilder_t* __this, String_t* ___format0, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___args1, const RuntimeMethod* method) ;
// System.Void System.Text.StringBuilder::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringBuilder__ctor_m1D99713357DE05DAFA296633639DB55F8C30587D (StringBuilder_t* __this, const RuntimeMethod* method) ;
// System.Void System.Attribute::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2 (Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* __this, const RuntimeMethod* method) ;
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
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// System.Void YamlDotNet.Samples.ConvertYamlToJson::.ctor(Xunit.Abstractions.ITestOutputHelper)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ConvertYamlToJson__ctor_mB94F6D3335748F14C71867A69AE7FEE3BCCAFA5E (ConvertYamlToJson_tBB2C23EB2A1FA789E90C44BD4ABF69525E9B74C7* __this, RuntimeObject* ___output0, const RuntimeMethod* method) 
{
	{
		// public ConvertYamlToJson(ITestOutputHelper output)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.output = output;
		RuntimeObject* L_0 = ___output0;
		__this->___output_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___output_0), (void*)L_0);
		// }
		return;
	}
}
// System.Void YamlDotNet.Samples.ConvertYamlToJson::Main()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ConvertYamlToJson_Main_m938C66010F7239346F4036FD10A0E82FFA13BA09 (ConvertYamlToJson_tBB2C23EB2A1FA789E90C44BD4ABF69525E9B74C7* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDeserializer_tBD7A4B4E3C42FE5F64A8A26C27801C7570507EE8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ISerializer_tBA1C2B389CD506D5E3CB5DC013F5C94C126A1136_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral55FDBD1185DCD87EC6576504CCAF9DED9FFDA000);
		s_Il2CppMethodInitialized = true;
	}
	StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* V_0 = NULL;
	RuntimeObject* V_1 = NULL;
	RuntimeObject* V_2 = NULL;
	RuntimeObject* V_3 = NULL;
	String_t* V_4 = NULL;
	{
		//             var r = new StringReader(@"
		// scalar: a scalar
		// sequence:
		//   - one
		//   - two
		// ");
		StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* L_0 = (StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8*)il2cpp_codegen_object_new(StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringReader__ctor_m72556EC1062F49E05CF41B0825AC7FA2DB2A81C0(L_0, _stringLiteral55FDBD1185DCD87EC6576504CCAF9DED9FFDA000, NULL);
		V_0 = L_0;
		// var deserializer = new DeserializerBuilder().Build();
		DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2* L_1 = (DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2*)il2cpp_codegen_object_new(DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2_il2cpp_TypeInfo_var);
		NullCheck(L_1);
		DeserializerBuilder__ctor_mF61FF59EE374A791EE891257388FBB95A1812C6F(L_1, NULL);
		NullCheck(L_1);
		RuntimeObject* L_2;
		L_2 = DeserializerBuilder_Build_mD3E9BBFA306704E1844C1BBC2D41F72734374069(L_1, NULL);
		V_1 = L_2;
		// var yamlObject = deserializer.Deserialize(r);
		RuntimeObject* L_3 = V_1;
		StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* L_4 = V_0;
		NullCheck(L_3);
		RuntimeObject* L_5;
		L_5 = InterfaceFuncInvoker1< RuntimeObject*, TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7* >::Invoke(2 /* System.Object YamlDotNet.Serialization.IDeserializer::Deserialize(System.IO.TextReader) */, IDeserializer_tBD7A4B4E3C42FE5F64A8A26C27801C7570507EE8_il2cpp_TypeInfo_var, L_3, L_4);
		V_2 = L_5;
		// var serializer = new SerializerBuilder()
		//     .JsonCompatible()
		//     .Build();
		SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C* L_6 = (SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C*)il2cpp_codegen_object_new(SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C_il2cpp_TypeInfo_var);
		NullCheck(L_6);
		SerializerBuilder__ctor_mC3F1F264785836360661ADBAF3C3EA09D974EFF0(L_6, NULL);
		NullCheck(L_6);
		SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C* L_7;
		L_7 = SerializerBuilder_JsonCompatible_m45540AA8D0A87ED0AB2AD1EDAF02D11A6C4F91B4(L_6, NULL);
		NullCheck(L_7);
		RuntimeObject* L_8;
		L_8 = SerializerBuilder_Build_m37AF80C3630297349F07BCF78CDFC8D3742DCE6E(L_7, NULL);
		V_3 = L_8;
		// var json = serializer.Serialize(yamlObject);
		RuntimeObject* L_9 = V_3;
		RuntimeObject* L_10 = V_2;
		NullCheck(L_9);
		String_t* L_11;
		L_11 = InterfaceFuncInvoker1< String_t*, RuntimeObject* >::Invoke(1 /* System.String YamlDotNet.Serialization.ISerializer::Serialize(System.Object) */, ISerializer_tBA1C2B389CD506D5E3CB5DC013F5C94C126A1136_il2cpp_TypeInfo_var, L_9, L_10);
		V_4 = L_11;
		// output.WriteLine(json);
		RuntimeObject* L_12 = __this->___output_0;
		String_t* L_13 = V_4;
		NullCheck(L_12);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_12, L_13);
		// }
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
// System.Void YamlDotNet.Samples.DeserializeObjectGraph::.ctor(Xunit.Abstractions.ITestOutputHelper)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DeserializeObjectGraph__ctor_mC0156C9944200DD91DFBC68BF3C611DA9C048AA2 (DeserializeObjectGraph_tFB1288C0A3F2618E3667101B7BA1F28A4F01CC81* __this, RuntimeObject* ___output0, const RuntimeMethod* method) 
{
	{
		// public DeserializeObjectGraph(ITestOutputHelper output)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.output = output;
		RuntimeObject* L_0 = ___output0;
		__this->___output_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___output_0), (void*)L_0);
		// }
		return;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph::Main()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DeserializeObjectGraph_Main_mD2BFEA1452A260E0C3BD2D8A4CFA38D37DAC6869 (DeserializeObjectGraph_tFB1288C0A3F2618E3667101B7BA1F28A4F01CC81* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&BuilderSkeleton_1_WithNamingConvention_mC36449BE7D8903C0DE46705F4CB97C7B34D8BAB3_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&CamelCaseNamingConvention_t12B61C131A233B37C7908239FD79D9CB20E7A9D7_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_Dispose_mD0224D5784331A02E72D2EE1F00EC806D852D377_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_MoveNext_m9DCB4F92A17E8A60784FC19CEA6C2042F94EF358_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_get_Current_mECC16CA0C47DDF96EECC358EC2FCDD3527495F54_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDeserializer_Deserialize_TisOrder_t455515D55F55269AEBF8AD3EF7162547E7AA0A53_m01A0A51FE5225C8C07C0690FC0025BD500C0E56F_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_GetEnumerator_m348416E1E4D61F1D9E285BB9043058B83707653F_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0CDC424E2E41D059A5128706BAF6E4498A03D39B);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral17F4D0F233F7C806CBB7F1E4936F65607B1F1168);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral62FE8FF641FE619F53CF8F5D38A8A6BC7AED19E4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral76F486E2D05B1268235F8E6A197128F313D7B852);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB4B7B6A99BC5C77B775F2D9887AF276A891D9D68);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBE647EF55536A560FF890347C80BB77C55E99DC0);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCB600D75C8BA5A8A7860C5C3ECBDFB0AB9CFA695);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE3E725A290611C2F36B4B1DC53D0D529279F0923);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE7A3075116E53C28B6D5986FA50919EFAD0B1D0B);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralEBBAC010CB9033B6FACD3A10B94A4919AF898822);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralEF4B3E7859FBAABF4652DE2F6675345692C335AA);
		s_Il2CppMethodInitialized = true;
	}
	StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* V_0 = NULL;
	RuntimeObject* V_1 = NULL;
	Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* V_2 = NULL;
	Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2 V_3;
	memset((&V_3), 0, sizeof(V_3));
	OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* V_4 = NULL;
	bool V_5 = false;
	{
		// var input = new StringReader(Document);
		StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* L_0 = (StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8*)il2cpp_codegen_object_new(StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringReader__ctor_m72556EC1062F49E05CF41B0825AC7FA2DB2A81C0(L_0, _stringLiteral0CDC424E2E41D059A5128706BAF6E4498A03D39B, NULL);
		V_0 = L_0;
		// var deserializer = new DeserializerBuilder()
		//     .WithNamingConvention(CamelCaseNamingConvention.Instance)
		//     .Build();
		DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2* L_1 = (DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2*)il2cpp_codegen_object_new(DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2_il2cpp_TypeInfo_var);
		NullCheck(L_1);
		DeserializerBuilder__ctor_mF61FF59EE374A791EE891257388FBB95A1812C6F(L_1, NULL);
		il2cpp_codegen_runtime_class_init_inline(CamelCaseNamingConvention_t12B61C131A233B37C7908239FD79D9CB20E7A9D7_il2cpp_TypeInfo_var);
		RuntimeObject* L_2 = ((CamelCaseNamingConvention_t12B61C131A233B37C7908239FD79D9CB20E7A9D7_StaticFields*)il2cpp_codegen_static_fields_for(CamelCaseNamingConvention_t12B61C131A233B37C7908239FD79D9CB20E7A9D7_il2cpp_TypeInfo_var))->___Instance_0;
		NullCheck(L_1);
		DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2* L_3;
		L_3 = BuilderSkeleton_1_WithNamingConvention_mC36449BE7D8903C0DE46705F4CB97C7B34D8BAB3(L_1, L_2, BuilderSkeleton_1_WithNamingConvention_mC36449BE7D8903C0DE46705F4CB97C7B34D8BAB3_RuntimeMethod_var);
		NullCheck(L_3);
		RuntimeObject* L_4;
		L_4 = DeserializerBuilder_Build_mD3E9BBFA306704E1844C1BBC2D41F72734374069(L_3, NULL);
		V_1 = L_4;
		// var order = deserializer.Deserialize<Order>(input);
		RuntimeObject* L_5 = V_1;
		StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* L_6 = V_0;
		NullCheck(L_5);
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_7;
		L_7 = GenericInterfaceFuncInvoker1< Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53*, TextReader_tB8D43017CB6BE1633E5A86D64E7757366507C1F7* >::Invoke(IDeserializer_Deserialize_TisOrder_t455515D55F55269AEBF8AD3EF7162547E7AA0A53_m01A0A51FE5225C8C07C0690FC0025BD500C0E56F_RuntimeMethod_var, L_5, L_6);
		V_2 = L_7;
		// output.WriteLine("Order");
		RuntimeObject* L_8 = __this->___output_0;
		NullCheck(L_8);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_8, _stringLiteralEF4B3E7859FBAABF4652DE2F6675345692C335AA);
		// output.WriteLine("-----");
		RuntimeObject* L_9 = __this->___output_0;
		NullCheck(L_9);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_9, _stringLiteral17F4D0F233F7C806CBB7F1E4936F65607B1F1168);
		// output.WriteLine();
		RuntimeObject* L_10 = __this->___output_0;
		NullCheck(L_10);
		InterfaceActionInvoker0::Invoke(0 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine() */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_10);
		// foreach (var item in order.Items)
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_11 = V_2;
		NullCheck(L_11);
		List_1_t125631862BDE1BA29850B762D409E23C07F29B40* L_12;
		L_12 = Order_get_Items_m04CBA6598E62087D34F49A24CFD74B42679DAE98_inline(L_11, NULL);
		NullCheck(L_12);
		Enumerator_t2E890ADFD6115F6E2DC83B1BA8A399AF384BC6E2 L_13;
		L_13 = List_1_GetEnumerator_m348416E1E4D61F1D9E285BB9043058B83707653F(L_12, List_1_GetEnumerator_m348416E1E4D61F1D9E285BB9043058B83707653F_RuntimeMethod_var);
		V_3 = L_13;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_00c5:
			{// begin finally (depth: 1)
				Enumerator_Dispose_mD0224D5784331A02E72D2EE1F00EC806D852D377((&V_3), Enumerator_Dispose_mD0224D5784331A02E72D2EE1F00EC806D852D377_RuntimeMethod_var);
				return;
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_00ba_1;
			}

IL_0066_1:
			{
				// foreach (var item in order.Items)
				OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* L_14;
				L_14 = Enumerator_get_Current_mECC16CA0C47DDF96EECC358EC2FCDD3527495F54_inline((&V_3), Enumerator_get_Current_mECC16CA0C47DDF96EECC358EC2FCDD3527495F54_RuntimeMethod_var);
				V_4 = L_14;
				// output.WriteLine("{0}\t{1}\t{2}\t{3}", item.PartNo, item.Quantity, item.Price, item.Descrip);
				RuntimeObject* L_15 = __this->___output_0;
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_16 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)4);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_17 = L_16;
				OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* L_18 = V_4;
				NullCheck(L_18);
				String_t* L_19;
				L_19 = OrderItem_get_PartNo_m27D22430CD8BED232A06E432F42E536DB38F3A61_inline(L_18, NULL);
				NullCheck(L_17);
				ArrayElementTypeCheck (L_17, L_19);
				(L_17)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_19);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_20 = L_17;
				OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* L_21 = V_4;
				NullCheck(L_21);
				int32_t L_22;
				L_22 = OrderItem_get_Quantity_mFF50B28BBA27E7FC844280AE7E024B8FF59E8164_inline(L_21, NULL);
				int32_t L_23 = L_22;
				RuntimeObject* L_24 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_23);
				NullCheck(L_20);
				ArrayElementTypeCheck (L_20, L_24);
				(L_20)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_24);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_25 = L_20;
				OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* L_26 = V_4;
				NullCheck(L_26);
				Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_27;
				L_27 = OrderItem_get_Price_mFD13B646F706F3E18735C3BAADB775C06AC88BFC_inline(L_26, NULL);
				Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_28 = L_27;
				RuntimeObject* L_29 = Box(Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F_il2cpp_TypeInfo_var, &L_28);
				NullCheck(L_25);
				ArrayElementTypeCheck (L_25, L_29);
				(L_25)->SetAt(static_cast<il2cpp_array_size_t>(2), (RuntimeObject*)L_29);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_30 = L_25;
				OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* L_31 = V_4;
				NullCheck(L_31);
				String_t* L_32;
				L_32 = OrderItem_get_Descrip_mC37419903157E1BBC8BF76B362D32D3E04A0D620_inline(L_31, NULL);
				NullCheck(L_30);
				ArrayElementTypeCheck (L_30, L_32);
				(L_30)->SetAt(static_cast<il2cpp_array_size_t>(3), (RuntimeObject*)L_32);
				NullCheck(L_15);
				InterfaceActionInvoker2< String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* >::Invoke(2 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String,System.Object[]) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_15, _stringLiteralBE647EF55536A560FF890347C80BB77C55E99DC0, L_30);
			}

IL_00ba_1:
			{
				// foreach (var item in order.Items)
				bool L_33;
				L_33 = Enumerator_MoveNext_m9DCB4F92A17E8A60784FC19CEA6C2042F94EF358((&V_3), Enumerator_MoveNext_m9DCB4F92A17E8A60784FC19CEA6C2042F94EF358_RuntimeMethod_var);
				if (L_33)
				{
					goto IL_0066_1;
				}
			}
			{
				goto IL_00d4;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_00d4:
	{
		// output.WriteLine();
		RuntimeObject* L_34 = __this->___output_0;
		NullCheck(L_34);
		InterfaceActionInvoker0::Invoke(0 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine() */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_34);
		// output.WriteLine("Shipping");
		RuntimeObject* L_35 = __this->___output_0;
		NullCheck(L_35);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_35, _stringLiteralE3E725A290611C2F36B4B1DC53D0D529279F0923);
		// output.WriteLine("--------");
		RuntimeObject* L_36 = __this->___output_0;
		NullCheck(L_36);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_36, _stringLiteral76F486E2D05B1268235F8E6A197128F313D7B852);
		// output.WriteLine();
		RuntimeObject* L_37 = __this->___output_0;
		NullCheck(L_37);
		InterfaceActionInvoker0::Invoke(0 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine() */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_37);
		// output.WriteLine(order.ShipTo.Street);
		RuntimeObject* L_38 = __this->___output_0;
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_39 = V_2;
		NullCheck(L_39);
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_40;
		L_40 = Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline(L_39, NULL);
		NullCheck(L_40);
		String_t* L_41;
		L_41 = Address_get_Street_mE2821ED2C5BC33B0F9643B86B64C5204930B37A7_inline(L_40, NULL);
		NullCheck(L_38);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_38, L_41);
		// output.WriteLine(order.ShipTo.City);
		RuntimeObject* L_42 = __this->___output_0;
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_43 = V_2;
		NullCheck(L_43);
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_44;
		L_44 = Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline(L_43, NULL);
		NullCheck(L_44);
		String_t* L_45;
		L_45 = Address_get_City_m8BADE3101BEEDDFFF69CC7FDE49BCBBB25B476F4_inline(L_44, NULL);
		NullCheck(L_42);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_42, L_45);
		// output.WriteLine(order.ShipTo.State);
		RuntimeObject* L_46 = __this->___output_0;
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_47 = V_2;
		NullCheck(L_47);
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_48;
		L_48 = Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline(L_47, NULL);
		NullCheck(L_48);
		String_t* L_49;
		L_49 = Address_get_State_m265F188BA17A5D8CAC8A3B89A114FC326E994908_inline(L_48, NULL);
		NullCheck(L_46);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_46, L_49);
		// output.WriteLine();
		RuntimeObject* L_50 = __this->___output_0;
		NullCheck(L_50);
		InterfaceActionInvoker0::Invoke(0 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine() */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_50);
		// output.WriteLine("Billing");
		RuntimeObject* L_51 = __this->___output_0;
		NullCheck(L_51);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_51, _stringLiteral62FE8FF641FE619F53CF8F5D38A8A6BC7AED19E4);
		// output.WriteLine("-------");
		RuntimeObject* L_52 = __this->___output_0;
		NullCheck(L_52);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_52, _stringLiteralB4B7B6A99BC5C77B775F2D9887AF276A891D9D68);
		// output.WriteLine();
		RuntimeObject* L_53 = __this->___output_0;
		NullCheck(L_53);
		InterfaceActionInvoker0::Invoke(0 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine() */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_53);
		// if (order.BillTo == order.ShipTo)
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_54 = V_2;
		NullCheck(L_54);
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_55;
		L_55 = Order_get_BillTo_m9263CDEAEC8DA36B2104C14B612CF8B3A2424EFC_inline(L_54, NULL);
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_56 = V_2;
		NullCheck(L_56);
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_57;
		L_57 = Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline(L_56, NULL);
		V_5 = (bool)((((RuntimeObject*)(Address_tD85CB003A9782A8A69C513245758DEC28AA76024*)L_55) == ((RuntimeObject*)(Address_tD85CB003A9782A8A69C513245758DEC28AA76024*)L_57))? 1 : 0);
		bool L_58 = V_5;
		if (!L_58)
		{
			goto IL_01b6;
		}
	}
	{
		// output.WriteLine("*same as shipping address*");
		RuntimeObject* L_59 = __this->___output_0;
		NullCheck(L_59);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_59, _stringLiteralE7A3075116E53C28B6D5986FA50919EFAD0B1D0B);
		goto IL_01fd;
	}

IL_01b6:
	{
		// output.WriteLine(order.ShipTo.Street);
		RuntimeObject* L_60 = __this->___output_0;
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_61 = V_2;
		NullCheck(L_61);
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_62;
		L_62 = Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline(L_61, NULL);
		NullCheck(L_62);
		String_t* L_63;
		L_63 = Address_get_Street_mE2821ED2C5BC33B0F9643B86B64C5204930B37A7_inline(L_62, NULL);
		NullCheck(L_60);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_60, L_63);
		// output.WriteLine(order.ShipTo.City);
		RuntimeObject* L_64 = __this->___output_0;
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_65 = V_2;
		NullCheck(L_65);
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_66;
		L_66 = Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline(L_65, NULL);
		NullCheck(L_66);
		String_t* L_67;
		L_67 = Address_get_City_m8BADE3101BEEDDFFF69CC7FDE49BCBBB25B476F4_inline(L_66, NULL);
		NullCheck(L_64);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_64, L_67);
		// output.WriteLine(order.ShipTo.State);
		RuntimeObject* L_68 = __this->___output_0;
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_69 = V_2;
		NullCheck(L_69);
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_70;
		L_70 = Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline(L_69, NULL);
		NullCheck(L_70);
		String_t* L_71;
		L_71 = Address_get_State_m265F188BA17A5D8CAC8A3B89A114FC326E994908_inline(L_70, NULL);
		NullCheck(L_68);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_68, L_71);
	}

IL_01fd:
	{
		// output.WriteLine();
		RuntimeObject* L_72 = __this->___output_0;
		NullCheck(L_72);
		InterfaceActionInvoker0::Invoke(0 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine() */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_72);
		// output.WriteLine("Delivery instructions");
		RuntimeObject* L_73 = __this->___output_0;
		NullCheck(L_73);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_73, _stringLiteralCB600D75C8BA5A8A7860C5C3ECBDFB0AB9CFA695);
		// output.WriteLine("---------------------");
		RuntimeObject* L_74 = __this->___output_0;
		NullCheck(L_74);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_74, _stringLiteralEBBAC010CB9033B6FACD3A10B94A4919AF898822);
		// output.WriteLine();
		RuntimeObject* L_75 = __this->___output_0;
		NullCheck(L_75);
		InterfaceActionInvoker0::Invoke(0 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine() */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_75);
		// output.WriteLine(order.SpecialDelivery);
		RuntimeObject* L_76 = __this->___output_0;
		Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* L_77 = V_2;
		NullCheck(L_77);
		String_t* L_78;
		L_78 = Order_get_SpecialDelivery_m29487155B8D9FD5468C08389480733E3E179D94B_inline(L_77, NULL);
		NullCheck(L_76);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_76, L_78);
		// }
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
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Order::get_Receipt()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Order_get_Receipt_m4DD0CFC3221D9AB969DC063DFBAA84B9B1EDC213 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public string Receipt { get; set; }
		String_t* L_0 = __this->___U3CReceiptU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Order::set_Receipt(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Order_set_Receipt_mC5F17461E8744BC0480DDAB4593D70DA1840E4F5 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string Receipt { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CReceiptU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CReceiptU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.DateTime YamlDotNet.Samples.DeserializeObjectGraph/Order::get_Date()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D Order_get_Date_mB75B064C981D215FF8B8527D629824713AE3F393 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public DateTime Date { get; set; }
		DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D L_0 = __this->___U3CDateU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Order::set_Date(System.DateTime)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Order_set_Date_m0D2CFA136AE30D96A92F362452516254C9E04100 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___value0, const RuntimeMethod* method) 
{
	{
		// public DateTime Date { get; set; }
		DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D L_0 = ___value0;
		__this->___U3CDateU3Ek__BackingField_1 = L_0;
		return;
	}
}
// YamlDotNet.Samples.DeserializeObjectGraph/Customer YamlDotNet.Samples.DeserializeObjectGraph/Order::get_Customer()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* Order_get_Customer_mA37E1988BFE1FDF44C66E19120F8823869BE0CA2 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public Customer Customer { get; set; }
		Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* L_0 = __this->___U3CCustomerU3Ek__BackingField_2;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Order::set_Customer(YamlDotNet.Samples.DeserializeObjectGraph/Customer)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Order_set_Customer_mF3D84DA69C28C896CE4BA0D894C171D331ED8958 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* ___value0, const RuntimeMethod* method) 
{
	{
		// public Customer Customer { get; set; }
		Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* L_0 = ___value0;
		__this->___U3CCustomerU3Ek__BackingField_2 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CCustomerU3Ek__BackingField_2), (void*)L_0);
		return;
	}
}
// System.Collections.Generic.List`1<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem> YamlDotNet.Samples.DeserializeObjectGraph/Order::get_Items()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR List_1_t125631862BDE1BA29850B762D409E23C07F29B40* Order_get_Items_m04CBA6598E62087D34F49A24CFD74B42679DAE98 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public List<OrderItem> Items { get; set; }
		List_1_t125631862BDE1BA29850B762D409E23C07F29B40* L_0 = __this->___U3CItemsU3Ek__BackingField_3;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Order::set_Items(System.Collections.Generic.List`1<YamlDotNet.Samples.DeserializeObjectGraph/OrderItem>)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Order_set_Items_mD4486A91102D7F14766AAABE7F7254E5EC1591BF (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, List_1_t125631862BDE1BA29850B762D409E23C07F29B40* ___value0, const RuntimeMethod* method) 
{
	{
		// public List<OrderItem> Items { get; set; }
		List_1_t125631862BDE1BA29850B762D409E23C07F29B40* L_0 = ___value0;
		__this->___U3CItemsU3Ek__BackingField_3 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CItemsU3Ek__BackingField_3), (void*)L_0);
		return;
	}
}
// YamlDotNet.Samples.DeserializeObjectGraph/Address YamlDotNet.Samples.DeserializeObjectGraph/Order::get_BillTo()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Address_tD85CB003A9782A8A69C513245758DEC28AA76024* Order_get_BillTo_m9263CDEAEC8DA36B2104C14B612CF8B3A2424EFC (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public Address BillTo { get; set; }
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_0 = __this->___U3CBillToU3Ek__BackingField_4;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Order::set_BillTo(YamlDotNet.Samples.DeserializeObjectGraph/Address)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Order_set_BillTo_mCCCDB8430BEC6C5B08AF176983E5D540DFD87422 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, Address_tD85CB003A9782A8A69C513245758DEC28AA76024* ___value0, const RuntimeMethod* method) 
{
	{
		// public Address BillTo { get; set; }
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_0 = ___value0;
		__this->___U3CBillToU3Ek__BackingField_4 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CBillToU3Ek__BackingField_4), (void*)L_0);
		return;
	}
}
// YamlDotNet.Samples.DeserializeObjectGraph/Address YamlDotNet.Samples.DeserializeObjectGraph/Order::get_ShipTo()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Address_tD85CB003A9782A8A69C513245758DEC28AA76024* Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public Address ShipTo { get; set; }
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_0 = __this->___U3CShipToU3Ek__BackingField_5;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Order::set_ShipTo(YamlDotNet.Samples.DeserializeObjectGraph/Address)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Order_set_ShipTo_m3B1CC184E5BD2F7781EF5354EA1A1FAE7C8D733F (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, Address_tD85CB003A9782A8A69C513245758DEC28AA76024* ___value0, const RuntimeMethod* method) 
{
	{
		// public Address ShipTo { get; set; }
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_0 = ___value0;
		__this->___U3CShipToU3Ek__BackingField_5 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CShipToU3Ek__BackingField_5), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Order::get_SpecialDelivery()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Order_get_SpecialDelivery_m29487155B8D9FD5468C08389480733E3E179D94B (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public string SpecialDelivery { get; set; }
		String_t* L_0 = __this->___U3CSpecialDeliveryU3Ek__BackingField_6;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Order::set_SpecialDelivery(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Order_set_SpecialDelivery_m93D22436E44DDD77E1E54F6AF2C98F6CB3632142 (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string SpecialDelivery { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CSpecialDeliveryU3Ek__BackingField_6 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CSpecialDeliveryU3Ek__BackingField_6), (void*)L_0);
		return;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Order::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Order__ctor_m098CF61FA36F2708D8E2B3A0F5BA87D7E028745B (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
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
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Customer::get_Given()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Customer_get_Given_mA81A35EB9717C39EB92B9A5889C9FB03299DAEA1 (Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* __this, const RuntimeMethod* method) 
{
	{
		// public string Given { get; set; }
		String_t* L_0 = __this->___U3CGivenU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Customer::set_Given(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Customer_set_Given_m62FB90E6FCFD8493407E1EBB009E4BFFCF8E6077 (Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string Given { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CGivenU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CGivenU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Customer::get_Family()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Customer_get_Family_m2CC37161FF82AAA381B4FA0CDFAEE1025BA4771D (Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* __this, const RuntimeMethod* method) 
{
	{
		// public string Family { get; set; }
		String_t* L_0 = __this->___U3CFamilyU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Customer::set_Family(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Customer_set_Family_m864F30465C2FDF440540DE55147D4FFD2DBC85C5 (Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string Family { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CFamilyU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CFamilyU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Customer::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Customer__ctor_mAB68E2C97B0DB02C4BDB5FAF6E4DC3E73A926C8E (Customer_t3A696A73AE9D0CD1502A51269581AAE969303040* __this, const RuntimeMethod* method) 
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
// System.String YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::get_PartNo()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* OrderItem_get_PartNo_m27D22430CD8BED232A06E432F42E536DB38F3A61 (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
{
	{
		// public string PartNo { get; set; }
		String_t* L_0 = __this->___U3CPartNoU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::set_PartNo(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OrderItem_set_PartNo_mF0F8DFD4FC25FDD173F07FDE271BC75F9AD5BD07 (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string PartNo { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CPartNoU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CPartNoU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::get_Descrip()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* OrderItem_get_Descrip_mC37419903157E1BBC8BF76B362D32D3E04A0D620 (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
{
	{
		// public string Descrip { get; set; }
		String_t* L_0 = __this->___U3CDescripU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::set_Descrip(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OrderItem_set_Descrip_m05D48097CBCDB32605985A7FA84E34D113BFD290 (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string Descrip { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CDescripU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CDescripU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
// System.Decimal YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::get_Price()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F OrderItem_get_Price_mFD13B646F706F3E18735C3BAADB775C06AC88BFC (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
{
	{
		// public decimal Price { get; set; }
		Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_0 = __this->___U3CPriceU3Ek__BackingField_2;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::set_Price(System.Decimal)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OrderItem_set_Price_mFBE9A6DBBF1E7F7670B62DF0D7175A99618CA1EF (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___value0, const RuntimeMethod* method) 
{
	{
		// public decimal Price { get; set; }
		Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_0 = ___value0;
		__this->___U3CPriceU3Ek__BackingField_2 = L_0;
		return;
	}
}
// System.Int32 YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::get_Quantity()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t OrderItem_get_Quantity_mFF50B28BBA27E7FC844280AE7E024B8FF59E8164 (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
{
	{
		// public int Quantity { get; set; }
		int32_t L_0 = __this->___U3CQuantityU3Ek__BackingField_3;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::set_Quantity(System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OrderItem_set_Quantity_m7B57680F66EF64696755ED5AE3CE289A4E995B1E (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, int32_t ___value0, const RuntimeMethod* method) 
{
	{
		// public int Quantity { get; set; }
		int32_t L_0 = ___value0;
		__this->___U3CQuantityU3Ek__BackingField_3 = L_0;
		return;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/OrderItem::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void OrderItem__ctor_m225C55A2AA1CAC9CFDCF7CD1BF023854DDA12144 (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
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
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::get_Street()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Address_get_Street_mE2821ED2C5BC33B0F9643B86B64C5204930B37A7 (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) 
{
	{
		// public string Street { get; set; }
		String_t* L_0 = __this->___U3CStreetU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Address::set_Street(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address_set_Street_mD1B624738FE3DA6FB0115E31377422764CA8740B (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string Street { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CStreetU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CStreetU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::get_City()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Address_get_City_m8BADE3101BEEDDFFF69CC7FDE49BCBBB25B476F4 (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) 
{
	{
		// public string City { get; set; }
		String_t* L_0 = __this->___U3CCityU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Address::set_City(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address_set_City_mA9955B40DEA08CC6F46FCAE5C9FF44C41FEA296E (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string City { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CCityU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CCityU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.DeserializeObjectGraph/Address::get_State()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Address_get_State_m265F188BA17A5D8CAC8A3B89A114FC326E994908 (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) 
{
	{
		// public string State { get; set; }
		String_t* L_0 = __this->___U3CStateU3Ek__BackingField_2;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Address::set_State(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address_set_State_m9412675D32D55F99A0D9A8C759BA5FEAD207E456 (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string State { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CStateU3Ek__BackingField_2 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CStateU3Ek__BackingField_2), (void*)L_0);
		return;
	}
}
// System.Void YamlDotNet.Samples.DeserializeObjectGraph/Address::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address__ctor_mBF5C45C77D805110F3939945494A314E92DBF60C (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) 
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
// System.Void YamlDotNet.Samples.DeserializingMultipleDocuments::.ctor(Xunit.Abstractions.ITestOutputHelper)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DeserializingMultipleDocuments__ctor_mB9DAA68288FA86F2C0784D97231C505708CF64C7 (DeserializingMultipleDocuments_tEF21A0E880B34920A6FFE552F54864A3948E0CA1* __this, RuntimeObject* ___output0, const RuntimeMethod* method) 
{
	{
		// public DeserializingMultipleDocuments(ITestOutputHelper output)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.output = output;
		RuntimeObject* L_0 = ___output0;
		__this->___output_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___output_0), (void*)L_0);
		// }
		return;
	}
}
// System.Void YamlDotNet.Samples.DeserializingMultipleDocuments::Main()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DeserializingMultipleDocuments_Main_mC0E30205B903F08C0FDE8774260E05817F206FC0 (DeserializingMultipleDocuments_tEF21A0E880B34920A6FFE552F54864A3948E0CA1* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDeserializer_Deserialize_TisList_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_mA7A32850F13BAB7E82BD96CB70EC0416B5241829_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ParserExtensions_Accept_TisDocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F_m583C79A9FA702726EDF18F5F87C013A0F6353566_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ParserExtensions_Consume_TisStreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18_mA969137CC45206737455EBA404755F3A0023B62C_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB7B50DE055D313D7C46CD4D2452069DA3871AB41);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralD65C2004FD674D31FC063C3BF88CF632907BE277);
		s_Il2CppMethodInitialized = true;
	}
	StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* V_0 = NULL;
	RuntimeObject* V_1 = NULL;
	Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4* V_2 = NULL;
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* V_3 = NULL;
	Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1 V_4;
	memset((&V_4), 0, sizeof(V_4));
	String_t* V_5 = NULL;
	bool V_6 = false;
	DocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F* V_7 = NULL;
	{
		// var input = new StringReader(Document);
		StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* L_0 = (StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8*)il2cpp_codegen_object_new(StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringReader__ctor_m72556EC1062F49E05CF41B0825AC7FA2DB2A81C0(L_0, _stringLiteralB7B50DE055D313D7C46CD4D2452069DA3871AB41, NULL);
		V_0 = L_0;
		// var deserializer = new DeserializerBuilder().Build();
		DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2* L_1 = (DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2*)il2cpp_codegen_object_new(DeserializerBuilder_tF945CF089023727952D8A6DFED03E76BF53E19E2_il2cpp_TypeInfo_var);
		NullCheck(L_1);
		DeserializerBuilder__ctor_mF61FF59EE374A791EE891257388FBB95A1812C6F(L_1, NULL);
		NullCheck(L_1);
		RuntimeObject* L_2;
		L_2 = DeserializerBuilder_Build_mD3E9BBFA306704E1844C1BBC2D41F72734374069(L_1, NULL);
		V_1 = L_2;
		// var parser = new Parser(input);
		StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* L_3 = V_0;
		Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4* L_4 = (Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4*)il2cpp_codegen_object_new(Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4_il2cpp_TypeInfo_var);
		NullCheck(L_4);
		Parser__ctor_m1A5F0024DBAD7675418CF593BFB634C95DF6D18B(L_4, L_3, NULL);
		V_2 = L_4;
		// parser.Consume<StreamStart>();
		Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4* L_5 = V_2;
		StreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18* L_6;
		L_6 = ParserExtensions_Consume_TisStreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18_mA969137CC45206737455EBA404755F3A0023B62C(L_5, ParserExtensions_Consume_TisStreamStart_t3E7060B1BD6A42973E6540889CA212BBB121AF18_mA969137CC45206737455EBA404755F3A0023B62C_RuntimeMethod_var);
		goto IL_0080;
	}

IL_0027:
	{
		// var doc = deserializer.Deserialize<List<string>>(parser);
		RuntimeObject* L_7 = V_1;
		Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4* L_8 = V_2;
		NullCheck(L_7);
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_9;
		L_9 = GenericInterfaceFuncInvoker1< List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*, RuntimeObject* >::Invoke(IDeserializer_Deserialize_TisList_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_mA7A32850F13BAB7E82BD96CB70EC0416B5241829_RuntimeMethod_var, L_7, L_8);
		V_3 = L_9;
		// output.WriteLine("## Document");
		RuntimeObject* L_10 = __this->___output_0;
		NullCheck(L_10);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_10, _stringLiteralD65C2004FD674D31FC063C3BF88CF632907BE277);
		// foreach (var item in doc)
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_11 = V_3;
		NullCheck(L_11);
		Enumerator_tA7A4B718FE1ED1D87565680D8C8195EC8AEAB3D1 L_12;
		L_12 = List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D(L_11, List_1_GetEnumerator_m7692B5F182858B7D5C72C920D09AD48738D1E70D_RuntimeMethod_var);
		V_4 = L_12;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0070:
			{// begin finally (depth: 1)
				Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7((&V_4), Enumerator_Dispose_m592BCCE7B7933454DED2130C810F059F8D85B1D7_RuntimeMethod_var);
				return;
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_0065_1;
			}

IL_004c_1:
			{
				// foreach (var item in doc)
				String_t* L_13;
				L_13 = Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_inline((&V_4), Enumerator_get_Current_m143541DD8FBCD313E7554EA738FA813B8F4DB11A_RuntimeMethod_var);
				V_5 = L_13;
				// output.WriteLine(item);
				RuntimeObject* L_14 = __this->___output_0;
				String_t* L_15 = V_5;
				NullCheck(L_14);
				InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_14, L_15);
			}

IL_0065_1:
			{
				// foreach (var item in doc)
				bool L_16;
				L_16 = Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED((&V_4), Enumerator_MoveNext_mDB47EEC4531D33B9C33FD2E70BA15E1535A0F3ED_RuntimeMethod_var);
				if (L_16)
				{
					goto IL_004c_1;
				}
			}
			{
				goto IL_007f;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_007f:
	{
	}

IL_0080:
	{
		// while (parser.Accept<DocumentStart>(out var _))
		Parser_t8D535DC0CB180A5C643FE6902932BC99A618DEF4* L_17 = V_2;
		bool L_18;
		L_18 = ParserExtensions_Accept_TisDocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F_m583C79A9FA702726EDF18F5F87C013A0F6353566(L_17, (&V_7), ParserExtensions_Accept_TisDocumentStart_t6BD38591480EB8184ACF7EEC1A8F965B117FF73F_m583C79A9FA702726EDF18F5F87C013A0F6353566_RuntimeMethod_var);
		V_6 = L_18;
		bool L_19 = V_6;
		if (L_19)
		{
			goto IL_0027;
		}
	}
	{
		// }
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
// System.Void YamlDotNet.Samples.LoadingAYamlStream::.ctor(Xunit.Abstractions.ITestOutputHelper)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void LoadingAYamlStream__ctor_m591D33A520DF052BEC8E828D4FD0E58105F6A4C1 (LoadingAYamlStream_tB3542F174EAAE1ADEE5AF9F9D0ED1378B9ED8A75* __this, RuntimeObject* ___output0, const RuntimeMethod* method) 
{
	{
		// public LoadingAYamlStream(ITestOutputHelper output)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.output = output;
		RuntimeObject* L_0 = ___output0;
		__this->___output_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___output_0), (void*)L_0);
		// }
		return;
	}
}
// System.Void YamlDotNet.Samples.LoadingAYamlStream::Main()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void LoadingAYamlStream_Main_mAE0D9DE961F0888EE8F8863C8535947E681FB173 (LoadingAYamlStream_tB3542F174EAAE1ADEE5AF9F9D0ED1378B9ED8A75* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDictionary_2_t27535D075559E1DDCA209D995D55F8D1942BFFDB_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerable_1_t1697387F8171A07386B91936BF35A2B40EBB7508_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerator_1_tC3AB957AEB9EF9C51A822113B42129D5A59E7C97_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerator_1_tDC519E97395FDC74301B7D8B9B4C483E428B6E7A_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IList_1_t81A6FC3C71AC30CAB39D6D83169B2BBD59B37452_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&KeyValuePair_2_get_Key_mBB9F6BBA53ADA88314F65DD538C0FA5E7C50425C_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral34564CCF416EA1A7EB32DF54E7A91E201275ADFD);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral47772B3950A6EC25FA3C56B90D0D638D84BA85C9);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral7069C7F1D4004126C126503625BAF9ED9D1B55A6);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralF300D2310959AF105732D339376803869D9B2B91);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralFB7070F86EF8CC6601805BCBB939D6865BC62D1F);
		s_Il2CppMethodInitialized = true;
	}
	StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* V_0 = NULL;
	YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30* V_1 = NULL;
	YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687* V_2 = NULL;
	YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC* V_3 = NULL;
	RuntimeObject* V_4 = NULL;
	KeyValuePair_2_t5C963D5768B7B0EF500458A15140E7DDB438E997 V_5;
	memset((&V_5), 0, sizeof(V_5));
	RuntimeObject* V_6 = NULL;
	YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687* V_7 = NULL;
	{
		// var input = new StringReader(Document);
		StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* L_0 = (StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8*)il2cpp_codegen_object_new(StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringReader__ctor_m72556EC1062F49E05CF41B0825AC7FA2DB2A81C0(L_0, _stringLiteral47772B3950A6EC25FA3C56B90D0D638D84BA85C9, NULL);
		V_0 = L_0;
		// var yaml = new YamlStream();
		YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30* L_1 = (YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30*)il2cpp_codegen_object_new(YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30_il2cpp_TypeInfo_var);
		NullCheck(L_1);
		YamlStream__ctor_mB02CD1E92A127D645E6DA520F903D67A2B9E121E(L_1, NULL);
		V_1 = L_1;
		// yaml.Load(input);
		YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30* L_2 = V_1;
		StringReader_t1A336148FF22A9584E759A9D720CC96C23E35DD8* L_3 = V_0;
		NullCheck(L_2);
		YamlStream_Load_m9D04AEC580EFE7E19FD5AA94054CE651F6346E0F(L_2, L_3, NULL);
		// var mapping =
		//     (YamlMappingNode)yaml.Documents[0].RootNode;
		YamlStream_t26AEA4D71AD2BAB3CA27735BB2909C2395EDEC30* L_4 = V_1;
		NullCheck(L_4);
		RuntimeObject* L_5;
		L_5 = YamlStream_get_Documents_mA9FE379D07185E210603717BE7DF5E78CC691B93(L_4, NULL);
		NullCheck(L_5);
		YamlDocument_tF61A99B79C0F9627DA9492381E0D86BF934CBFEE* L_6;
		L_6 = InterfaceFuncInvoker1< YamlDocument_tF61A99B79C0F9627DA9492381E0D86BF934CBFEE*, int32_t >::Invoke(0 /* T System.Collections.Generic.IList`1<YamlDotNet.RepresentationModel.YamlDocument>::get_Item(System.Int32) */, IList_1_t81A6FC3C71AC30CAB39D6D83169B2BBD59B37452_il2cpp_TypeInfo_var, L_5, 0);
		NullCheck(L_6);
		YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* L_7;
		L_7 = YamlDocument_get_RootNode_mFDA2A8A64BF409D91B71A3F0C1C96722AF279D30_inline(L_6, NULL);
		V_2 = ((YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687*)CastclassSealed((RuntimeObject*)L_7, YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687_il2cpp_TypeInfo_var));
		// foreach (var entry in mapping.Children)
		YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687* L_8 = V_2;
		NullCheck(L_8);
		RuntimeObject* L_9;
		L_9 = YamlMappingNode_get_Children_m422A37EDF7D14EDE2AA4B23D6CE7AA440C5C823D(L_8, NULL);
		NullCheck(L_9);
		RuntimeObject* L_10;
		L_10 = InterfaceFuncInvoker0< RuntimeObject* >::Invoke(0 /* System.Collections.Generic.IEnumerator`1<T> System.Collections.Generic.IEnumerable`1<System.Collections.Generic.KeyValuePair`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode>>::GetEnumerator() */, IEnumerable_1_t1697387F8171A07386B91936BF35A2B40EBB7508_il2cpp_TypeInfo_var, L_9);
		V_4 = L_10;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0074:
			{// begin finally (depth: 1)
				{
					RuntimeObject* L_11 = V_4;
					if (!L_11)
					{
						goto IL_0080;
					}
				}
				{
					RuntimeObject* L_12 = V_4;
					NullCheck(L_12);
					InterfaceActionInvoker0::Invoke(0 /* System.Void System.IDisposable::Dispose() */, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_12);
				}

IL_0080:
				{
					return;
				}
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_0069_1;
			}

IL_0041_1:
			{
				// foreach (var entry in mapping.Children)
				RuntimeObject* L_13 = V_4;
				NullCheck(L_13);
				KeyValuePair_2_t5C963D5768B7B0EF500458A15140E7DDB438E997 L_14;
				L_14 = InterfaceFuncInvoker0< KeyValuePair_2_t5C963D5768B7B0EF500458A15140E7DDB438E997 >::Invoke(0 /* T System.Collections.Generic.IEnumerator`1<System.Collections.Generic.KeyValuePair`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode>>::get_Current() */, IEnumerator_1_tC3AB957AEB9EF9C51A822113B42129D5A59E7C97_il2cpp_TypeInfo_var, L_13);
				V_5 = L_14;
				// output.WriteLine(((YamlScalarNode)entry.Key).Value);
				RuntimeObject* L_15 = __this->___output_0;
				YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* L_16;
				L_16 = KeyValuePair_2_get_Key_mBB9F6BBA53ADA88314F65DD538C0FA5E7C50425C_inline((&V_5), KeyValuePair_2_get_Key_mBB9F6BBA53ADA88314F65DD538C0FA5E7C50425C_RuntimeMethod_var);
				NullCheck(((YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648*)CastclassSealed((RuntimeObject*)L_16, YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648_il2cpp_TypeInfo_var)));
				String_t* L_17;
				L_17 = YamlScalarNode_get_Value_m615F96CDDF044477CF47679024333453EEF98711_inline(((YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648*)CastclassSealed((RuntimeObject*)L_16, YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648_il2cpp_TypeInfo_var)), NULL);
				NullCheck(L_15);
				InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_15, L_17);
			}

IL_0069_1:
			{
				// foreach (var entry in mapping.Children)
				RuntimeObject* L_18 = V_4;
				NullCheck(L_18);
				bool L_19;
				L_19 = InterfaceFuncInvoker0< bool >::Invoke(0 /* System.Boolean System.Collections.IEnumerator::MoveNext() */, IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var, L_18);
				if (L_19)
				{
					goto IL_0041_1;
				}
			}
			{
				goto IL_0081;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0081:
	{
		// var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("items")];
		YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687* L_20 = V_2;
		NullCheck(L_20);
		RuntimeObject* L_21;
		L_21 = YamlMappingNode_get_Children_m422A37EDF7D14EDE2AA4B23D6CE7AA440C5C823D(L_20, NULL);
		YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648* L_22 = (YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648*)il2cpp_codegen_object_new(YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648_il2cpp_TypeInfo_var);
		NullCheck(L_22);
		YamlScalarNode__ctor_mCF0C9B856F9F2EBDEA48475C7D6864C36A96DAC4(L_22, _stringLiteralF300D2310959AF105732D339376803869D9B2B91, NULL);
		NullCheck(L_21);
		YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* L_23;
		L_23 = InterfaceFuncInvoker1< YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA*, YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* >::Invoke(0 /* TValue System.Collections.Generic.IDictionary`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode>::get_Item(TKey) */, IDictionary_2_t27535D075559E1DDCA209D995D55F8D1942BFFDB_il2cpp_TypeInfo_var, L_21, L_22);
		V_3 = ((YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC*)CastclassSealed((RuntimeObject*)L_23, YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC_il2cpp_TypeInfo_var));
		// foreach (YamlMappingNode item in items)
		YamlSequenceNode_t7AF207638CAC2C43D4671760739B1AD1D5B369CC* L_24 = V_3;
		NullCheck(L_24);
		RuntimeObject* L_25;
		L_25 = YamlSequenceNode_GetEnumerator_m542190AA8E38FE2F1BF41B3473402A1BCCCA6ED5(L_24, NULL);
		V_6 = L_25;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_010b:
			{// begin finally (depth: 1)
				{
					RuntimeObject* L_26 = V_6;
					if (!L_26)
					{
						goto IL_0117;
					}
				}
				{
					RuntimeObject* L_27 = V_6;
					NullCheck(L_27);
					InterfaceActionInvoker0::Invoke(0 /* System.Void System.IDisposable::Dispose() */, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_27);
				}

IL_0117:
				{
					return;
				}
			}// end finally (depth: 1)
		});
		try
		{// begin try (depth: 1)
			{
				goto IL_0100_1;
			}

IL_00a7_1:
			{
				// foreach (YamlMappingNode item in items)
				RuntimeObject* L_28 = V_6;
				NullCheck(L_28);
				YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* L_29;
				L_29 = InterfaceFuncInvoker0< YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* >::Invoke(0 /* T System.Collections.Generic.IEnumerator`1<YamlDotNet.RepresentationModel.YamlNode>::get_Current() */, IEnumerator_1_tDC519E97395FDC74301B7D8B9B4C483E428B6E7A_il2cpp_TypeInfo_var, L_28);
				V_7 = ((YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687*)CastclassSealed((RuntimeObject*)L_29, YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687_il2cpp_TypeInfo_var));
				// output.WriteLine(
				//     "{0}\t{1}",
				//     item.Children[new YamlScalarNode("part_no")],
				//     item.Children[new YamlScalarNode("descrip")]
				// );
				RuntimeObject* L_30 = __this->___output_0;
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_31 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)2);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_32 = L_31;
				YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687* L_33 = V_7;
				NullCheck(L_33);
				RuntimeObject* L_34;
				L_34 = YamlMappingNode_get_Children_m422A37EDF7D14EDE2AA4B23D6CE7AA440C5C823D(L_33, NULL);
				YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648* L_35 = (YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648*)il2cpp_codegen_object_new(YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648_il2cpp_TypeInfo_var);
				NullCheck(L_35);
				YamlScalarNode__ctor_mCF0C9B856F9F2EBDEA48475C7D6864C36A96DAC4(L_35, _stringLiteral7069C7F1D4004126C126503625BAF9ED9D1B55A6, NULL);
				NullCheck(L_34);
				YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* L_36;
				L_36 = InterfaceFuncInvoker1< YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA*, YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* >::Invoke(0 /* TValue System.Collections.Generic.IDictionary`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode>::get_Item(TKey) */, IDictionary_2_t27535D075559E1DDCA209D995D55F8D1942BFFDB_il2cpp_TypeInfo_var, L_34, L_35);
				NullCheck(L_32);
				ArrayElementTypeCheck (L_32, L_36);
				(L_32)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_36);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_37 = L_32;
				YamlMappingNode_t47685D491034AC5E7423EE3BBBB93203307EB687* L_38 = V_7;
				NullCheck(L_38);
				RuntimeObject* L_39;
				L_39 = YamlMappingNode_get_Children_m422A37EDF7D14EDE2AA4B23D6CE7AA440C5C823D(L_38, NULL);
				YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648* L_40 = (YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648*)il2cpp_codegen_object_new(YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648_il2cpp_TypeInfo_var);
				NullCheck(L_40);
				YamlScalarNode__ctor_mCF0C9B856F9F2EBDEA48475C7D6864C36A96DAC4(L_40, _stringLiteral34564CCF416EA1A7EB32DF54E7A91E201275ADFD, NULL);
				NullCheck(L_39);
				YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* L_41;
				L_41 = InterfaceFuncInvoker1< YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA*, YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* >::Invoke(0 /* TValue System.Collections.Generic.IDictionary`2<YamlDotNet.RepresentationModel.YamlNode,YamlDotNet.RepresentationModel.YamlNode>::get_Item(TKey) */, IDictionary_2_t27535D075559E1DDCA209D995D55F8D1942BFFDB_il2cpp_TypeInfo_var, L_39, L_40);
				NullCheck(L_37);
				ArrayElementTypeCheck (L_37, L_41);
				(L_37)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_41);
				NullCheck(L_30);
				InterfaceActionInvoker2< String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* >::Invoke(2 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String,System.Object[]) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_30, _stringLiteralFB7070F86EF8CC6601805BCBB939D6865BC62D1F, L_37);
			}

IL_0100_1:
			{
				// foreach (YamlMappingNode item in items)
				RuntimeObject* L_42 = V_6;
				NullCheck(L_42);
				bool L_43;
				L_43 = InterfaceFuncInvoker0< bool >::Invoke(0 /* System.Boolean System.Collections.IEnumerator::MoveNext() */, IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA_il2cpp_TypeInfo_var, L_42);
				if (L_43)
				{
					goto IL_00a7_1;
				}
			}
			{
				goto IL_0118;
			}
		}// end try (depth: 1)
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0118:
	{
		// }
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
// System.Void YamlDotNet.Samples.SerializeObjectGraph::.ctor(Xunit.Abstractions.ITestOutputHelper)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SerializeObjectGraph__ctor_mCE618144965D14FE4DCE7642F1E81AFDDCD2A4FC (SerializeObjectGraph_t6D234A16443587BC3F34E50A277696E6616C7073* __this, RuntimeObject* ___output0, const RuntimeMethod* method) 
{
	{
		// public SerializeObjectGraph(ITestOutputHelper output)
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		// this.output = output;
		RuntimeObject* L_0 = ___output0;
		__this->___output_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___output_0), (void*)L_0);
		// }
		return;
	}
}
// System.Void YamlDotNet.Samples.SerializeObjectGraph::Main()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SerializeObjectGraph_Main_m4FC261B958D345CD71704D4BE21A811AB9A5B199 (SerializeObjectGraph_t6D234A16443587BC3F34E50A277696E6616C7073* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ISerializer_tBA1C2B389CD506D5E3CB5DC013F5C94C126A1136_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Receipt_tE506B8843866A1445C321C463687A56F99457821_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral22222F4E849B4072CE91FDF62F9BE7096EF5A56D);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral3EF25B754B6A9B1A80F4D678709B7226A5AAC355);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral55300DE6DA6DDD8D71829680B0ECD972C284D15B);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral661F8132E7CA6894B80DC3DCD155CD99652BCE79);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral76341B1F170DDD858BE9AB81FFF2E8F58575E06C);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral7854BB2558C26BF20C4E12AE79BBB1B7A7B78B79);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral836EE39547BFB34DD863BF3BD7388E1DCE1CD167);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral9D452B73AB02C81825A66E4A40E989C8C105BAAB);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralAF0FE9B404A1D2065495B62C369E335378810973);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralC9FEB70F083D88ECA4A1F7EF7AB0A7BD3AC0892F);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralD42C2EFEBF138F5432C644B4CF104F04D3987CAC);
		s_Il2CppMethodInitialized = true;
	}
	Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* V_0 = NULL;
	Receipt_tE506B8843866A1445C321C463687A56F99457821* V_1 = NULL;
	RuntimeObject* V_2 = NULL;
	String_t* V_3 = NULL;
	Receipt_tE506B8843866A1445C321C463687A56F99457821* V_4 = NULL;
	{
		// var address = new Address
		// {
		//     street = "123 Tornado Alley\nSuite 16",
		//     city = "East Westville",
		//     state = "KS"
		// };
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_0 = (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22*)il2cpp_codegen_object_new(Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		Address__ctor_mCE4E9D8BE7BD99B32E4D0AFCB8B1E3B2A19EB4C3(L_0, NULL);
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_1 = L_0;
		NullCheck(L_1);
		Address_set_street_m82E05D93979FB39D928C9AB7831517EB2AB07A89_inline(L_1, _stringLiteral55300DE6DA6DDD8D71829680B0ECD972C284D15B, NULL);
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_2 = L_1;
		NullCheck(L_2);
		Address_set_city_mACF46014990F092127EE327EAF11315F91CF7068_inline(L_2, _stringLiteral9D452B73AB02C81825A66E4A40E989C8C105BAAB, NULL);
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_3 = L_2;
		NullCheck(L_3);
		Address_set_state_m1434642D6CA9506A144D13B027C05046DE39E0F4_inline(L_3, _stringLiteralAF0FE9B404A1D2065495B62C369E335378810973, NULL);
		V_0 = L_3;
		// var receipt = new Receipt
		// {
		//     receipt = "Oz-Ware Purchase Invoice",
		//     date = new DateTime(2007, 8, 6),
		//     customer = new Customer
		//     {
		//         given = "Dorothy",
		//         family = "Gale"
		//     },
		//     items = new Item[]
		//     {
		//         new Item
		//         {
		//             part_no = "A4786",
		//             descrip = "Water Bucket (Filled)",
		//             price = 1.47M,
		//             quantity = 4
		//         },
		//         new Item
		//         {
		//             part_no = "E1628",
		//             descrip = "High Heeled \"Ruby\" Slippers",
		//             price = 100.27M,
		//             quantity = 1
		//         }
		//     },
		//     bill_to = address,
		//     ship_to = address,
		//     specialDelivery = "Follow the Yellow Brick\n" +
		//                       "Road to the Emerald City.\n" +
		//                       "Pay no attention to the\n" +
		//                       "man behind the curtain."
		// };
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_4 = (Receipt_tE506B8843866A1445C321C463687A56F99457821*)il2cpp_codegen_object_new(Receipt_tE506B8843866A1445C321C463687A56F99457821_il2cpp_TypeInfo_var);
		NullCheck(L_4);
		Receipt__ctor_mD915A41D50BADDE2D4D689EFBB82D1627068AAE7(L_4, NULL);
		V_4 = L_4;
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_5 = V_4;
		NullCheck(L_5);
		Receipt_set_receipt_m2CD41C0DDE1FC9E896EC613C5547907EE1E23295_inline(L_5, _stringLiteral3EF25B754B6A9B1A80F4D678709B7226A5AAC355, NULL);
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_6 = V_4;
		DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D L_7;
		memset((&L_7), 0, sizeof(L_7));
		DateTime__ctor_mA3BF7CE28807F0A02634FD43913FAAFD989CEE88((&L_7), ((int32_t)2007), 8, 6, /*hidden argument*/NULL);
		NullCheck(L_6);
		Receipt_set_date_mDC90FDE684124E33C1EC1D615BCBA1BCE397F3DC_inline(L_6, L_7, NULL);
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_8 = V_4;
		Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* L_9 = (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B*)il2cpp_codegen_object_new(Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B_il2cpp_TypeInfo_var);
		NullCheck(L_9);
		Customer__ctor_m711E3C5703FFABEA25C2D6283E60D7367CBE9F41(L_9, NULL);
		Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* L_10 = L_9;
		NullCheck(L_10);
		Customer_set_given_m5F124768B94E969B24A8F47D6B023888D0DA16D3_inline(L_10, _stringLiteral76341B1F170DDD858BE9AB81FFF2E8F58575E06C, NULL);
		Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* L_11 = L_10;
		NullCheck(L_11);
		Customer_set_family_m8565386D9A7735C1A82A1A05A6A6D2147A7ADF12_inline(L_11, _stringLiteralC9FEB70F083D88ECA4A1F7EF7AB0A7BD3AC0892F, NULL);
		NullCheck(L_8);
		Receipt_set_customer_m4DB78B0D39289ECC0F1007C4FF166FB6BF377B89_inline(L_8, L_11, NULL);
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_12 = V_4;
		ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* L_13 = (ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A*)(ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A*)SZArrayNew(ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A_il2cpp_TypeInfo_var, (uint32_t)2);
		ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* L_14 = L_13;
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_15 = (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274*)il2cpp_codegen_object_new(Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274_il2cpp_TypeInfo_var);
		NullCheck(L_15);
		Item__ctor_mB35BD96F76794831AA646FC20FDDCEFDC86914F4(L_15, NULL);
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_16 = L_15;
		NullCheck(L_16);
		Item_set_part_no_m0CE760B9F26A06ABF2BC7AB2B1B6430E317423D4_inline(L_16, _stringLiteral836EE39547BFB34DD863BF3BD7388E1DCE1CD167, NULL);
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_17 = L_16;
		NullCheck(L_17);
		Item_set_descrip_m0B38A9144A563C9FC6A4E850C448F6249C9B4C83_inline(L_17, _stringLiteral661F8132E7CA6894B80DC3DCD155CD99652BCE79, NULL);
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_18 = L_17;
		Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_19;
		memset((&L_19), 0, sizeof(L_19));
		Decimal__ctor_mC089D0AF6A28E017DE6F2F0966D8EBEBFE2DAAF7((&L_19), ((int32_t)147), 0, 0, (bool)0, (uint8_t)2, /*hidden argument*/NULL);
		NullCheck(L_18);
		Item_set_price_mAC9D0E64ABBC779377E02DCD3DBD29964DCB7C33_inline(L_18, L_19, NULL);
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_20 = L_18;
		NullCheck(L_20);
		Item_set_quantity_mA66BE7C18DA97A2F5F894510F17294506BCA3520_inline(L_20, 4, NULL);
		NullCheck(L_14);
		ArrayElementTypeCheck (L_14, L_20);
		(L_14)->SetAt(static_cast<il2cpp_array_size_t>(0), (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274*)L_20);
		ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* L_21 = L_14;
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_22 = (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274*)il2cpp_codegen_object_new(Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274_il2cpp_TypeInfo_var);
		NullCheck(L_22);
		Item__ctor_mB35BD96F76794831AA646FC20FDDCEFDC86914F4(L_22, NULL);
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_23 = L_22;
		NullCheck(L_23);
		Item_set_part_no_m0CE760B9F26A06ABF2BC7AB2B1B6430E317423D4_inline(L_23, _stringLiteral7854BB2558C26BF20C4E12AE79BBB1B7A7B78B79, NULL);
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_24 = L_23;
		NullCheck(L_24);
		Item_set_descrip_m0B38A9144A563C9FC6A4E850C448F6249C9B4C83_inline(L_24, _stringLiteral22222F4E849B4072CE91FDF62F9BE7096EF5A56D, NULL);
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_25 = L_24;
		Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_26;
		memset((&L_26), 0, sizeof(L_26));
		Decimal__ctor_mC089D0AF6A28E017DE6F2F0966D8EBEBFE2DAAF7((&L_26), ((int32_t)10027), 0, 0, (bool)0, (uint8_t)2, /*hidden argument*/NULL);
		NullCheck(L_25);
		Item_set_price_mAC9D0E64ABBC779377E02DCD3DBD29964DCB7C33_inline(L_25, L_26, NULL);
		Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* L_27 = L_25;
		NullCheck(L_27);
		Item_set_quantity_mA66BE7C18DA97A2F5F894510F17294506BCA3520_inline(L_27, 1, NULL);
		NullCheck(L_21);
		ArrayElementTypeCheck (L_21, L_27);
		(L_21)->SetAt(static_cast<il2cpp_array_size_t>(1), (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274*)L_27);
		NullCheck(L_12);
		Receipt_set_items_m9D9FB2BE4E4C12C4F585B69EA91A8B43C593F1B3_inline(L_12, L_21, NULL);
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_28 = V_4;
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_29 = V_0;
		NullCheck(L_28);
		Receipt_set_bill_to_m2B8A801F8C633D488890CD4AEDD5EE1E14244A52_inline(L_28, L_29, NULL);
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_30 = V_4;
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_31 = V_0;
		NullCheck(L_30);
		Receipt_set_ship_to_mF15187ACF9688D1A226755C41B16861D53DE4BD2_inline(L_30, L_31, NULL);
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_32 = V_4;
		NullCheck(L_32);
		Receipt_set_specialDelivery_m23A683515B0AE248B3B3A159ACC04EAF330693C6_inline(L_32, _stringLiteralD42C2EFEBF138F5432C644B4CF104F04D3987CAC, NULL);
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_33 = V_4;
		V_1 = L_33;
		// var serializer = new SerializerBuilder().Build();
		SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C* L_34 = (SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C*)il2cpp_codegen_object_new(SerializerBuilder_t53E794FB2BD68D193D578394A2AB30AD735FE67C_il2cpp_TypeInfo_var);
		NullCheck(L_34);
		SerializerBuilder__ctor_mC3F1F264785836360661ADBAF3C3EA09D974EFF0(L_34, NULL);
		NullCheck(L_34);
		RuntimeObject* L_35;
		L_35 = SerializerBuilder_Build_m37AF80C3630297349F07BCF78CDFC8D3742DCE6E(L_34, NULL);
		V_2 = L_35;
		// var yaml = serializer.Serialize(receipt);
		RuntimeObject* L_36 = V_2;
		Receipt_tE506B8843866A1445C321C463687A56F99457821* L_37 = V_1;
		NullCheck(L_36);
		String_t* L_38;
		L_38 = InterfaceFuncInvoker1< String_t*, RuntimeObject* >::Invoke(1 /* System.String YamlDotNet.Serialization.ISerializer::Serialize(System.Object) */, ISerializer_tBA1C2B389CD506D5E3CB5DC013F5C94C126A1136_il2cpp_TypeInfo_var, L_36, L_37);
		V_3 = L_38;
		// output.WriteLine(yaml);
		RuntimeObject* L_39 = __this->___output_0;
		String_t* L_40 = V_3;
		NullCheck(L_39);
		InterfaceActionInvoker1< String_t* >::Invoke(1 /* System.Void Xunit.Abstractions.ITestOutputHelper::WriteLine(System.String) */, ITestOutputHelper_t1B099C18AE1294491C569E26B329B5C8BF581898_il2cpp_TypeInfo_var, L_39, L_40);
		// }
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
// System.String YamlDotNet.Samples.Address::get_street()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Address_get_street_m6E51AFCFA1524D49458FED26057F67BAA3AC6C2C (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, const RuntimeMethod* method) 
{
	{
		// public string street { get; set; }
		String_t* L_0 = __this->___U3CstreetU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Address::set_street(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address_set_street_m82E05D93979FB39D928C9AB7831517EB2AB07A89 (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string street { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CstreetU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CstreetU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.Address::get_city()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Address_get_city_mA3A13394F7077E20956D995EA54FF4D31E741D4A (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, const RuntimeMethod* method) 
{
	{
		// public string city { get; set; }
		String_t* L_0 = __this->___U3CcityU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Address::set_city(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address_set_city_mACF46014990F092127EE327EAF11315F91CF7068 (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string city { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CcityU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CcityU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.Address::get_state()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Address_get_state_m532740020BA4717EEEB2A8F196295B1472EE1370 (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, const RuntimeMethod* method) 
{
	{
		// public string state { get; set; }
		String_t* L_0 = __this->___U3CstateU3Ek__BackingField_2;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Address::set_state(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address_set_state_m1434642D6CA9506A144D13B027C05046DE39E0F4 (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string state { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CstateU3Ek__BackingField_2 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CstateU3Ek__BackingField_2), (void*)L_0);
		return;
	}
}
// System.Void YamlDotNet.Samples.Address::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Address__ctor_mCE4E9D8BE7BD99B32E4D0AFCB8B1E3B2A19EB4C3 (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, const RuntimeMethod* method) 
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
// System.String YamlDotNet.Samples.Receipt::get_receipt()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Receipt_get_receipt_mA551318002FC13851628A84C45817BAF3E359171 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) 
{
	{
		// public string receipt { get; set; }
		String_t* L_0 = __this->___U3CreceiptU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Receipt::set_receipt(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt_set_receipt_m2CD41C0DDE1FC9E896EC613C5547907EE1E23295 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string receipt { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CreceiptU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CreceiptU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.DateTime YamlDotNet.Samples.Receipt::get_date()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D Receipt_get_date_mE7A93C065B52E0A7212015CC83749D9D8FC7E7ED (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) 
{
	{
		// public DateTime date { get; set; }
		DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D L_0 = __this->___U3CdateU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Receipt::set_date(System.DateTime)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt_set_date_mDC90FDE684124E33C1EC1D615BCBA1BCE397F3DC (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___value0, const RuntimeMethod* method) 
{
	{
		// public DateTime date { get; set; }
		DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D L_0 = ___value0;
		__this->___U3CdateU3Ek__BackingField_1 = L_0;
		return;
	}
}
// YamlDotNet.Samples.Customer YamlDotNet.Samples.Receipt::get_customer()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* Receipt_get_customer_m2A7F218636AC0061CEA9547D46668E389D5D0BF6 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) 
{
	{
		// public Customer customer { get; set; }
		Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* L_0 = __this->___U3CcustomerU3Ek__BackingField_2;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Receipt::set_customer(YamlDotNet.Samples.Customer)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt_set_customer_m4DB78B0D39289ECC0F1007C4FF166FB6BF377B89 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* ___value0, const RuntimeMethod* method) 
{
	{
		// public Customer customer { get; set; }
		Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* L_0 = ___value0;
		__this->___U3CcustomerU3Ek__BackingField_2 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CcustomerU3Ek__BackingField_2), (void*)L_0);
		return;
	}
}
// YamlDotNet.Samples.Item[] YamlDotNet.Samples.Receipt::get_items()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* Receipt_get_items_m16E2A7FB845C94BE1B56910D341B310DB9BEB492 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) 
{
	{
		// public Item[] items { get; set; }
		ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* L_0 = __this->___U3CitemsU3Ek__BackingField_3;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Receipt::set_items(YamlDotNet.Samples.Item[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt_set_items_m9D9FB2BE4E4C12C4F585B69EA91A8B43C593F1B3 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* ___value0, const RuntimeMethod* method) 
{
	{
		// public Item[] items { get; set; }
		ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* L_0 = ___value0;
		__this->___U3CitemsU3Ek__BackingField_3 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CitemsU3Ek__BackingField_3), (void*)L_0);
		return;
	}
}
// YamlDotNet.Samples.Address YamlDotNet.Samples.Receipt::get_bill_to()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* Receipt_get_bill_to_m829FB5B0E77B9A1CB6E3A969EADE0024901E69A5 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) 
{
	{
		// public Address bill_to { get; set; }
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_0 = __this->___U3Cbill_toU3Ek__BackingField_4;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Receipt::set_bill_to(YamlDotNet.Samples.Address)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt_set_bill_to_m2B8A801F8C633D488890CD4AEDD5EE1E14244A52 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* ___value0, const RuntimeMethod* method) 
{
	{
		// public Address bill_to { get; set; }
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_0 = ___value0;
		__this->___U3Cbill_toU3Ek__BackingField_4 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3Cbill_toU3Ek__BackingField_4), (void*)L_0);
		return;
	}
}
// YamlDotNet.Samples.Address YamlDotNet.Samples.Receipt::get_ship_to()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* Receipt_get_ship_to_m6824ADE743E195B5348F0010C6A7C6B1130B5024 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) 
{
	{
		// public Address ship_to { get; set; }
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_0 = __this->___U3Cship_toU3Ek__BackingField_5;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Receipt::set_ship_to(YamlDotNet.Samples.Address)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt_set_ship_to_mF15187ACF9688D1A226755C41B16861D53DE4BD2 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* ___value0, const RuntimeMethod* method) 
{
	{
		// public Address ship_to { get; set; }
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_0 = ___value0;
		__this->___U3Cship_toU3Ek__BackingField_5 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3Cship_toU3Ek__BackingField_5), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.Receipt::get_specialDelivery()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Receipt_get_specialDelivery_m4311035AE4D53D60D18C19A351BAF3EAAA99F76D (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) 
{
	{
		// public string specialDelivery { get; set; }
		String_t* L_0 = __this->___U3CspecialDeliveryU3Ek__BackingField_6;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Receipt::set_specialDelivery(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt_set_specialDelivery_m23A683515B0AE248B3B3A159ACC04EAF330693C6 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string specialDelivery { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CspecialDeliveryU3Ek__BackingField_6 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CspecialDeliveryU3Ek__BackingField_6), (void*)L_0);
		return;
	}
}
// System.Void YamlDotNet.Samples.Receipt::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Receipt__ctor_mD915A41D50BADDE2D4D689EFBB82D1627068AAE7 (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, const RuntimeMethod* method) 
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
// System.String YamlDotNet.Samples.Customer::get_given()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Customer_get_given_m833E18E9F0B8985C89B937547F4B655CAF9AF899 (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, const RuntimeMethod* method) 
{
	{
		// public string given { get; set; }
		String_t* L_0 = __this->___U3CgivenU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Customer::set_given(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Customer_set_given_m5F124768B94E969B24A8F47D6B023888D0DA16D3 (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string given { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CgivenU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CgivenU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.Customer::get_family()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Customer_get_family_m2C3B33E6AAB664A6A15E5E87E1602C0699F9784D (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, const RuntimeMethod* method) 
{
	{
		// public string family { get; set; }
		String_t* L_0 = __this->___U3CfamilyU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Customer::set_family(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Customer_set_family_m8565386D9A7735C1A82A1A05A6A6D2147A7ADF12 (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string family { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CfamilyU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CfamilyU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
// System.Void YamlDotNet.Samples.Customer::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Customer__ctor_m711E3C5703FFABEA25C2D6283E60D7367CBE9F41 (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, const RuntimeMethod* method) 
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
// System.String YamlDotNet.Samples.Item::get_part_no()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Item_get_part_no_mD3F583A7F601DC147899C406FC7AF8AA5CC9F55D (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, const RuntimeMethod* method) 
{
	{
		// public string part_no { get; set; }
		String_t* L_0 = __this->___U3Cpart_noU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Item::set_part_no(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Item_set_part_no_m0CE760B9F26A06ABF2BC7AB2B1B6430E317423D4 (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string part_no { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3Cpart_noU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3Cpart_noU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.Item::get_descrip()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Item_get_descrip_m317668AAF18DFD8CB317B1C84664EF10FFAAA70C (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, const RuntimeMethod* method) 
{
	{
		// public string descrip { get; set; }
		String_t* L_0 = __this->___U3CdescripU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Item::set_descrip(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Item_set_descrip_m0B38A9144A563C9FC6A4E850C448F6249C9B4C83 (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string descrip { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CdescripU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CdescripU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
// System.Decimal YamlDotNet.Samples.Item::get_price()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F Item_get_price_mDB4EDF629AF8B7AE0906D93865D792711B3AF2CB (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, const RuntimeMethod* method) 
{
	{
		// public decimal price { get; set; }
		Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_0 = __this->___U3CpriceU3Ek__BackingField_2;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Item::set_price(System.Decimal)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Item_set_price_mAC9D0E64ABBC779377E02DCD3DBD29964DCB7C33 (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___value0, const RuntimeMethod* method) 
{
	{
		// public decimal price { get; set; }
		Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_0 = ___value0;
		__this->___U3CpriceU3Ek__BackingField_2 = L_0;
		return;
	}
}
// System.Int32 YamlDotNet.Samples.Item::get_quantity()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Item_get_quantity_m2D4E9FC339E46DEB7CB21EF1B759C4FFD47F273E (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, const RuntimeMethod* method) 
{
	{
		// public int quantity { get; set; }
		int32_t L_0 = __this->___U3CquantityU3Ek__BackingField_3;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Item::set_quantity(System.Int32)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Item_set_quantity_mA66BE7C18DA97A2F5F894510F17294506BCA3520 (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, int32_t ___value0, const RuntimeMethod* method) 
{
	{
		// public int quantity { get; set; }
		int32_t L_0 = ___value0;
		__this->___U3CquantityU3Ek__BackingField_3 = L_0;
		return;
	}
}
// System.Void YamlDotNet.Samples.Item::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Item__ctor_mB35BD96F76794831AA646FC20FDDCEFDC86914F4 (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, const RuntimeMethod* method) 
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
// System.String[] YamlDotNet.Samples.Helpers.ExampleRunner::GetAllTestNames()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ExampleRunner_GetAllTestNames_mC20DA32B264F88714446AF4C380A97AF820AD963 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExampleRunner_GetAllTestNames_mC20DA32B264F88714446AF4C380A97AF820AD963_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral32D59C370DFA7384C9A1965AED35526215B01B41);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral3F86111F44D66C543B732847E04E3C2A5B38BB3D);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* V_1 = NULL;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* V_2 = NULL;
	int32_t V_3 = 0;
	Type_t* V_4 = NULL;
	bool V_5 = false;
	MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* V_6 = NULL;
	int32_t V_7 = 0;
	MethodInfo_t* V_8 = NULL;
	bool V_9 = false;
	SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* V_10 = NULL;
	bool V_11 = false;
	bool V_12 = false;
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* V_13 = NULL;
	int32_t G_B4_0 = 0;
	{
		// var results = new List<string>();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_0 = (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*)il2cpp_codegen_object_new(List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E(L_0, List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		V_1 = L_0;
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		Assembly_t* L_1;
		L_1 = il2cpp_codegen_get_executing_assembly(ExampleRunner_GetAllTestNames_mC20DA32B264F88714446AF4C380A97AF820AD963_RuntimeMethod_var);
		NullCheck(L_1);
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_2;
		L_2 = VirtualFuncInvoker0< TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* >::Invoke(15 /* System.Type[] System.Reflection.Assembly::GetTypes() */, L_1);
		V_2 = L_2;
		V_3 = 0;
		goto IL_00cd;
	}

IL_001a:
	{
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_3 = V_2;
		int32_t L_4 = V_3;
		NullCheck(L_3);
		int32_t L_5 = L_4;
		Type_t* L_6 = (L_3)->GetAt(static_cast<il2cpp_array_size_t>(L_5));
		V_4 = L_6;
		// if (t.Namespace == "YamlDotNet.Samples" && t.IsClass) {
		Type_t* L_7 = V_4;
		NullCheck(L_7);
		String_t* L_8;
		L_8 = VirtualFuncInvoker0< String_t* >::Invoke(23 /* System.String System.Type::get_Namespace() */, L_7);
		bool L_9;
		L_9 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_8, _stringLiteral32D59C370DFA7384C9A1965AED35526215B01B41, NULL);
		if (!L_9)
		{
			goto IL_003c;
		}
	}
	{
		Type_t* L_10 = V_4;
		NullCheck(L_10);
		bool L_11;
		L_11 = Type_get_IsClass_mACC1E0E79C9996ADE9973F81971B740132B64549(L_10, NULL);
		G_B4_0 = ((int32_t)(L_11));
		goto IL_003d;
	}

IL_003c:
	{
		G_B4_0 = 0;
	}

IL_003d:
	{
		V_5 = (bool)G_B4_0;
		bool L_12 = V_5;
		if (!L_12)
		{
			goto IL_00c8;
		}
	}
	{
		// skipMethods = false;
		V_0 = (bool)0;
		// foreach (MethodInfo mi in t.GetMethods()) {
		Type_t* L_13 = V_4;
		NullCheck(L_13);
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_14;
		L_14 = Type_GetMethods_m5D4A53D1E667CF33173EEA37D0111FE92A572559(L_13, NULL);
		V_6 = L_14;
		V_7 = 0;
		goto IL_00bf;
	}

IL_0058:
	{
		// foreach (MethodInfo mi in t.GetMethods()) {
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_15 = V_6;
		int32_t L_16 = V_7;
		NullCheck(L_15);
		int32_t L_17 = L_16;
		MethodInfo_t* L_18 = (L_15)->GetAt(static_cast<il2cpp_array_size_t>(L_17));
		V_8 = L_18;
		// if (mi.Name == "Main") {
		MethodInfo_t* L_19 = V_8;
		NullCheck(L_19);
		String_t* L_20;
		L_20 = VirtualFuncInvoker0< String_t* >::Invoke(7 /* System.String System.Reflection.MemberInfo::get_Name() */, L_19);
		bool L_21;
		L_21 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_20, _stringLiteral3F86111F44D66C543B732847E04E3C2A5B38BB3D, NULL);
		V_9 = L_21;
		bool L_22 = V_9;
		if (!L_22)
		{
			goto IL_00af;
		}
	}
	{
		// SampleAttribute sa = (SampleAttribute) Attribute.GetCustomAttribute(mi, typeof(SampleAttribute));
		MethodInfo_t* L_23 = V_8;
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_24 = { reinterpret_cast<intptr_t> (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_25;
		L_25 = Type_GetTypeFromHandle_m2570A2A5B32A5E9D9F0F38B37459DA18736C823E(L_24, NULL);
		Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* L_26;
		L_26 = Attribute_GetCustomAttribute_mF9CB9F03A29701923B68556A396459E8FBEAE6B0(L_23, L_25, NULL);
		V_10 = ((SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF*)CastclassClass((RuntimeObject*)L_26, SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_il2cpp_TypeInfo_var));
		// if (sa != null) {
		SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* L_27 = V_10;
		V_11 = (bool)((!(((RuntimeObject*)(SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF*)L_27) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
		bool L_28 = V_11;
		if (!L_28)
		{
			goto IL_00ae;
		}
	}
	{
		// results.Add(t.Name);
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_29 = V_1;
		Type_t* L_30 = V_4;
		NullCheck(L_30);
		String_t* L_31;
		L_31 = VirtualFuncInvoker0< String_t* >::Invoke(7 /* System.String System.Reflection.MemberInfo::get_Name() */, L_30);
		NullCheck(L_29);
		List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_inline(L_29, L_31, List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_RuntimeMethod_var);
		// skipMethods = true;
		V_0 = (bool)1;
		// break;
		goto IL_00c7;
	}

IL_00ae:
	{
	}

IL_00af:
	{
		// if (skipMethods) break;
		bool L_32 = V_0;
		V_12 = L_32;
		bool L_33 = V_12;
		if (!L_33)
		{
			goto IL_00b8;
		}
	}
	{
		// if (skipMethods) break;
		goto IL_00c7;
	}

IL_00b8:
	{
		int32_t L_34 = V_7;
		V_7 = ((int32_t)il2cpp_codegen_add(L_34, 1));
	}

IL_00bf:
	{
		// foreach (MethodInfo mi in t.GetMethods()) {
		int32_t L_35 = V_7;
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_36 = V_6;
		NullCheck(L_36);
		if ((((int32_t)L_35) < ((int32_t)((int32_t)(((RuntimeArray*)L_36)->max_length)))))
		{
			goto IL_0058;
		}
	}

IL_00c7:
	{
	}

IL_00c8:
	{
		int32_t L_37 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_37, 1));
	}

IL_00cd:
	{
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		int32_t L_38 = V_3;
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_39 = V_2;
		NullCheck(L_39);
		if ((((int32_t)L_38) < ((int32_t)((int32_t)(((RuntimeArray*)L_39)->max_length)))))
		{
			goto IL_001a;
		}
	}
	{
		// return results.ToArray();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_40 = V_1;
		NullCheck(L_40);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_41;
		L_41 = List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A(L_40, List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A_RuntimeMethod_var);
		V_13 = L_41;
		goto IL_00e0;
	}

IL_00e0:
	{
		// }
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_42 = V_13;
		return L_42;
	}
}
// System.String[] YamlDotNet.Samples.Helpers.ExampleRunner::GetAllTestDisplayNames()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ExampleRunner_GetAllTestDisplayNames_mA7F999F8B96722BA70EAD672B45027E1B2678708 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExampleRunner_GetAllTestDisplayNames_mA7F999F8B96722BA70EAD672B45027E1B2678708_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral32D59C370DFA7384C9A1965AED35526215B01B41);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral3F86111F44D66C543B732847E04E3C2A5B38BB3D);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* V_1 = NULL;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* V_2 = NULL;
	int32_t V_3 = 0;
	Type_t* V_4 = NULL;
	bool V_5 = false;
	MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* V_6 = NULL;
	int32_t V_7 = 0;
	MethodInfo_t* V_8 = NULL;
	bool V_9 = false;
	SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* V_10 = NULL;
	bool V_11 = false;
	bool V_12 = false;
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* V_13 = NULL;
	int32_t G_B4_0 = 0;
	{
		// var results = new List<string>();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_0 = (List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD*)il2cpp_codegen_object_new(List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E(L_0, List_1__ctor_mCA8DD57EAC70C2B5923DBB9D5A77CEAC22E7068E_RuntimeMethod_var);
		V_1 = L_0;
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		Assembly_t* L_1;
		L_1 = il2cpp_codegen_get_executing_assembly(ExampleRunner_GetAllTestDisplayNames_mA7F999F8B96722BA70EAD672B45027E1B2678708_RuntimeMethod_var);
		NullCheck(L_1);
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_2;
		L_2 = VirtualFuncInvoker0< TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* >::Invoke(15 /* System.Type[] System.Reflection.Assembly::GetTypes() */, L_1);
		V_2 = L_2;
		V_3 = 0;
		goto IL_00cd;
	}

IL_001a:
	{
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_3 = V_2;
		int32_t L_4 = V_3;
		NullCheck(L_3);
		int32_t L_5 = L_4;
		Type_t* L_6 = (L_3)->GetAt(static_cast<il2cpp_array_size_t>(L_5));
		V_4 = L_6;
		// if (t.Namespace == "YamlDotNet.Samples" && t.IsClass) {
		Type_t* L_7 = V_4;
		NullCheck(L_7);
		String_t* L_8;
		L_8 = VirtualFuncInvoker0< String_t* >::Invoke(23 /* System.String System.Type::get_Namespace() */, L_7);
		bool L_9;
		L_9 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_8, _stringLiteral32D59C370DFA7384C9A1965AED35526215B01B41, NULL);
		if (!L_9)
		{
			goto IL_003c;
		}
	}
	{
		Type_t* L_10 = V_4;
		NullCheck(L_10);
		bool L_11;
		L_11 = Type_get_IsClass_mACC1E0E79C9996ADE9973F81971B740132B64549(L_10, NULL);
		G_B4_0 = ((int32_t)(L_11));
		goto IL_003d;
	}

IL_003c:
	{
		G_B4_0 = 0;
	}

IL_003d:
	{
		V_5 = (bool)G_B4_0;
		bool L_12 = V_5;
		if (!L_12)
		{
			goto IL_00c8;
		}
	}
	{
		// skipMethods = false;
		V_0 = (bool)0;
		// foreach (MethodInfo mi in t.GetMethods()) {
		Type_t* L_13 = V_4;
		NullCheck(L_13);
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_14;
		L_14 = Type_GetMethods_m5D4A53D1E667CF33173EEA37D0111FE92A572559(L_13, NULL);
		V_6 = L_14;
		V_7 = 0;
		goto IL_00bf;
	}

IL_0058:
	{
		// foreach (MethodInfo mi in t.GetMethods()) {
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_15 = V_6;
		int32_t L_16 = V_7;
		NullCheck(L_15);
		int32_t L_17 = L_16;
		MethodInfo_t* L_18 = (L_15)->GetAt(static_cast<il2cpp_array_size_t>(L_17));
		V_8 = L_18;
		// if (mi.Name == "Main") {
		MethodInfo_t* L_19 = V_8;
		NullCheck(L_19);
		String_t* L_20;
		L_20 = VirtualFuncInvoker0< String_t* >::Invoke(7 /* System.String System.Reflection.MemberInfo::get_Name() */, L_19);
		bool L_21;
		L_21 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_20, _stringLiteral3F86111F44D66C543B732847E04E3C2A5B38BB3D, NULL);
		V_9 = L_21;
		bool L_22 = V_9;
		if (!L_22)
		{
			goto IL_00af;
		}
	}
	{
		// SampleAttribute sa = (SampleAttribute) Attribute.GetCustomAttribute(mi, typeof(SampleAttribute));
		MethodInfo_t* L_23 = V_8;
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_24 = { reinterpret_cast<intptr_t> (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_25;
		L_25 = Type_GetTypeFromHandle_m2570A2A5B32A5E9D9F0F38B37459DA18736C823E(L_24, NULL);
		Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* L_26;
		L_26 = Attribute_GetCustomAttribute_mF9CB9F03A29701923B68556A396459E8FBEAE6B0(L_23, L_25, NULL);
		V_10 = ((SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF*)CastclassClass((RuntimeObject*)L_26, SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_il2cpp_TypeInfo_var));
		// if (sa != null) {
		SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* L_27 = V_10;
		V_11 = (bool)((!(((RuntimeObject*)(SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF*)L_27) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
		bool L_28 = V_11;
		if (!L_28)
		{
			goto IL_00ae;
		}
	}
	{
		// results.Add(sa.DisplayName);
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_29 = V_1;
		SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* L_30 = V_10;
		NullCheck(L_30);
		String_t* L_31;
		L_31 = SampleAttribute_get_DisplayName_m10A912CE310DFE82E23C1C51096136E724B2402E_inline(L_30, NULL);
		NullCheck(L_29);
		List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_inline(L_29, L_31, List_1_Add_mF10DB1D3CBB0B14215F0E4F8AB4934A1955E5351_RuntimeMethod_var);
		// skipMethods = true;
		V_0 = (bool)1;
		// break;
		goto IL_00c7;
	}

IL_00ae:
	{
	}

IL_00af:
	{
		// if (skipMethods) break;
		bool L_32 = V_0;
		V_12 = L_32;
		bool L_33 = V_12;
		if (!L_33)
		{
			goto IL_00b8;
		}
	}
	{
		// if (skipMethods) break;
		goto IL_00c7;
	}

IL_00b8:
	{
		int32_t L_34 = V_7;
		V_7 = ((int32_t)il2cpp_codegen_add(L_34, 1));
	}

IL_00bf:
	{
		// foreach (MethodInfo mi in t.GetMethods()) {
		int32_t L_35 = V_7;
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_36 = V_6;
		NullCheck(L_36);
		if ((((int32_t)L_35) < ((int32_t)((int32_t)(((RuntimeArray*)L_36)->max_length)))))
		{
			goto IL_0058;
		}
	}

IL_00c7:
	{
	}

IL_00c8:
	{
		int32_t L_37 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_37, 1));
	}

IL_00cd:
	{
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		int32_t L_38 = V_3;
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_39 = V_2;
		NullCheck(L_39);
		if ((((int32_t)L_38) < ((int32_t)((int32_t)(((RuntimeArray*)L_39)->max_length)))))
		{
			goto IL_001a;
		}
	}
	{
		// return results.ToArray();
		List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD* L_40 = V_1;
		NullCheck(L_40);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_41;
		L_41 = List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A(L_40, List_1_ToArray_m2C402D882AA60FC1D5C7C09A129BE7779F833B4A_RuntimeMethod_var);
		V_13 = L_41;
		goto IL_00e0;
	}

IL_00e0:
	{
		// }
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_42 = V_13;
		return L_42;
	}
}
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner::Start()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ExampleRunner_Start_m3143B4441D1B7C4035C46E351676E1D6B63F7C3A (ExampleRunner_tF444DF9613423B29748C32062F07F3469F498BE1* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Array_IndexOf_TisString_t_m9107AABCE77608D1D21B9ECB6DA42D0D4334AF32_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExampleRunner_Start_m3143B4441D1B7C4035C46E351676E1D6B63F7C3A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral32D59C370DFA7384C9A1965AED35526215B01B41);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral35D9703651C0B5FE577BAA089212BEF91D370ADB);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral3F86111F44D66C543B732847E04E3C2A5B38BB3D);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* V_1 = NULL;
	int32_t V_2 = 0;
	Type_t* V_3 = NULL;
	bool V_4 = false;
	MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* V_5 = NULL;
	int32_t V_6 = 0;
	MethodInfo_t* V_7 = NULL;
	bool V_8 = false;
	SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* V_9 = NULL;
	bool V_10 = false;
	RuntimeObject* V_11 = NULL;
	bool V_12 = false;
	int32_t G_B5_0 = 0;
	{
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		Assembly_t* L_0;
		L_0 = il2cpp_codegen_get_executing_assembly(ExampleRunner_Start_m3143B4441D1B7C4035C46E351676E1D6B63F7C3A_RuntimeMethod_var);
		NullCheck(L_0);
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_1;
		L_1 = VirtualFuncInvoker0< TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* >::Invoke(15 /* System.Type[] System.Reflection.Assembly::GetTypes() */, L_0);
		V_1 = L_1;
		V_2 = 0;
		goto IL_015e;
	}

IL_0014:
	{
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_2 = V_1;
		int32_t L_3 = V_2;
		NullCheck(L_2);
		int32_t L_4 = L_3;
		Type_t* L_5 = (L_2)->GetAt(static_cast<il2cpp_array_size_t>(L_4));
		V_3 = L_5;
		// if (t.Namespace == "YamlDotNet.Samples" && t.IsClass && Array.IndexOf(disabledTests, t.Name) == -1) {
		Type_t* L_6 = V_3;
		NullCheck(L_6);
		String_t* L_7;
		L_7 = VirtualFuncInvoker0< String_t* >::Invoke(23 /* System.String System.Type::get_Namespace() */, L_6);
		bool L_8;
		L_8 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_7, _stringLiteral32D59C370DFA7384C9A1965AED35526215B01B41, NULL);
		if (!L_8)
		{
			goto IL_0049;
		}
	}
	{
		Type_t* L_9 = V_3;
		NullCheck(L_9);
		bool L_10;
		L_10 = Type_get_IsClass_mACC1E0E79C9996ADE9973F81971B740132B64549(L_9, NULL);
		if (!L_10)
		{
			goto IL_0049;
		}
	}
	{
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_11 = __this->___disabledTests_5;
		Type_t* L_12 = V_3;
		NullCheck(L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(7 /* System.String System.Reflection.MemberInfo::get_Name() */, L_12);
		int32_t L_14;
		L_14 = Array_IndexOf_TisString_t_m9107AABCE77608D1D21B9ECB6DA42D0D4334AF32(L_11, L_13, Array_IndexOf_TisString_t_m9107AABCE77608D1D21B9ECB6DA42D0D4334AF32_RuntimeMethod_var);
		G_B5_0 = ((((int32_t)L_14) == ((int32_t)(-1)))? 1 : 0);
		goto IL_004a;
	}

IL_0049:
	{
		G_B5_0 = 0;
	}

IL_004a:
	{
		V_4 = (bool)G_B5_0;
		bool L_15 = V_4;
		if (!L_15)
		{
			goto IL_0159;
		}
	}
	{
		// skipMethods = false;
		V_0 = (bool)0;
		// foreach (MethodInfo mi in t.GetMethods()) {
		Type_t* L_16 = V_3;
		NullCheck(L_16);
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_17;
		L_17 = Type_GetMethods_m5D4A53D1E667CF33173EEA37D0111FE92A572559(L_16, NULL);
		V_5 = L_17;
		V_6 = 0;
		goto IL_014d;
	}

IL_0067:
	{
		// foreach (MethodInfo mi in t.GetMethods()) {
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_18 = V_5;
		int32_t L_19 = V_6;
		NullCheck(L_18);
		int32_t L_20 = L_19;
		MethodInfo_t* L_21 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_20));
		V_7 = L_21;
		// if (mi.Name == "Main") {
		MethodInfo_t* L_22 = V_7;
		NullCheck(L_22);
		String_t* L_23;
		L_23 = VirtualFuncInvoker0< String_t* >::Invoke(7 /* System.String System.Reflection.MemberInfo::get_Name() */, L_22);
		bool L_24;
		L_24 = String_op_Equality_m0D685A924E5CD78078F248ED1726DA5A9D7D6AC0(L_23, _stringLiteral3F86111F44D66C543B732847E04E3C2A5B38BB3D, NULL);
		V_8 = L_24;
		bool L_25 = V_8;
		if (!L_25)
		{
			goto IL_013d;
		}
	}
	{
		// SampleAttribute sa = (SampleAttribute) Attribute.GetCustomAttribute(mi, typeof(SampleAttribute));
		MethodInfo_t* L_26 = V_7;
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_27 = { reinterpret_cast<intptr_t> (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_28;
		L_28 = Type_GetTypeFromHandle_m2570A2A5B32A5E9D9F0F38B37459DA18736C823E(L_27, NULL);
		Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* L_29;
		L_29 = Attribute_GetCustomAttribute_mF9CB9F03A29701923B68556A396459E8FBEAE6B0(L_26, L_28, NULL);
		V_9 = ((SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF*)CastclassClass((RuntimeObject*)L_29, SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF_il2cpp_TypeInfo_var));
		// if (sa != null) {
		SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* L_30 = V_9;
		V_10 = (bool)((!(((RuntimeObject*)(SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF*)L_30) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
		bool L_31 = V_10;
		if (!L_31)
		{
			goto IL_013c;
		}
	}
	{
		// helper.WriteLine("{0} - {1}", sa.DisplayName, sa.Description);
		StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* L_32 = __this->___helper_4;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_33 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)2);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_34 = L_33;
		SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* L_35 = V_9;
		NullCheck(L_35);
		String_t* L_36;
		L_36 = SampleAttribute_get_DisplayName_m10A912CE310DFE82E23C1C51096136E724B2402E_inline(L_35, NULL);
		NullCheck(L_34);
		ArrayElementTypeCheck (L_34, L_36);
		(L_34)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_36);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_37 = L_34;
		SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* L_38 = V_9;
		NullCheck(L_38);
		String_t* L_39;
		L_39 = SampleAttribute_get_Description_m512B7F186A5F27E873A27EDE67129475F74E1F11_inline(L_38, NULL);
		NullCheck(L_37);
		ArrayElementTypeCheck (L_37, L_39);
		(L_37)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_39);
		NullCheck(L_32);
		StringTestOutputHelper_WriteLine_mE3837EA67D411B8ABBF742B538C073EF5EDBADE1(L_32, _stringLiteral35D9703651C0B5FE577BAA089212BEF91D370ADB, L_37, NULL);
		// var testObject = t.GetConstructor(new Type[] { typeof(StringTestOutputHelper) }).Invoke(new object[] { helper });
		Type_t* L_40 = V_3;
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_41 = (TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB*)(TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB*)SZArrayNew(TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB_il2cpp_TypeInfo_var, (uint32_t)1);
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_42 = L_41;
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_43 = { reinterpret_cast<intptr_t> (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_44;
		L_44 = Type_GetTypeFromHandle_m2570A2A5B32A5E9D9F0F38B37459DA18736C823E(L_43, NULL);
		NullCheck(L_42);
		ArrayElementTypeCheck (L_42, L_44);
		(L_42)->SetAt(static_cast<il2cpp_array_size_t>(0), (Type_t*)L_44);
		NullCheck(L_40);
		ConstructorInfo_t1B5967EE7E5554272F79F8880183C70AD240EEEB* L_45;
		L_45 = Type_GetConstructor_m7F0E5E1A61477DE81B35AE780C21FA6830124554(L_40, L_42, NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_46 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_47 = L_46;
		StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* L_48 = __this->___helper_4;
		NullCheck(L_47);
		ArrayElementTypeCheck (L_47, L_48);
		(L_47)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_48);
		NullCheck(L_45);
		RuntimeObject* L_49;
		L_49 = ConstructorInfo_Invoke_m15FDF2B682BD01CC934BE4D314EF2193103CECFE(L_45, L_47, NULL);
		V_11 = L_49;
		// mi.Invoke(testObject, new object[] {});
		MethodInfo_t* L_50 = V_7;
		RuntimeObject* L_51 = V_11;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_52 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)0);
		NullCheck(L_50);
		RuntimeObject* L_53;
		L_53 = MethodBase_Invoke_mEEF3218648F111A8C338001A7804091A0747C826(L_50, L_51, L_52, NULL);
		// Debug.Log(helper.ToString());
		StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* L_54 = __this->___helper_4;
		NullCheck(L_54);
		String_t* L_55;
		L_55 = VirtualFuncInvoker0< String_t* >::Invoke(3 /* System.String System.Object::ToString() */, L_54);
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_Log_m86567BCF22BBE7809747817453CACA0E41E68219(L_55, NULL);
		// helper.Clear();
		StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* L_56 = __this->___helper_4;
		NullCheck(L_56);
		StringTestOutputHelper_Clear_mCA2A3B194BD449A39A02B21AAEA20484309C2815(L_56, NULL);
		// skipMethods = true;
		V_0 = (bool)1;
		// break;
		goto IL_0158;
	}

IL_013c:
	{
	}

IL_013d:
	{
		// if (skipMethods) break;
		bool L_57 = V_0;
		V_12 = L_57;
		bool L_58 = V_12;
		if (!L_58)
		{
			goto IL_0146;
		}
	}
	{
		// if (skipMethods) break;
		goto IL_0158;
	}

IL_0146:
	{
		int32_t L_59 = V_6;
		V_6 = ((int32_t)il2cpp_codegen_add(L_59, 1));
	}

IL_014d:
	{
		// foreach (MethodInfo mi in t.GetMethods()) {
		int32_t L_60 = V_6;
		MethodInfoU5BU5D_tDF3670604A0AECF814A0B0BA09B91FBF0D6A3265* L_61 = V_5;
		NullCheck(L_61);
		if ((((int32_t)L_60) < ((int32_t)((int32_t)(((RuntimeArray*)L_61)->max_length)))))
		{
			goto IL_0067;
		}
	}

IL_0158:
	{
	}

IL_0159:
	{
		int32_t L_62 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_62, 1));
	}

IL_015e:
	{
		// foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
		int32_t L_63 = V_2;
		TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* L_64 = V_1;
		NullCheck(L_64);
		if ((((int32_t)L_63) < ((int32_t)((int32_t)(((RuntimeArray*)L_64)->max_length)))))
		{
			goto IL_0014;
		}
	}
	{
		// }
		return;
	}
}
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ExampleRunner__ctor_m81EE008B68E3F921280655A9BF87148073CA924E (ExampleRunner_tF444DF9613423B29748C32062F07F3469F498BE1* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// private StringTestOutputHelper helper = new StringTestOutputHelper();
		StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* L_0 = (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29*)il2cpp_codegen_object_new(StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringTestOutputHelper__ctor_m230467E77F649FE332A6A42405D4125B3D3F2BFD(L_0, NULL);
		__this->___helper_4 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___helper_4), (void*)L_0);
		// public string[] disabledTests = new string[] {};
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_1 = (StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*)(StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*)SZArrayNew(StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var, (uint32_t)0);
		__this->___disabledTests_5 = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___disabledTests_5), (void*)L_1);
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
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::WriteLine()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringTestOutputHelper_WriteLine_m62B68D1AD3D788A87A6DBCAB45CD5DCBCD12CFB3 (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, const RuntimeMethod* method) 
{
	{
		// output.AppendLine();
		StringBuilder_t* L_0 = __this->___output_0;
		NullCheck(L_0);
		StringBuilder_t* L_1;
		L_1 = StringBuilder_AppendLine_m3BC704C4E6A8531027D8C9287D0AB2AA0188AC4E(L_0, NULL);
		// }
		return;
	}
}
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::WriteLine(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringTestOutputHelper_WriteLine_mEE9912C35FE713100704B8A8B92E92C7981C5DD9 (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// output.AppendLine(value);
		StringBuilder_t* L_0 = __this->___output_0;
		String_t* L_1 = ___value0;
		NullCheck(L_0);
		StringBuilder_t* L_2;
		L_2 = StringBuilder_AppendLine_mF75744CE941C63E33188E22E936B71A24D3CBF88(L_0, L_1, NULL);
		// }
		return;
	}
}
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::WriteLine(System.String,System.Object[])
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringTestOutputHelper_WriteLine_mE3837EA67D411B8ABBF742B538C073EF5EDBADE1 (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, String_t* ___format0, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___args1, const RuntimeMethod* method) 
{
	{
		// output.AppendFormat(format, args);
		StringBuilder_t* L_0 = __this->___output_0;
		String_t* L_1 = ___format0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___args1;
		NullCheck(L_0);
		StringBuilder_t* L_3;
		L_3 = StringBuilder_AppendFormat_m14CB447291E6149BCF32E5E37DA21514BAD9C151(L_0, L_1, L_2, NULL);
		// output.AppendLine();
		StringBuilder_t* L_4 = __this->___output_0;
		NullCheck(L_4);
		StringBuilder_t* L_5;
		L_5 = StringBuilder_AppendLine_m3BC704C4E6A8531027D8C9287D0AB2AA0188AC4E(L_4, NULL);
		// }
		return;
	}
}
// System.String YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::ToString()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* StringTestOutputHelper_ToString_m9F7841ED549B921280456A86276BA21A8AAF2A7C (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, const RuntimeMethod* method) 
{
	String_t* V_0 = NULL;
	{
		// public override string ToString() { return output.ToString(); }
		StringBuilder_t* L_0 = __this->___output_0;
		NullCheck(L_0);
		String_t* L_1;
		L_1 = VirtualFuncInvoker0< String_t* >::Invoke(3 /* System.String System.Object::ToString() */, L_0);
		V_0 = L_1;
		goto IL_000f;
	}

IL_000f:
	{
		// public override string ToString() { return output.ToString(); }
		String_t* L_2 = V_0;
		return L_2;
	}
}
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::Clear()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringTestOutputHelper_Clear_mCA2A3B194BD449A39A02B21AAEA20484309C2815 (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringBuilder_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// public          void   Clear()    { output = new StringBuilder(); }
		StringBuilder_t* L_0 = (StringBuilder_t*)il2cpp_codegen_object_new(StringBuilder_t_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringBuilder__ctor_m1D99713357DE05DAFA296633639DB55F8C30587D(L_0, NULL);
		__this->___output_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___output_0), (void*)L_0);
		// public          void   Clear()    { output = new StringBuilder(); }
		return;
	}
}
// System.Void YamlDotNet.Samples.Helpers.ExampleRunner/StringTestOutputHelper::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringTestOutputHelper__ctor_m230467E77F649FE332A6A42405D4125B3D3F2BFD (StringTestOutputHelper_t9B2FD6CF8769997D9A503B222993566748E58D29* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringBuilder_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		// private StringBuilder output = new StringBuilder();
		StringBuilder_t* L_0 = (StringBuilder_t*)il2cpp_codegen_object_new(StringBuilder_t_il2cpp_TypeInfo_var);
		NullCheck(L_0);
		StringBuilder__ctor_m1D99713357DE05DAFA296633639DB55F8C30587D(L_0, NULL);
		__this->___output_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___output_0), (void*)L_0);
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
// System.String YamlDotNet.Samples.Helpers.SampleAttribute::get_DisplayName()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SampleAttribute_get_DisplayName_m10A912CE310DFE82E23C1C51096136E724B2402E (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, const RuntimeMethod* method) 
{
	{
		// public string DisplayName { get; set; }
		String_t* L_0 = __this->___U3CDisplayNameU3Ek__BackingField_0;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Helpers.SampleAttribute::set_DisplayName(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SampleAttribute_set_DisplayName_m586EBF5BCAA56C4DE2D44BB9255F77BAED1D354E (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string DisplayName { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CDisplayNameU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CDisplayNameU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
// System.String YamlDotNet.Samples.Helpers.SampleAttribute::get_Description()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SampleAttribute_get_Description_m512B7F186A5F27E873A27EDE67129475F74E1F11 (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, const RuntimeMethod* method) 
{
	{
		// public string Description { get; set; }
		String_t* L_0 = __this->___U3CDescriptionU3Ek__BackingField_1;
		return L_0;
	}
}
// System.Void YamlDotNet.Samples.Helpers.SampleAttribute::set_Description(System.String)
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SampleAttribute_set_Description_mDFBC956DE79713EDEEF62661683203699749B81C (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string Description { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CDescriptionU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CDescriptionU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
// System.Void YamlDotNet.Samples.Helpers.SampleAttribute::.ctor()
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SampleAttribute__ctor_mDC4D44D6FFC4A5D93E45E378B499B416B1DD99B8 (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR List_1_t125631862BDE1BA29850B762D409E23C07F29B40* Order_get_Items_m04CBA6598E62087D34F49A24CFD74B42679DAE98_inline (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public List<OrderItem> Items { get; set; }
		List_1_t125631862BDE1BA29850B762D409E23C07F29B40* L_0 = __this->___U3CItemsU3Ek__BackingField_3;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* OrderItem_get_PartNo_m27D22430CD8BED232A06E432F42E536DB38F3A61_inline (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
{
	{
		// public string PartNo { get; set; }
		String_t* L_0 = __this->___U3CPartNoU3Ek__BackingField_0;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t OrderItem_get_Quantity_mFF50B28BBA27E7FC844280AE7E024B8FF59E8164_inline (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
{
	{
		// public int Quantity { get; set; }
		int32_t L_0 = __this->___U3CQuantityU3Ek__BackingField_3;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F OrderItem_get_Price_mFD13B646F706F3E18735C3BAADB775C06AC88BFC_inline (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
{
	{
		// public decimal Price { get; set; }
		Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_0 = __this->___U3CPriceU3Ek__BackingField_2;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* OrderItem_get_Descrip_mC37419903157E1BBC8BF76B362D32D3E04A0D620_inline (OrderItem_tCCCD1F4ADC7BD7D663FBC884F3D5B2A58FED13F4* __this, const RuntimeMethod* method) 
{
	{
		// public string Descrip { get; set; }
		String_t* L_0 = __this->___U3CDescripU3Ek__BackingField_1;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Address_tD85CB003A9782A8A69C513245758DEC28AA76024* Order_get_ShipTo_m64A0D7EB0173579B9D123199F7A658FF6956DBD9_inline (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public Address ShipTo { get; set; }
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_0 = __this->___U3CShipToU3Ek__BackingField_5;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* Address_get_Street_mE2821ED2C5BC33B0F9643B86B64C5204930B37A7_inline (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) 
{
	{
		// public string Street { get; set; }
		String_t* L_0 = __this->___U3CStreetU3Ek__BackingField_0;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* Address_get_City_m8BADE3101BEEDDFFF69CC7FDE49BCBBB25B476F4_inline (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) 
{
	{
		// public string City { get; set; }
		String_t* L_0 = __this->___U3CCityU3Ek__BackingField_1;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* Address_get_State_m265F188BA17A5D8CAC8A3B89A114FC326E994908_inline (Address_tD85CB003A9782A8A69C513245758DEC28AA76024* __this, const RuntimeMethod* method) 
{
	{
		// public string State { get; set; }
		String_t* L_0 = __this->___U3CStateU3Ek__BackingField_2;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Address_tD85CB003A9782A8A69C513245758DEC28AA76024* Order_get_BillTo_m9263CDEAEC8DA36B2104C14B612CF8B3A2424EFC_inline (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public Address BillTo { get; set; }
		Address_tD85CB003A9782A8A69C513245758DEC28AA76024* L_0 = __this->___U3CBillToU3Ek__BackingField_4;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* Order_get_SpecialDelivery_m29487155B8D9FD5468C08389480733E3E179D94B_inline (Order_t455515D55F55269AEBF8AD3EF7162547E7AA0A53* __this, const RuntimeMethod* method) 
{
	{
		// public string SpecialDelivery { get; set; }
		String_t* L_0 = __this->___U3CSpecialDeliveryU3Ek__BackingField_6;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* YamlDocument_get_RootNode_mFDA2A8A64BF409D91B71A3F0C1C96722AF279D30_inline (YamlDocument_tF61A99B79C0F9627DA9492381E0D86BF934CBFEE* __this, const RuntimeMethod* method) 
{
	{
		// public YamlNode RootNode { get; private set; }
		YamlNode_t348B1AC9822EA3D8717FB2B81DB37BDD415DFDBA* L_0 = __this->___U3CRootNodeU3Ek__BackingField_0;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* YamlScalarNode_get_Value_m615F96CDDF044477CF47679024333453EEF98711_inline (YamlScalarNode_t4163B47F7F956F2C83C80FF6E61ECDB53F9B5648* __this, const RuntimeMethod* method) 
{
	{
		// public string? Value { get; set; }
		String_t* L_0 = __this->___U3CValueU3Ek__BackingField_6;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Address_set_street_m82E05D93979FB39D928C9AB7831517EB2AB07A89_inline (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string street { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CstreetU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CstreetU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Address_set_city_mACF46014990F092127EE327EAF11315F91CF7068_inline (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string city { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CcityU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CcityU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Address_set_state_m1434642D6CA9506A144D13B027C05046DE39E0F4_inline (Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string state { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CstateU3Ek__BackingField_2 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CstateU3Ek__BackingField_2), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_receipt_m2CD41C0DDE1FC9E896EC613C5547907EE1E23295_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string receipt { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CreceiptU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CreceiptU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_date_mDC90FDE684124E33C1EC1D615BCBA1BCE397F3DC_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___value0, const RuntimeMethod* method) 
{
	{
		// public DateTime date { get; set; }
		DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D L_0 = ___value0;
		__this->___U3CdateU3Ek__BackingField_1 = L_0;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Customer_set_given_m5F124768B94E969B24A8F47D6B023888D0DA16D3_inline (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string given { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CgivenU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CgivenU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Customer_set_family_m8565386D9A7735C1A82A1A05A6A6D2147A7ADF12_inline (Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string family { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CfamilyU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CfamilyU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_customer_m4DB78B0D39289ECC0F1007C4FF166FB6BF377B89_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* ___value0, const RuntimeMethod* method) 
{
	{
		// public Customer customer { get; set; }
		Customer_t8A32EF9D480D0330E64BB6411E0DA1A850BBCC4B* L_0 = ___value0;
		__this->___U3CcustomerU3Ek__BackingField_2 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CcustomerU3Ek__BackingField_2), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Item_set_part_no_m0CE760B9F26A06ABF2BC7AB2B1B6430E317423D4_inline (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string part_no { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3Cpart_noU3Ek__BackingField_0 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3Cpart_noU3Ek__BackingField_0), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Item_set_descrip_m0B38A9144A563C9FC6A4E850C448F6249C9B4C83_inline (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string descrip { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CdescripU3Ek__BackingField_1 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CdescripU3Ek__BackingField_1), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Item_set_price_mAC9D0E64ABBC779377E02DCD3DBD29964DCB7C33_inline (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F ___value0, const RuntimeMethod* method) 
{
	{
		// public decimal price { get; set; }
		Decimal_tDA6C877282B2D789CF97C0949661CC11D643969F L_0 = ___value0;
		__this->___U3CpriceU3Ek__BackingField_2 = L_0;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Item_set_quantity_mA66BE7C18DA97A2F5F894510F17294506BCA3520_inline (Item_t9986835E2D3595D8A267690EAA8AB0AF6DA75274* __this, int32_t ___value0, const RuntimeMethod* method) 
{
	{
		// public int quantity { get; set; }
		int32_t L_0 = ___value0;
		__this->___U3CquantityU3Ek__BackingField_3 = L_0;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_items_m9D9FB2BE4E4C12C4F585B69EA91A8B43C593F1B3_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* ___value0, const RuntimeMethod* method) 
{
	{
		// public Item[] items { get; set; }
		ItemU5BU5D_tFD83E617B5B291A3A48F9A3C8F18FED3231EA27A* L_0 = ___value0;
		__this->___U3CitemsU3Ek__BackingField_3 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CitemsU3Ek__BackingField_3), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_bill_to_m2B8A801F8C633D488890CD4AEDD5EE1E14244A52_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* ___value0, const RuntimeMethod* method) 
{
	{
		// public Address bill_to { get; set; }
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_0 = ___value0;
		__this->___U3Cbill_toU3Ek__BackingField_4 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3Cbill_toU3Ek__BackingField_4), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_ship_to_mF15187ACF9688D1A226755C41B16861D53DE4BD2_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* ___value0, const RuntimeMethod* method) 
{
	{
		// public Address ship_to { get; set; }
		Address_tF253509D9E28C08161AA1DE4D06B328A488DCB22* L_0 = ___value0;
		__this->___U3Cship_toU3Ek__BackingField_5 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3Cship_toU3Ek__BackingField_5), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Receipt_set_specialDelivery_m23A683515B0AE248B3B3A159ACC04EAF330693C6_inline (Receipt_tE506B8843866A1445C321C463687A56F99457821* __this, String_t* ___value0, const RuntimeMethod* method) 
{
	{
		// public string specialDelivery { get; set; }
		String_t* L_0 = ___value0;
		__this->___U3CspecialDeliveryU3Ek__BackingField_6 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CspecialDeliveryU3Ek__BackingField_6), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* SampleAttribute_get_DisplayName_m10A912CE310DFE82E23C1C51096136E724B2402E_inline (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, const RuntimeMethod* method) 
{
	{
		// public string DisplayName { get; set; }
		String_t* L_0 = __this->___U3CDisplayNameU3Ek__BackingField_0;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* SampleAttribute_get_Description_m512B7F186A5F27E873A27EDE67129475F74E1F11_inline (SampleAttribute_tCB60C9F0AEAE2206AE3CE4A87C7D1E247CCDA8BF* __this, const RuntimeMethod* method) 
{
	{
		// public string Description { get; set; }
		String_t* L_0 = __this->___U3CDescriptionU3Ek__BackingField_1;
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
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* KeyValuePair_2_get_Key_mBD8EA7557C27E6956F2AF29DA3F7499B2F51A282_gshared_inline (KeyValuePair_2_tFC32D2507216293851350D29B64D79F950B55230* __this, const RuntimeMethod* method) 
{
	{
		RuntimeObject* L_0 = (RuntimeObject*)__this->___key_0;
		return L_0;
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
