using System;
using System.Windows.Forms;

namespace OgfTool
{
    public partial class OgfInfo : Form
    {
        public Description descr = new Description();
        public bool res = false;
        public OgfInfo(XRayModel OGF, bool refs_correct, float lod)
        {
            InitializeComponent();

            uint links = 0;
            bool cop_links = false;

            long verts = 0, faces = 0;
            foreach (var ch in OGF.Childs)
            {
                if (ch.to_delete) continue;

                if (ch.links >= 0x12071980)
                    links = Math.Max(links, ch.links / 0x12071980);
                else
                {
                    links = Math.Max(links, ch.links);
                    cop_links = true;
                }

                verts += ch.Vertices.Count;
                faces += ch.Faces_SWI(lod).Count;
            }

            OgfVersLabel.Text = OGF.Header.FormatVersion.ToString();
            ModelTypeLabel.Text = (OGF.Header.IsStaticSingle() ? "Single Static" : OGF.Header.IsStatic() ? "Static" : OGF.Header.IsAnimated() ? "Animated" : "Rigid");
            LinksLabel.Text = OGF.Header.IsSkeleton() ? links.ToString() + ", " + (cop_links ? "CoP" : "SoC") : "None";
            MotionRefsTypeLabel.Text = (OGF.MotionRefs == null || !refs_correct) ? "None" : (OGF.MotionRefs.Soc ? "SoC" : "CoP");

            bool bit8 = false;
            bool bit16 = false;
            bool no_bit = false;

            if (OGF.Motions.Anims != null && OGF.Motions.Anims.Count > 0)
            {
                for (int i = 0; i < OGF.Motions.Anims.Count; i++)
                {
                    byte flag = OGF.Motions.Anims[i].flags;

                    bool key16bit = (flag & (int)MotionKeyFlags.flTKey16IsBit) == (int)MotionKeyFlags.flTKey16IsBit;
                    bool keynocompressbit = (flag & (int)MotionKeyFlags.flTKeyFFT_Bit) == (int)MotionKeyFlags.flTKeyFFT_Bit;

                    if (!key16bit && !keynocompressbit && !bit8)
                        bit8 = true;
                    else if (key16bit && !keynocompressbit && !bit16)
                        bit16 = true;
                    else if (keynocompressbit && !no_bit)
                        no_bit = true;
                }

                MotionsLabel.Text = string.Empty;

                if (bit8)
                    MotionsLabel.Text += "8 bit";

                if (bit16)
                {
                    if (!string.IsNullOrEmpty(MotionsLabel.Text))
                        MotionsLabel.Text += " | ";

                    MotionsLabel.Text += "16 bit";
                }

                if (no_bit)
                {
                    if (!string.IsNullOrEmpty(MotionsLabel.Text))
                        MotionsLabel.Text += " | ";

                    MotionsLabel.Text += "no compress";
                }
            }
            else
                MotionsLabel.Text = "None";

            VertsLabel.Text = verts.ToString();
            FacesLabel.Text = faces.ToString();

            if (OGF.Description != null)
            {
                ByteLabel.Text = OGF.Description.FourByte ? "4 byte" : "8 byte";
                RepairTimersButton.Enabled = !OGF.Description.FourByte;

                SourceTextBox.Text = OGF.Description.Source;
                ConverterTextBox.Text = OGF.Description.ExportTool;
                CreatorTextBox.Text = OGF.Description.OwnerName;
                EditorTextBox.Text = OGF.Description.ExportModifNameTool;

                System.DateTime dt_e = new System.DateTime(1970, 1, 1).AddSeconds(OGF.Description.ExportTime);
                System.DateTime dt_c = new System.DateTime(1970, 1, 1).AddSeconds(OGF.Description.CreationTime);
                System.DateTime dt_m = new System.DateTime(1970, 1, 1).AddSeconds(OGF.Description.ModifiedTime);

                ExportTimeDate.Value = dt_e;
                CreationTimeDate.Value = dt_c;
                ModifedTimeDate.Value = dt_m;
            }
            else
                RepairTimersButton.Enabled = false;

            SourceTextBox.Enabled = OGF.Description != null;
            ConverterTextBox.Enabled = OGF.Description != null;
            CreatorTextBox.Enabled = OGF.Description != null;
            EditorTextBox.Enabled = OGF.Description != null;

            ExportTimeDate.Enabled = OGF.Description != null;
            CreationTimeDate.Enabled = OGF.Description != null;
            ModifedTimeDate.Enabled = OGF.Description != null;

            ActiveControl = OgfVersTextLabel;
        }

        private void RepairTimersButton_Click(object sender, EventArgs e)
        {
            RepairTimersButton.Enabled = false;
            ByteLabel.Text = "4 byte";
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            res = true;

            descr.Source = SourceTextBox.Text;
            descr.ExportTool = ConverterTextBox.Text;
            descr.OwnerName = CreatorTextBox.Text;
            descr.ExportModifNameTool = EditorTextBox.Text;

            descr.ExportTime = Convert.ToUInt32(ExportTimeDate.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            descr.CreationTime = Convert.ToUInt32(CreationTimeDate.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            descr.ModifiedTime = Convert.ToUInt32(ModifedTimeDate.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            descr.FourByte = !RepairTimersButton.Enabled;

            Close();
        }
    }
}
