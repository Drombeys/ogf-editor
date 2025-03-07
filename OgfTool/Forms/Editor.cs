using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;
using GitHubUpdate;
using System.Reflection;

namespace OgfTool
{
	public partial class Editor : Form
	{
		// File sytem
		public EditorSettings pSettings { get; private set; } = null;
		public XRayModel Model = null;
		FolderSelectDialog SaveSklDialog = null;
		FolderSelectDialog SyncFirstDialog = null;
		FolderSelectDialog SyncSecondDialog = null;
		public string[] game_materials = { };
		public bool UseTexturesCache = false;

		static public string PROGRAM_VERSION = "4.1";

		// Input
		public bool bKeyIsDown = false;
        private string number_mask = @"^-[0-9.]*$";
		float CurrentLod = 0.0f; // 0 - HQ, 1 - LQ

		Process ViewerProcess = new Process();
		public bool ViewerWorking = false;
		public Thread ViewerThread = null;
		public bool ViewPortAlpha = true;
        public bool ViewPortTextures = true;
		public bool ViewPortBBox = false;
        public bool ViewPortBones = false;
		public bool ViewPortNeedReload = false;
        List<bool> OldChildVisible = new List<bool>();
		List<string> OldChildTextures = new List<string>();

        public Process[] ConverterProcess = new Process[2] { new Process(), new Process() };
		public bool[] ConverterWorking = new bool[2] { false, false };

        [DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32")]
		private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

		[DllImport("user32")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);


		public enum ExportFormat
		{
			Unknown,
			OGF,
			Object,
			DM,
            Obj,
			FBX,
            Bones,
			OMF,
			Skl,
			Skls,
			Detail
		}

		public Editor()
		{
			Aspose.ThreeD.TrialException.SuppressTrialException = true; // Осудительно так делать

            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            InitializeComponent();

            Model = new XRayModel();

            if (!Directory.Exists(TempFolder()))
                Directory.CreateDirectory(TempFolder());

            Text += " " + PROGRAM_VERSION;

            // Start init settings

            string file_path = AppPath() + "\\Settings.ini";
			bool SettingsExist = File.Exists(file_path);
			pSettings = new EditorSettings(file_path);

			string gamemtl = "";

			if (!pSettings.CheckVers())
			{
				if (SettingsExist)
					MessageBox.Show("Settings version conflict! Load defaults.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

				File.Delete(file_path);
				Settings settings = new Settings(pSettings);
				settings.Settings_Load(null, null); // Load defaults
				settings.SaveParams(); // Save defaults
			}

			pSettings.LoadText("GameMtlPath", ref gamemtl);

			if (File.Exists(gamemtl))
				game_materials = GameMtlParser(gamemtl);

            bool BBoxEnabled = false;
            pSettings.Load("BBoxEnabled", ref BBoxEnabled);
			if (BBoxEnabled)
				showBBoxToolStripMenuItem_Click(showBBoxToolStripMenuItem, null);

            bool BonesEnabled = false;
            pSettings.Load("BonesEnabled", ref BonesEnabled);
            if (BonesEnabled)
                showBonesToolStripMenuItem_Click(showBonesToolStripMenuItem, null);

            bool DisableTextures = false;
            pSettings.Load("DisableTextures", ref DisableTextures, true);
			if (!DisableTextures)
				DisableTexturesMenuItem_Click(null, null);

            bool DisableAlpha = false;
            pSettings.Load("DisableAlpha", ref DisableAlpha, true);

            if (!DisableAlpha)
                disableAlphaToolStripMenuItem_Click(null, null);

            pSettings.Load(BkpCheckBox);

            // End init settings

            OgfInfo.Enabled = false;
			SaveMenuParam.Enabled = false;
			saveAsToolStripMenuItem.Enabled = false;

			// Tools
			OpenInObjectEditor.Enabled = false;
			importDataFromModelToolStripMenuItem.Enabled = false;
            recalcNormalsToolStripMenuItem.Enabled = false;
            recalcBoundingBoxToolStripMenuItem.Enabled = false;
			removeProgressiveMeshesToolStripMenuItem.Enabled = false;
			moveRotateModelToolStripMenuItem.Enabled = false;
			converterToolStripMenuItem.Enabled = false;

			exportToolStripMenuItem.Enabled = false;
			LabelBroken.Visible = false;
			viewPortToolStripMenuItem.Visible = false;
            LodMenuItem.Enabled = false;
            reloadToolStripMenuItem.Enabled = false;
            CurrentFormat.Enabled = false;
            AddMeshesMenuItem.Enabled = false;

            SaveSklDialog = new FolderSelectDialog();
            SyncFirstDialog = new FolderSelectDialog();
            SyncSecondDialog = new FolderSelectDialog();

            if (Environment.GetCommandLineArgs().Length > 1)
			{
                Clear();
                if (Model.OpenFile(Environment.GetCommandLineArgs()[1]))
                    AfterLoad(true);
			}
			else
            {
				TabControl.Controls.Clear();
			}

			AutoCheckUpdates();
		}

		public void AutoCheckUpdates()
		{
			DateTime current_time = DateTime.Now;
			DateTime last_update_time = pSettings.Load("LastUpdateTime", new System.DateTime(1970, 1, 1));

			int days_passed = Convert.ToInt32((current_time - last_update_time).TotalDays);

			if (days_passed >= 1)
			{
                string user = "VaIeroK";
                string repo = "OGF-tool";
                string vers = PROGRAM_VERSION;
                string asset = "OGF.Editor.";

                try
                {
                    UpdateChecker checker;
                    checker = new UpdateChecker(user, repo, vers);
                    checker.CheckUpdate().ContinueWith((continuation) =>
                    {
                        Invoke(new Action(() => // Go back to the UI thread
                        {
                            if (continuation.Result != UpdateType.None)
                            {
                                var result = new UpdateNotifyDialog(checker).ShowDialog();
                                if (result == DialogResult.Yes)
                                {
                                    checker.DownloadAsset(asset);
                                }
                            }
                        }));
                    });
                }
                catch (Exception) { }
                pSettings.Save("LastUpdateTime", current_time);
			}
        }

		public static string AppPath()
		{
			return Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\'));
		}

		public static string TempFolder(bool check = true)
		{
			if (check)
				CheckTempFolder();
			return Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp";
		}

		public static void CheckTempFolder()
		{
			if (!Directory.Exists(TempFolder(false)))
				Directory.CreateDirectory(TempFolder(false));
		}

		private void Clear()
		{
			TexturesPage.Controls.Clear();
			BoneParamsPage.Controls.Clear();
			TabControl.Controls.Clear();
			MotionRefsBox.Clear();
			BoneNamesBox.Clear();
			UserDataBox.Clear();
			MotionBox.Clear();
			LodPathBox.Clear();
		}

        private void AfterLoad(bool main_file)
		{
            if (main_file)
			{
				StatusFile.Text = Model.FileName.Substring(Model.FileName.LastIndexOf('\\') + 1);

                reloadToolStripMenuItem.Enabled = true;
                SaveMenuParam.Enabled = true;
				saveAsToolStripMenuItem.Enabled = true;

                OpenInObjectEditor.Enabled = !Model.Is(XRayModel.ModelFormat.eDM);
                importDataFromModelToolStripMenuItem.Enabled = !Model.Is(XRayModel.ModelFormat.eDM);
                recalcNormalsToolStripMenuItem.Enabled = !Model.Is(XRayModel.ModelFormat.eDM);
                recalcBoundingBoxToolStripMenuItem.Enabled = !Model.Is(XRayModel.ModelFormat.eDM);
                moveRotateModelToolStripMenuItem.Enabled = !Model.Is(XRayModel.ModelFormat.eDM);
                converterToolStripMenuItem.Enabled = !Model.Is(XRayModel.ModelFormat.eDM);
                removeProgressiveMeshesToolStripMenuItem.Enabled = LodMenuItem.Enabled = Model.IsProgressive();

                exportToolStripMenuItem.Enabled = true;
				bonesToolStripMenuItem.Enabled = Model.Header.IsSkeleton();
                oGFToolStripMenuItem.Enabled = !Model.Is(XRayModel.ModelFormat.eOGF);
                AddMeshesMenuItem.Enabled = Model.Header.IsSkeleton();
                OgfInfo.Enabled = !Model.Is(XRayModel.ModelFormat.eDM);
				showBonesToolStripMenuItem.Enabled = Model.BoneData != null && Model.IkData != null;
				objectToolStripMenuItem.Enabled = !Model.Is(XRayModel.ModelFormat.eDetail);
				dMToolStripMenuItem.Enabled = !Model.Is(XRayModel.ModelFormat.eDM);

                OpenOGFDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
                OpenDMDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
                OpenOGF_DmDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				SaveAsDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				SaveAsDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.'));
				OpenOMFDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				OpenProgramDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				SaveSklDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				SaveSklsDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				SaveSklsDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".skls";
				SaveOmfDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				SaveOmfDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".omf";
				SaveBonesDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				SaveBonesDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".bones";
				SaveObjectDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
				SaveObjectDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".object";
                SaveObjDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
                SaveObjDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".obj";
                SaveDMDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
                SaveDMDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".dm";
                SaveOGFDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
                SaveOGFDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".ogf";
                SaveFBXDialog.InitialDirectory = Model.FileName.Substring(0, Model.FileName.LastIndexOf('\\'));
                SaveFBXDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".fbx";

                CurrentLod = 0;
            }

            omfToolStripMenuItem.Enabled = Model.Motions.Data() != null;
            sklToolStripMenuItem.Enabled = Model.Motions.Data() != null;
            sklsToolStripMenuItem.Enabled = Model.Motions.Data() != null;

            // Textures
            TabControl.Controls.Add(TexturesPage);

			if (Model.Header.IsSkeleton())
			{
				//Userdata
				TabControl.Controls.Add(UserDataPage);
				UserDataPage.Controls.Clear();
				UserDataPage.Controls.Add(UserDataBox);
				UserDataPage.Controls.Add(CreateUserdataButton);
				CreateUserdataButton.Visible = false;
				UserDataBox.Visible = false;

				if (Model.UserData != null)
					UserDataBox.Visible = true;
				else
					CreateUserdataButton.Visible = true;

				// Motion Refs
				TabControl.Controls.Add(MotionRefsPage);
				MotionRefsPage.Controls.Clear();
				MotionRefsPage.Controls.Add(MotionRefsBox);
				MotionRefsPage.Controls.Add(CreateMotionRefsButton);
				CreateMotionRefsButton.Visible = false;
				MotionRefsBox.Visible = false;

				if (Model.MotionRefs != null)
					MotionRefsBox.Visible = true;
				else
					CreateMotionRefsButton.Visible = true;

				// Motions
				TabControl.Controls.Add(MotionPage);
				MotionBox.Text = "";

				if (Model.Motions.Data() != null)
				{
					AppendOMFButton.Visible = false;
					MotionBox.Visible = true;
                    MotionBox.Text = Model.Motions.ToString();
                }
				else
				{
					MotionBox.Visible = false;
					AppendOMFButton.Visible = true;
				}

				// Bones
				if (Model.BoneData != null)
				{
					BoneNamesBox.Clear();
					TabControl.Controls.Add(BoneNamesPage);

					BoneNamesBox.Text += $"Bones count : {Model.BoneData.Bones.Count}\n\n";
					for (int i = 0; i < Model.BoneData.Bones.Count; i++)
					{
						BoneNamesBox.Text += $"{i + 1}. {Model.BoneData.Bones[i].Name}";

						if (i != Model.BoneData.Bones.Count - 1)
							BoneNamesBox.Text += "\n";
					}

                    // Ik Data
                    if (Model.IkData != null)
					{
						TabControl.Controls.Add(BoneParamsPage);

						for (int i = Model.BoneData.Bones.Count - 1; i >= 0; i--)
						{
							CreateBoneGroupBox(i, Model.BoneData.Bones[i].Name, Model.BoneData.Bones[i].ParentName, Model.IkData.Bones[i].Material, Model.IkData.Bones[i].Mass, Model.IkData.Bones[i].CenterMass, Model.IkData.Bones[i].Position, Model.IkData.Bones[i].Rotation);
						}
                    }
				}

				// Lod
				if (Model.Header.FormatVersion == 4)
					TabControl.Controls.Add(LodPage);

				if (Model.Lod != null)
				{
					CreateLodButton.Visible = false;
					LodPathBox.Text = Model.Lod.LodPath;
				}
				else
					CreateLodButton.Visible = true;
			}

            for (int i = Model.Childs.Count - 1; i >= 0; i--)
			{
				CreateTextureGroupBox(i);

				var TextureGroupBox = TexturesPage.Controls["TextureGrpBox_" + i.ToString()];
                TextureGroupBox.Controls["textureBox_" + i.ToString()].Text = Model.Childs[i].Texture;
                TextureGroupBox.Controls["shaderBox_" + i.ToString()].Text = Model.Childs[i].Shader;
			}

            MotionRefsBox.Clear();
			UserDataBox.Clear();

			if (Model.MotionRefs != null)
				MotionRefsBox.Lines = Model.MotionRefs.Refs.ToArray();

			if (Model.UserData != null)
				UserDataBox.Text = Model.UserData.Userdata;

			if (main_file && !Model.Is(XRayModel.ModelFormat.eDM))
			{
				LabelBroken.Text = "Broken type: " + Model.BrokenType.ToString();
				LabelBroken.Visible = Model.BrokenType > 0;
			}

			UpdateModelType();
			UpdateModelFormat();
            UpdateNPC();

            // View
            TabControl.Controls.Add(ViewPage);
        }

		private void ApplyParams()
		{
			if (Model.MotionRefs != null)
			{
                Model.MotionRefs.Refs.Clear();

				if (IsTextCorrect(MotionRefsBox.Text))
				{
					for (int i = 0; i < MotionRefsBox.Lines.Count(); i++)
					{
						if (IsTextCorrect(MotionRefsBox.Lines[i]))
                            Model.MotionRefs.Refs.Add(GetCorrectString(MotionRefsBox.Lines[i]));
					}
				}
			}

			if (Model.UserData != null)
			{
                Model.UserData.Userdata = "";

				if (IsTextCorrect(UserDataBox.Text))
				{
					for (int i = 0; i < UserDataBox.Lines.Count(); i++)
					{
						string ext = i == UserDataBox.Lines.Count() - 1 ? "" : "\r\n";
                        Model.UserData.Userdata += UserDataBox.Lines[i] + ext;
					}
				}
			}

			if (Model.Lod != null)
			{
                Model.Lod.LodPath = "";

				if (IsTextCorrect(LodPathBox.Text))
                    Model.Lod.LodPath = GetCorrectString(LodPathBox.Text);
			}

			UpdateModelType();
		}

		private bool CheckMeshes()
		{
			foreach (var ch in Model.Childs)
			{
				if (!ch.to_delete)
					return true;
			}

            return false;
		}

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
		{
			bKeyIsDown = true;
		}

		private void ButtonFilter(object sender, EventArgs e)
		{
			Button curBox = sender as Button;

			string currentField = curBox.Name.ToString().Split('_')[0];
			int idx = Convert.ToInt32(curBox.Name.ToString().Split('_')[1]);

			switch (currentField)
			{
				case "DeleteButton":
					if (Model.Is(XRayModel.ModelFormat.eDM))
					{
                        OpenDMDialog.FileName = "";
                        if (OpenDMDialog.ShowDialog() == DialogResult.OK)
                        {
                            XRayModel DM = new XRayModel();
                            if (DM.OpenFile(OpenDMDialog.FileName))
                            {
								OgfChild old_child = Model.Childs[idx];
                                Model.Childs[idx] = DM.Childs[0];
								Model.Childs[idx].SetLocalOffsetMain(old_child.GetLocalOffsetMain());

                                var TextureGroupBox = TexturesPage.Controls["TextureGrpBox_" + idx.ToString()];
                                TextureGroupBox.Controls["textureBox_" + idx.ToString()].Text = Model.Childs[idx].Texture;
                                TextureGroupBox.Controls["shaderBox_" + idx.ToString()].Text = Model.Childs[idx].Shader;
                                RecalcMeshInfo();
                                ReloadViewPort();
                            }
                        }
                    }
					else
					{
						Model.Childs[idx].to_delete = !Model.Childs[idx].to_delete;

						if (Model.Childs[idx].to_delete)
						{
							curBox.Text = "Return Mesh";
							curBox.BackColor = Color.FromArgb(255, 255, 128, 128);
						}
						else
						{
							curBox.Text = "Delete Mesh";
							curBox.BackColor = SystemColors.Control;
						}
						UpdateModelType();
						Model.RecalcBBox(false);
					}
                    break;
                case "MoveButton":
					float[] old_offs = Model.Childs[idx].GetLocalOffset();
                    float[] old_rot = Model.Childs[idx].GetLocalRotation();
                    bool old_rot_flag = Model.Childs[idx].GetLocalRotationFlag();

                    MoveMesh moveMesh = new MoveMesh(old_offs, old_rot, old_rot_flag, true);
					moveMesh.ShowDialog();

					if (moveMesh.res)
					{
                        Model.Childs[idx].SetLocalOffset(moveMesh.offset);
                        Model.Childs[idx].SetLocalRotation(moveMesh.rotation, Model.Childs[idx].Header.BSphere.Center, moveMesh.LocalRotation);
                    }

					if (!FVec.Similar(old_offs, Model.Childs[idx].GetLocalOffset()) || !FVec.Similar(old_rot, Model.Childs[idx].GetLocalRotation()) || old_rot_flag != Model.Childs[idx].GetLocalRotationFlag())
					{
						if (!Model.Is(XRayModel.ModelFormat.eDM))
							Model.RecalcBBox(true);
                        ReloadViewPort(true, false, true);
					}
                    break;
                case "DataButton":
					DmData dmData = new DmData(Model.Childs[idx].MinScale, Model.Childs[idx].MaxScale, Model.Childs[idx].Flags);
					dmData.ShowDialog();
					Model.Childs[idx].MinScale = dmData.fMinScale;
					Model.Childs[idx].MaxScale = dmData.fMaxScale;
					Model.Childs[idx].Flags = dmData.iFlags;
                    break;
            }
		}

		private void TextBoxFilter(object sender, EventArgs e)
		{
			TextBox curBox = sender as TextBox;

			string currentField = curBox.Name.ToString().Split('_')[0];
			int idx = Convert.ToInt32(curBox.Name.ToString().Split('_')[1]);

			switch (currentField)
			{
				case "textureBox": Model.Childs[idx].Texture = curBox.Text; break;
				case "shaderBox": Model.Childs[idx].Shader = curBox.Text; break;
			}
		}

		void ReloadControlText(Control control, int cursor_pos)
		{
            string currentField = control.Name.ToString().Split('_')[0];
            int idx = Convert.ToInt32(control.Name.ToString().Split('_')[1]);

            switch (currentField)
            {
                case "MassBox": control.Text = ((decimal)Model.IkData.Bones[idx].Mass).ToString(); break;
                case "CenterBoxX": control.Text = ((decimal)Model.IkData.Bones[idx].CenterMass[0]).ToString(); break;
                case "CenterBoxY": control.Text = ((decimal)Model.IkData.Bones[idx].CenterMass[1]).ToString(); break;
                case "CenterBoxZ": control.Text = ((decimal)Model.IkData.Bones[idx].CenterMass[2]).ToString(); break;
                case "PositionX": control.Text = ((decimal)Model.IkData.Bones[idx].Position[0]).ToString(); break;
                case "PositionY": control.Text = ((decimal)Model.IkData.Bones[idx].Position[1]).ToString(); break;
                case "PositionZ": control.Text = ((decimal)Model.IkData.Bones[idx].Position[2]).ToString(); break;
                case "RotationX": control.Text = ((decimal)Model.IkData.Bones[idx].Rotation[0]).ToString(); break;
                case "RotationY": control.Text = ((decimal)Model.IkData.Bones[idx].Rotation[1]).ToString(); break;
                case "RotationZ": control.Text = ((decimal)Model.IkData.Bones[idx].Rotation[2]).ToString(); break;
			}

			if (control is TextBox)
			{
                TextBox curBox = control as TextBox;

                if (curBox.SelectionStart < 1)
					curBox.SelectionStart = control.Text.Length;

				curBox.SelectionStart = cursor_pos - 1;
			}
        }

		private void TextBoxBonesFilter(object sender, EventArgs e)
		{
			Control curControl = sender as Control;

			string currentField = curControl.Name.ToString().Split('_')[0];
			int idx = Convert.ToInt32(curControl.Name.ToString().Split('_')[1]);

			if (curControl.Text != "-" || currentField == "MassBox")
			{
				switch (curControl.Tag.ToString())
				{
					case "float":
						{
							if (bKeyIsDown)
							{
								TextBox curBox = sender as TextBox;

								if (curControl.Text.Length == 0)
									return;

								int temp = curBox.SelectionStart;

                                Regex.Match(curControl.Text, number_mask);

                                if (currentField == "MassBox" && curControl.Text.Contains("-"))
                                    ReloadControlText(curControl, temp);

                                try
								{
									Convert.ToSingle(curControl.Text);
								}
								catch (Exception)
								{
									ReloadControlText(curControl, temp);
								}
                            }
						}
						break;
				}

				bool need_recalc_bones = false;

				switch (currentField)
				{
					case "boneBox":
						{
                            Model.BoneData.Bones[idx].Name = curControl.Text;

							for (int j = 0; j < Model.BoneData.Bones[idx].ChildsId.Count; j++)
							{
								int child_id = Model.BoneData.Bones[idx].ChildsId[j];
								var MainGroup = BoneParamsPage.Controls["BoneGrpBox_" + child_id.ToString()];
                                Model.BoneData.Bones[child_id].ParentName = curControl.Text;
								MainGroup.Controls["ParentboneBox_" + child_id.ToString()].Text = Model.BoneData.Bones[child_id].ParentName;
							}

							BoneNamesBox.Clear();
							BoneNamesBox.Text += $"Bones count : {Model.BoneData.Bones.Count}\n\n";

							for (int i = 0; i < Model.BoneData.Bones.Count; i++)
							{
								BoneNamesBox.Text += $"{i + 1}. {Model.BoneData.Bones[i].Name}";
								if (i != Model.BoneData.Bones.Count - 1)
									BoneNamesBox.Text += "\n";
							}
                            if (ViewPortBones)
								ViewPortNeedReload = true;
                        }
						break;
					case "MaterialBox": Model.IkData.Bones[idx].Material = curControl.Text; break;
					case "MassBox": Model.IkData.Bones[idx].Mass = Convert.ToSingle(curControl.Text); break;
					case "CenterBoxX": Model.IkData.Bones[idx].CenterMass[0] = Convert.ToSingle(curControl.Text); break;
					case "CenterBoxY": Model.IkData.Bones[idx].CenterMass[1] = Convert.ToSingle(curControl.Text); break;
					case "CenterBoxZ": Model.IkData.Bones[idx].CenterMass[2] = Convert.ToSingle(curControl.Text); break;
					case "PositionX": Model.IkData.Bones[idx].Position[0] = Convert.ToSingle(curControl.Text); need_recalc_bones = true; break;
					case "PositionY": Model.IkData.Bones[idx].Position[1] = Convert.ToSingle(curControl.Text); need_recalc_bones = true; break;
					case "PositionZ": Model.IkData.Bones[idx].Position[2] = Convert.ToSingle(curControl.Text); need_recalc_bones = true; break;
					case "RotationX": Model.IkData.Bones[idx].Rotation[0] = Convert.ToSingle(curControl.Text); need_recalc_bones = true; break;
					case "RotationY": Model.IkData.Bones[idx].Rotation[1] = Convert.ToSingle(curControl.Text); need_recalc_bones = true; break;
					case "RotationZ": Model.IkData.Bones[idx].Rotation[2] = Convert.ToSingle(curControl.Text); need_recalc_bones = true; break;
				}

				if (need_recalc_bones && (ViewPortBones || Model.IkData != null && Model.IkData.ChunkVersion == 2)) // Если показываем кости или загружен старый меш зависящий от костей
					ViewPortNeedReload = true;
            }

			bKeyIsDown = false;
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (Model.FileName == "") return;

			if (!CheckMeshes())
			{
                AutoClosingMessageBox.Show("Can't save model without meshes!", "Error", 1500, MessageBoxIcon.Error);
				return;
            }

			ApplyParams();
            Model.SaveFile(Model.FileName, BkpCheckBox.Checked);
			AutoClosingMessageBox.Show(Model.NeedRepair() ? "Repaired and Saved!" : "Saved!", "Info", Model.NeedRepair() ? 700 : 500, MessageBoxIcon.Information);
		}

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
			OpenOGF_DmDialog.FileName = "";
			DialogResult res = OpenOGF_DmDialog.ShowDialog();

			if (res == DialogResult.OK)
			{
				if (Model.OpenFile(OpenOGF_DmDialog.FileName))
				{
                    Clear();
                    OpenOGF_DmDialog.InitialDirectory = "";
					Model.FileName = OpenOGF_DmDialog.FileName;
					AfterLoad(true);
				}
			}
		}

        private void oGFInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");

			OgfInfo Info = new OgfInfo(Model, IsTextCorrect(MotionRefsBox.Text), CurrentLod);
            Info.ShowDialog();

			if (Info.res && Model.Description != null)
			{
				Model.Description.Source = Info.descr.Source;
				Model.Description.ExportTool = Info.descr.ExportTool;
				Model.Description.OwnerName = Info.descr.OwnerName;
				Model.Description.ExportModifNameTool = Info.descr.ExportModifNameTool;
				Model.Description.CreationTime = Info.descr.CreationTime;
				Model.Description.ExportTime = Info.descr.ExportTime;
				Model.Description.ModifiedTime = Info.descr.ModifiedTime;
                Model.Description.FourByte = Info.descr.FourByte;
			}

			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckMeshes())
            {
                AutoClosingMessageBox.Show("Can't save file without meshes!", "Error", 1500, MessageBoxIcon.Error);
                return;
            }

			ExportFormat fmt = ExportFormat.Unknown;

			if (Model.Is(XRayModel.ModelFormat.eDetail))
			{
				SaveAsDialog.Filter = "Detail file|*.details";
				fmt = ExportFormat.Detail;
			}
			else if (Model.Is(XRayModel.ModelFormat.eDM))
			{
				SaveAsDialog.Filter = "DM file|*.dm";
				fmt = ExportFormat.DM;
			}
			else if (Model.Is(XRayModel.ModelFormat.eOGF))
			{
				SaveAsDialog.Filter = "OGF file|*.ogf";
				fmt = ExportFormat.OGF;
			}
			else
			{
				SaveAsDialog.Filter = "OGF file|*.ogf|DM file|*.dm";
				SaveAsDialog.DefaultExt = "ogf";
            }

			if (SaveAsDialog.ShowDialog() == DialogResult.OK)
			{
				if (fmt == ExportFormat.Unknown)
                    fmt = (Path.GetExtension(SaveAsDialog.FileName) == ".ogf" ? ExportFormat.OGF : ExportFormat.DM);

				SaveTools(SaveAsDialog.FileName, fmt);
				SaveAsDialog.InitialDirectory = "";
			}
		}

		private void SaveTools(string filename, ExportFormat format)
		{
            if (!CheckMeshes())
            {
                AutoClosingMessageBox.Show("Can't save file without meshes!", "Error", 1500, MessageBoxIcon.Error);
                return;
            }

            if (File.Exists(filename) && filename != Model.FileName)
                File.Delete(filename);

			int exit_code = 0;

            switch (format)
			{
				case ExportFormat.OGF:
                    ApplyParams();
                    Model.SaveOGF(filename, BkpCheckBox.Checked);
                    break;
                case ExportFormat.DM:
					int cnt = 0, idx = 0;
					for (int i = 0; i < Model.Childs.Count; i++)
					{
						if (!Model.Childs[i].to_delete)
						{
							cnt++;
							idx = i;
						}
					}

					if (cnt > 1)
					{
                        AutoClosingMessageBox.Show("DM file may have only one mesh!", "Error", 1500, MessageBoxIcon.Error);
						return;
                    }

                    ApplyParams();
                    DmData dmData = new DmData(Model.Childs[idx].MinScale, Model.Childs[idx].MaxScale, Model.Childs[idx].Flags);
                    dmData.ShowDialog();
                    Model.Childs[idx].MinScale = dmData.fMinScale;
                    Model.Childs[idx].MaxScale = dmData.fMaxScale;
                    Model.Childs[idx].Flags = dmData.iFlags;
                    Model.SaveDM(filename, idx, BkpCheckBox.Checked);
                    break;
                case ExportFormat.Obj:
                    ApplyParams();
                    Model.SaveObj(filename, CurrentLod);
                    break;
                case ExportFormat.FBX:
                    ApplyParams();
                    Model.SaveFBX(filename, CurrentLod);
                    break;
                case ExportFormat.Object:
                    ApplyParams();
                    exit_code = Model.SaveObject(filename, BkpCheckBox.Checked);
                    break;
				case ExportFormat.OMF:
                    ApplyParams();
                    Model.SaveOMF(filename);
                    break;
                case ExportFormat.Bones:
                    exit_code = Model.SaveBones(filename);
                    break;
                case ExportFormat.Skl:
                    exit_code = Model.SaveSkl(filename);
                    break;
                case ExportFormat.Skls:
                    exit_code = Model.SaveSkls(filename);
                    break;
                case ExportFormat.Detail:
                    Model.SaveDetail(filename, BkpCheckBox.Checked);
                    break;
				case ExportFormat.Unknown:
                    AutoClosingMessageBox.Show("Unknown export format! Report for developer", "Error", 1500, MessageBoxIcon.Error);
                    break;
            }

			if (exit_code == 0)
			{
				string Text = (Model.NeedRepair() ? "Repaired and " : "") + (format == ExportFormat.OGF ? "Saved!" : "Exported!");
				AutoClosingMessageBox.Show(Text, "Info", Model.NeedRepair() ? 700 : 500, MessageBoxIcon.Information);
			}
			else
				AutoClosingMessageBox.Show("Export aborted!", "Error", 1500, MessageBoxIcon.Error);
        }

        private void oGFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveOGFDialog.ShowDialog() == DialogResult.OK)
            {
                SaveTools(SaveOGFDialog.FileName, ExportFormat.OGF);
                SaveOGFDialog.InitialDirectory = "";
            }
        }

        private void objectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveObjectDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveObjectDialog.FileName, ExportFormat.Object);
				SaveObjectDialog.InitialDirectory = "";
			}
		}

        private void dMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveDMDialog.ShowDialog() == DialogResult.OK)
            {
                SaveTools(SaveDMDialog.FileName, ExportFormat.DM);
                SaveDMDialog.InitialDirectory = "";
            }
        }

        private void bonesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveBonesDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveBonesDialog.FileName, ExportFormat.Bones);
				SaveBonesDialog.InitialDirectory = "";
			}
		}

		private void omfToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveOmfDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveOmfDialog.FileName, ExportFormat.OMF);
				SaveOmfDialog.InitialDirectory = "";
			}
		}

		private void objToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveObjDialog.ShowDialog() == DialogResult.OK)
			{
				float old_lod = CurrentLod;
				CurrentLod = 0.0f;
				SaveTools(SaveObjDialog.FileName, ExportFormat.Obj);
				CurrentLod = old_lod;
				SaveObjDialog.InitialDirectory = "";
			}
		}

        private void fBXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveFBXDialog.ShowDialog() == DialogResult.OK)
            {
                float old_lod = CurrentLod;
                CurrentLod = 0.0f;
                SaveTools(SaveFBXDialog.FileName, ExportFormat.FBX);
                CurrentLod = old_lod;
                SaveFBXDialog.InitialDirectory = "";
            }
        }

        private void sklToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveSklDialog.ShowDialog(this.Handle))
			{
				SaveTools(SaveSklDialog.FileName, ExportFormat.Skl);
				SaveSklDialog.InitialDirectory = "";
			}
		}

		private void sklsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveSklsDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveSklsDialog.FileName, ExportFormat.Skls);
				SaveSklsDialog.InitialDirectory = "";
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (MessageBox.Show("Are you sure you want to exit?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				Close();
		}

        private void CreateUserdataButton_Click(object sender, EventArgs e)
        {
			CreateUserdataButton.Visible = false;
			UserDataBox.Visible = true;
			UserDataBox.Clear();
			if (Model.UserData == null)
                Model.UserData = new UserData();
		}

        private void CreateMotionRefsButton_Click(object sender, EventArgs e)
        {
			if (Model.Motions.Data() == null || Model.Motions.Data() != null && MessageBox.Show("New motion refs chunk will remove built-in motions, continue?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				// Чистим все связанное со встроенными анимами
				MotionBox.Clear();
				MotionBox.Visible = false;
				AppendOMFButton.Visible = true;
                Model.Motions.SetData(null);

				// Обновляем тип модели
				UpdateModelType();
				UpdateModelFormat();

				// Обновляем визуал интерфейса моушн рефов
				CreateMotionRefsButton.Visible = false; 
				MotionRefsBox.Visible = true;
				MotionRefsBox.Clear();

				if (Model.MotionRefs == null)
                    Model.MotionRefs = new MotionRefs();
			}
		}

		private void CreateLodButton_Click(object sender, EventArgs e)
		{
			CreateLodButton.Visible = false;
			LodPathBox.Clear();
			if (Model.Lod == null)
                Model.Lod = new Lod();
		}

		private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
			ReloadModel();
        }

		public void ReloadModel()
		{
            if (Model.FileName == "") return;

            if (Model.OpenFile(Model.FileName))
            {
                Clear();
                AfterLoad(true);
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (TabControl.SelectedIndex < 0) return;
			bool ViewPortItemVisible = false;

			switch (TabControl.Controls[TabControl.SelectedIndex].Name)
			{
				case "UserDataPage":
					{
						if (!IsTextCorrect(UserDataBox.Text))
						{
							CreateUserdataButton.Visible = true;
							UserDataBox.Visible = false;
						}
						break;
					}
				case "MotionRefsPage":
					{
						if (!IsTextCorrect(MotionRefsBox.Text))
						{
							CreateMotionRefsButton.Visible = true;
							MotionRefsBox.Visible = false;
						}
						break;
					}
				case "LodPage":
					{
						if (!IsTextCorrect(LodPathBox.Text))
						{
							CreateLodButton.Visible = true;
						}
						break;
					}
				case "ViewPage":
					{
						InitViewPort(true, false, ViewPortNeedReload);
						ViewPortItemVisible = true;
						break;
					}
				case "BoneParamsPage":
					{
                        for (int i = 0; i < BoneParamsPage.Controls.Count; i++)
						{
							if (Model.IkData == null || Model.IkData.Bones.Count <= i)
								break;

							GroupBox box = BoneParamsPage.Controls["BoneGrpBox_" + i.ToString()] as GroupBox;
							TableLayoutPanel layoutPanel = box.Controls["LayoutPanel_" + i.ToString()] as TableLayoutPanel;
							(layoutPanel.Controls["PositionX_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].Position[0]).ToString();
							(layoutPanel.Controls["PositionY_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].Position[1]).ToString();
							(layoutPanel.Controls["PositionZ_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].Position[2]).ToString();
							(layoutPanel.Controls["RotationX_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].Rotation[0]).ToString();
							(layoutPanel.Controls["RotationY_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].Rotation[1]).ToString();
							(layoutPanel.Controls["RotationZ_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].Rotation[2]).ToString();
                            (layoutPanel.Controls["CenterBoxX_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].CenterMass[0]).ToString();
                            (layoutPanel.Controls["CenterBoxY_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].CenterMass[1]).ToString();
                            (layoutPanel.Controls["CenterBoxZ_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].CenterMass[2]).ToString();
                            (layoutPanel.Controls["MassBox_" + i.ToString()] as TextBox).Text = ((decimal)Model.IkData.Bones[i].Mass).ToString();
						}
						break;
					}

            }
			viewPortToolStripMenuItem.Visible = ViewPortItemVisible;
		}

		private void RichTextBoxTextChanged(object sender, EventArgs e)
		{
			RichTextBox curBox = sender as RichTextBox;
			switch (curBox.Name)
			{
                case "MotionRefsBox":
                    {
						UpdateModelType();
						UpdateModelFormat();
						break;
                    }
            }
		}

        private void EditInOmfEditor(object sender, EventArgs e)
        {
            string Filename = TempFolder() + $"\\{StatusFile.Text}_temp.omf";
            string OmfEditor = pSettings.Load("OmfEditorPath");

            if (!File.Exists(OmfEditor))
            {
                MessageBox.Show("Please, set OMF Editor path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var fileStream = new FileStream(Filename, FileMode.OpenOrCreate))
            {
                fileStream.Write(Model.Motions.Data(), 0, Model.Motions.Data().Length);
				fileStream.Close();
            }

            Process proc = new Process();
            proc.StartInfo.FileName = OmfEditor;
            proc.StartInfo.Arguments += $"\"{Filename}\"";
            proc.Start();
            proc.WaitForExit();

            OpenOMFDialog.FileName = Filename;
            AppendMotion(null, null);
            OpenOMFDialog.FileName = "";

            File.Delete(Filename);
        }

        private void DeleteOmf(object sender, EventArgs e)
        {
            MotionBox.Visible = false;
            AppendOMFButton.Visible = true;
            Model.Motions.SetData(null);
            MotionBox.Clear();
            UpdateModelType();
            UpdateModelFormat();
        }

        private void AppendOMFButton_Click(object sender, EventArgs e)
        {
			if (!IsTextCorrect(MotionRefsBox.Text) && (Model.MotionRefs == null || Model.MotionRefs.Refs.Count() == 0) || (IsTextCorrect(MotionRefsBox.Text) || Model.MotionRefs != null && Model.MotionRefs.Refs.Count() > 0) && MessageBox.Show("Build-in motions will remove motion refs, continue?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				OpenOMFDialog.ShowDialog();
        }

		private void AppendMotion(object sender, CancelEventArgs e)
		{
			if (sender != null)
				OpenOMFDialog.InitialDirectory = "";

            byte[] OpenedOmf = File.ReadAllBytes(OpenOMFDialog.FileName);

			if (Model.Motions.SetData(OpenedOmf))
			{
                // Апдейтим визуал встроенных анимаций
                AppendOMFButton.Visible = false;
                MotionBox.Visible = true;

                // Чистим встроенные рефы, интерфейс почистится сам при активации вкладки
                MotionRefsBox.Clear();
                if (Model.MotionRefs != null)
                    Model.MotionRefs.Refs.Clear();

                MotionBox.Text = Model.Motions.ToString();
            }

			UpdateModelType();
			UpdateModelFormat();
		}

		public void importDataFromModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenOGFDialog.FileName = "";
			if (OpenOGFDialog.ShowDialog() == DialogResult.OK)
			{
				bool Update = false;

				XRayModel SecondOgf = new XRayModel();
				if (SecondOgf.OpenFile(OpenOGFDialog.FileName))
				{
					AutoClosingMessageBox.Show("Can't import OGF Model!", "Error", 1000, MessageBoxIcon.Error);
					return;
				}

                if (SecondOgf.Header.IsSkeleton())
				{
					ImportParams Params = new ImportParams(Model, SecondOgf);

					Params.ShowDialog();

					if (Params.res)
					{
						if (Params.Textures)
						{
							for (int i = 0; i < Model.Childs.Count; i++)
							{
                                Model.Childs[i].Texture = SecondOgf.Childs[i].Texture;
                                Model.Childs[i].Shader = SecondOgf.Childs[i].Shader;
							}

							Update = true;
						}

						if (Params.Userdata)
						{
							if (Model.UserData == null)
                                Model.UserData = new UserData();

                            Model.UserData.Userdata = SecondOgf.UserData.Userdata;
                            Model.UserData.OldFormat = SecondOgf.UserData.OldFormat;

                            Update = true;
						}
						else if (Params.Remove && Model.UserData != null)
						{
                            Model.UserData.Userdata = "";
							Update = true;
						}

						if (Params.Lod)
						{
							if (Model.Lod == null)
                                Model.Lod = new Lod();

                            Model.Lod.LodPath = SecondOgf.Lod.LodPath;

							Update = true;
						}
						else if (Params.Remove && Model.Lod != null)
						{
                            Model.Lod.LodPath = "";
							Update = true;
						}

						if (Params.MotionRefs)
						{
							if (Model.MotionRefs == null)
                                Model.MotionRefs = new MotionRefs();

                            Model.MotionRefs.Refs = SecondOgf.MotionRefs.Refs;

							Update = true;
						}
						else if (Params.Remove && Model.MotionRefs != null)
						{
                            Model.MotionRefs.Refs.Clear();
							Update = true;
						}

						if (Params.Motions)
						{
                            Model.Motions.SetData(SecondOgf.Motions.Data());

							if (Model.MotionRefs != null)
                                Model.MotionRefs.Refs.Clear();

							Update = true;
						}
						else if (Params.Remove)
						{
                            Model.Motions.SetData(null);
							Update = true;
						}

						if (Params.Materials)
						{
							for (int i = 0; i < Model.BoneData.Bones.Count; i++)
							{
								Model.IkData.Bones[i].Material = SecondOgf.IkData.Bones[i].Material;
                                Model.IkData.Bones[i].Mass = SecondOgf.IkData.Bones[i].Mass;
							}

							Update = true;
						}

						if (Update)
						{
							Clear();
							AfterLoad(false);
							AutoClosingMessageBox.Show("OGF Params changed!", "Info", 1000, MessageBoxIcon.Information);
						}
						else
						{
							AutoClosingMessageBox.Show("OGF Params don't changed!", "Warning", 1000, MessageBoxIcon.Warning);
						}
					}
				}
				else
                    AutoClosingMessageBox.Show("Can't load params from non skeleton model!", "Warning", 1000, MessageBoxIcon.Warning);
            }
		}

        private void batchToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Batch batchForm = new Batch(this);
            batchForm.ShowDialog();
        }

        private void ChangeModelFormat(object sender, EventArgs e)
		{
			if (Model.Opened)
			{
				Model.ChangeModelFormat();
                UpdateModelFormat();
			}
		}

		private void UpdateModelType()
        {
			if (!Model.Opened) return;

			if (Model.BoneData == null)
                Model.Header.Static(Model.Childs);
			else if (Model.Motions.Data() != null || IsTextCorrect(MotionRefsBox.Text))
                Model.Header.Animated();
			else
                Model.Header.Skeleton();

			// Апдейтим экспорт аним тут, т.к. при любом изменении омф вызывается эта функция
			omfToolStripMenuItem.Enabled = Model.Motions.Data() != null;
			sklToolStripMenuItem.Enabled = Model.Motions.Data() != null;
			sklsToolStripMenuItem.Enabled = Model.Motions.Data() != null;
		}

		private void UpdateModelFormat()
		{
			CurrentFormat.Enabled = (Model.Opened && !Model.Is(XRayModel.ModelFormat.eDM) && Model.Header.IsSkeleton());

			if (!CurrentFormat.Enabled)
			{
				CurrentFormat.Text = strings.AllFormat;
				return;
			}

			uint links = 0;

			foreach (var ch in Model.Childs)
				links = Math.Max(links, ch.links);

            Model.IsCopModel = (IsTextCorrect(MotionRefsBox.Text) && Model.MotionRefs != null && !Model.MotionRefs.Soc || !IsTextCorrect(MotionRefsBox.Text)) && links < 0x12071980;

			CurrentFormat.Text = (Model.IsCopModel ? strings.CoPFormat : strings.SoCFormat);
		}

		private void openSkeletonInObjectEditorToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (!CheckMeshes())
            {
                AutoClosingMessageBox.Show("Can't open model without meshes!", "Error", 1500, MessageBoxIcon.Error);
                return;
            }

            string Filename = TempFolder() + $"\\{StatusFile.Text}_temp.ogf";
			string ObjectName = Filename.Substring(0, Filename.LastIndexOf('.'));
			ObjectName = ObjectName.Substring(0, ObjectName.LastIndexOf('.')) + ".object";

			string ObjectEditor = pSettings.Load("ObjectEditorPath");

			if (!File.Exists(ObjectEditor))
			{
				MessageBox.Show("Please, set Object Editor path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (File.Exists(Filename))
				File.Delete(Filename);

            ApplyParams();
            XRayModel temp_model = new XRayModel();
			temp_model.Copy(Model);
			temp_model.FileName = Filename;

            File.Copy(Model.FileName, Filename);

			int exit_code = temp_model.SaveObject(ObjectName, BkpCheckBox.Checked);

			if (exit_code == 0)
			{
				Process proc = new Process();
				proc.StartInfo.FileName = ObjectEditor;
				proc.StartInfo.Arguments += $"\"{ObjectName}\" skeleton_only \"{temp_model.FileName}\"";
				proc.Start();
				proc.WaitForExit();
			}
			else
				AutoClosingMessageBox.Show("Can't convert model to object!", "Error", 1500, MessageBoxIcon.Error);
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string old_game_mtl = "";
			pSettings.Load("GameMtlPath", ref old_game_mtl);

            bool OldViewPortAlpha = false;
            pSettings.Load("ViewportAlpha", ref OldViewPortAlpha);

            Settings ProgramSettings = new Settings(pSettings);
			ProgramSettings.ShowDialog();

			string game_mtl = "";
			pSettings.Load("GameMtlPath", ref game_mtl);

			if (old_game_mtl != game_mtl)
				ReloadGameMtl(game_mtl);

            bool ViewPortAlpha = false;
            pSettings.Load("ViewportAlpha", ref ViewPortAlpha);

			if (OldViewPortAlpha != ViewPortAlpha)
                disableAlphaToolStripMenuItem_Click(null, null);
        }

		private void changeLodToolStripMenuItem_Click(object sender, EventArgs e)
		{
			float old_lod = CurrentLod;
			SwiLod swiLod = new SwiLod(CurrentLod);
			swiLod.ShowDialog();

			if (swiLod.res)
			{
				CurrentLod = swiLod.Lod;
				if (old_lod != CurrentLod)
				{
                    RecalcMeshInfo();
					ReloadViewPort(true, false, true);
				}
			}
        }

        private void RecalcMeshInfo()
        {
            for (int idx = 0; idx < Model.Childs.Count; idx++)
            {
                Control Mesh = TexturesPage.Controls["TextureGrpBox_" + idx.ToString()];

                Label FaceLbl = (Label)Mesh.Controls["FacesLbl_" + idx.ToString()];
				string NewFaceText = FaceLabel.Text + Model.Childs[idx].Faces_SWI(CurrentLod).Count.ToString();
				int FaceLocationDiff = NewFaceText.Length - FaceLbl.Text.Length;

                FaceLbl.Text = NewFaceText;
                FaceLbl.Size = new Size(FaceLbl.Size.Width + (FaceLocationDiff * 6), FaceLbl.Size.Height);
                FaceLbl.Location = new Point(FaceLbl.Location.X - (FaceLocationDiff * 6), FaceLbl.Location.Y);

                Label VertsLbl = (Label)Mesh.Controls["VertsLbl_" + idx.ToString()];
                string NewVertsText = VertsLabel.Text + Model.Childs[idx].Vertices.Count.ToString();
                int VertsLocationDiff = NewVertsText.Length - VertsLbl.Text.Length;

                VertsLbl.Text = NewVertsText;
                VertsLbl.Size = new Size(VertsLbl.Size.Width + (VertsLocationDiff * 6), VertsLbl.Size.Height);
                VertsLbl.Location = new Point(VertsLbl.Location.X - (VertsLocationDiff * 6) - (FaceLocationDiff * 6), VertsLbl.Location.Y);

				int LinksLocationDiff = 0;

                if (Model.Header != null && Model.Header.IsSkeleton())
				{
					Label LinksLbl = (Label)Mesh.Controls["LinksLbl_" + idx.ToString()];
                    string NewLinksText = LinksLabel.Text + Model.Childs[idx].LinksCount().ToString();
                    LinksLocationDiff = NewVertsText.Length - VertsLbl.Text.Length;

                    LinksLbl.Text = NewLinksText;
					LinksLbl.Size = new Size(LinksLbl.Size.Width + (LinksLocationDiff * 6), LinksLbl.Size.Height);
					LinksLbl.Location = new Point(LinksLbl.Location.X - (LinksLocationDiff * 6) - (VertsLocationDiff * 6) - (FaceLocationDiff * 6), LinksLbl.Location.Y);
				}

				if (Model.Childs[idx].SWI.Count > 0)
				{
					Label LodsLbl = (Label)Mesh.Controls["LodsLbl_" + idx.ToString()];
					string NewLodsText = LodLabel.Text + Model.Childs[idx].SWI.Count.ToString();
                    int LodsLocationDiff = NewVertsText.Length - VertsLbl.Text.Length;

                    LodsLbl.Text = NewLodsText;
					LodsLbl.Size = new Size(LodsLbl.Size.Width + (LodsLocationDiff * 6), LodsLbl.Size.Height);
					LodsLbl.Location = new Point(LodsLbl.Location.X - (LodsLocationDiff * 6) - (LinksLocationDiff * 6) - (VertsLocationDiff * 6) - (FaceLocationDiff * 6), LodsLbl.Location.Y);
				}
            }
        }

        private void removeProgressiveMeshesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float old_lod = CurrentLod;

            SwiLod swiLod = new SwiLod(CurrentLod);
            swiLod.ShowDialog();

			if (swiLod.res)
			{
                Model.RemoveProgressive(CurrentLod);

				for (int idx = 0; idx < Model.Childs.Count; idx++)
				{
					Control Mesh = TexturesPage.Controls["TextureGrpBox_" + idx.ToString()];
					Label FaceLbl = (Label)Mesh.Controls["LodsLbl_" + idx.ToString()];
					FaceLbl.Visible = false;
				}

				removeProgressiveMeshesToolStripMenuItem.Enabled = LodMenuItem.Enabled = Model.IsProgressive();

				if (old_lod != CurrentLod)
					ReloadViewPort(true, false, true);
			}
        }

        private void moveRotateModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (Model == null) return;

            float[] old_offs = Model.LocalOffset;
            float[] old_rot = Model.LocalRotation;

            MoveMesh moveMesh = new MoveMesh(old_offs, old_rot, false, false);
            moveMesh.ShowDialog();

			if (moveMesh.res)
			{
                Model.LocalOffset = moveMesh.offset;
                Model.LocalRotation = moveMesh.rotation;

                for (int i = 0; i < Model.Childs.Count; i++)
				{
                    Model.Childs[i].SetLocalOffsetMain(Model.LocalOffset);
                    Model.Childs[i].SetLocalRotationMain(Model.LocalRotation);
				}
            }

            if (!FVec.Similar(old_offs, Model.LocalOffset) || !FVec.Similar(old_rot, Model.LocalRotation))
            {
                Model.RecalcBBox(true);
                ReloadViewPort(true, false, true);
            }
        }

        private string[] GameMtlParser(string filename)
		{
			List<string> materials = new List<string>();

			if (File.Exists(filename))
            {
                using (var xr_loader = new XRayLoader())
                {
                    using (var r = new BinaryReader(new FileStream(filename, FileMode.Open)))
                    {
                        xr_loader.SetStream(r.BaseStream);
                        xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)MTL.GAMEMTLS_CHUNK_MTLS, false, true));

                        int id = 0;
                        uint size;

                        while (true)
                        {
                            if (!xr_loader.find_chunk(id)) break;

                            Stream temp = xr_loader.Reader.BaseStream;

                            if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

                            size = xr_loader.find_chunkSize((int)MTL.GAMEMTL_CHUNK_MAIN);
                            if (size == 0) break;
                            xr_loader.ReadBytes(4);
                            materials.Add(xr_loader.ReadStringZ());

                            id++;
                            xr_loader.SetStream(temp);
                        }
                    }
                }
            }
            string[] ret = materials.ToArray();
			Array.Sort(ret);
			return ret;
		}

		public void ReloadGameMtl(string filename)
		{
			game_materials = GameMtlParser(filename);

			if (Model.Opened && Model.BoneData != null)
			{
				BoneParamsPage.Controls.Clear();
				for (int i = 0; i < Model.BoneData.Bones.Count; i++)
				{
					CreateBoneGroupBox(i, Model.BoneData.Bones[i].Name, Model.BoneData.Bones[i].ParentName, Model.IkData.Bones[i].Material, Model.IkData.Bones[i].Mass, Model.IkData.Bones[i].CenterMass, Model.IkData.Bones[i].Position, Model.IkData.Bones[i].Rotation);
				}
			}
		}

        private string CheckNaN(float val)
        {
			if (val.ToString() == "NaN")
				return "0";
			return ((decimal)val).ToString();
		}

        private void RichTextBoxImgDefender(object sender, KeyEventArgs e)
		{
			RichTextBox TextBox = sender as RichTextBox;
			if (e.Control && e.KeyCode == Keys.V)
			{
				if (Clipboard.ContainsText())
					TextBox.Paste(DataFormats.GetFormat(DataFormats.Text));
				e.Handled = true;
			}
		}

		static public bool IsTextCorrect(string text)
        {
			foreach (char ch in text)
            {
				if (ch > 0x1F && ch != 0x20)
					return true;
			}
			return false;
        }

		private string GetCorrectString(string text)
		{
			string ret_text = "", symbols = "";
			bool started = false;
			foreach (char ch in text)
			{
				if (started)
                {
					if (ch <= 0x1F || ch == 0x20)
						symbols += ch;
                    else
					{
						ret_text += symbols + ch;
						symbols = "";
					}
				}
				else if (ch > 0x1F && ch != 0x20)
				{
					started = true;
					ret_text += ch;
				}
			}
			return ret_text;
		}

		private void ClosingForm(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (ViewerThread != null && ViewerThread.ThreadState != System.Threading.ThreadState.Stopped)
					ViewerThread.Abort();
			}
			catch (Exception) { }

			try
			{
				Model.Destroy();
			}
			catch (Exception) { }

			if (ViewerWorking)
				pSettings.Save("FirstLoad", false);

			try
			{
				if (ViewerWorking)
				{
					ViewerProcess.Kill();
					ViewerProcess.Close();
					ViewerWorking = false;
				}
			}
			catch (Exception) { }

			for (int i = 0; i < 2; i++)
			{
				try
				{
					if (ConverterWorking[i])
					{
						ConverterProcess[i].Kill();
						ConverterProcess[i].Close();
						ConverterWorking[i] = false;
					}
				}
				catch (Exception) { }
			}

			try
			{
				if (!UseTexturesCache && Directory.Exists(TempFolder(false)))
					Directory.Delete(TempFolder(false), true);
			}
			catch (Exception) { }
		}

		private void ClosedForm(object sender, FormClosedEventArgs e)
		{
			ClosingForm(sender, null);
		}

		private void DragEnterCallback(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

			string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

			foreach (string file in fileList)
			{
				if (Path.GetExtension(file) == ".ogf" || Path.GetExtension(file) == ".dm" || Path.GetExtension(file) == ".detail" || Path.GetExtension(file) == ".obj")
				{
					e.Effect = DragDropEffects.Copy;
					break;
				}
			}
		}

		private void DragDropCallback(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

			string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

			foreach (string file in fileList)
			{
				if (Path.GetExtension(file) == ".ogf" || Path.GetExtension(file) == ".dm" || Path.GetExtension(file) == ".detail" || Path.GetExtension(file) == ".obj")
				{
					if (Model.OpenFile(file))
					{
                        Clear();
                        Model.FileName = file;
						AfterLoad(true);
					}
					break;
				}
			}
		}

		private void addMeshesToolStripMenuItem_Click(object sender, EventArgs e)
        {
			OpenOGFDialog.FileName = "";
            if (OpenOGFDialog.ShowDialog() == DialogResult.OK)
			{
                XRayModel SecondModel = new XRayModel();
                SecondModel.OpenFile(OpenOGFDialog.FileName);

				if (SecondModel.Header.IsSkeleton())
				{
					int old_childs_count = Model.Childs.Count;

					AddMesh addMeshDialog = new AddMesh(ref Model, SecondModel);
					addMeshDialog.ShowDialog();

					if (addMeshDialog.Res && old_childs_count != Model.Childs.Count)
					{
						TexturesPage.Controls.Clear();
						for (int i = Model.Childs.Count - 1; i >= 0; i--)
						{
							CreateTextureGroupBox(i);

							var TextureGroupBox = TexturesPage.Controls["TextureGrpBox_" + i.ToString()];
							TextureGroupBox.Controls["textureBox_" + i.ToString()].Text = Model.Childs[i].Texture; ;
							TextureGroupBox.Controls["shaderBox_" + i.ToString()].Text = Model.Childs[i].Shader;
						}

                        Model.RecalcBBox(false);
                    }
				}
				else
                    AutoClosingMessageBox.Show("Can't merge non skeleton model!", "Warning", 1000, MessageBoxIcon.Warning);
            }
        }

        private void recalcNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Model.Opened)
            {
                return;
            }

            SelectMeshes selectMeshes = new SelectMeshes(Model);
            selectMeshes.ShowDialog();

            if (!selectMeshes.res || selectMeshes.MeshChecked.Count != Model.Childs.Count)
            {
                return;
            }

            bool Reloaded = false;

            for (int i = 0; i < Model.Childs.Count; i++)
            {
                if (selectMeshes.MeshChecked[i])
                {
                    Reloaded = true;
                    Model.Childs[i].MeshNormalize();
                }
            }

            if (Reloaded)
            {
                ReloadViewPort(true, false, true);
                AutoClosingMessageBox.Show("Mesh normals recalculated!", "Info", 1000, MessageBoxIcon.Information);
            }
            else
                AutoClosingMessageBox.Show("Mesh normals don't changed!", "Warning", 1000, MessageBoxIcon.Warning);
        }

        private void openImageFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string image_path = string.Empty;
            pSettings.Load("ImagePath", ref image_path);

            if (!string.IsNullOrEmpty(image_path) && Directory.Exists(image_path))
            {
                Process PrFolder = new Process();
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Normal;
                psi.FileName = "explorer";
                psi.Arguments = image_path;
                PrFolder.StartInfo = psi;
                PrFolder.Start();
            }
        }

        private void BkpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            pSettings.Save(BkpCheckBox);
        }

        private void recalcBoundingBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Model.RecalcBBox(true);
            ReloadViewPort(true, false, true);
            AutoClosingMessageBox.Show("Bounding Box and Sphere recalculated!", "Info", 1000, MessageBoxIcon.Information);
        }

        private void UpdateNPC()
		{
			nPCCoPToSoCToolStripMenuItem.Enabled = CheckNPC(true);
			nPCSoCToCoPToolStripMenuItem.Enabled = CheckNPC(false);
        }

        private void NPC_ToSoC(object sender, EventArgs e)
        {
			if (CheckNPC(true))
			{
                Model.RemoveBone("root_stalker");
                Model.RemoveBone("bip01");

                Model.ChangeParent("root_stalker", "bip01_pelvis");
                Model.ChangeParent("bip01", "bip01_pelvis");

                Model.BoneData.Bones[0].ParentName = "";
                Model.BoneData.RecalcChilds();

                for (int i = 0; i < Model.BoneData.Bones.Count; i++)
				{
                    Model.IkData.Bones[i].Position = Resources.SoCSkeleton.Pos(i);
                    Model.IkData.Bones[i].Rotation = Resources.SoCSkeleton.Rot(i);
                    Model.IkData.Bones[i].CenterMass = FVec.RotateXYZ(Model.IkData.Bones[i].CenterMass, 0.0f, 180.0f, 0.0f);
				}

                foreach (var ch in Model.Childs)
				{
					uint links = ch.LinksCount();

					for (int i = 0; i < ch.Vertices.Count; i++)
					{
						for (int j = 0; j < links; j++)
							ch.Vertices[i].bones_id[j] = (ch.Vertices[i].bones_id[j] >= 2 ? ch.Vertices[i].bones_id[j] - 2 : 0);

						ch.Vertices[i].Offs = FVec.RotateXYZ(ch.Vertices[i].Offs, 0.0f, 180.0f, 0.0f);
                        ch.Vertices[i].local_offset = FVec.RotateXYZ(ch.Vertices[i].local_offset, 0.0f, 180.0f, 0.0f);
                        ch.Vertices[i].norm = FVec.RotateXYZ(ch.Vertices[i].norm, 0.0f, 180.0f, 0.0f);
						ch.Vertices[i].tang = FVec.RotateXYZ(ch.Vertices[i].tang, 0.0f, 180.0f, 0.0f);
						ch.Vertices[i].binorm = FVec.RotateXYZ(ch.Vertices[i].binorm, 0.0f, 180.0f, 0.0f);
					}
				}
                ReloadViewPort(true, false, true);
                AutoClosingMessageBox.Show("Successful!", "Info", 700, MessageBoxIcon.Information);
			}
			else
				AutoClosingMessageBox.Show("This model is not CoP NPC!", "Error", 2500, MessageBoxIcon.Error);

			UpdateNPC();
        }

        private void NPC_ToCoP(object sender, EventArgs e)
        {
			if (CheckNPC(false))
			{
                Model.AddBone("root_stalker", "", 0);
                Model.AddBone("bip01", "root_stalker", 1);

                Model.BoneData.Bones[2].ParentName = "bip01";
                Model.BoneData.RecalcChilds();

                for (int i = 0; i < Model.BoneData.Bones.Count; i++)
                {
                    Model.IkData.Bones[i].Position = Resources.CoPSkeleton.Pos(i);
                    Model.IkData.Bones[i].Rotation = Resources.CoPSkeleton.Rot(i);
                    Model.IkData.Bones[i].CenterMass = FVec.RotateXYZ(Model.IkData.Bones[i].CenterMass, 0.0f, 180.0f, 0.0f);
                }

                foreach (var ch in Model.Childs)
                {
                    for (int i = 0; i < ch.Vertices.Count; i++)
                    {
                        for (int j = 0; j < ch.LinksCount(); j++)
                            ch.Vertices[i].bones_id[j] = ch.Vertices[i].bones_id[j] + 2;

                        ch.Vertices[i].Offs = FVec.RotateXYZ(ch.Vertices[i].Offs, 0.0f, 180.0f, 0.0f);
                        ch.Vertices[i].local_offset = FVec.RotateXYZ(ch.Vertices[i].local_offset, 0.0f, 180.0f, 0.0f);
                        ch.Vertices[i].norm = FVec.RotateXYZ(ch.Vertices[i].norm, 0.0f, 180.0f, 0.0f);
                        ch.Vertices[i].tang = FVec.RotateXYZ(ch.Vertices[i].tang, 0.0f, 180.0f, 0.0f);
                        ch.Vertices[i].binorm = FVec.RotateXYZ(ch.Vertices[i].binorm, 0.0f, 180.0f, 0.0f);
                    }
                }
                ReloadViewPort(true, false, true);
                AutoClosingMessageBox.Show("Successful!", "Info", 700, MessageBoxIcon.Information);
            }
            else
                AutoClosingMessageBox.Show("This model is not SoC NPC!", "Error", 2500, MessageBoxIcon.Error);

            UpdateNPC();
        }

        private bool CheckNPC(bool cop_npc)
        {
			if (Model.Opened)
			{
				if (cop_npc)
				{
					if (Model.Header.IsSkeleton())
					{
						if (Model.BoneData.Bones.Count == 47 && Model.BoneData.GetBoneID("root_stalker") != -1)
							return true;
					}
				}
				else
				{
					if (Model.Header.IsSkeleton())
					{
						if (Model.BoneData.Bones.Count == 45 && Model.BoneData.GetBoneID("bip01_pelvis") != -1 && Model.BoneData.GetBoneID("root_stalker") == -1)
							return true;
					}
				}
			}

            return false;
        }

        private void syncUserdataAndMotionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
			string FirstFolder, SecondFolder;

			if (SyncFirstDialog.ShowDialog() && SyncSecondDialog.ShowDialog())
			{
                FirstFolder = SyncFirstDialog.FileName;
				SecondFolder = SyncSecondDialog.FileName;

				string FirstSubFolder = Path.GetDirectoryName(FirstFolder);
				string SecondSubFolder = Path.GetDirectoryName(SecondFolder);

				string[] FirstFilesList = Directory.GetFiles(FirstFolder, "*.ogf", SearchOption.AllDirectories);
				string[] SecondFilesList = Directory.GetFiles(SecondFolder, "*.ogf", SearchOption.AllDirectories);

				int FilesChanged = 0;
                for (int i = 0; i < FirstFilesList.Length; i++)
				{
					for (int j = 0; j < SecondFilesList.Length; j++)
					{
						string FirstFile = FirstFilesList[i].Replace(FirstSubFolder, "");
						string SecondFile = SecondFilesList[j].Replace(SecondSubFolder, "");
						if (FirstFile == SecondFile)
						{
                            XRayModel SourceModel = new XRayModel();
                            XRayModel DestModel = new XRayModel();
							if (SourceModel.OpenFile(FirstFilesList[i]) && DestModel.OpenFile(SecondFilesList[j]))
							{
								bool Changed = false;
								if (DestModel.MotionRefs != null)
								{
									if (SourceModel.MotionRefs == null)
										SourceModel.MotionRefs = new MotionRefs();

									if (SourceModel.MotionRefs.Refs != DestModel.MotionRefs.Refs)
									{
										bool tolstyak_exists = SourceModel.MotionRefs.Refs.Contains("actors\\tolstyak_animation");
										SourceModel.MotionRefs.Refs = DestModel.MotionRefs.Refs;
										if (tolstyak_exists)
											SourceModel.MotionRefs.Refs.Add("actors\\tolstyak_animation");
										Changed = true;
									}
								}
								else if (SourceModel.MotionRefs != null)
								{
									SourceModel.MotionRefs.Refs.Clear();
                                    Changed = true;
                                }

								if (DestModel.UserData != null)
								{
									if (SourceModel.UserData == null)
										SourceModel.UserData = new UserData();

									if (SourceModel.UserData.Userdata != DestModel.UserData.Userdata)
									{
										SourceModel.UserData.Userdata = DestModel.UserData.Userdata;
										Changed = true;
									}
								}
								else if (SourceModel.UserData != null)
								{
									SourceModel.UserData.Userdata = "";
                                    Changed = true;
                                }

								//if (DestModel.Motions != null)
								//{
								//	if (SourceModel.Motions != DestModel.Motions)
								//	{
								//		SourceModel.Motions = DestModel.Motions;

								//		if (SourceModel.MotionRefs != null && SourceModel.MotionRefs.refs.Count > 0)
								//			SourceModel.MotionRefs.refs.Clear();
        //                                Changed = true;
        //                            }
        //                        }

								if (Changed)
								{
									SourceModel.SaveFile(FirstFilesList[i]);
									FilesChanged++;
                                }
                            }
						}
					}
				}

                MessageBox.Show(FilesChanged.ToString() + " files changed!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void Msg(string text)
        {
			MessageBox.Show(text);
        }

        // Interface
		private void ReloadViewPort(bool create_model = true, bool force_texture_reload = false, bool force_reload = false)
		{
			if (ViewerWorking && ViewerProcess != null)
			{
                InitViewPort(create_model, force_texture_reload, force_reload);
			}
        }

        private void InitViewPort(bool create_model = true, bool force_texture_reload = false, bool force_reload = false)
        {
			if (!Model.Opened) return;

			if (ViewerWorking && ViewerProcess != null && CheckViewportModelVers() && !force_reload) return;

			bool old_viewer = ViewerWorking;
			ViewerWorking = false;
            ViewPortNeedReload = false;

            Model.FixOldBonesBind();
            Model.CalcBonesTransform();

            if (ViewerThread != null && ViewerThread.ThreadState != System.Threading.ThreadState.Stopped)
				ViewerThread.Abort();

			ViewerThread = new Thread(() => {
				string ObjName = TempFolder() + "\\" + Path.GetFileName(Path.ChangeExtension(Model.FileName, ".obj"));
				string exe_path = AppPath() + "\\f3d.exe";

				if (!File.Exists(exe_path))
				{
					MessageBox.Show("Can't find Viewport module.\nPlease, reinstall the app.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

                this.Invoke((MethodInvoker)delegate ()
				{
                    viewPortToolStripMenuItem.Enabled = false;
                });

                if (old_viewer)
				{
					ViewerProcess.Kill();
					ViewerProcess.Close();
				}

				for (int i = 0; i < 2; i++)
				{
					if (ConverterWorking[i])
					{
						ConverterProcess[i].Kill();
						ConverterProcess[i].Close();
						ConverterWorking[i] = false;
					}
				}

				if (ViewPortTextures)
				{
					string Textures = "";
					pSettings.LoadText("TexturesPath", ref Textures);

					List<string> pTextures = new List<string>();
					List<string> pConvertTextures = new List<string>();

					for (int i = 0; i < Model.Childs.Count; i++)
					{
						string texture_main = Textures + "\\" + Model.Childs[i].Texture + ".dds";
						string texture_temp = TempFolder() + "\\" + Path.GetFileName(Model.Childs[i].Texture + ".png");

						if (File.Exists(texture_temp) && !force_texture_reload)
							continue;

						pTextures.Add(texture_main);
						pTextures.Add(texture_temp);
					}

					int chld = 0;
					for (int i = 0; i < pTextures.Count; i++)
					{
						if (File.Exists(pTextures[i]) && (!File.Exists(pTextures[i + 1]) || force_texture_reload))
						{
							if (Model.Childs[chld].to_delete)
								continue;

							pConvertTextures.Add(pTextures[i]);
							pConvertTextures.Add(pTextures[i + 1]);
						}
						i++;
						chld++;
					}

					OldChildVisible.Clear();
					foreach (var ch in Model.Childs)
						OldChildVisible.Add(ch.to_delete);

					OldChildTextures.Clear();
					foreach (var ch in Model.Childs)
						OldChildTextures.Add(ch.Texture);

					if (pConvertTextures.Count > 0)
					{
						string ConverterArgs = "";
						ConverterArgs += $"{(ViewPortAlpha ? 1 : 0)}";
						ConverterArgs += $" {pConvertTextures.Count}";

						for (int i = 0; i < pConvertTextures.Count; i++)
						{
							ConverterArgs += $" \"{pConvertTextures[i]}\"";
						}

						ConverterWorking[0] = true;
						ConverterProcess[0] = new Process();
						ProcessStartInfo psi = new ProcessStartInfo();
						psi.CreateNoWindow = true;
						psi.UseShellExecute = false;
						psi.FileName = AppPath() + "\\TextureConverter.exe";
						psi.Arguments = ConverterArgs;
						ConverterProcess[0].StartInfo = psi;
						ConverterProcess[0].Start();
					}
				}

				if (ViewPortBones && showBonesToolStripMenuItem.Enabled)
				{
                    string ConverterArgs = "";
					int TexturesCount = 0;

					for (int i = 0; i < Model.BoneData.Bones.Count; i++)
					{
                        if (File.Exists($"{TempFolder()}\\{Model.BoneData.Bones[i].GetNotNullName()}.png"))
							continue;
						ConverterArgs += $" \"{Model.BoneData.Bones[i].Name}\" \"{TempFolder()}\\{Model.BoneData.Bones[i].GetNotNullName()}.png\"";
						TexturesCount++;
                    }
					
					ConverterWorking[1] = true;
					ConverterProcess[1] = new Process();
					ProcessStartInfo psi = new ProcessStartInfo();
					psi.CreateNoWindow = true;
					psi.UseShellExecute = false;
					psi.FileName = AppPath() + "\\TextToPng.exe";
					psi.Arguments = $"{TexturesCount}{ConverterArgs}";
					psi.WorkingDirectory = AppPath();
                    ConverterProcess[1].StartInfo = psi;
					ConverterProcess[1].Start();
				}

                for (int i = 0; i < 2; i++)
				{
					if (ConverterWorking[i])
					{
						ConverterProcess[i].WaitForExit();
						ConverterWorking[i] = false;
                    }
                }

				string bbox_texture_main = TempFolder() + "\\bbox_main_texture.png";
                string bbox_texture = TempFolder() + "\\bbox_texture.png";
                string null_texture = TempFolder() + "\\null_texture.png";
                if (ViewPortBBox && !File.Exists(bbox_texture_main))
				{
                    using (Bitmap b = new Bitmap(8, 8))
                    {
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.Clear(Color.FromArgb(127, Color.Green));
                        }
                        b.Save(bbox_texture_main, ImageFormat.Png);
                    }
                }

                if (ViewPortBBox && !File.Exists(bbox_texture))
                {
                    using (Bitmap b = new Bitmap(8, 8))
                    {
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.Clear(Color.FromArgb(127, Color.Red));
                        }
                        b.Save(bbox_texture, ImageFormat.Png);
                    }
                }

				if (ViewPortBones && showBonesToolStripMenuItem.Enabled && !File.Exists(null_texture))
				{
                    using (Bitmap b = new Bitmap(8, 8))
                    {
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.Clear(Color.FromArgb(70, 15, 25, 15));
                        }
                        b.Save(null_texture, ImageFormat.Png);
                    }
                }

                string image_path = "";
				pSettings.Load("ImagePath", ref image_path);

				bool first_load = true;
				pSettings.Load("FirstLoad", ref first_load, true);

				if (create_model)
                    Model.SaveObj(ObjName, CurrentLod, ViewPortBones && showBonesToolStripMenuItem.Enabled, ViewPortBBox, ViewPortTextures);

				ViewerProcess.StartInfo.FileName = exe_path;
				ViewerProcess.StartInfo.Arguments = $"--input=\"{ObjName}\" --output=\"{image_path}\"" + (first_load ? " --filename" : "");
				ViewerProcess.StartInfo.UseShellExecute = false;
				ViewerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

				ViewerProcess.Start();
				ViewerProcess.WaitForInputIdle();
				ViewerWorking = true;

				try
				{
					this.Invoke((MethodInvoker)delegate ()
					{
						const int GWL_STYLE = -16;
						const int WS_CAPTION = 0x00C00000;
						const int WS_THICKFRAME = 0x00040000;

						SetParent(ViewerProcess.MainWindowHandle, ViewPage.Handle);
						int style = GetWindowLong(ViewerProcess.MainWindowHandle, GWL_STYLE);
						style = style & ~WS_CAPTION & ~WS_THICKFRAME;
						SetWindowLong(ViewerProcess.MainWindowHandle, GWL_STYLE, style);
						ResizeEmbeddedApp(null, null);
                        viewPortToolStripMenuItem.Enabled = true;
                    });
				}
				catch(Exception) { }
            });
			ViewerThread.Start();
        }

		private bool CheckViewportModelVers()
        {
            if (OldChildTextures.Count != Model.Childs.Count || OldChildVisible.Count != Model.Childs.Count) return false;

			if (OldChildTextures.Count != 0)
            {
				int i = 0;
				foreach (var ch in Model.Childs)
				{
					if (ch.Texture != OldChildTextures[i])
						return false;
					i++;
				}
			}

			if (OldChildVisible.Count != 0)
			{
				int i = 0;
				foreach (var ch in Model.Childs)
				{
					if (ch.to_delete != OldChildVisible[i])
						return false;
					i++;
				}
			}

			return true;
		}

		private void reloadToolStripMenuItem1_Click(object sender, EventArgs e)
		{
            ReloadViewPort(true, true, true);
		}

		private void disableAlphaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ViewPortAlpha = !ViewPortAlpha;
            pSettings.Save("DisableAlpha", ViewPortAlpha);

            if (ViewPortAlpha)
                disableAlphaToolStripMenuItem.Text = "Disable Alpha";
            else
                disableAlphaToolStripMenuItem.Text = "Enable Alpha";

            ReloadViewPort(false, true, true);
		}

        private void DisableTexturesMenuItem_Click(object sender, EventArgs e)
        {
            ViewPortTextures = !ViewPortTextures;
            pSettings.Save("DisableTextures", ViewPortTextures);

			if (Model.Opened)
			{
                string ObjName = TempFolder() + "\\" + Path.GetFileName(Path.ChangeExtension(Model.FileName, ".obj"));
                Model.SaveObj(ObjName, CurrentLod, ViewPortBones, ViewPortBBox, ViewPortTextures);
			}

            if (!ViewPortTextures)
				DisableTexturesMenuItem.Text = "Enable Textures";
			else
                DisableTexturesMenuItem.Text = "Disable Textures";

            ReloadViewPort(false, false, true);
        }

        private void showBBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ViewPortBBox = !ViewPortBBox;

            pSettings.Save("BBoxEnabled", ViewPortBBox);

            if (!ViewPortBBox)
                item.Text = "Show Bounding Box";
            else
                item.Text = "Hide Bounding Box";

            ReloadViewPort(true, false, true);
        }

        private void showBonesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ViewPortBones = !ViewPortBones;

			if (e != null)
				pSettings.Save("BonesEnabled", ViewPortBones);

            if (!ViewPortBones)
                item.Text = "Show Bones";
            else
                item.Text = "Hide Bones";

            ReloadViewPort(true, false, true);
        }

		private void ResizeEmbeddedApp(object sender, EventArgs e)
		{
			if (ViewerProcess == null || !ViewerWorking)
				return;

			const int SWP_NOACTIVATE = 0x0010;
			const int SWP_NOZORDER = 0x0004;

			int width = ViewPage.Width;
			int height = ViewPage.Height;
			SetWindowPos(ViewerProcess.MainWindowHandle, IntPtr.Zero, 0, 0, width, height, SWP_NOACTIVATE | SWP_NOZORDER);
		}

        private void CreateTextureGroupBox(int idx)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(TexturesGropuBox.Location.X, TexturesGropuBox.Location.Y + (TexturesGropuBox.Size.Height + 2) * idx);
			GroupBox.Size = TexturesGropuBox.Size;
			GroupBox.Text = TexturesGropuBox.Text + " [" + idx + "]";
			GroupBox.Name = "TextureGrpBox_" + idx;
			GroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			GroupBox.Dock = TexturesGropuBox.Dock;
			CreateTextureBoxes(idx, GroupBox);
			CreateTextureLabels(idx, GroupBox);
			TexturesPage.Controls.Add(GroupBox);
		}

		private void CreateTextureBoxes(int idx, GroupBox box)
		{
			var newTextBox = Copy.TextBox(TexturesTextBoxEx);
			newTextBox.Name = "textureBox_" + idx;
			newTextBox.TextChanged += new System.EventHandler(this.TextBoxFilter);

			var newTextBox2 = Copy.TextBox(ShaderTextBoxEx);
			newTextBox2.Name = "shaderBox_" + idx;
			newTextBox2.TextChanged += new System.EventHandler(this.TextBoxFilter);

			var newButton = Copy.Button(DeleteMesh);
			newButton.Name = "DeleteButton_" + idx;
			newButton.Click += new System.EventHandler(this.ButtonFilter);
			newButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			newButton.Text = Model.Is(XRayModel.ModelFormat.eDM) ? "Replace" : newButton.Text;

            if (Model.Childs[idx].to_delete)
            {
                newButton.Text = "Return Mesh";
                newButton.BackColor = Color.FromArgb(255, 255, 128, 128);
            }
            else
            {
                newButton.Text = "Delete Mesh";
                newButton.BackColor = SystemColors.Control;
            }

            var newButton2 = Copy.Button(MoveMeshButton);
            newButton2.Name = "MoveButton_" + idx;
            newButton2.Click += new System.EventHandler(this.ButtonFilter);
            newButton2.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var newButton3 = Copy.Button(MeshDataButton);
            newButton3.Name = "DataButton_" + idx;
            newButton3.Click += new System.EventHandler(this.ButtonFilter);
            newButton3.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            box.Controls.Add(newTextBox);
			box.Controls.Add(newTextBox2);
			box.Controls.Add(newButton);
            box.Controls.Add(newButton2);

			if (Model.Is(XRayModel.ModelFormat.eDM))
				box.Controls.Add(newButton3);
        }

		private void CreateTextureLabels(int idx, GroupBox box)
		{
			var newLbl = Copy.Label(TexturesPathLabelEx);
			newLbl.Name = "textureLbl_" + idx;

			var newLbl2 = Copy.Label(ShaderNameLabelEx);
			newLbl2.Name = "shaderLbl_" + idx;

			var newLbl3 = Copy.Label(FaceLabel);
			newLbl3.Name = "FacesLbl_" + idx;
			newLbl3.Text = FaceLabel.Text + Model.Childs[idx].Faces_SWI(CurrentLod).Count.ToString();
			newLbl3.Size = new Size(FaceLabel.Size.Width + (Model.Childs[idx].Faces_SWI(CurrentLod).Count.ToString().Length * 6), FaceLabel.Size.Height);
			newLbl3.Location = new Point(FaceLabel.Location.X - (Model.Childs[idx].Faces_SWI(CurrentLod).Count.ToString().Length * 6), FaceLabel.Location.Y);

            var newLbl4 = Copy.Label(VertsLabel);
			newLbl4.Name = "VertsLbl_" + idx;
			newLbl4.Text = VertsLabel.Text + Model.Childs[idx].Vertices.Count.ToString();
			newLbl4.Size = new Size(VertsLabel.Size.Width + (Model.Childs[idx].Vertices.Count.ToString().Length * 6), VertsLabel.Size.Height);
			newLbl4.Location = new Point(VertsLabel.Location.X - (Model.Childs[idx].Vertices.Count.ToString().Length * 6) - (Model.Childs[idx].Faces_SWI(CurrentLod).Count.ToString().Length * 6), VertsLabel.Location.Y);

			var newLbl5 = Copy.Label(LinksLabel);
			newLbl5.Name = "LinksLbl_" + idx;
			newLbl5.Text = LinksLabel.Text + Model.Childs[idx].LinksCount().ToString();
			newLbl5.Size = new Size(LinksLabel.Size.Width + (Model.Childs[idx].LinksCount().ToString().Length * 6), LinksLabel.Size.Height);
			newLbl5.Location = new Point(LinksLabel.Location.X - (Model.Childs[idx].Vertices.Count.ToString().Length * 6) - (Model.Childs[idx].Faces_SWI(CurrentLod).Count.ToString().Length * 6) - (Model.Childs[idx].LinksCount().ToString().Length * 6), LinksLabel.Location.Y);

			var newLbl6 = Copy.Label(LodLabel);
			newLbl6.Name = "LodsLbl_" + idx;
			newLbl6.Text = LodLabel.Text + Model.Childs[idx].SWI.Count.ToString();
			newLbl6.Size = new Size(LodLabel.Size.Width + (Model.Childs[idx].SWI.Count.ToString().Length * 6), LodLabel.Size.Height);
			newLbl6.Location = new Point(LodLabel.Location.X - (Model.Childs[idx].Vertices.Count.ToString().Length * 6) - (Model.Childs[idx].Faces_SWI(CurrentLod).Count.ToString().Length * 6) - (Model.Childs[idx].LinksCount().ToString().Length * 6) - (Model.Childs[idx].SWI.Count.ToString().Length * 6), LodLabel.Location.Y);

			box.Controls.Add(newLbl);
			box.Controls.Add(newLbl2);
			box.Controls.Add(newLbl3);
			box.Controls.Add(newLbl4);

			if (Model.Header.IsSkeleton())
				box.Controls.Add(newLbl5);

			if (Model.Childs[idx].SWI.Count > 0)
				box.Controls.Add(newLbl6);
		}

		private void CreateBoneGroupBox(int idx, string bone_name, string parent_bone_name, string material, float mass, float[] center, float[] pos, float[] rot)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(BoneParamsGroupBox.Location.X, BoneParamsGroupBox.Location.Y + (BoneParamsGroupBox.Size.Height + 2) * idx);
			GroupBox.Size = BoneParamsGroupBox.Size;
			GroupBox.Text = BoneParamsGroupBox.Text + " [" + idx + "]";
			GroupBox.Name = "BoneGrpBox_" + idx;
			GroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			GroupBox.Dock = BoneParamsGroupBox.Dock;

			CreateBoneTextBox(idx, GroupBox, bone_name, parent_bone_name, material, mass, center, pos, rot);
			BoneParamsPage.Controls.Add(GroupBox);
		}

		private void CreateBoneTextBox(int idx, GroupBox box, string bone_name, string parent_bone_name, string material, float mass, float[] center, float[] pos, float[] rot)
		{
			var BoneNameTextBox = Copy.TextBox(BoneNameTextBoxEx);
			BoneNameTextBox.Name = "boneBox_" + idx;
			BoneNameTextBox.Text = bone_name;
			BoneNameTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			BoneNameTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var BoneNameLabel = Copy.Label(BoneNameLabelEx);
			BoneNameLabel.Name = "boneLabel_" + idx;

			var ParentBoneNameTextBox = Copy.TextBox(ParentBoneTextBoxEx);
			ParentBoneNameTextBox.Name = "ParentboneBox_" + idx;
			ParentBoneNameTextBox.Text = parent_bone_name;

			var ParentBoneNameLabel = Copy.Label(ParentBoneLabelEx);
			ParentBoneNameLabel.Name = "ParentboneLabel_" + idx;
			ParentBoneNameLabel.Size = ParentBoneLabelEx.Size;
			ParentBoneNameLabel.Location = ParentBoneLabelEx.Location;
			ParentBoneNameLabel.Text = ParentBoneLabelEx.Text;

			var MateriaBox = new Control();
			if (game_materials.Count() == 0)
			{
				var MaterialTextBox = Copy.TextBox(MaterialTextBoxEx);
				MaterialTextBox.Name = "MaterialBox_" + idx;
				MaterialTextBox.Text = material;
				MaterialTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
				MaterialTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

				MateriaBox = MaterialTextBox;
			}
			else
			{
				var MaterialTextBox = new ComboBox();
				MaterialTextBox.Name = "MaterialBox_" + idx;
				MaterialTextBox.Size = MaterialTextBoxEx.Size;
				MaterialTextBox.Location = MaterialTextBoxEx.Location;
				MaterialTextBox.Text = material;
				MaterialTextBox.Tag = "string";
				MaterialTextBox.SelectedIndexChanged += new System.EventHandler(this.TextBoxBonesFilter);
				MaterialTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
				MaterialTextBox.Items.AddRange(game_materials);
				MaterialTextBox.DropDownStyle = ComboBoxStyle.DropDownList;

				if (MaterialTextBox.Items.Contains(material))
					MaterialTextBox.SelectedIndex = MaterialTextBox.Items.IndexOf(material);
				else
				{
					MaterialTextBox.Items.Insert(0, material);
					MaterialTextBox.SelectedIndex = 0;
				}

				MateriaBox = MaterialTextBox;
			}

			var MaterialLabel = Copy.Label(MaterialLabelEx);
			MaterialLabel.Name = "MaterialLabel_" + idx;

			var MassTextBox = Copy.TextBox(MassTextBoxEx);
			MassTextBox.Name = "MassBox_" + idx;
			MassTextBox.Text = CheckNaN(mass);
			MassTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			MassTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var MassLabel = Copy.Label(MassLabelEx);
			MassLabel.Name = "MassLabel_" + idx;

			var LayoutPanel = Copy.TableLayoutPanel(BonesParamsPanel);
			LayoutPanel.Name = "LayoutPanel_" + idx;

			var CenterMassTextBoxX = Copy.TextBox(CenterOfMassXTextBox);
			CenterMassTextBoxX.Name = "CenterBoxX_" + idx;
			CenterMassTextBoxX.Text = CheckNaN(center[0]);
			CenterMassTextBoxX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var CenterMassTextBoxY = Copy.TextBox(CenterOfMassYTextBox);
			CenterMassTextBoxY.Name = "CenterBoxY_" + idx;
			CenterMassTextBoxY.Text = CheckNaN(center[1]);
			CenterMassTextBoxY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var CenterMassTextBoxZ = Copy.TextBox(CenterOfMassZTextBox);
			CenterMassTextBoxZ.Name = "CenterBoxZ_" + idx;
			CenterMassTextBoxZ.Text = CheckNaN(center[2]);
			CenterMassTextBoxZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var CenterMassLabel = Copy.Label(CenterOfMassLabelEx);
			CenterMassLabel.Name = "CenterMassLabel_" + idx;

			var PositionX = Copy.TextBox(PositionXTextBox);
			PositionX.Name = "PositionX_" + idx;
			PositionX.Text = CheckNaN(pos[0]);
			PositionX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var PositionY = Copy.TextBox(PositionYTextBox);
			PositionY.Name = "PositionY_" + idx;
			PositionY.Text = CheckNaN(pos[1]);
			PositionY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var PositionZ = Copy.TextBox(PositionZTextBox);
			PositionZ.Name = "PositionZ_" + idx;
			PositionZ.Text = CheckNaN(pos[2]);
			PositionZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var PositionLabel = Copy.Label(PositionLabelEx);
			PositionLabel.Name = "PositionLabel_" + idx;

			var RotationX = Copy.TextBox(RotationXTextBox);
			RotationX.Name = "RotationX_" + idx;
			RotationX.Text = CheckNaN(rot[0]);
			RotationX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var RotationY = Copy.TextBox(RotationYTextBox);
			RotationY.Name = "RotationY_" + idx;
			RotationY.Text = CheckNaN(rot[1]);
			RotationY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var RotationZ = Copy.TextBox(RotationZTextBox);
			RotationZ.Name = "RotationZ_" + idx;
			RotationZ.Text = CheckNaN(rot[2]);
			RotationZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var RotationLabel = Copy.Label(RotationLabelEx);
			RotationLabel.Name = "RotationLabel_" + idx;

			LayoutPanel.Controls.Add(MassTextBox, 0, 0);
			LayoutPanel.Controls.Add(CenterMassTextBoxX, 0, 1);
			LayoutPanel.Controls.Add(CenterMassTextBoxY, 1, 1);
			LayoutPanel.Controls.Add(CenterMassTextBoxZ, 2, 1);
			LayoutPanel.Controls.Add(PositionX, 0, 2);
			LayoutPanel.Controls.Add(PositionY, 1, 2);
			LayoutPanel.Controls.Add(PositionZ, 2, 2);
			LayoutPanel.Controls.Add(RotationX, 0, 3);
			LayoutPanel.Controls.Add(RotationY, 1, 3);
			LayoutPanel.Controls.Add(RotationZ, 2, 3);

			box.Controls.Add(LayoutPanel);

			box.Controls.Add(BoneNameTextBox);
			box.Controls.Add(ParentBoneNameTextBox);
			box.Controls.Add(MateriaBox);

			box.Controls.Add(BoneNameLabel);
			box.Controls.Add(ParentBoneNameLabel);
			box.Controls.Add(MaterialLabel);
			box.Controls.Add(MassLabel);
			box.Controls.Add(CenterMassLabel);
			box.Controls.Add(PositionLabel);
			box.Controls.Add(RotationLabel);
		}
    }
}
