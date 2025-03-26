using System;

namespace FindByGuid
{
    using System.Windows.Forms;

    using BIMVision;

    class FindByGuid : Plugin
    {
        private ApiWrapper api;

        private OBJECT_ID allId;
        private int button1;

        private IntPtr viewerHwnd;
        private Control viewer;

        private FindByGuidWindow form;


        public override void GetPluginInfo(ref PluginInfo info)
        {
            info.name = "Find By Guid";
            info.producer = "Matheus Henrique Sabadin";
            info.www = "www.linkedin.com/in/m-sab/";
            info.email = "-";
            info.description = "Plugin to find elements by its Guid";
            //info.help_directory = "";
        }

        public override byte[] GetPluginKey()
        {
            //byte[] plugin_key ={......};
            //return plugin_key;

            return null;
        }

        public override void OnLoad(PLUGIN_ID pid, bool registered, IntPtr viewerHwnd)
        {
            api = new ApiWrapper(pid);

            this.viewerHwnd = viewerHwnd;
            viewer = Control.FromHandle(viewerHwnd);

            api.OnModelLoad(onModelLoad);

            button1 = api.CreateButton(0, buttonClick);
            api.SetButtonText(button1, "Find By GUID", "Searches and selects the object with the given Global ID.");
        }

        public override void OnCallLimit()
        {
        }

        public override void OnUnload()
        {
        }

        public override void OnGuiColorsChange()
        {
        }

        private void onModelLoad()
        {
            allId = api.GetAllObjectsId();
        }

        private void buttonClick()
        {
            if (form == null)
            {
                form = new FindByGuidWindow(api);

                // behavior 1 - modal windows
                //if (!form.Visible)
                //    form.ShowDialog();
                // or
                //if (!form.Visible)
                //    form.ShowDialog(Control.FromHandle(viewerHwnd));

                // behavior 2 - separate window
                //if (!form.Visible)
                //    form.Show();

                // behavior 3 - separate window abowe viewer
                //if (!form.Visible)
                //    form.Show(Control.FromHandle(viewerHwnd));

                // behavior 4 - tool window in viewer
                form.FormBorderStyle = FormBorderStyle.FixedToolWindow; //FormBorderStyle.SizableToolWindow;
                form.ShowInTaskbar = false;
            }
            if(!form.Visible)
                form.Show(Control.FromHandle(viewerHwnd));

        }      
    }
}
