using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static unsafe class LibretroVulkan {


    // vulkan
    private delegate void VulcanImageCBHandler(void* vkImage);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_set_vulkan_image_cb(VulcanImageCBHandler _vulcanImageCBHandler);

    public static void Init()
    {
        wrapper_set_vulkan_image_cb(new VulcanImageCBHandler(VulcanImageCB));
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
