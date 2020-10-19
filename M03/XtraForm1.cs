using System;
using System.Text;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Utils.Extensions;
using DBConnection;
using MDS00;

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
            txeCREATE.EditValue = "0";
            txeDATE.EditValue = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            StringBuilder sbSQL = new StringBuilder();
            sbSQL.Append("SELECT OIDCOLOR AS No, ColorNo, ColorName, ColorType, CreatedBy, CreatedDate ");
            sbSQL.Append("FROM ProductColor ");
            sbSQL.Append("ORDER BY ColorType, ColorName, OIDCOLOR ");
            new ObjDevEx.setGridControl(gcColor, gvColor, sbSQL).getData(false, false, true, true);

        }

        private void NewData()
        {
            txeColorID.EditValue = new DBQuery("SELECT CASE WHEN ISNULL(MAX(OIDCOLOR), '') = '' THEN 1 ELSE MAX(OIDCOLOR) + 1 END AS NewNo FROM ProductColor").getString();
            txeColorNo.EditValue = "";
            txeColorName.EditValue = "";
            cbeColorType.EditValue = "";
        }

        private int findColorType(string ColorType)
        {
            int return_value = 999;
            switch (ColorType)
            {
                case "Finished Goods":
                    return_value = 0;
                    break;
                case "Fabric":
                    return_value = 1;
                    break;
                case "Accessory":
                    return_value = 2;
                    break;
                case "Packaging":
                    return_value = 3;
                    break;
                default:
                    return_value = 999;
                    break;
            }
            return return_value;
        }

        private string findColorTypeName(string ColorType)
        {
            string return_value = "";
            switch (ColorType)
            {
                case "0":
                    return_value = "Finished Goods";
                    break;
                case "1":
                    return_value = "Fabric";
                    break;
                case "2":
                    return_value = "Accessory";
                    break;
                case "3":
                    return_value = "Packaging";
                    break;
                default:
                    return_value = "";
                    break;
            }
            return return_value;
        }

        private void gvColor_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            txeColorID.EditValue = gvColor.GetFocusedRowCellValue("No").ToString();
            txeColorNo.EditValue = gvColor.GetFocusedRowCellValue("ColorNo").ToString();
            txeColorName.EditValue = gvColor.GetFocusedRowCellValue("ColorName").ToString();
            cbeColorType.EditValue = findColorTypeName(gvColor.GetFocusedRowCellValue("ColorType").ToString());

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
            if (txeColorNo.EditValue.ToString() == "")
            {
                FUNC.msgWarning("Please input color no.");
                txeColorNo.Focus();
            }
            else if (txeColorName.EditValue.ToString() == "")
            {
                FUNC.msgWarning("Please input color name.");
                txeColorName.Focus();
            }
            else if (cbeColorType.EditValue.ToString() == "")
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
                    int ComType = findColorType(cbeColorType.EditValue.ToString());

                    string strCREATE = "0";
                    if (txeCREATE.EditValue != null)
                    {
                        strCREATE = txeCREATE.EditValue.ToString();
                    }

                    sbSQL.Append("IF NOT EXISTS(SELECT OIDCOLOR FROM ProductColor WHERE OIDCOLOR = '" + txeColorID.EditValue.ToString() + "') ");
                    sbSQL.Append(" BEGIN ");
                    sbSQL.Append("  INSERT INTO ProductColor(ColorNo, ColorName, ColorType, CreatedBy, CreatedDate) ");
                    sbSQL.Append("  VALUES('" + txeColorNo.EditValue.ToString() + "', '" + txeColorName.EditValue.ToString() + "', '" + ComType.ToString() + "', '" + strCREATE + "', GETDATE()) ");
                    sbSQL.Append(" END ");
                    sbSQL.Append("ELSE ");
                    sbSQL.Append(" BEGIN ");
                    sbSQL.Append("  UPDATE ProductColor SET ");
                    sbSQL.Append("      ColorNo = '" + txeColorNo.EditValue.ToString() + "', ColorName = '" + txeColorName.EditValue.ToString() + "', ColorType = '" + ComType.ToString() + "' ");
                    sbSQL.Append("  WHERE(OIDCOLOR = '" + txeColorID.EditValue.ToString() + "') ");
                    sbSQL.Append(" END ");

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
    }
}