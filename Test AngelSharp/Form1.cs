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

namespace Test_AngelSharp
{
    public partial class Form1 : Form
    {
       
        string error_page = "Исключено из КТРУ";
        string not_page = "Страница не найдена";
        string okpd_code = null;
        string error_page2 = null;
        int count_site_item = 0;
        public Form1()
        {
            InitializeComponent();
        }
    

        async void test_site_script()
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            string connString = "Server=localhost; Port=5432; Database=System; User id=postgres; Password=Nikrus48;";
            var conn = new NpgsqlConnection(connString);
            conn.Open();
            NpgsqlCommand comm = new NpgsqlCommand();
            comm.Connection = conn;
            comm.CommandType = CommandType.Text;


            for (int i = 29112; i <= 29112; i++)
            {
                var url = "https://zakupki.gov.ru/epz/ktru/ktruCard/commonInfo.html?itemId="+i+"";
                var doc = await context.OpenAsync(url);

                var title = doc.Title;
                if (title == not_page )
                {
                    
                    continue;
                }
                var error = doc.GetElementsByClassName("col margTop8 padTop8")[0].GetElementsByClassName("section__info");
                error_page2 = error[0].Text().Trim();
                if (error_page2 == error_page)
                {
                   
                    continue;
                   
                }

                var code = doc.GetElementsByClassName("row blockInfo")[1].GetElementsByClassName("section__info");
                var name = doc.GetElementsByClassName("row blockInfo")[1].GetElementsByClassName("section__info");

                string code_ktry = code[0].Text().Trim();  
                string name_ktry = code[1].Text().Trim();

                
               
                string[] code1 = code_ktry.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                
                url = " https://zakupki44fz.ru/app/okpd2/" + code1[0] + "";
               
           

                doc = await context.OpenAsync(url);
                okpd_code = doc.Title;

                MessageBox.Show(okpd_code);
               
               

                string[] words = okpd_code.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);


               
                comm.CommandText = "Delete from okpd2,ktru,nomenclature,describe,characteristic";
                

                comm.CommandText = "Insert into okpd2 (ocpd2_code,okpd2_name) Values(:_search0,:_search1)";                    
                comm.Parameters.AddWithValue("_search0", code1[0].ToString().Trim());
                comm.Parameters.AddWithValue("_search1", words[1]);
               
                                                
                comm.CommandText = "Insert into ktru (ocpd2_code,ktru_code ,ktru_name) Values(:_search0,:_search2,:_search1)";
                comm.Parameters.AddWithValue("_search0", code1[0].ToString().Trim());
                comm.Parameters.AddWithValue("_search2", code_ktry.ToString().Trim());
                comm.Parameters.AddWithValue("_search1", name_ktry);
                
                                  
                comm.CommandText = "Insert into nomenclature (ocpd2_code,ktru_code ,nomenclature_name,nomenclature_id) Values(:_search0,:_search2,:_search1,default)";
                comm.Parameters.AddWithValue("_search0", code1[0].ToString().Trim());
                comm.Parameters.AddWithValue("_search2", code_ktry.ToString().Trim());
                comm.Parameters.AddWithValue("_search1", name_ktry);

                comm.ExecuteNonQuery();


                // MessageBox.Show("Назавание ОКПД2 - " + words[1] + "\nКод - " + code1[0]);
                // MessageBox.Show("Назавание КТРУ - " + name_ktry + "\nКод - " + code_ktry);
                url = "https://zakupki.gov.ru/epz/ktru/ktruCard/ktru-description.html?itemId=" + i + "";
                

                doc = await context.OpenAsync(url);

                string name_char = "";
                string unit_char = "";
                string need_char = "";
                string[] words1 = null;
                var size = doc.GetElementsByClassName("tableBlock__body")[0].GetElementsByClassName("tableBlock__row");
               

                int size_tab = size.Count();
               


                for(int j=0; j<size_tab; j++)
                {
                    var name_chara = doc.GetElementsByClassName("tableBlock__row")[j].GetElementsByClassName("tableBlock__col tableBlock__col_first");
                    var unit = doc.GetElementsByClassName("tableBlock__row")[j].GetElementsByClassName("tableBlock__col");
                    var need = doc.GetElementsByClassName("tableBlock__row")[j].GetElementsByClassName("revert");

                    if(name_chara.Count() == 1)
                    {
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
                        MessageBox.Show("Наименование - "+ words1[0].Trim() + "\nОбязательность"+ need_char + "\nЗначение - "+unit_char);

                     
                            comm.CommandText = "Insert into characteristic (characteristic_name, characteristic_id , characteristic_value, characteristic_unit,characteristic_into_tech_spec,characteristic_necessarily) " +
                                "Values(:_search0,default,' ',:_search2,default,:_search1)";
                            comm.Parameters.AddWithValue("_search0", words1[0].ToString().Trim());
                            comm.Parameters.AddWithValue("_search2", unit_char.ToString().Trim());
                            comm.Parameters.AddWithValue("_search1", need_char.ToString().Trim());
                            
                                                    
                            comm.CommandText = "Select nomenclature_id from nomenclature where ktru_code = :_search0";
                            comm.Parameters.AddWithValue("_search0", code_ktry.ToString());
                            int int_code_bd = (int)comm.ExecuteScalar();
                            comm.CommandText = "Select characteristic_id from  characteristic where  characteristic_name = :_search0";
                            comm.Parameters.AddWithValue("_search0", code_ktry.ToString());
                            int int_code_bd1 = (int)comm.ExecuteScalar();

                            comm.CommandText = "Insert into describe (characteristic_id,nomenclature_id) Values(:_search0,:_search2)";
                            comm.Parameters.AddWithValue("_search0", int_code_bd);
                            comm.Parameters.AddWithValue("_search2", int_code_bd1);

                            comm.ExecuteNonQuery();


                        
                    }

                }



                // таблица номенклатура
                // id\название\код ктуръкод опкд
                // таблица характеристики
                // id\название\значение\единица измер\ добавление в тех\ обязательность
                // таблица связь
                // id хар\ id ном

               

              
                
                /* comm.CommandText = "Insert into technical_specification (tech_spec_name_item,tech_spec_value_char,tech_spec_unit_char,tech_spec_count,tech_spec_price," +
                "tech_spec_id,tech_spec_date,tech_spec_to_doc,tech_spec_used,user_login)" +
                "Values(:_search0,:_search1,:_search2,:_search3,:_search4,default, default, default, default,:_user)";*/
                //comm.Parameters.AddWithValue("_search0", Tech_Spec0);

              
                comm.Dispose();
                conn.Close();


            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //search_site_count_items();
            test_site_script();
           
        }
    }
}
