using Silk.NET.Core.Native;

namespace Engine
{
	public unsafe class VkArrayStrings(List<string> enabledLayers, List<string> enabledInstanceExtensions)
		: IDisposable
	{
		public struct PointerString
		{
			public byte** value;
			public uint length;
		}

		public readonly PointerString ppEnabledLayers = new()
		{
			value = (byte**) SilkMarshal.StringArrayToPtr([.. enabledLayers]),
			length = (uint) enabledLayers.Count
		};

		public readonly PointerString ppEnabledInstanceExtensions = new()
		{
			value = (byte**) SilkMarshal.StringArrayToPtr([.. enabledInstanceExtensions]),
			length = (uint) enabledInstanceExtensions.Count
		};

		public PointerString ppEnabledDeviceExtensions = new();

		public void Dispose()
		{
			SilkMarshal.Free((nint) ppEnabledDeviceExtensions.value);
			SilkMarshal.Free((nint) ppEnabledLayers.value);
			SilkMarshal.Free((nint) ppEnabledInstanceExtensions.value);
		}
	}
}