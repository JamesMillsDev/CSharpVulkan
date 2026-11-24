using System.Runtime.InteropServices;

using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Engine
{
	public unsafe class VkContext : IDisposable
	{
		private static List<string> EnabledInstanceExtensions
		{
			get
			{
				List<string> enabledInstanceExtensions = [ExtDebugUtils.ExtensionName];
				if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					enabledInstanceExtensions.Add("VK_LAYER_portability_enumeration");
				}
				
				return enabledInstanceExtensions;
			}
		}
		
		private static List<string> EnabledLayers => ["VK_LAYER_KHRONOS_validation"];
		
		private readonly VkInstance? instance;
		private readonly VkDevice? device;
		private readonly VkQueue? mainQueue;

		public VkContext()
		{
			try
			{
				VkArrayStrings arrayStrings = new(EnabledLayers, EnabledInstanceExtensions);
				
				instance = new VkInstance(ref arrayStrings);
				
				uint mainQueueIndex = 0u;
				device = new VkDevice(ref arrayStrings, instance, ref mainQueueIndex);

				instance.GetApi().GetDeviceQueue(device, mainQueueIndex, 0, out Queue mainVkQueue);
				mainQueue = new VkQueue(mainQueueIndex, mainVkQueue);
				
				arrayStrings.Dispose();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public void Dispose()
		{
			device?.Dispose();
			instance?.Dispose();
		}
	}
}