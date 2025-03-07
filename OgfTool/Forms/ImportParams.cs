using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OgfTool
{
    public partial class ImportParams : Form
    {
        public bool Textures = false;
        public bool MotionRefs = false;
        public bool Motions = false;
        public bool Userdata = false;
        public bool Lod = false;
        public bool Materials = false;
        public bool Remove = false;

        public bool res = false;

        public ImportParams(XRayModel Model, XRayModel ImportedModel)
        {
            InitializeComponent();
            ConstructUI(Model, ImportedModel);
        }

        public void ConstructUI(XRayModel Model, XRayModel ImportedModel)
        {
            bool CanMergeTextures = (Model.Childs.Count == ImportedModel.Childs.Count);
            bool CanMergeRefs = (Model.Header.IsSkeleton() && ImportedModel.MotionRefs != null);
            bool CanMergeOMF = (Model.Header.IsSkeleton() && ImportedModel.Motions.Data() != null);
            bool CanMergeUserdata = (Model.Header.IsSkeleton() && ImportedModel.UserData != null);
            bool CanMergeLod = (Model.Header.IsSkeleton() && ImportedModel.Lod != null);
            bool CanMergeIkData = (Model.Header.IsSkeleton() && Model.IkData != null && ImportedModel.IkData != null && Model.IkData.Bones.Count == ImportedModel.IkData.Bones.Count);

            TexturesChbx.Enabled = CanMergeTextures;
            MotionRefsChbx.Enabled = CanMergeRefs;
            MotionsChbx.Enabled = CanMergeOMF;
            UserdataChbx.Enabled = CanMergeUserdata;
            LodPathChbx.Enabled = CanMergeLod;
            IKdataChbx.Enabled = CanMergeIkData;
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            res = true;

            Textures = TexturesChbx.Checked;
            MotionRefs = MotionRefsChbx.Checked;
            Motions = MotionsChbx.Checked;
            Userdata = UserdataChbx.Checked;
            Lod = LodPathChbx.Checked;
            Materials = IKdataChbx.Checked;

            Remove = RemoveChbx.Checked;

            Close();
        }
    }
}
