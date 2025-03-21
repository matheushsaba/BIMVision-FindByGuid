using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using BIMVision;


namespace FindByGuid
{
    public partial class FindByGuidWindow : Form
    {
        const string GUID_PROPERTY_SET_NAME = " Element Specific";
        const string GUID_PROPERTY_NAME = "Guid";

        private string userInput = string.Empty;
        private ApiWrapper api;

        public FindByGuidWindow(ApiWrapper apiWrapper)
        {
            api = apiWrapper;
            InitializeComponent();
        }

        private void FindGuid_Click(object sender, EventArgs e)
        {
            if (userInput == string.Empty)
            {
                return;
            }

            var objectsWithGuid = GetObjectsWithGuid(userInput);
            if (objectsWithGuid.Count == 0)
            {
                return;
            }
            else if (objectsWithGuid.Count == 1)
            {
                SetObjectStatus(objectsWithGuid[0]);
            }
            else
            {
                api.MessageBox("Alert", "There is more than object with the same Guid.", 0);
                SetObjectsStatus(objectsWithGuid);
            }

            Hide();
        }

        private void SetObjectStatus(OBJECT_ID objectId)
        {
            api.Select(objectId, true);
            api.ZoomToObjects(new OBJECT_ID[] { objectId }, 1);
            api.SetVisibleObject(objectId, VisibleType.vis_visible);
            api.SetObjectVisible(objectId, 1, false);
            api.SetObjectActive(objectId, true, false);
        }

        private void SetObjectsStatus(List<OBJECT_ID> objectIds)
        {
            var objectIdsArray = objectIds.ToArray();

            api.SelectMany(objectIdsArray, SelectType.select_with_openings);
            api.ZoomToObjects(objectIdsArray, 1);
            api.SetVisibleManyObjects(objectIdsArray, VisibleType.vis_visible);
            api.SetVisibleMany(objectIdsArray, VisibleType.vis_visible);

            foreach (var objectId in objectIdsArray)
            {
                api.SetObjectActive(objectId, true, false);
            }
        }

        private List<OBJECT_ID> GetObjectsWithGuid(string guid)
        {
            var objectsWithGuid = new List<OBJECT_ID>();
            var allObjectIds = api.GetAllObjects();
            foreach (var objectId in allObjectIds)
            {
                if (TryGetObjectGuid(objectId, out string objectGuid) && objectGuid == guid)
                {
                    objectsWithGuid.Add(objectId);
                }
            }

            return objectsWithGuid;
        }

        private bool TryGetObjectGuid(OBJECT_ID objectId, out string guid)
        {
            guid = string.Empty;

            // First try with guid FilterProperties
            var guidProperties = api.FilterProperties(objectId, GUID_PROPERTY_SET_NAME, GUID_PROPERTY_NAME);
            if (guidProperties != null && guidProperties.Any())
            {
                guid = guidProperties.First().value.value_str;
                if (guid != string.Empty)
                {
                    return true;
                }
            }

            // Second try in case FilterProperties fails
            guid = api
                .GetObjectProperties(objectId, 0)?
                .FirstOrDefault(x => x.name == GUID_PROPERTY_NAME)
                .value_str;

            if (guid != null && guid != string.Empty)
            {
                return true;
            }

            // The guid was not found
            return false;
        }

        private void FindGuidWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void GuidTextBox_TextChanged(object sender, EventArgs e)
        {
            userInput = _guidTextBox.Text;
        }
    }
}
