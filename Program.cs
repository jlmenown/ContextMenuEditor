using System;
using System.Windows.Forms;

namespace ContextMenuEditor
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Create a ManagedRegistryEditor object to read and write context menu items
            // Provide it a key to identify managed items; key defined in settings file
            RegistryEditor editor = 
                new RegistryEditor(Properties.Settings.Default.ManagedRegistryKeyID);

            // Construct a RegistryForm
            RegistryForm form = new RegistryForm();

            // Set the form contents to display the current managed context menu items
            form.Set(editor.Get());

            // Subscribe to its "OnSave" event. 
            // Upon firing get the contents of the form and pass them to the registry editor
            form.OnSave += (o, a) => editor.Set(form.Get());

            // Pass control to the form
            form.Show();
            Application.Run(form);
        }
    }
}
