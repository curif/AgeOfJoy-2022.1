using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static unsafe class LibretroVulkan
{

    static IntPtr vkInstance;
    static IntPtr vkPhysicalDevice;
    static IntPtr vkDevice;
    static IntPtr pfnvkGetInstanceProcAddr;
    static VulcanImageCBHandler vulcanImageCBHandler;

    // vulkan
    private delegate void VulcanImageCBHandler(void* vkImage);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_init_vulkan(IntPtr vkInstance, IntPtr vkPhysicalDevice, IntPtr vkDevice, IntPtr vkGetInstanceProcAddr, VulcanImageCBHandler vulcanImageCBHandler);

    public static void Init(IntPtr _vkInstance, IntPtr _vkPhysicalDevice, IntPtr _vkDevice, IntPtr _pfnvkGetInstanceProcAddr)
    {
        ConfigManager.WriteConsole("[LibretroVulkan.Init]" +
            $" vkInstance:{_vkInstance.ToString("x16")}" +
            $" vkPhysicalDevice:{_vkPhysicalDevice.ToString("x16")}" +
            $" vkDevice:{_vkDevice.ToString("x16")}" +
            $" pfnvkGetInstanceProcAddr:{_pfnvkGetInstanceProcAddr.ToString("x16")}");
        vkInstance = _vkInstance;
        vkPhysicalDevice = _vkPhysicalDevice;
        vkDevice = _vkDevice;
        pfnvkGetInstanceProcAddr = _pfnvkGetInstanceProcAddr;
        vulcanImageCBHandler = new VulcanImageCBHandler(VulcanImageCB);
    }

    public static void WrapperInit()
    {
        ConfigManager.WriteConsole("[LibretroVulkan.WrapperInit]" +
            $" vkInstance:{vkInstance.ToString("x16")}" +
            $" vkPhysicalDevice:{vkPhysicalDevice.ToString("x16")}" +
            $" vkDevice:{vkDevice.ToString("x16")}" +
            $" pfnvkGetInstanceProcAddr:{pfnvkGetInstanceProcAddr.ToString("x16")}");
        wrapper_init_vulkan(vkInstance, vkPhysicalDevice, vkDevice, pfnvkGetInstanceProcAddr, vulcanImageCBHandler);
    }

    [AOT.MonoPInvokeCallback(typeof(VulcanImageCBHandler))]
    static void VulcanImageCB(void* vkImage)
    {
        ConfigManager.WriteConsole($"[LibRetroMameCore.VulcanImageCB start]");
        //GameTexture.UpdateExternalTexture((IntPtr)vkImage);
        //Texture2D vulkanTexture = Texture2D.CreateExternalTexture(640, 480, TextureFormat.RGB565, false, true, (IntPtr)vkImage);
        ConfigManager.WriteConsole($"[LibRetroMameCore.VulcanImageCB ok]");
    }
}
