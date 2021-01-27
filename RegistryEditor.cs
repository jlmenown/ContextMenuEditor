using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContextMenuEditor
{
    /// <summary>
    /// A limited-scope registry editor, which gets or sets custom desktop context menu items. 
    /// Custom items are managed by identifying them in the registry with a Guid
    /// </summary>
    class RegistryEditor
    {
        const string rootRegistryPath = @"Directory\background\shell";
        const string commandKeyName = "command";
        const string commandValueName = "";

        private string managedKeyString;

        /// <summary>
        /// Constructs a RegistryEditor with the given management key
        /// </summary>
        /// <param name="managedKeyId">A Guid used to identify registry keys managed by this program</param>
        public RegistryEditor(Guid managedKeyId)
        {
            managedKeyString = managedKeyId.ToString();
        }

        /// <summary>
        /// Reads the registry and returns all managed context menu items
        /// </summary>
        /// <returns>
        /// A Dictionary of managed context menu items 
        /// with Keys corresponding to context menu item names
        /// and Values corresponding to context menu item commands
        /// </returns>
        public Dictionary<string, string> Get()
        {
            // Create an empty dictionary
            var keys = new Dictionary<string, string>();

            // For each subkey in the registry root key,
            // (subkeys correspond to individual context menu items)
            using (RegistryKey rootKey = Registry.ClassesRoot.OpenSubKey(rootRegistryPath))
            foreach (string subKeyName in rootKey.GetSubKeyNames())
            using (RegistryKey subKey = rootKey.OpenSubKey(subKeyName))
            using (RegistryKey commandKey = subKey.OpenSubKey(commandKeyName))
            {
                // If the subkey has a "command" key (which Windows uses to define the *target* executable for context menu items),
                // And the subkey contains the Guid identifier signalling that it's managed by this program,
                // And the "command" key has a string value (this should never not be the case),
                if (commandKey != null &&
                    subKey.GetValue(managedKeyString) != null &&
                    commandKey.GetValueKind(commandValueName) == RegistryValueKind.String)
                {
                    // Get the "command" value as a string
                    // (this string is the path of the executable that the context menu launches)
                    string commandValue = commandKey.GetValue(commandValueName) as string;

                    // Add the value and the name of the subkey to the dictionary
                    // (ex: subKeyName = "Notepad", commandValue = "C:\Windows\Notepad.exe"
                    keys.Add(subKeyName, commandValue);
                }
            }

            // Return the completed dictionary
            return keys;
        }

        /// <summary>
        /// Writes the given managed context menu items to the registry,
        /// and deletes any managed items that are present in the registry but not in the argument
        /// </summary>
        /// <param name="content">
        /// A Dictionary of managed context menu items 
        /// with Keys corresponding to context menu item names
        /// and Values corresponding to context menu item commands
        /// </param>
        public void Set(Dictionary<string, string> content)
        {
            // Get() all existing managed context menu items
            Dictionary<string, string> oldItems = this.Get();

            // Compare the existing items to the argument to find old items to remove from the registry
            Dictionary<string, string> itemsToRemove = oldItems
                .Where(item => !content.Contains(item))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Compare the existing items to the argument to find new items to add to the registry
            Dictionary<string, string> itemsToCreate = content
                .Where(item => !oldItems.Contains(item))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // If there are no new or obsolete items, stop now
            if (itemsToRemove.Count == 0
                && itemsToCreate.Count == 0)
                return;

            // Open the root registry key
            using (RegistryKey rootKey = Registry.ClassesRoot.OpenSubKey(rootRegistryPath, true))
            {
                // For each obsolete item,
                foreach (var item in itemsToRemove)
                {
                    // Confirm that its registry key has our Guid signifier
                    // (this should always be true - just a confirmation to prevent damaging the registry)
                    bool isManaged;
                    using (RegistryKey subKey = rootKey.OpenSubKey(item.Key))
                        isManaged = subKey.GetValue(managedKeyString) != null;

                    // If so, delete the key
                    if (isManaged)
                        rootKey.DeleteSubKeyTree(item.Key);
                }

                // For each new item,
                foreach (var item in itemsToCreate)
                {
                    // Create a new registry key with the desired name
                    using (RegistryKey subKey = rootKey.CreateSubKey(item.Key))
                    {
                        // Set its Guid signifier so we can find it again
                        subKey.SetValue(managedKeyString, string.Empty, RegistryValueKind.String);

                        // Create a "command" key (required by Windows) containing the target executable for the context meny item
                        using (RegistryKey commandKey = subKey.CreateSubKey(commandKeyName))
                            commandKey.SetValue(commandValueName, item.Value, RegistryValueKind.String);
                    }
                }
            }
        }
    }
}
