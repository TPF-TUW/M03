using System;
using System.Text;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Utils.Extensions;
using DBConnection;
using MDS00;
using System.Drawing;
using DevExpress.XtraGrid.Views.Grid;

namespace M03
{
    public partial class XtraForm1 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private Functionality.Function FUNC = new Functionality.Function();
        public XtraForm1()
        {
            InitializeComponent();
            UserLookAndFeel.Default.StyleChanged += MyStyleChanged;
            iniConfig = new IniFile("Config.ini");
            UserLookAndFeel.Default.SetSkinStyle(iniConfig.Read("SkinName", "DevExpress"), iniConfig.Read("SkinPalette", "DevExpress"));
        }

        private IniFile iniConfig;

        private void MyStyleChanged(object sender, EventArgs e)
        {
            UserLookAndFeel userLookAndFeel = (UserLookAndFeel)sender;
            LookAndFeelChangedEventArgs lookAndFeelChangedEventArgs = (DevExpress.LookAndFeel.LookAndFeelChangedEventArgs)e;
            //MessageBox.Show("MyStyleChanged: " + lookAndFeelChangedEventArgs.Reason.ToString() + ", " + userLookAndFeel.SkinName + ", " + userLookAndFeel.ActiveSvgPaletteName);
            iniConfig.Write("SkinName", userLookAndFeel.SkinName, "DevExpress");
            iniConfig.Write("SkinPalette", userLookAndFeel.ActiveSvgPaletteName, "DevExpress");
        }

        private void XtraForm1_Load(object sender, EventArgs e)
        {
            bbiNew.PerformClick();
        }

        private void LoadData()
        {
            StringBuilder sbSQL = new StringBuilder();
            sbSQL.Append("SELECT OIDCOLOR AS No, ColorNo, ColorName, ColorType, CASE WHEN ColorType=0 THEN 'Finished Goods' ELSE CASE WHEN ColorType=1 THEN 'Fabric' ELSE CASE WHEN ColorType=2 THEN 'Accessory' ELSE CASE WHEN ColorType=3 THEN 'Packaging' ELSE '' END END END END AS ColorTypeName, CreatedBy, CreatedDate ");
            sbSQL.Append("FROM ProductColor ");
            sbSQL.Append("ORDER BY ColorType, ColorName, OIDCOLOR ");
            new ObjDevEx.setGridControl(gcColor, gvColor, sbSQL).getData(false, false, true, true);

            sbSQL.Clear();
            sbSQL.Append("SELECT '0' AS ID, 'Finished Goods' AS ColorType ");
            sbSQL.Append("UNION ALL ");
            sbSQL.Append("SELECT '1' AS ID, 'Fabric' AS ColorType ");
            sbSQL.Append("UNION ALL ");
            sbSQL.Append("SELECT '2' AS ID, 'Accessory' AS ColorType ");
            sbSQL.Append("UNION ALL ");
            sbSQL.Append("SELECT '3' AS ID, 'Packaging' AS ColorType ");
            new ObjDevEx.setGridLookUpEdit(cbeColorType, sbSQL, "ColorType", "ID").getData();
        }

        private void NewData()
        {
            txeColorID.EditValue = new DBQuery("SELECT CASE WHEN ISNULL(MAX(OIDCOLOR), '') = '' THEN 1 ELSE MAX(OIDCOLOR) + 1 END AS NewNo FROM ProductColor").getString();
            txeColorNo.EditValue = "";
            txeColorName.EditValue = "";
            cbeColorType.EditValue = "";

            lblStatus.Text = "* Add Color";
            lblStatus.ForeColor = Color.Green;

            txeCREATE.EditValue = "0";
            txeDATE.EditValue = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void gvColor_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            lblStatus.Text = "* Edit Color";
            lblStatus.ForeColor = Color.Red;
            txeColorID.EditValue = gvColor.GetFocusedRowCellValue("No").ToString();
            txeColorNo.EditValue = gvColor.GetFocusedRowCellValue("ColorNo").ToString();
            txeColorName.EditValue = gvColor.GetFocusedRowCellValue("ColorName").ToString();
            cbeColorType.EditValue = gvColor.GetFocusedRowCellValue("ColorType").ToString();

            txeCREATE.EditValue = gvColor.GetFocusedRowCellValue("CreatedBy").ToString();
            txeDATE.EditValue = gvColor.GetFocusedRowCellValue("CreatedDate").ToString();
        }

        private void bbiNew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadData();
            NewData();
        }

        private void bbiSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (txeColorNo.Text.Trim() == "")
            {
                FUNC.msgWarning("Please input color no.");
                txeColorNo.Focus();
            }
            else if (txeColorName.Text.Trim() == "")
            {
                FUNC.msgWarning("Please input color name.");
                txeColorName.Focus();
            }
            else if (cbeColorType.Text.Trim() == "")
            {
                FUNC.msgWarning("Please select color type.");
                cbeColorType.Focus();
            }
            else
            {
                if (FUNC.msgQuiz("Confirm save data ?") == true)
                {
                    StringBuilder sbSQL = new StringBuilder();
                    //CalendarMaster

                    int ComType = Convert.ToInt32(cbeColorType.EditValue.ToString());      

                    string strCREATE = "0";
                    if (txeCREATE.Text.Trim() != "")
                    {
                        strCREATE = txeCREATE.Text.Trim();
                    }

                    sbSQL.Append("IF NOT EXISTS(SELECT OIDCOLOR FROM ProductColor WHERE ColorNo = N'" + txeColorNo.Text.Trim().Replace("'", "''") + "') ");
                    sbSQL.Append(" BEGIN ");
                    sbSQL.Append("  INSERT INTO ProductColor(ColorNo, ColorName, ColorType, CreatedBy, CreatedDate) ");
                    sbSQL.Append("  VALUES(N'" + txeColorNo.Text.Trim().Replace("'", "''") + "', N'" + txeColorName.Text.Trim().Replace("'", "''") + "', '" + ComType.ToString() + "', '" + strCREATE + "', GETDATE()) ");
                    sbSQL.Append(" END ");
                    sbSQL.Append("ELSE ");
                    sbSQL.Append(" BEGIN ");
                    sbSQL.Append("  UPDATE ProductColor SET ");
                    sbSQL.Append("      ColorNo = N'" + txeColorNo.Text.Trim().Replace("'", "''") + "', ColorName = N'" + txeColorName.Text.Trim().Replace("'", "''") + "', ColorType = '" + ComType.ToString() + "' ");
                    sbSQL.Append("  WHERE(OIDCOLOR = '" + txeColorID.Text.Trim() + "') ");
                    sbSQL.Append(" END ");
                    //MessageBox.Show(sbSQL.ToString());
                    if (sbSQL.Length > 0)
                    {
                        try
                        {
                            bool chkSAVE = new DBQuery(sbSQL).runSQL();
                            if (chkSAVE == true)
                            {
                                FUNC.msgInfo("Save complete.");
                                bbiNew.PerformClick();
                            }
                        }
                        catch (Exception)
                        { }
                    }
                }

            }
        }

        private void txeColorNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txeColorName.Focus();
            }
        }

        private void txeColorNo_LostFocus(object sender, EventArgs e)
        {
            if (txeColorNo.Text.Trim() != "" && lblStatus.Text == "* Add Color")
            {
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.Append("SELECT TOP(1) ColorNo FROM ProductColor WHERE (ColorNo = N'" + txeColorNo.Text.Trim().Replace("'", "''") + "') ");
                if (new DBQuery(sbSQL).getString() != "")
                {
                    FUNC.msgWarning("Duplicate color no. !! Please Change.");
                    txeColorNo.Text = "";
                    txeColorNo.Focus();
                }
            }
            else if (txeColorNo.Text.Trim() != "" && lblStatus.Text == "* Edit Color")
            {
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.Append("SELECT TOP(1) OIDCOLOR ");
                sbSQL.Append("FROM ProductColor ");
                sbSQL.Append("WHERE (ColorNo = N'" + txeColorNo.Text.Trim() + "') ");
                string strCHK = new DBQuery(sbSQL).getString();
                if (strCHK != "" && strCHK != txeColorID.Text.Trim())
                {
                    FUNC.msgWarning("Duplicate color no. !! Please Change.");
                    txeColorNo.Text = "";
                    txeColorNo.Focus();
                }
                else
                {
                    txeColorName.Focus();
                }
            }
            else
            {
                txeColorName.Focus();
            }
        }

        private void txeColorName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                cbeColorType.Focus();
            }
        }

        private void txeColorName_LostFocus(object sender, EventArgs e)
        {
            txeColorName.Text = txeColorName.Text.ToUpper().Trim();
        }

        private void gvColor_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (sender is GridView)
            {
                GridView gView = (GridView)sender;
                if (!gView.IsValidRowHandle(e.RowHandle)) return;
                int parent = gView.GetParentRowHandle(e.RowHandle);
                if (gView.IsGroupRow(parent))
                {
                    for (int i = 0; i < gView.GetChildRowCount(parent); i++)
                    {
                        if (gView.GetChildRowHandle(parent, i) == e.RowHandle)
                        {
                            e.Appearance.BackColor = i % 2 == 0 ? Color.AliceBlue : Color.White;
                        }
                    }
                }
                else
                {
                    e.Appearance.BackColor = e.RowHandle % 2 == 0 ? Color.AliceBlue : Color.White;
                }
            }
        }
    }
}