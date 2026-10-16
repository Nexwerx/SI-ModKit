using System;

namespace SIModKit
{
    // Uses the game's supported ModBehaviour settings API. Does not access the filesystem.
    internal static class EnvironmentProbe
    {
        private const string TestKey = "SIModKit.StorageProbe.v1";
        internal static string TestLocalStorage()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null)
                return "NOT READY: The SI ModKit runtime has not been initialized.";
            try
            {
                string value = "SI-ModKit-test-" + Guid.NewGuid().ToString("N");
                owner.SaveSetting(TestKey, value);
                string actual = owner.LoadSetting(TestKey, "");
                if (actual != value) return "FAIL: The saved value differs from the immediate readback.";
                return "PASS: Game-managed setting saved and read back. Restart Software Inc. and press Check saved setting to validate persistence across launches.";
            }
            catch (Exception ex) { return "FAIL: " + ex.Message; }
        }

        internal static string TestSettingAfterRestart()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null)
                return "NOT READY: The SI ModKit runtime has not been initialized.";
            try
            {
                string value = owner.LoadSetting(TestKey, "");
                if (string.IsNullOrEmpty(value)) return "NOT FOUND: Run Test write + read first, then restart the game.";
                if (!value.StartsWith("SI-ModKit-test-")) return "FAIL: Unexpected diagnostic setting value.";
                return "PASS: Game-managed diagnostic setting exists: " + value;
            }
            catch (Exception ex) { return "FAIL: " + ex.Message; }
        }

        private const string CapacityKey = "SIModKit.StorageCapacityProbe.v1";
        private const string CapacitySizeKey = "SIModKit.StorageCapacityProbeSize.v1";

        internal static string TestStorageCapacity()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null)
                return "NOT READY: The SI ModKit runtime has not been initialized.";

            // This is deliberately a capacity-floor test, not a destructive maximum hunt.
            // ASCII payloads also correspond closely to bytes in the text settings file.
            int[] sizes = { 1024, 8192, 32768, 131072, 524288, 1048576 };
            int largest = 0;
            try
            {
                for (int i = 0; i < sizes.Length; i++)
                {
                    int size = sizes[i];
                    string payload = MakeCapacityPayload(size);
                    owner.SaveSetting(CapacityKey, payload);
                    string actual = owner.LoadSetting(CapacityKey, "");
                    if (actual != payload)
                    {
                        owner.SaveSetting(CapacitySizeKey, largest.ToString());
                        return "STOP: " + size + " characters did not round-trip. Largest confirmed size: " + largest + ".";
                    }
                    largest = size;
                }

                owner.SaveSetting(CapacitySizeKey, largest.ToString());
                return "PASS: At least " + largest + " ASCII characters round-tripped. Restart the game and run Verify capacity after restart before clearing the test data.";
            }
            catch (Exception ex)
            {
                owner.SaveSetting(CapacitySizeKey, largest.ToString());
                return "STOP after " + largest + " confirmed characters: " + ex.Message;
            }
        }

        internal static string TestStorageCapacityAfterRestart()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null)
                return "NOT READY: The SI ModKit runtime has not been initialized.";
            try
            {
                string sizeText = owner.LoadSetting(CapacitySizeKey, "0");
                int size;
                if (!int.TryParse(sizeText, out size) || size <= 0)
                    return "NOT FOUND: Run the capacity-floor test first, then restart Software Inc.";
                string expected = MakeCapacityPayload(size);
                string actual = owner.LoadSetting(CapacityKey, "");
                if (actual != expected)
                    return "FAIL: The persisted capacity payload did not match after restart (expected " + size + " characters).";
                return "PASS: " + size + " characters persisted intact across a full game restart.";
            }
            catch (Exception ex) { return "FAIL: " + ex.Message; }
        }

        internal static string ClearStorageCapacityTest()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null)
                return "NOT READY: The SI ModKit runtime has not been initialized.";
            try
            {
                owner.DeleteSetting(CapacityKey);
                owner.DeleteSetting(CapacitySizeKey);
                return "CLEARED: Capacity-test payload and size marker removed from SI-ModKit settings.";
            }
            catch (Exception ex) { return "FAIL: " + ex.Message; }
        }

        private static string MakeCapacityPayload(int size)
        {
            string prefix = "SIMODKIT-CAPACITY-v1|" + size + "|";
            string suffix = "|END";
            int body = size - prefix.Length - suffix.Length;
            if (body < 0) body = 0;
            return prefix + new string('X', body) + suffix;
        }
    }
}
