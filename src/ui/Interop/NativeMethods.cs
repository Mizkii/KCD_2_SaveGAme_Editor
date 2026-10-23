using System;
using System.Runtime.InteropServices;

namespace KCD2SaveEditor.UI.Interop
{
    public enum Kcd2Result : int
    {
        Ok = 0,
        InvalidArgument = -1,
        FileNotFound = -2,
        FileReadFailed = -3,
        FileWriteFailed = -4,
        InvalidContainer = -5,
        CompressionFailed = -6,
        DecompressionFailed = -7,
        ChecksumMismatch = -8,
        ParsingFailed = -9,
        BufferTooSmall = -10,
        NotInitialized = -11,
        Unknown = -99
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    public struct Kcd2SaveMetadata
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string SaveName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string PlaylineId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string GameVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Timestamp;

        public uint SaveVersion;
        public uint CharacterLevel;
        public ulong PlaytimeSeconds;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Kcd2PlayerStats
    {
        public float Health;
        public float MaxHealth;
        public float Stamina;
        public float MaxStamina;
        public float Energy;
        public float Nourishment;

        public long Groschen;

        public int Strength;
        public int Agility;
        public int Vitality;
        public int Speech;
        public int Charisma;
        public int Speed;

        public int CharacterLevel;

        public int Conspicuousness;
        public int Noise;
        public int Visibility;

        public float CarryWeight;
        public float MaxCarryWeight;

        public int Warfare;
        public int Sword;
        public int Defense;
        public int Stealth;
        public int Lockpicking;
        public int Pickpocketing;
        public int Alchemy;
        public int Marksmanship;
        public int Horsemanship;
        public int Houndmaster;
        public int Scholarship;
        public int Blacksmithing;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Kcd2HorseStats
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] HorseNameBytes;

        public float Health;
        public float MaxHealth;
        public float Stamina;
        public float MaxStamina;
        public float Speed;
        public float Capacity;
        public float Courage;

        public string HorseName
        {
            get => HorseNameBytes != null ? System.Text.Encoding.UTF8.GetString(HorseNameBytes).TrimEnd('\0') : string.Empty;
            set => SetHorseName(value);
        }

        public void SetHorseName(string? name)
        {
            HorseNameBytes = new byte[64];
            if (!string.IsNullOrEmpty(name))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(name);
                Array.Copy(bytes, HorseNameBytes, Math.Min(bytes.Length, 63));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Kcd2InventoryItem
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] ItemIdBytes;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] ItemUiNameBytes;

        public ulong UniqueId;
        public int Amount;
        public float Condition;
        public int SlotId;
        public uint Flags;

        public string ItemId
        {
            get => ItemIdBytes != null ? System.Text.Encoding.UTF8.GetString(ItemIdBytes).TrimEnd('\0') : string.Empty;
            set => SetItemId(value);
        }

        public string ItemUiName
        {
            get => ItemUiNameBytes != null ? System.Text.Encoding.UTF8.GetString(ItemUiNameBytes).TrimEnd('\0') : string.Empty;
            set => SetItemUiName(value);
        }

        public void SetItemId(string? id)
        {
            ItemIdBytes = new byte[64];
            if (!string.IsNullOrEmpty(id))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(id);
                Array.Copy(bytes, ItemIdBytes, Math.Min(bytes.Length, 63));
            }
        }

        public void SetItemUiName(string? name)
        {
            ItemUiNameBytes = new byte[128];
            if (!string.IsNullOrEmpty(name))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(name);
                Array.Copy(bytes, ItemUiNameBytes, Math.Min(bytes.Length, 127));
            }
        }
    }

    public static class NativeMethods
    {
        private const string DllName = "KCD2SaveCore.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern Kcd2Result kcd2_save_open(string filePath, out IntPtr outHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern Kcd2Result kcd2_save_write(IntPtr handle, string? destFilePath, int createBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void kcd2_save_close(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_get_metadata(IntPtr handle, out Kcd2SaveMetadata outMetadata);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_get_manifest_xml(IntPtr handle, [Out] byte[]? buffer, uint bufferSize, out uint outSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_set_manifest_xml(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string xml);

        public static string? GetManifestXmlString(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return null;
            if (kcd2_save_get_manifest_xml(handle, null, 0, out uint reqSize) != Kcd2Result.Ok || reqSize == 0)
                return null;

            byte[] buf = new byte[reqSize];
            if (kcd2_save_get_manifest_xml(handle, buf, reqSize, out _) == Kcd2Result.Ok)
            {
                return System.Text.Encoding.UTF8.GetString(buf).TrimEnd('\0');
            }
            return null;
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_get_player_stats(IntPtr handle, out Kcd2PlayerStats outStats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_set_player_stats(IntPtr handle, in Kcd2PlayerStats inStats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_get_horse_stats(IntPtr handle, out Kcd2HorseStats outStats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_set_horse_stats(IntPtr handle, in Kcd2HorseStats inStats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_set_horse(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string horseId);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_get_all_quests(IntPtr handle, [Out] byte[]? buffer, uint bufferSize, out uint outSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_set_quest_status(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string questId, [MarshalAs(UnmanagedType.LPUTF8Str)] string status);

        public static string? GetAllQuestsString(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return null;
            if (kcd2_save_get_all_quests(handle, null, 0, out uint reqSize) != Kcd2Result.Ok || reqSize == 0)
                return null;

            byte[] buf = new byte[reqSize];
            if (kcd2_save_get_all_quests(handle, buf, reqSize, out _) == Kcd2Result.Ok)
            {
                return System.Text.Encoding.UTF8.GetString(buf).TrimEnd('\0');
            }
            return null;
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_get_inventory_count(IntPtr handle, out uint outCount);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_get_inventory_items(
            IntPtr handle,
            [In, Out] Kcd2InventoryItem[] items,
            uint maxCount,
            out uint outRetrieved);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_set_inventory_item(IntPtr handle, in Kcd2InventoryItem item);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_add_inventory_item(IntPtr handle, in Kcd2InventoryItem newItem);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_remove_inventory_item(IntPtr handle, ulong uniqueId);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_get_uint32_prop(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string propName, out uint outVal);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Kcd2Result kcd2_save_set_uint32_prop(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string propName, uint newVal);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        public static extern string kcd2_get_last_error_message();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPUTF8Str)]
        public static extern string kcd2_core_version();
    }
}
