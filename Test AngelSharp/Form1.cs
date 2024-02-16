using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using AngleSharp;
using AngleSharp.Dom;
using Npgsql;
using System.Threading;

namespace Test_AngelSharp
{
    public partial class Form1 : Form
    {
       
        string error_page = "Исключено из КТРУ";
        string not_page = "Страница не найдена";
        string okpd_code = string.Empty;
        string error_page2 = string.Empty;
        
        public Form1()
        {
            InitializeComponent();
        }


        async void Test_site_script()
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            string connString = "Server=localhost; Port=5432; Database=System; User id=postgres; Password=Nikrus48;";
            var conn = new NpgsqlConnection(connString);
            conn.Open();
            //7540 //7900 okay 
            for (int i = 7540; i <= 7950; i++)
            {
                try
                {



                    NpgsqlCommand comm = new NpgsqlCommand();
                    comm.Connection = conn;
                    comm.CommandType = CommandType.Text;



                    if (i == 7540)
                    {

                        comm.CommandText = "DELETE FROM nomenclature ";
                        comm.ExecuteNonQuery();

                        comm.CommandText = "DELETE FROM characteristic ";
                        comm.ExecuteNonQuery();

                        comm.CommandText = "DELETE FROM ktru ";
                        comm.ExecuteNonQuery();

                        comm.CommandText = "DELETE FROM okpd2 ";
                        comm.ExecuteNonQuery();
                    }

                    var url = "https://zakupki.gov.ru/epz/ktru/ktruCard/commonInfo.html?itemId=" + i + "";
                    var doc = await context.OpenAsync(url);

                    var title = doc.Title;
                    if (title == not_page)
                    {

                        continue;
                    }
                    var error = doc.GetElementsByClassName("col margTop8 padTop8")[0].GetElementsByClassName("section__info");
                    error_page2 = error[0].Text().Trim();
                    if (error_page2 == error_page)
                    {

                        continue;

                    }


                    // MessageBox.Show(""+ i);
                    var code = doc.GetElementsByClassName("row blockInfo")[1].GetElementsByClassName("section__info");
                    var name = doc.GetElementsByClassName("row blockInfo")[1].GetElementsByClassName("section__info");
                    string code_ktry = code[0].Text().Trim();
                    string name_ktry = code[1].Text().Trim();
                    string[] code1 = code_ktry.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                    url = " https://moy-zakupki.ru/okpd-2/" + code1[0] + "/";

                    doc = await context.OpenAsync(url);
                    var name_ocpd = doc.GetElementsByClassName("lg:text-3xl text-3xl font-bold text-gray-800 lg:w-3/4 items-center");
                    okpd_code = name_ocpd[0].Text().Trim();
                    string[] words = okpd_code.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    string nomcharid = code_ktry.Replace("-", ".");

                    comm.CommandText = " SELECT EXISTS(SELECT * FROM okpd2 WHERE ocpd2_code = :_search0)";
                    comm.Parameters.AddWithValue("_search0", code1[0].Trim());
                    bool check = (bool)comm.ExecuteScalar();
                    if (check == false)
                    {
                        comm.CommandText = "Insert into okpd2 (ocpd2_code,okpd2_name) Values(:_search0,:_search1)";
                        comm.Parameters.AddWithValue("_search0", code1[0].Trim());
                        comm.Parameters.AddWithValue("_search1", words[2].Trim());
                        comm.ExecuteNonQuery();


                    }

                    comm.CommandText = "Insert into ktru (ocpd2_code,ktru_code ,ktru_name) Values(:_search0,:_search2,:_search1)";
                    comm.Parameters.AddWithValue("_search0", code1[0].Trim());
                    comm.Parameters.AddWithValue("_search2", code_ktry.Trim());
                    comm.Parameters.AddWithValue("_search1", name_ktry.Trim());
                    comm.ExecuteNonQuery();


                    comm.CommandText = "Insert into nomenclature (ocpd2_code,ktru_code ,nomenclature_name,nomenclature_id,nomcharid) Values(:_search0,:_search2,:_search1,default,:_search3)";
                    comm.Parameters.AddWithValue("_search0", code1[0].Trim());
                    comm.Parameters.AddWithValue("_search2", code_ktry.Trim());
                    comm.Parameters.AddWithValue("_search1", name_ktry.Trim());
                    comm.Parameters.AddWithValue("_search3", nomcharid.Trim());
                    comm.ExecuteNonQuery();
                    comm.Dispose();


                    url = "https://zakupki.gov.ru/epz/ktru/ktruCard/ktru-description.html?itemId=" + i + "";


                    doc = await context.OpenAsync(url);

                    string name_char = string.Empty;
                    string unit_char = string.Empty;
                    string need_char = string.Empty;
                    string value_char = string.Empty;
                    string[] words1 = null;
                    List<string> values = new List<string>();
                    var size = doc.GetElementsByClassName("tableBlock__body")[0].GetElementsByClassName("tableBlock__row");

                    string words12 = string.Empty;
                    int size_tab = size.Count();
                    string test = string.Empty;

                    for (int j = 1; j < size_tab; j++)
                    {
                        values.Clear();
                        var name_chara = doc.GetElementsByClassName("tableBlock__row")[j].GetElementsByClassName("tableBlock__col tableBlock__col_first");
                        var value = doc.GetElementsByClassName("tableBlock__row")[j].GetElementsByClassName("tableBlock__col tableBlock__col_last");
                        var value2 = doc.GetElementsByClassName("tableBlock__row")[j].GetElementsByClassName("tableBlock__col");
                        var unit = doc.GetElementsByClassName("tableBlock__row")[j].GetElementsByClassName("tableBlock__col");
                        var need = doc.GetElementsByClassName("tableBlock__row")[j].GetElementsByClassName("revert");

                        if (name_chara.Count() == 1)
                        {
                            test = string.Empty;
                            if (j != 1)
                            {
                                NpgsqlCommand comm1 = new NpgsqlCommand();
                                comm1.Connection = conn;
                                comm1.CommandType = CommandType.Text;

                                comm1.CommandText = "Insert into characteristic (characteristic_name, characteristic_id , characteristic_value, " +
                                    "characteristic_unit,characteristic_into_tech_spec,characteristic_necessarily,nomcharid) " +
                                        "Values(:_search0,default,:_search4,:_search2,false,:_search1,:_search3)";
                                comm1.Parameters.AddWithValue("_search0", words1[0].Trim());
                                comm1.Parameters.AddWithValue("_search2", unit_char.Trim());
                                comm1.Parameters.AddWithValue("_search1", need_char.Trim());
                                comm1.Parameters.AddWithValue("_search3", nomcharid.Trim());
                                comm1.Parameters.AddWithValue("_search4", value_char.Trim().ToString());
                                comm1.ExecuteNonQuery();
                                comm1.Dispose();

                                name_char = string.Empty;
                                unit_char = string.Empty;
                                need_char = string.Empty;
                                value_char = string.Empty;
                                words1 = null;
                            }



                            foreach (var par in name_chara)
                            {
                                name_char = (par.Text().Trim());

                            }

                            words1 = name_char.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var par in need)
                            {
                                need_char = (par.Text().Trim());

                            }
                            foreach (var par in unit)
                            {
                                unit_char = (par.Text().Trim());

                            }

                            foreach (var par in value)
                            {
                                value_char += (par.Text().Trim());

                            }
                            if (value_char == string.Empty)
                            {
                                foreach (var par in value2)
                                {
                                    values.Add(par.Text().Trim());

                                }
                                value_char += values[1].Replace(" ", "").Replace(Environment.NewLine, "");
                            }

                        }
                        else
                        {
                            test = string.Empty;
                            foreach (var par in value)
                            {
                                test = (par.Text().Trim());
                                value_char += ", ";
                                value_char += (par.Text().Trim());

                            }

                            foreach (var par in value2)
                            {
                                values.Add(par.Text().Trim());



                            }
                            if (test == string.Empty)
                            {

                                value_char += "," + values[0].Replace(" ", "").Replace(Environment.NewLine, "");
                            }

                        }

                    }


                    if (i == 10000)
                    {
                        MessageBox.Show("10% готово");
                    }

                    if (i == 25000)
                    {
                        MessageBox.Show("25% готово");
                    }
                    if (i == 50000)
                    {
                        MessageBox.Show("50% готово");
                    }
                    if (i == 75000)
                    {
                        MessageBox.Show("75% готово");
                    }
                    if (i == 100000)
                    {
                        MessageBox.Show("100% готово");
                    }
                }
                catch
                {
                    continue;
                }
                
            }
            MessageBox.Show("Stop");
            conn.Dispose();
            conn.Close();

        }
        private void button1_Click(object sender, EventArgs e)
        {
            Test_site_script();
           
        }
    }
}
