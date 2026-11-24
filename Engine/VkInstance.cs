using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

using System.Runtime.InteropServices;

namespace Engine
{
	public unsafe class VkInstance : IDisposable
	{
		private static uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT severityFlags,
		                                  DebugUtilsMessageTypeFlagsEXT messageTypeFlags,
		                                  DebugUtilsMessengerCallbackDataEXT* pCallbackData,
		                                  void* pUserData)
		{
			string? message = Marshal.PtrToStringAnsi((nint) pCallbackData->PMessage);
			Console.WriteLine($"[Vulkan]: {severityFlags}: {message}");

			return Vk.False;
		}
		
		private readonly Vk vk = Vk.GetApi();
		private readonly Instance instance;
		private readonly DebugUtilsMessengerEXT debugUtilsMessenger;
		
		private readonly ExtDebugUtils? extDebugUtils;

		public VkInstance(ref VkArrayStrings arrayStrings)
		{
			InstanceCreateFlags flags = InstanceCreateFlags.None;

			if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				flags |= InstanceCreateFlags.EnumeratePortabilityBitKhr;
			}

			ApplicationInfo appInfo = new()
			{
				SType = StructureType.ApplicationInfo,
				ApiVersion = Vk.Version13
			};

			DebugUtilsMessengerCreateInfoEXT debugInfo = new()
			{
				SType = StructureType.DebugUtilsMessengerCreateInfoExt,
				MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt,
				MessageType = DebugUtilsMessageTypeFlagsEXT.ValidationBitExt | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt | DebugUtilsMessageTypeFlagsEXT.DeviceAddressBindingBitExt,
				PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT) DebugCallback
			};

			InstanceCreateInfo instanceInfo = new()
			{
				SType = StructureType.InstanceCreateInfo,
				Flags = flags,
				EnabledLayerCount = arrayStrings.ppEnabledLayers.length,
				PpEnabledLayerNames = arrayStrings.ppEnabledLayers.value,
				EnabledExtensionCount = arrayStrings.ppEnabledInstanceExtensions.length,
				PpEnabledExtensionNames = arrayStrings.ppEnabledInstanceExtensions.value,
				PApplicationInfo = &appInfo,
				PNext = &debugInfo
			};

			if(vk.CreateInstance(ref instanceInfo, null, out instance) != Result.Success)
			{
				throw new Exception("Instance could not be created");
			}

			if(!vk.TryGetInstanceExtension(instance, out extDebugUtils))
			{
				throw new Exception($"Could not get instance extension {ExtDebugUtils.ExtensionName}");
			}

			extDebugUtils!.CreateDebugUtilsMessenger(instance, ref debugInfo, null, out debugUtilsMessenger);
		}

		public void Dispose()
		{
			extDebugUtils?.DestroyDebugUtilsMessenger(instance, debugUtilsMessenger, null);
			vk.DestroyInstance(instance, null);
			vk.Dispose();
			extDebugUtils?.Dispose();
		}

		public Vk GetApi() => vk;

		public static implicit operator Instance(VkInstance instance) => instance.instance;
	}
}